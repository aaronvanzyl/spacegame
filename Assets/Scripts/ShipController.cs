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

        void Awake()
        {
            canOccupy = true;
        }

        void Update()
        {
            if (isOccupied && photonView.IsMine)
            {
                if (!ship.photonView.IsMine) {
                    ship.photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
                }
                moveDirection = Vector2.zero;
                if (Input.GetButton("Fire2"))
                {
                    Vector2 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    moveDirection = (cursorPos - (Vector2)GameManager.Instance.localPlayer.transform.position).normalized;
                }
                else
                {
                    //moveDirection += Input.GetAxis("Horizontal") * Vector2.right;// * (Vector2)GameManager.Instance.localPlayer.transform.right;
                    //moveDirection += Input.GetAxis("Vertical") * Vector2.up;// * (Vector2)GameManager.Instance.localPlayer.transform.up;
                    moveDirection += Input.GetAxis("Horizontal") * (Vector2)ship.transform.right;
                    moveDirection += Input.GetAxis("Vertical") * (Vector2)ship.transform.up;
                }

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
            //info.
            base.OnPhotonSerializeView(stream, info);
            if (stream.IsWriting)
            {
                stream.SendNext(moveDirection);
                stream.SendNext(rotation);
            }
            else
            {
                moveDirection = (Vector2)stream.ReceiveNext();
                rotation = (float)stream.ReceiveNext();
            }
        }

    }
}
