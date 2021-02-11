using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceGame
{
    public class Tile : MonoBehaviourPunCallbacks
    {
        public bool canRotate;

        [HideInInspector]
        public int tileType;
        [HideInInspector]
        public Ship ship;
        [HideInInspector]
        public Vector2Int pos;

        IPunObservable[] syncedComponents;

        private void Awake()
        {
            syncedComponents = GetComponentsInChildren<IPunObservable>();
        }

        public bool HasSyncedComponents() {
            return syncedComponents.Length > 0;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            foreach (IPunObservable component in syncedComponents)
            {
                component.OnPhotonSerializeView(stream, info);
                //stream.SendNext(component.NeedSync);
                //if (component.NeedSync)
                //{
                //    component.NeedSync = false;
                    
                //}
            }
            //if (stream.IsWriting)
            //{
                

            //}
            //else
            //{
            //    foreach (ITileSync sync in syncedComponents)
            //    {
            //        if ((bool)stream.ReceiveNext())
            //        {
            //            sync.Deserialize(stream);
            //        }
            //    }
            //}
        //}
        }

        public void NetworkDestroy() {
            ship.DestroyTileNetwork(pos);
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