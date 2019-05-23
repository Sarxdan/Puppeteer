using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * AUTHOR:
 * Ludvig "Kät" Björk Förare
 * 
 * DESCRIPTION:
 * Script for handling muzzle flares
 * 
 * CODE REVIEWED BY:
 * 
 * 
 * CLEANED
 */

public class MuzzleFlash : MonoBehaviour
{
    public float FlashSpeed;
    public float FlashStartValue;
    private ParticleSystem smoke;
    private ParticleSystem sparks;
    private Material cone;
    private float progress;
    void Start()
    {
        smoke = transform.Find("Smoke").GetComponent<ParticleSystem>();
        sparks = transform.Find("Sparks_Bubbles").GetComponent<ParticleSystem>();
        cone = transform.Find("Flash").Find("pCylinder").GetComponent<Renderer>().material;
        progress = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if(progress == 1) return;
        progress = Mathf.Min(1, progress + FlashSpeed * Time.deltaTime);
        cone.SetFloat("_Progress", progress);
    }

    public void Fire()
    {
        progress = FlashStartValue;
        smoke.Play();
        sparks.Play();
    }
}
