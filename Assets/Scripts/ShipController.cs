using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGame
{
    [RequireComponent(typeof(Ship))]
    public class ShipController : MonoBehaviour
    {
        [HideInInspector]
        Ship ship;
        [HideInInspector]
        public Vector2 moveDirection = Vector2.zero;
        [HideInInspector]
        public float rotation = 0;

        private void Awake()
        {
            ship = GetComponent<Ship>();
        }

        void Update()
        {
            if (!ship.photonView.IsMine)
            {
                return;
            }
            moveDirection = Vector2.zero;
            if (Input.GetButton("Fire2"))
            {
                Vector2 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                moveDirection = (cursorPos - (Vector2)GameManager.Instance.localShip.transform.position).normalized;
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

    }
}
