using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGame
{
    public class Controller : MonoBehaviour
    {

        public GameObject moveMarker;
        public GameObject rotateMarker;

        [HideInInspector]
        Ship selectedShip;
        [HideInInspector]
        public Vector2 moveDirection = Vector2.zero;
        [HideInInspector]
        public float rotation = 0;

        void Start()
        {
            selectedShip = GameManager.Instance.localShip;
        }

        void Update()
        {
            //moveDirection = Vector2.zero;
            if (Input.GetButton("Fire2"))
            {
                Vector2 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    rotateMarker.transform.position = cursorPos;
                    selectedShip.photonView.RPC("SetRotateTarget", selectedShip.photonView.Owner, cursorPos);
                }
                else {
                    rotateMarker.transform.position = cursorPos;
                    selectedShip.photonView.RPC("SetRotateTarget", selectedShip.photonView.Owner, cursorPos);
                    moveMarker.transform.position = cursorPos;
                    selectedShip.photonView.RPC("SetMoveTarget", selectedShip.photonView.Owner, cursorPos);
                }
            }
            //else
            //{
            //    //moveDirection += Input.GetAxis("Horizontal") * Vector2.right;// * (Vector2)GameManager.Instance.localPlayer.transform.right;
            //    //moveDirection += Input.GetAxis("Vertical") * Vector2.up;// * (Vector2)GameManager.Instance.localPlayer.transform.up;
            //    moveDirection += Input.GetAxis("Horizontal") * (Vector2)ship.transform.right;
            //    moveDirection += Input.GetAxis("Vertical") * (Vector2)ship.transform.up;
            //}

            //rotation = Input.GetAxis("Rotate");
        }

    }
}
