using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * AUTHOR:
 * Ludvig Björk Förare
 * 
 * DESCRIPTION:
 * Makes camera shake
 * 
 * CODE REVIEWED BY:
 * 
 */
public class CameraShake : MonoBehaviour
{
    [Range(0,1)]
    public float SmoothFactor;
    public float Strength;
    public float Decay;

    private Vector3 originalPosition;
    void Start()
    {
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        Vector3 newPosition = originalPosition + (Vector3.right * Mathf.Sin(Random.Range(0,6.28f)) + Vector3.up * Mathf.Sin(Random.Range(0,6.28f))) * Strength;
        transform.localPosition = Vector3.Lerp(newPosition, transform.position, SmoothFactor);
        Strength = Mathf.Max(Strength - Decay * Time.deltaTime, 0);
    }

}
