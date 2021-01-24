using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGame
{
    public class Thruster : Tile
    {
        public float activation;
        public float force;
        public GameObject fireEffect;

        private void Update()
        {
            Vector3 fireScale = fireEffect.transform.localScale;
            fireScale.y = activation;
            fireEffect.transform.localScale = new Vector3(1,activation,1);
            fireEffect.transform.localPosition = new Vector3(0, -0.5f - activation * 0.25f, 0);
        }

        public void ApplyForce()
        {
            ship.rb2d.AddForceAtPosition(transform.up * force * activation * Time.fixedDeltaTime, transform.position, ForceMode2D.Impulse);
        }

        public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            base.OnPhotonSerializeView(stream, info);
            if (stream.IsWriting)
            {
                stream.SendNext(activation);
            }
            else
            {
                activation = (float)stream.ReceiveNext();
            }
        }
    }
}