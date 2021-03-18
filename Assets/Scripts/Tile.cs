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
                    health = (float)stream.ReceiveNext();
                }
                component.OnPhotonSerializeView(stream, info);
            }
        }

        public void NetworkDestroy()
        {
            ship.DestroyTileNetwork(pos);
        }

        public void ReceiveDamage(float damage)
        {
            if (health > 0)
            {
                health -= damage;
                damageParticles.system.Emit((int)damage * 3);
                if (health <= 0)
                {
                    damageParticles.Detach();
                    foreach (GameObject g in spawnOnDeath)
                    {
                        PhotonNetwork.Instantiate(g.name, transform.position, Quaternion.identity);
                    }
                    NetworkDestroy();
                }
            }
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