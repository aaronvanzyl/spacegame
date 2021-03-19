using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGame
{
    [RequireComponent(typeof(Tile))]
    public class Turret : MonoBehaviour, IPunObservable
    {

       
        [HideInInspector]
        public Tile tile;

        public GameObject turretChild;
        public Projectile bulletPrefab;

        public float bulletWidth;
        public float bulletOffset;


        public float fireDelay = 0.1f;
        private float timeSinceFire = 0f;

        private Vector2 rotation;
    
      

        void Awake()
        {
            tile = GetComponent<Tile>();
        }

        private void Update()
        {
            if (!tile.photonView.IsMine) // only the owner can shoot the turret, rest just adjust angles (with photonserialize...)
            {
                return;
            }

            if(tile.ship.GetTurretTarget() != null)
            {
                Vector2 vec2target = tile.ship.GetTurretTarget() - (Vector2)transform.position;
                turretChild.transform.up = vec2target;
                rotation = vec2target;
                if (tile.ship.GetTurretActive())
                {
                    if (timeSinceFire > fireDelay)
                    {
                        Vector2 bulletSpawn = transform.position + (Vector3)vec2target.normalized * bulletOffset;

                        // raycast to target. if first tile hit belongs to us dont fire (if first tile is itself look at second tile owo)
                        RaycastHit2D[] hits = Physics2D.CircleCastAll(bulletSpawn, bulletWidth, vec2target);
                        float minDist = float.MaxValue;
                        Tile closestTile = null;
                        foreach (RaycastHit2D hit in hits)
                        {
                            if (hit.distance < minDist && hit.collider.TryGetComponent(out Tile t) && t != tile) // C# voodoo!
                            {
                                minDist = hit.distance;
                                closestTile = t;
                            }
                        }
                        if (closestTile == null || closestTile.ship != tile.ship)
                        {
                            Projectile p = PhotonNetwork.Instantiate(bulletPrefab.name, bulletSpawn, Quaternion.identity).GetComponent<Projectile>();
                            p.direction = vec2target;
                            p.tileSource = tile;
                            p.baseVelocity = tile.ship.rb2d.velocity;
                            timeSinceFire = 0;
                        }
                    }
                }
            }
            timeSinceFire += Time.deltaTime;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            //sync turret rotation
            if (stream.IsWriting)
            {
                stream.SendNext(rotation);
                //stream.SendNext(turretChild.transform.up);
            }
            else
            {
                rotation = (Vector2)stream.ReceiveNext();
                //turretChild.transform.up = (Vector3)stream.ReceiveNext();
            }

            turretChild.transform.up = (Vector3)rotation;
        }
    }
}