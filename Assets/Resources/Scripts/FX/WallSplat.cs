using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Ludvig Björk Förare
 * 
 * 
 * DESCRIPTION:
 * Handles wall splat object
 * 
 * CODE REVIEWED BY:
 * 
 * 
 * CLEANED
*/
public class WallSplat : MonoBehaviour
{
    public float LifeTime;
    public float SmokeLifeTime;
    private float decayProgress;

    public Renderer BackgroundSmokeRenderer;
    
    void Start()
    {
        decayProgress = SmokeLifeTime;
    }
    
    void Update()
    {
        LifeTime = Mathf.Max(0, LifeTime - Time.deltaTime);
        decayProgress = Mathf.Max(0, decayProgress - Time.deltaTime);
        BackgroundSmokeRenderer.material.SetFloat("_Lifetime", decayProgress / SmokeLifeTime);

        if(LifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }
}
