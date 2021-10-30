using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    int health;
    public int maxHealth;
    public Manager manager;
    public Text attackField;
    public Text healthCount;
    public Image healthBar;

    [SerializeField] SpriteRenderer sprite;

    [SerializeField] int minDamage = 3;
    [SerializeField] int maxDamage = 6;
    int curDamage;

    Attacks curAttack;

    enum Attacks
    {
        damage
    }

    public void Setup(Text attackField, Text healthCount, Image hb, Manager m)
    {
        this.attackField = attackField;
        this.healthCount = healthCount;
        healthBar = hb;
        health = maxHealth;
        HealthUpdate();
        manager = m;
    }

    public void TakeDamage(int value)
    {
        health -= value;
        if(health <= 0)
        {
            //dead
        }

        HealthUpdate();
    }

    void HealthUpdate()
    {
        healthCount.text = health + " / " + maxHealth;
        healthBar.fillAmount = (float)health / maxHealth;
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
        curDamage = Random.Range(minDamage, maxDamage + 1);
        attackField.text = "Attacking for: " + curDamage;
    }
}
