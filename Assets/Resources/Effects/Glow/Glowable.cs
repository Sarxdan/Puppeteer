using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        enabled = true;
    }

    void Update()
    {
        currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * LerpFactor);
    }
}
