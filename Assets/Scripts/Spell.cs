using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Spell : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject pattern;
    public float avgDistMin, avgDistMax;
    public float speedMin, speedMax;
    public int manaCost;
    public int selfDamage;
    public Manager manager;

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
