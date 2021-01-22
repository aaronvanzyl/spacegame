﻿using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGame
{
    public class ShipController : Tile, IPunObservable
    {
        public Vector2 moveDirection = Vector2.zero;

        // Start is called before the first frame update
        void Start()
        {
            //this.photonView.TransferOwnership(PhotonNetwork.MasterClient);
        }

        // Update is called once per frame
        void Update()
        {
            if (!photonView.IsMine)
            {
                return;
            }
            if (isOccupied)
            {
                moveDirection.x = Input.GetAxis("Horizontal");
                moveDirection.y = Input.GetAxis("Vertical");
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