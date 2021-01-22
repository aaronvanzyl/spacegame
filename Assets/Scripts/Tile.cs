using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceGame
{
    public class Tile : MonoBehaviourPunCallbacks, IPunObservable, IPunInstantiateMagicCallback
    {
        [HideInInspector]
        public Ship ship;
        [HideInInspector]
        public Vector2Int pos;
        [HideInInspector]
        public bool canOccupy = false;
        [HideInInspector]
        public bool isOccupied;

        public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                if (canOccupy)
                {
                    stream.SendNext(isOccupied);
                }
            }
            else
            {
                if (canOccupy)
                {
                    isOccupied = (bool)stream.ReceiveNext();
                }
            }
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            object[] data = info.photonView.InstantiationData;
            pos.x = (int)data[0];
            pos.y = (int)data[1];
            int viewId = (int)data[2];

            ship = PhotonView.Find(viewId).GetComponent<Ship>();
            transform.SetParent(ship.transform);
            transform.localPosition = (Vector2)pos;
            transform.up = ship.transform.up;

            ship.OnTileAdded(this);
        }


        public void OnDestroy()
        {
            if (ship != null)
            {
                ship.OnTileDestroyed(this);
            }
        }
    }
}