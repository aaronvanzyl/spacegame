using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpaceGame {
    public class HitBox : MonoBehaviour
    {
        public float maxHealth;
        float health;

        /// <summary>
        /// This object receives damage equal to value * |force| on collision.
        /// </summary>
        public float forceCollisionDamage;

        /// <summary>
        /// This object receives damage equal to value on collision.
        /// </summary>
        public float flatCollisionDamage;

        /// <summary>
        /// Destroy object when health reaches 0. Disable if destruction is handled through onDeath events.
        /// </summary>
        public bool destroyOnDeath;

        /// <summary>
        /// Events called when health reaches 0.
        /// </summary>
        public UnityEvent onDeath;

        private void Awake()
        {
            health = maxHealth;
        }

        public void ReceiveDamage(float damage) {
            if (health > 0)
            {
                health -= damage;
                if (health <= 0)
                {
                    onDeath.Invoke();
                    if (destroyOnDeath)
                    {
                        if (TryGetComponent(out PhotonView photonView))
                        {
                            PhotonNetwork.Destroy(photonView);

                        }
                        else
                        {
                            Destroy(gameObject);
                        }
                    }
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            //ReceiveDamage(flatCollisionDamage + forceCollisionDamage * collision.otherRigidbody.mass * collision.relativeVelocity.magnitude);
        }
    }
}
