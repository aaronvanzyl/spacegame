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

        ITileSync[] syncedComponents;

        private void Awake()
        {
            syncedComponents = GetComponentsInChildren<ITileSync>();
            foreach (ITileSync component in syncedComponents) {
                component.Parent = this;
            }
        }

        public bool NeedSync() {
            foreach (ITileSync component in syncedComponents) {
                if (component.NeedSync) {
                    return true;
                }
            }

            return false;
        }

        public void Serialize(PhotonStream stream)
        {
            foreach (ITileSync component in syncedComponents) {

                stream.SendNext(component.NeedSync);
                if (component.NeedSync)
                {
                    component.NeedSync = false;
                    component.Serialize(stream);
                }
            }
        }

        public void Deserialize(PhotonStream stream) {
            foreach (ITileSync sync in syncedComponents)
            {
                if ((bool)stream.ReceiveNext())
                {
                    sync.Deserialize(stream);
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