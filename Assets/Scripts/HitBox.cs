using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HitBox: MonoBehaviour
{
    public float maxHealth;
    float health;
    public bool localDestroyOnDeath;
    public UnityEvent onDeath;

    private void Awake()
    {
        health = maxHealth;
    }

    public void ReceiveDamage(float damage) {
        if (health > 0) {
            health -= damage;
            if (health <= 0)
            {
                onDeath.Invoke();
                if (localDestroyOnDeath)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
