using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

/*
 * AUTHOR:
 * Sandra Andersson
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * Script is placed on the player prefab to allow interaction with interactable objects.
 * 
 * CODE REVIEWED BY:
 * Ludvig Björk Förare
 * 
 * 
 * CONTRIBUTORS:
 * Kristoffer Lundgren (Interact Tooltip)
 */

//CLEANED
public class InteractionController : NetworkBehaviour
{
    public float Lookahead = 4.0f;      //Length for raycast

    private Interactable curInteractable;       //Gameobject that the raycast collides with
    private bool isInteracting = false;     //Is the player interacting with something?

    // The image which is displayed when a player looking at something that can be interacted with
    public RawImage InteractionTooltip;

    void Update()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f)), out hitInfo, Lookahead, ~(1 << 8 | 1 << 2 | 1 << 13)))
        {
            var hit = hitInfo.transform.GetComponent<Interactable>();
            //Sets current interactable object
            if(hit != curInteractable)
            {
                if(curInteractable != null && isInteracting)
                {
                    CmdStopInteracting(new InteractStruct(gameObject, curInteractable.gameObject));
                    isInteracting = false;
                    InteractionTooltip.enabled = false;
                    curInteractable = null;
                }

                    
                curInteractable = hit;
                if(curInteractable != null)
                {
                    curInteractable.OnRaycastEnter(gameObject);
                }
                else
                {
                    InteractionTooltip.enabled = false;
                }

            }
            else if(hit == null)
            {
                InteractionTooltip.enabled = false;
            }
            


        }
        //If raycast hits nothing 
        else
        {
           if(curInteractable != null && isInteracting)
            {
                CmdStopInteracting(new InteractStruct(gameObject, curInteractable.gameObject));
                isInteracting = false;
                InteractionTooltip.enabled = false;
                curInteractable = null;
            }
            else if(curInteractable != null)
            {
                InteractionTooltip.enabled = false;
                curInteractable = null;
            }
        }

        //If the raycast hits something, check for input
        if(curInteractable != null)
        {
            //Attempt to start interaction
            if(Input.GetButtonDown("Use") && !isInteracting)
            {
				CmdBeginInteract(new InteractStruct(gameObject, curInteractable.gameObject));
                isInteracting = true; // TODO: syncvar if works :)
            }

            //Cancel interaction
            if (Input.GetButtonUp("Use") && isInteracting)
            {
                CmdStopInteracting(new InteractStruct(gameObject, curInteractable.gameObject));
                isInteracting = false;
                InteractionTooltip.enabled = false;
                curInteractable = null;
            }
           
        }
    }
    #region Commands
    [Command]
    private void CmdStopInteracting(InteractStruct info)
    {
        info.Target.GetComponent<Interactable>().OnInteractEnd(info.Source);
    }

	[Command]
	public void CmdBeginInteract(InteractStruct info)
	{
		info.Target.GetComponent<Interactable>().OnInteractBegin(info.Source);
	}
    #endregion
}