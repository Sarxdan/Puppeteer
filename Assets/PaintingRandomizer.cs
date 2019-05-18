using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * AUTHOR:
 * Ludvig "Kät" Björk Förare
 * 
 * DESCRIPTION:
 * Script to randomize painting images
 * 
 * CODE REVIEWED BY:
 * 
 */
public class PaintingRandomizer : MonoBehaviour
{
    public int NumberOfPictures;
    private Renderer rend;
    void Start()
    {
        rend = transform.Find("Canvas").GetComponent<Renderer>();
        rend.material.SetInt("_Picture_Select", Random.Range(0, NumberOfPictures - 1));
    }
}
