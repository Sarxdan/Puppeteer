using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
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
