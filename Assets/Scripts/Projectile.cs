using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGame
{
    [RequireComponent(typeof(Rigidbody2D))]

    public class Projectile : MonoBehaviourPunCallbacks
    {
        public float speed;
        public float damage;
        public Tile tileSource;
        public float maxDuration;

        private Rigidbody2D rb2d;

        [HideInInspector]
        public Vector2 direction = Vector2.up;
        [HideInInspector]
        public Vector2 baseVelocity = Vector2.zero;

        private float elapsedDuration = 0;

        // Start is called before the first frame update
        void Start()
        {
            rb2d = GetComponent<Rigidbody2D>();
            rb2d.velocity = direction.normalized * speed + baseVelocity;
            transform.up = direction;
        }

        // Update is called once per frame
        void Update()
        {
            elapsedDuration += Time.deltaTime;
            if(elapsedDuration > maxDuration)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (photonView.IsMine) // true for owner (just for one client)
            {
                if (other.TryGetComponent(out Tile tile) && tile != tileSource)
                {
                    tile.ReceiveDamage(damage);
                    PhotonNetwork.Destroy(gameObject);
                }

            }
        }
    }
}