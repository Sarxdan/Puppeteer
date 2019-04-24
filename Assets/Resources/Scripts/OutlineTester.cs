using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineTester : MonoBehaviour
{
    private Material outlineMat;

    void Start()
    {
        var materials = GetComponent<Renderer>().materials;
        foreach (var mat in materials)
        {
            if (mat.FindPass("Outline") != -1)
            {
                outlineMat = mat;
            }
        }

        if (outlineMat == null)
        {
            Debug.LogError("Interactable objects requires an outline material");
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.X))
        {
            outlineMat.SetFloat("_EnableOutline", 1);
        }

        if (Input.GetKeyUp(KeyCode.X))
        {
            outlineMat.SetFloat("_EnableOutline", 0);
        }
    }
}
