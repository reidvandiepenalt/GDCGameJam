using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    public enum TurnState
    {
        draw,
        pickSpell,
        countdown,
        drawSpell,
        enemyAttack
    }

    
    public static TurnState turnState = TurnState.draw;

    [SerializeField] GameObject gameOver;
    [SerializeField] GameObject canvas;
    [SerializeField] GameObject spellFinishText;
    [SerializeField] List<GameObject> enemyPrefabs;
    [SerializeField] Drawing drawManager;
    [SerializeField] GameObject cardDisplay;
    [SerializeField] GameObject darkenPanel;
    [SerializeField] Text countdown;
    [SerializeField] Text manaCounter;
    [SerializeField] Image manaBar;
    [SerializeField] Text healthCounter;
    [SerializeField] Image healthBar;
    [SerializeField] Text blockCounter;

    [SerializeField] float fizzleDist = 0.8f;

    [SerializeField] Text enemyAttackField;
    [SerializeField] Text enemyHealthCount;
    [SerializeField] Image enemyHealthBar;
    [SerializeField] GameObject enemyStatus;

    bool countingDown = false;
    bool isDead = false;

    public static bool canDraw = false;

    Enemy currentEnemy;

    public Spell currentSpell;
    EdgeCollider2D currentPattern;

    int playerHealth;
    [SerializeField] int playerMaxHealth = 25;
    int currentBlock = 0;
    int playerMana;
    [SerializeField] int playerMaxMana = 3;
    public int PlayerMana { get => playerMana; }
    [SerializeField] List<GameObject> hand = new List<GameObject>();
    List<GameObject> handDisplay = new List<GameObject>();
    [SerializeField] List<GameObject> deck = new List<GameObject>();
    [SerializeField] List<GameObject> undrawn = new List<GameObject>();
    [SerializeField]List<GameObject> discard = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        NewEnemy();
    }

    private void FixedUpdate()
    {
        if (isDead) { return; }

        switch (turnState)
        {
            case TurnState.draw:
                playerMana = playerMaxMana;
                currentEnemy.GetNextAttack();
                currentBlock = 0;

                DrawCards(3);

                turnState = TurnState.pickSpell;

                UIUpdate();
                break;
            case TurnState.pickSpell:
                cardDisplay.SetActive(true);
                break;
            case TurnState.countdown:
                currentPattern = Instantiate(currentSpell.pattern, Vector3.zero,
                    Quaternion.identity).GetComponent<EdgeCollider2D>();
                darkenPanel.SetActive(true);
                StartCoroutine("Countdown");
                countingDown = true;
                turnState++;

                currentEnemy.gameObject.SetActive(false);
                enemyStatus.SetActive(false);
                break;
            case TurnState.drawSpell:
                if(!countingDown)
                {
                    canDraw = true;
                }
                break;
            case TurnState.enemyAttack:
                CardReset();

                currentEnemy.Attack();

                UIUpdate();

                turnState = TurnState.draw;

                break;
        }
    }

    void CardReset()
    {
        Spell[] displayedCards = cardDisplay.GetComponentsInChildren<Spell>();
        foreach (Spell spell in displayedCards)
        {
            Destroy(spell.gameObject);
        }
        handDisplay.Clear();

        discard.AddRange(hand);
        hand.Clear();
    }

    public void CardPlayed(Spell spell)
    {
        currentSpell = spell;
        handDisplay.Remove(spell.gameObject);
        Destroy(spell.gameObject);

        playerMana -= spell.manaCost;

        //do self damage
        AdjustHealth(-spell.selfDamage);

        cardDisplay.SetActive(false);

        UIUpdate();

        turnState++;
    }

    public void NewEnemy()
    {
        CardReset();
        turnState = TurnState.draw;

        playerHealth = playerMaxHealth;
        playerMana = playerMaxMana;

        currentEnemy = Instantiate(enemyPrefabs[0]).GetComponent<Enemy>();
        currentEnemy.Setup(enemyAttackField, enemyHealthCount, enemyHealthBar, this);

        undrawn.AddRange(deck);

        UIUpdate();
    }

    IEnumerator Countdown()
    {
        countdown.text = "3";
        yield return new WaitForSeconds(1);
        countdown.text = "2";
        yield return new WaitForSeconds(1);
        countdown.text = "1";
        yield return new WaitForSeconds(1);
        darkenPanel.SetActive(false);
        countingDown = false;
    }

    public void AdjustHealth(int value)
    {
        if(value < 0 && currentBlock > 0)
        {
            currentBlock += value;
            if(currentBlock < 0)
            {
                playerHealth += currentBlock;
                currentBlock = 0;
            }
        }
        else
        {
            playerHealth += value;
        }
        UIUpdate();
        if(playerHealth < 0)
        {
            gameOver.SetActive(true);
            isDead = true;
        }
    }

    public void Reload()
    {
        SceneManager.LoadScene("SampleScene");
    }

    void UIUpdate()
    {
        healthCounter.text = $"{playerHealth} / {playerMaxHealth}";
        healthBar.fillAmount = (float)playerHealth / playerMaxHealth;
        manaCounter.text = $"{playerMana} / {playerMaxMana}";
        manaBar.fillAmount = (float)playerMana / playerMaxMana;
        blockCounter.text = currentBlock.ToString();
    }


    public void DrawEnded(LineRenderer rend, float timer)
    {
        Vector3[] drawPoints = new Vector3[rend.positionCount];
        rend.GetPositions(drawPoints);

        //fizzle check
        bool[] hasDrawnOn = new bool[currentPattern.pointCount];
        for(int i = 0; i < hasDrawnOn.Length; i++)
        {
            hasDrawnOn[i] = false;
        }
        for (int j = 0; j < currentPattern.pointCount; j++)
        {
            for(int i = 0; i < drawPoints.Length; i++)
            {
                if(Vector2.Distance(currentPattern.points[j], drawPoints[i]) <= fizzleDist)
                {
                    hasDrawnOn[j] = true;
                    break;
                }
            }
        }
        bool fizzled = false;
        foreach(bool drawnOn in hasDrawnOn)
        {
            if (!drawnOn) { fizzled = true; break; }
        }

        if (fizzled)
        {
            Text ft = Instantiate(spellFinishText, canvas.transform).GetComponent<Text>();
            ft.text = "Fizzled";
            Destroy(ft.gameObject, 0.5f);

            Destroy(currentPattern.gameObject);
            Destroy(rend.gameObject);

            currentEnemy.gameObject.SetActive(true);
            enemyStatus.SetActive(true);

            canDraw = false;

            if (playerMana > 0 && handDisplay.Count > 0)
            {
                turnState = TurnState.pickSpell;
            }
            else
            {
                turnState = TurnState.enemyAttack;
            }
            return;
        }

        float sum = 0;
        for(int i = 0; i < drawPoints.Length; i++)
        {
            sum += Vector2.Distance(currentPattern.ClosestPoint(drawPoints[i]),
                drawPoints[i]);
        }
        float avgDist = sum / drawPoints.Length;
        float modifier = (Mathf.Clamp((currentSpell.TimeAvg - timer) / currentSpell.TimeDif,
            -1f, 1f) +
            Mathf.Clamp((currentSpell.DistAvg - avgDist) / currentSpell.DistDif,
            -1f, 1f)) / 4;
        currentEnemy.TakeDamage(Mathf.RoundToInt(currentSpell.baseDamage * (1 + modifier)));
        currentBlock += Mathf.RoundToInt(currentSpell.baseBlock * (1 + modifier));
        DrawCards(Mathf.RoundToInt(currentSpell.baseDraw * (1 + modifier)));

        Text finishText = Instantiate(spellFinishText, canvas.transform).GetComponent<Text>();
        if(modifier < -0.3f)
        {
            finishText.text = "Bad";
        }else if (modifier < -0.1f)
        {
            finishText.text = "Poor";
        }else if (modifier < 0.1f)
        {
            finishText.text = "OK";
        }else if (modifier < 0.3f)
        {
            finishText.text = "Good";
        }
        else
        {
            finishText.text = "Great";
        }
        Destroy(finishText.gameObject, 0.5f);

        UIUpdate();

        Debug.Log("t: " + timer);
        Debug.Log("avg: " + avgDist);

        Destroy(currentPattern.gameObject);
        Destroy(rend.gameObject);

        currentEnemy.gameObject.SetActive(true);
        enemyStatus.SetActive(true);

        canDraw = false;

        if(playerMana > 0 && handDisplay.Count > 0)
        {
            turnState = TurnState.pickSpell;
        }
        else
        {
            turnState = TurnState.enemyAttack;
        }
    }

    void DrawCards(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (undrawn.Count == 0)//discard to undrawn
            {
                undrawn.AddRange(discard);
                discard.Clear();
            }
            int randIndex = Random.Range(0, undrawn.Count);
            hand.Add(undrawn[randIndex]);
            GameObject displayCard = Instantiate(undrawn[randIndex], cardDisplay.transform);
            handDisplay.Add(displayCard);
            displayCard.GetComponent<Spell>().manager = this;

            undrawn.RemoveAt(randIndex);
        }
    }

    public void EndTurn()
    {
        if(turnState == TurnState.pickSpell)
        {
            turnState = TurnState.enemyAttack;
        }
    }
}
