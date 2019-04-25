using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GlowController : MonoBehaviour
{
    private static GlowController instance;
    private CommandBuffer buffer;

    // registered glowable objects
    private List<Glowable> glowables = new List<Glowable>();
    
    private Material glowMaterial;
    private Material blurMaterial;
    private Vector3 blurTexelSize;

    // identifiers
    private int prePassID;
    private int blurPassID;
    private int tempPassID;
    private int blurSizeID;
    private int glowColorID;

    void Awake()
    {
        instance = this;

        glowMaterial = new Material(Shader.Find("Hidden/GlowCommand"));
        blurMaterial = new Material(Shader.Find("Hidden/Blur"));

        prePassID = Shader.PropertyToID("_GlowPressPassTex");
        blurPassID = Shader.PropertyToID("_GlowBlurPassTex");
        tempPassID = Shader.PropertyToID("_TempTex0");
        blurSizeID = Shader.PropertyToID("_BlurSize");
        glowColorID = Shader.PropertyToID("_GlowColor");

        buffer = new CommandBuffer();
        GetComponent<Camera>().AddCommandBuffer(CameraEvent.BeforeImageEffects, buffer);
    }

    // add all commands to the buffer
    void Update()
    {
        buffer.Clear();

        // perform pre-pass
        buffer.GetTemporaryRT(prePassID, Screen.width, Screen.height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, QualitySettings.antiAliasing);
        buffer.SetRenderTarget(prePassID);
        buffer.ClearRenderTarget(true, true, Color.clear);

        for (int i = 0; i < glowables.Count; i++)
        { 
            buffer.SetGlobalColor(glowColorID, glowables[i].CurrentColor);

            for(int j = 0; j < glowables[i].Renderers.Length; j++)
            {
                buffer.DrawRenderer(glowables[i].Renderers[j], glowMaterial);
            }
        }

        buffer.GetTemporaryRT(blurPassID, Screen.width >> 1, Screen.height >> 1, 0, FilterMode.Bilinear);
        buffer.GetTemporaryRT(tempPassID, Screen.width >> 1, Screen.height >> 1, 0, FilterMode.Bilinear);
        buffer.Blit(prePassID, blurPassID);

        blurTexelSize = new Vector2(1.5f / (Screen.width >> 1), 1.5f / (Screen.height >> 1));
        buffer.SetGlobalVector(blurSizeID, blurTexelSize);
    
        for(int i = 0; i < 4; i++)
        {
            buffer.Blit(blurPassID, tempPassID, blurMaterial, 0);
            buffer.Blit(tempPassID, blurPassID, blurMaterial, 1);
        }
    }

    public static void Register(in Glowable obj)
    {
        if(instance != null)
        {
            instance.glowables.Add(obj);
        }
    }
}
