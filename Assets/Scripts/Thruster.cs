using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGame
{
    public class Thruster : Tile
    {
        public float Activation { 
            get {
                return activation;
            } 
            set {
                if (activation != value)
                {
                    activation = value;
                    MarkForSync();
                }
            } 
        }
        float activation;
        public float force;
        public GameObject fireEffect;

        private void Update()
        {
            Vector3 fireScale = fireEffect.transform.localScale;
            fireScale.y = Activation;
            fireEffect.transform.localScale = new Vector3(1,Activation,1);
            fireEffect.transform.localPosition = new Vector3(0, -0.5f - Activation * 0.25f, 0);
        }

        public override void Serialize(PhotonStream stream)
        {
            base.Serialize(stream);
            stream.SendNext(Activation);
        }

        public override void Deserialize(PhotonStream stream)
        {
            base.Deserialize(stream);
            Activation = (float)stream.ReceiveNext();
        }
    }
}