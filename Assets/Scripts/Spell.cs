using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Spell : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Text manaText;

    public GameObject pattern;
    [SerializeField] float avgDistMin, avgDistMax;
    [SerializeField] float timeMin, timeMax;
    public int baseDamage;
    public int baseBlock;
    public int baseDraw;
    public int manaCost;
    public int selfDamage;
    public Manager manager;

    public float DistDif { get => (avgDistMax - avgDistMin) / 2;}
    public float TimeDif { get => (timeMax - timeMin) / 2;}
    public float DistAvg { get => avgDistMax - DistDif; }
    public float TimeAvg { get => timeMax - TimeDif; }

    void Start()
    {
        manaText.text = manaCost.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(manager.PlayerMana >= manaCost)
        {
            manager.CardPlayed(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = new Vector3(transform.localScale.x * 1.25f,
            transform.localScale.y * 1.25f, transform.localScale.z * 1.25f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = new Vector3(transform.localScale.x / 1.25f,
            transform.localScale.y / 1.25f, transform.localScale.z / 1.25f);
    }
}
