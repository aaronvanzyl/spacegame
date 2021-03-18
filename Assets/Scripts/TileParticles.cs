using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class TileParticles : MonoBehaviour
{
    [HideInInspector]
    public ParticleSystem system;
    
    private void Awake()
    {
        system = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (transform.parent == null && system.particleCount == 0) {
            Destroy(gameObject);
        }
    }

    public void Detach()
    {
        transform.parent = null;
    }
}
