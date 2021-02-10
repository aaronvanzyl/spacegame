using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGame
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        public float minSize;
        public float maxSize;

        Camera cam;
        Vector2 offset;
        bool mouseDownLastFrame;
        Vector2 mousePosLastFrame;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }
        void Update()
        {
            bool mouseDown = Input.GetMouseButton(2);
            Vector2 mousePos = Input.mousePosition;
            if (mouseDown && mouseDownLastFrame) {
                offset -= (Vector2)cam.ScreenToWorldPoint(mousePos) - (Vector2)cam.ScreenToWorldPoint(mousePosLastFrame);
            }

            Vector3 playerPos = GameManager.Instance.localShip.transform.position;
            transform.position = new Vector3(playerPos.x, playerPos.y, transform.position.z) + (Vector3)offset;
            cam.orthographicSize -= Input.mouseScrollDelta.y;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minSize, maxSize);
            //Vector3 eulerAngles = transform.eulerAngles;
            //eulerAngles.z = Mathf.Abs(GameManager.Instance.localPlayer.transform.eulerAngles.z);
            //transform.eulerAngles = eulerAngles;
            mouseDownLastFrame = mouseDown;
            mousePosLastFrame = mousePos;
        }
    }
}