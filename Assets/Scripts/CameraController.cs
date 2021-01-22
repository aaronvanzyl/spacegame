using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGame
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        Camera cam;
        private void Awake()
        {
            cam = GetComponent<Camera>();
        }
        void Update()
        {
            Vector3 playerPos = GameManager.Instance.localPlayer.transform.position;
            transform.position = new Vector3(playerPos.x, playerPos.y, transform.position.z);
            cam.orthographicSize -= Input.mouseScrollDelta.y;
            Vector3 eulerAngles = transform.eulerAngles;
            eulerAngles.z = Mathf.Abs(GameManager.Instance.localPlayer.transform.eulerAngles.z);
            transform.eulerAngles = eulerAngles;

        }
    }
}