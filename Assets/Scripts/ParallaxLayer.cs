using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGame
{
    public class ParallaxLayer : MonoBehaviour
    {
        public float depth;

        void Update()
        {
            if (Camera.main == null)
            {
                return;
            }
            Vector2 cameraPos = Camera.main.transform.position;
            transform.position = cameraPos - (cameraPos) / depth;
        }
    }
}
