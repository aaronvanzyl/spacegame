using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGame
{
    public class ShipController : Tile
    {
        [HideInInspector]
        public Vector2 moveDirection = Vector2.zero;
        [HideInInspector]
        public float rotation = 0;

        void Awake() {
            canOccupy = true;
        }

        void Update()
        {
            if (isOccupied && photonView.IsMine)
            {
                moveDirection = Vector2.zero;
                moveDirection += Input.GetAxis("Horizontal") * (Vector2)GameManager.Instance.localPlayer.transform.right;
                moveDirection += Input.GetAxis("Vertical") * (Vector2)GameManager.Instance.localPlayer.transform.up;
                rotation = Input.GetAxis("Rotate");
            }
            //if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.01f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.01f)
            //{
            //    direction.x = Input.GetAxis("Horizontal");
            //    direction.y = Input.GetAxis("Vertical");
            //    if (direction.magnitude > 1)
            //    {
            //        direction.Normalize();
            //    }
            //}
            //else
            //{
            //    direction = Vector2.zero;
            //}
        }

        public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            base.OnPhotonSerializeView(stream, info);
            if (stream.IsWriting)
            {
                stream.SendNext(moveDirection);
            }
            else
            {
                moveDirection = (Vector2)stream.ReceiveNext();
            }
        }

    }
}
