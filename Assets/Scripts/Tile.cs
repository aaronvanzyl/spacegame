using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceGame
{
    public class Tile : MonoBehaviourPunCallbacks
    {
        public int tileType;
        public bool canRotate;
        [HideInInspector]
        public Ship ship;
        [HideInInspector]
        public Vector2Int pos;
        public bool awaitingSync;

        protected void MarkForSync() {
            if (!awaitingSync) {
                awaitingSync = true;
                ship.tileSyncList.Add(this);
            }
        }

        public virtual void Serialize(PhotonStream stream)
        {
        }

        public virtual void Deserialize(PhotonStream stream) { 
        
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