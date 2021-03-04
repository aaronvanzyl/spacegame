using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGame
{
    public class Controller : MonoBehaviour
    {
        public CameraController camController;
        public ShipEditor editor;
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
        }

        void Update()
        {
            Vector2 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (!editor.isActiveAndEnabled)
            {
                if (Input.GetButtonDown("Fire1"))
                {
                    Collider2D[] overlap = Physics2D.OverlapPointAll(cursorPos);
                    foreach (Collider2D col in overlap)
                    {
                        if (col.TryGetComponent(out Tile tile))
                        {
                            SelectShip(tile.ship);
                        }
                    }
                }
                //moveDirection = Vector2.zero;
                if (Input.GetButton("Fire2"))
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        rotateMarker.transform.position = cursorPos;
                        selectedShip.photonView.RPC("SetRotateTarget", selectedShip.photonView.Owner, cursorPos);
                    }
                    else
                    {
                        rotateMarker.transform.position = cursorPos;
                        selectedShip.photonView.RPC("SetRotateTarget", selectedShip.photonView.Owner, cursorPos);
                        moveMarker.transform.position = cursorPos;
                        selectedShip.photonView.RPC("SetMoveTarget", selectedShip.photonView.Owner, cursorPos);
                    }
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

        public void SelectShip(Ship ship)
        {
            selectedShip = ship;
            editor.selectedShip = ship;
            camController.SetFollowTarget(ship.transform);
            moveMarker.transform.position = ship.GetMoveTarget();
            rotateMarker.transform.position = ship.GetRotateTarget();
        }
    }
}
