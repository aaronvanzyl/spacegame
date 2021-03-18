using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGame
{
    public class Projectile : MonoBehaviour
    {
        public float speed;
        public float damage;

        [HideInInspector]
        public Vector2 direction = Vector2.up;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out Tile tile))
            {
                tile.ReceiveDamage(damage);
            }
            //if(other.TryGetComponent)
        }
    }
}