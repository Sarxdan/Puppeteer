using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * AUTHOR:
 * Ludvig Björk Förare
 * 
 * DESCRIPTION:
 * Rotates the 
 * 
 * CODE REVIEWED BY:
 * Benjamin Vesterlund
 * 
 * 
 * CONTRIBUTORS:
 * Kristoffer Lundgren
 * 
 * CLEANED
 */
public class GatlingGunAnimator : MonoBehaviour
{
    public Transform Barrel;
    public Transform Gear;

    public float TeethRatio;
    public float MaxSpeed;
    public float CurrentSpeed;

    void Update()
    {
        if(CurrentSpeed < 0.1f) return;
        Barrel.Rotate(Vector3.forward, MaxSpeed * CurrentSpeed * Time.deltaTime);
        Gear.Rotate(Vector3.right, MaxSpeed * TeethRatio * CurrentSpeed * Time.deltaTime);
    }
}
