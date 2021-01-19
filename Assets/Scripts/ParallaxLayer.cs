using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGame
{
    public class ParallaxLayer : MonoBehaviour
    {
        public float depth;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (GameManager.Instance.localPlayer == null)
            {
                return;
            }
            Vector2 playerPos = GameManager.Instance.localPlayer.transform.position;
            transform.position = playerPos - (playerPos) / depth;
        }
    }
}
