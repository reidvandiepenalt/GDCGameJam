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
    bool countingDown = false;

    public static bool canDraw = false;

    Enemy currentEnemy;

    public Spell currentSpell;
    EdgeCollider2D currentPattern;

    [SerializeField] int playerHealth = 25;
    int currentBlock = 0;
    [SerializeField] int playerMana = 5;
    public int PlayerMana { get => playerMana; }
    List<GameObject> hand = new List<GameObject>();
    List<GameObject> handDisplay = new List<GameObject>();
    [SerializeField] List<GameObject> deck = new List<GameObject>();
    List<GameObject> undrawn = new List<GameObject>();
    List<GameObject> discard = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        currentEnemy = Instantiate(enemyPrefabs[0]).GetComponent<Enemy>();

        //do this at start of fight
        undrawn.AddRange(deck);
    }

    private void FixedUpdate()
    {
        switch (turnState)
        {
            case TurnState.draw:
                //get enemies attack, display to player?
                currentEnemy.GetNextAttack();

                for(int i = 0; i < 3; i++)
                {
                    if(undrawn.Count == 0)//discard to undrawn
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

                break;
        }
    }

    void EndTurn()
    {
        Spell[] displayedCards = cardDisplay.GetComponentsInChildren<Spell>();
        foreach (Spell spell in displayedCards)
        {
            Destroy(spell.gameObject);
        }
    }

    public void CardPlayed(Spell spell)
    {
        currentSpell = spell;
        Destroy(spell.gameObject);

        //do self damage
        AdjustHealth(-spell.selfDamage);

        cardDisplay.SetActive(false);

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


    public void DrawEnded(LineRenderer rend, float timer)
    {
        Vector3[] drawPoints = new Vector3[rend.positionCount];
        rend.GetPositions(drawPoints);

        float sum = 0;
        for(int i = 0; i < drawPoints.Length; i++)
        {
            sum += Vector2.Distance(currentPattern.ClosestPoint(drawPoints[i]),
                drawPoints[i]);
        }

        Debug.Log("t: " + timer);
        Debug.Log("avg: " + (sum / drawPoints.Length));

        Destroy(currentPattern.gameObject);
        Destroy(rend.gameObject);

        canDraw = false;

        if(playerMana > 0)
        {
            turnState = TurnState.pickSpell;
        }
        else
        {
            turnState = TurnState.enemyAttack;
        }
    }
}
