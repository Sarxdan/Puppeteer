using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class GlowController : MonoBehaviour
{
    private static List<Glowable> targets = new List<Glowable>();

    private CommandBuffer buffer;
    private Material glowMat;
    private Material blurMat;
    private Vector2 blurTexelSize;

    private int prePassID;
    private int blurPassID;
    private int tempPassID;
    private int blurSizeID;
    private int glowColorID;

    void Awake()
    {
        glowMat = new Material(Shader.Find("Hidden/GlowCommand"));
        blurMat = new Material(Shader.Find("Hidden/Blur"));

        prePassID = Shader.PropertyToID("_GlowPrePassTex");
        blurPassID = Shader.PropertyToID("_GlowBlurPassTex");
        tempPassID = Shader.PropertyToID("_TempTex0");
        blurSizeID = Shader.PropertyToID("_BlurSize");
        glowColorID = Shader.PropertyToID("_GlowColor");

        buffer = new CommandBuffer();
        buffer.name = "Glowing Objects";
        GetComponent<Camera>().AddCommandBuffer(CameraEvent.BeforeImageEffects, buffer);
    }

    void Update()
    {
        buffer.Clear();

        buffer.GetTemporaryRT(prePassID, Screen.width, Screen.height, 1, FilterMode.Bilinear);
        buffer.SetRenderTarget(prePassID);
        buffer.ClearRenderTarget(true, true, Color.clear);

        for (int i = 0; i < targets.Count; i++)
        {
            if(targets[i] == null)
            {
                targets.RemoveAt(i);
                continue;
            }

            buffer.SetGlobalColor(glowColorID, targets[i].CurrentColor);

            for (int j = 0; j < targets[i].Renderers.Length; j++)
            {
                buffer.DrawRenderer(targets[i].Renderers[j], glowMat);
            }
        }

        buffer.GetTemporaryRT(blurPassID, Screen.width >> 1, Screen.height >> 1, 0, FilterMode.Bilinear);
        buffer.GetTemporaryRT(tempPassID, Screen.width >> 1, Screen.height >> 1, 0, FilterMode.Bilinear);
        buffer.Blit(prePassID, blurPassID);

        blurTexelSize = new Vector2(1.0f / (Screen.width >> 1), 1.0f / (Screen.height >> 1));
        buffer.SetGlobalVector(blurSizeID, blurTexelSize);

        for (int i = 0; i < 4; i++)
        {
            buffer.Blit(blurPassID, tempPassID, blurMat, 0);
            buffer.Blit(tempPassID, blurPassID, blurMat, 1);
        }
    }

    // register a glow payload
    public static void Register(in Glowable obj)
    {
        if (!targets.Contains(obj))
        {
            targets.Add(obj);
        }
    }
}
