﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *CLEANED
 */

public class PSSelfDestruct : MonoBehaviour
{
    private ParticleSystem particleSystem;
    // Start is called before the first frame update
    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
        if(particleSystem == null) Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if(!particleSystem.IsAlive())
        {
            Destroy(gameObject);
        }
    }
}
