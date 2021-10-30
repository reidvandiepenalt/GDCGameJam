using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health;
    public Manager manager;

    [SerializeField] SpriteRenderer sprite;

    [SerializeField] int minDamage = 3;
    [SerializeField] int maxDamage = 6;
    int curDamage;

    Attacks curAttack;

    enum Attacks
    {
        damage
    }

    public void Attack()
    {
        switch (curAttack)
        {
            case Attacks.damage:
                manager.AdjustHealth(-curDamage);
                break;
        }
    }

    public void GetNextAttack()
    {
        curAttack = Attacks.damage;
        int curDamage = Random.Range(minDamage, maxDamage + 1);
    }
}
