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

        Transform follow;
        Vector2 comOffset;

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

            if (follow == null)
            {
                transform.position = new Vector3(offset.x, offset.y, transform.position.z);
            }
            else {
                transform.position = new Vector3(follow.position.x, follow.position.y, transform.position.z) + (Vector3)offset + (Vector3) comOffset;
            }
            
            cam.orthographicSize -= Input.mouseScrollDelta.y;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minSize, maxSize);
            //Vector3 eulerAngles = transform.eulerAngles;
            //eulerAngles.z = Mathf.Abs(GameManager.Instance.localPlayer.transform.eulerAngles.z);
            //transform.eulerAngles = eulerAngles;
            mouseDownLastFrame = mouseDown;
            mousePosLastFrame = mousePos;
        }

        public void SetFollowTarget(Transform target) {
            follow = target;
            Rigidbody2D rb2d = follow.GetComponentInParent<Rigidbody2D>();
            if (rb2d != null)
            {
                comOffset = rb2d.centerOfMass;
            }
            else {
                comOffset = Vector2.zero;
            }
            offset = Vector2.zero;
        }
    }
}