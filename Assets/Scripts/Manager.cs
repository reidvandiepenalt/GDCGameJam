using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public static bool canDraw = false;

    Enemy currentEnemy;

    public Spell currentSpell;
    EdgeCollider2D currentPattern;

    int playerHealth;
    [SerializeField] int playerMaxHealth = 25;
    int currentBlock = 0;
    int playerMana;
    [SerializeField] int playerMaxMana = 5;
    public int PlayerMana { get => playerMana; }
    List<GameObject> hand = new List<GameObject>();
    List<GameObject> handDisplay = new List<GameObject>();
    [SerializeField] List<GameObject> deck = new List<GameObject>();
    List<GameObject> undrawn = new List<GameObject>();
    List<GameObject> discard = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        playerHealth = playerMaxHealth;
        playerMana = playerMaxMana;

        currentEnemy = Instantiate(enemyPrefabs[0]).GetComponent<Enemy>();
        currentEnemy.Setup(enemyAttackField, enemyHealthCount, enemyHealthBar, this);

        //do this at start of fight
        undrawn.AddRange(deck);

        UIUpdate();
    }

    private void FixedUpdate()
    {
        switch (turnState)
        {
            case TurnState.draw:
                currentEnemy.GetNextAttack();
                currentBlock = 0;

                DrawCards(3);

                turnState = TurnState.pickSpell;
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
                //clear card display (may need to move)
                Spell[] displayedCards = cardDisplay.GetComponentsInChildren<Spell>();
                foreach(Spell spell in displayedCards)
                {
                    Destroy(spell.gameObject);
                }

                currentEnemy.Attack();

                UIUpdate();

                break;
        }
    }

    public void CardPlayed(Spell spell)
    {
        currentSpell = spell;
        Destroy(spell.gameObject);

        playerMana -= spell.manaCost;

        //do self damage
        AdjustHealth(-spell.selfDamage);

        cardDisplay.SetActive(false);

        UIUpdate();

        turnState++;
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
        playerHealth += value;
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
        foreach (Vector2 point in currentPattern.points)
        {
            for(int i = 0; i < drawPoints.Length; i++)
            {
                if(Vector2.Distance(point, drawPoints[i]) <= fizzleDist)
                {
                    hasDrawnOn[i] = true;
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
            //add text

            Destroy(currentPattern.gameObject);
            Destroy(rend.gameObject);

            currentEnemy.gameObject.SetActive(true);
            enemyStatus.SetActive(true);

            canDraw = false;

            if (playerMana > 0 && hand.Count > 0)
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


        Debug.Log("t: " + timer);
        Debug.Log("avg: " + avgDist);

        Destroy(currentPattern.gameObject);
        Destroy(rend.gameObject);

        currentEnemy.gameObject.SetActive(true);
        enemyStatus.SetActive(true);

        canDraw = false;

        if(playerMana > 0 && hand.Count > 0)
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
