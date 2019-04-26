using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

/*
 * AUTHOR:
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * Manages a command buffer to perform separate render passes that allows glowing outlines to be shown.
 * Three different buffers are used to create the effect. 
 * The process is structured as following:
 * 1. Render only glowing objects in their solid glow color (pre-pass).
 * 2. Process the pre-pass using pre-calculated gaussian blur (blur-pass).
 * 3. Subtract the blur-pass result with the pre-pass result the get the outline.
 * 4. Apply final result to scene default framebuffer.
 * 
 * CODE REVIEWED BY:
 * 
 * 
 */
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(GlowComposite))]
public class GlowController : MonoBehaviour
{
    private static List<Glowable> targets = new List<Glowable>();

    private CommandBuffer buffer;
    private Material glowMat;
    private Material blurMat;
    private Vector2 blurTexelSize;
    private float blurScale = 1.3f;

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
            if (targets[i] == null)
            {
                // remove invalid targets
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

        blurTexelSize = new Vector2(blurScale / (Screen.width >> 1), blurScale / (Screen.height >> 1));
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
