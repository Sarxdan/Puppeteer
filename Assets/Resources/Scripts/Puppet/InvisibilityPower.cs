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
 */
public class InvisibilityPower : PowerupBase
{
    [Tooltip("The container that contains all the meshes for the model")]
    public GameObject MeshContainer;

    //Set layer so the gameobject is invisible for the puppeteer and set the gameobject tag to something that is not player
    //so minions stop attacking you!
    public override void OnActivate()
    {
        CmdSetLayers(1);
        CmdSetTag("Untagged");
    }

    //Revert everything we did OnActivate()
    public override void OnComplete()
    {
        CmdSetLayers(9);
        CmdSetTag("Player");
    }

    [Command]
    public void CmdSetLayers(int layer)
    {
        setLayers(layer);
        RpcSetLayers(layer);

    }

    [ClientRpc]
    public void RpcSetLayers(int layer)
    {
        setLayers(layer);
    }

    [Command]
    public void CmdSetTag(string tag)
    {
        setTag(tag);
        RpcSetTag(tag);
    }

    [ClientRpc]
    public void RpcSetTag(string tag)
    {
        setTag(tag);
    }

    private void setLayers(int layer)
    {
        gameObject.layer = layer;
        foreach (Transform item in MeshContainer.transform)
        {
            item.gameObject.layer = layer;
        }
    }

    private void setTag(string tag)
    {
        gameObject.tag = tag;
    }
}
