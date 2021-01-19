using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ShipController : MonoBehaviourPunCallbacks, IPunObservable
{
    public Vector2 direction = Vector2.zero;

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

        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.01f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.01f)
        {
            direction.x = Input.GetAxis("Horizontal");
            direction.y = Input.GetAxis("Vertical");
            if (direction.magnitude > 1)
            {
                direction.Normalize();
            }
        }
        else
        {
            direction = Vector2.zero;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(direction);
        }
        else
        {
            // Network player, receive data
            direction = (Vector2)stream.ReceiveNext();
        }
    }

}

