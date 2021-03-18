using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceGame
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Tile : MonoBehaviourPunCallbacks
    {
        public bool canRotate;

        [HideInInspector]
        public int tileType;
        public Ship ship;
        [HideInInspector]
        public Vector2Int pos;
        public float maxHealth;
        public float health;
        public TileParticles damageParticles;

        /// <summary>
        /// This object receives damage equal to value * |force| on collision.
        /// </summary>
        public float forceCollisionDamage;

        /// <summary>
        /// This object receives damage equal to value on collision.
        /// </summary>
        public float flatCollisionDamage;

        public List<GameObject> spawnOnDeath;

        IPunObservable[] syncedComponents;

        private void Awake()
        {
            syncedComponents = GetComponentsInChildren<IPunObservable>();
            health = maxHealth;


        }

        private void Start()
        {
            damageParticles.system.textureSheetAnimation.SetSprite(0, GetComponent<SpriteRenderer>().sprite);
        }

        public bool HasSyncedComponents()
        {
            return syncedComponents.Length > 0;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            foreach (IPunObservable component in syncedComponents)
            {
                if (stream.IsWriting)
                {
                    stream.SendNext(health);
                }
                else
                {
                    float newHealth = (float)stream.ReceiveNext();
                    damageParticles.system.Emit((int)(health - newHealth) * 3);
                    health = newHealth;
                }
                component.OnPhotonSerializeView(stream, info);
            }
        }

        public void NetworkDestroy(bool silent)
        {
            ship.DestroyTileNetwork(pos, silent);
        }

        public void ReceiveDamage(float damage)
        {
            if (!ship.photonView.IsMine) {
                return;
            }
            if (health > 0)
            {

                damageParticles.system.Emit((int)Mathf.Min(damage, health) * 3);
                health -= damage;
                if (health <= 0)
                {
                    NetworkDestroy(false);
                }
            }
        }

        public void Die(bool silent)
        {
            if (!silent)
            {
                damageParticles.system.Emit((int)health * 3);
                damageParticles.Detach();
                foreach (GameObject g in spawnOnDeath)
                {
                    Instantiate(g, transform.position, Quaternion.identity);
                }
            }
            Destroy(gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!ship.photonView.IsMine)
            {
                return;
            }
            if (collision.collider.TryGetComponent(out Tile other))
            {
                if (other.ship != ship)
                {
                    float damage = flatCollisionDamage + forceCollisionDamage * collision.relativeVelocity.magnitude;
                    ReceiveDamage(damage);
                }
            }

        }

        //public void OnPhotonInstantiate(PhotonMessageInfo info)
        //{
        //    object[] data = info.photonView.InstantiationData;
        //    pos.x = (int)data[0];
        //    pos.y = (int)data[1];
        //    int viewId = (int)data[2];
        //    float rotation = (float)data[3];

        //    ship = PhotonView.Find(viewId).GetComponent<Ship>();
        //    transform.SetParent(ship.transform);
        //    transform.localPosition = (Vector2)pos;
        //    transform.up = ship.transform.up;
        //    transform.localEulerAngles = new Vector3(0, 0, rotation);

        //    ship.OnTileAdded(this);
        //}

    }
}