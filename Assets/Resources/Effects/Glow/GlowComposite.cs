using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * Applies the final composite built by the GlowController to the scene.
 * 
 * CODE REVIEWED BY:
 * 
 */
public class GlowComposite : MonoBehaviour
{
    [Range(0, 10)]
    public float Intensity = 2.0f;
    private Material composite;

    void OnEnable()
    {
        composite = new Material(Shader.Find("Hidden/GlowComposite"));
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        composite.SetFloat("_Intensity", this.Intensity);
        Graphics.Blit(source, destination, composite);
    }
}
