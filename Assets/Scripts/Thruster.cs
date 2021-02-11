using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGame
{
    [RequireComponent(typeof(Tile))]
    public class Thruster : MonoBehaviour, IPunObservable
    {

        public bool NeedSync { get => needSync; set => needSync = value; }
        bool needSync;

        public float Activation
        {
            get
            {
                return activation;
            }
            set
            {
                if (activation != value)
                {
                    activation = value;
                    needSync = true;
                }
            }
        }
        float activation;

        public Tile tile;
        public float force;
        public GameObject fireEffect;

        void Awake()
        {
            tile = GetComponent<Tile>();
        }

        private void Update()
        {
            Vector3 fireScale = fireEffect.transform.localScale;
            fireScale.y = Activation;
            fireEffect.transform.localScale = new Vector3(1, Activation, 1);
            fireEffect.transform.localPosition = new Vector3(0, -0.5f - Activation * 0.25f, 0);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(Activation);
            }
            else
            {
                Activation = (float)stream.ReceiveNext();
            }
        }
    }
}