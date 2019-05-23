using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * AUTHOR:
 * Ludvig "Kät" Björk Förare
 * 
 * DESCRIPTION:
 * Script for handling bullet trails
 * 
 * CODE REVIEWED BY:
 *
 * 
 * CLEANED
 */

public class BulletTrail : MonoBehaviour
{
    public float Duration;
    public LineRenderer Line;
    private float runTime;
    private bool firing;

    void Start()
    {
        runTime = Duration;
    }
    void Update()
    {
        runTime = Mathf.Max(0, runTime - Time.deltaTime);
        Line.material.SetFloat("_Transparency", runTime/Duration);

        if(runTime <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void hit(Vector3 point)
    {
        Line.SetPosition(0, transform.InverseTransformPoint(point));
    }
}
