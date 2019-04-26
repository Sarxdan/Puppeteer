using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * Enables an object to display a glowing outline when toggled.
 * All glowable objects are managed by the centralized GlowController.
 * 
 * CODE REVIEWED BY:
 * 
 */
public class Glowable : MonoBehaviour
{
    public Color GlowColor = Color.white;
    public float LerpFactor = 10;

    // store all meshes of this object
    public Renderer[] Renderers
    {
        get;
        private set;
    }

    // the current glow color
    public Color CurrentColor
    {
        get { return currentColor; }
    }

    private Color currentColor;
    private Color targetColor;

    void Start()
    {
        Renderers = GetComponentsInChildren<Renderer>();
        GlowController.Register(this);
    }

    // toggle the glow effect
    public void Toggle(bool state)
    {
        targetColor = state ? GlowColor : Color.black;
    }

    void Update()
    {
        currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * LerpFactor);
    }
}
