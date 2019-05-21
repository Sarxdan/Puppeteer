using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
 * AUTHOR:
 * Philip Stenmark, Filip Renman
 * 
 * DESCRIPTION:
 * When activated, the owner will become invisible for all observers except other players.
 * 
 * CODE REVIEWED BY:
 * Anton Jonsson (07/05-2019)
 *
 * CONTRIBUTORS:
 * Ludvig "Kät" Björk Förare (Added shader animation)
 */
public class InvisibilityPower : PowerupBase
{
    [Tooltip("The container that contains all the meshes for the model")]
    public GameObject MeshContainer;
    public GameObject FPVMeshContainer;
    public float TransitionSpeed;
    public bool IsActive = false;

    [SyncVar(hook = nameof(UpdateMaterial))]
    public float StealthValue;

    private Coroutine fadeRoutineInstance;

    //Set layer so the gameobject is invisible for the puppeteer and set the gameobject tag to something that is not player
    //so minions stop attacking you!
    
    
    
    public override void OnActivate()
    {
        CmdSetIsActive(true);
        CmdSetLayers(1);
        CmdStartFade(1);
        CmdSetTag("Untagged");
    }

    //Revert everything we did OnActivate()
    public override void OnComplete()
    {
        CmdSetIsActive(false);
        CmdSetLayers(9);
        CmdStartFade(0);
        CmdSetTag("Player");
    }

    [Command]
    public void CmdSetLayers(int layer)
    {
        setLayers(layer);
        RpcSetLayers(layer);
    }

    [Command]
    public void CmdStartFade(float target)
    {
        if(fadeRoutineInstance != null) StopCoroutine(fadeRoutineInstance);
        fadeRoutineInstance = StartCoroutine("fadeTo", target);
    }

    [Command]
    public void CmdSetTag(string tag)
    {
        setTag(tag);
        RpcSetTag(tag);
    }

    [Command]
    public void CmdSetIsActive(bool value)
    {
        setIsActive(value);
    }


    [ClientRpc]
    public void RpcSetLayers(int layer)
    {
        setLayers(layer);
    }


    [ClientRpc]
    public void RpcSetTag(string tag)
    {
        setTag(tag);
    }

    private IEnumerator fadeTo(float target)
    {
        float direction = (target - StealthValue)/Mathf.Abs(target - StealthValue);
        while(StealthValue != target)
        {
            Debug.Log("Value: " + StealthValue + " Target: " + target + "Direction: " + direction);
            StealthValue = Mathf.Clamp(StealthValue + TransitionSpeed * Time.deltaTime * direction, 0, 1);
            yield return new WaitForEndOfFrame();
        }
        fadeRoutineInstance = null;
    }

    private void UpdateMaterial(float newValue)
    {
        Shader.SetGlobalFloat("_Gekko_Animate", newValue);
    }

    private void setLayers(int layer)
    {
        gameObject.layer = layer;
        foreach (Transform item in MeshContainer.transform)
        {
            item.gameObject.layer = layer;
        }

        foreach(Transform item in FPVMeshContainer.transform)
        {
            item.gameObject.layer = layer;
        }
    }

    private void setTag(string tag)
    {
        gameObject.tag = tag;
    }

    private void setIsActive(bool value)
    {
        IsActive = value;
    }
}
