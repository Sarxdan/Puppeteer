using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
 */
public class InteractionController : NetworkBehaviour
{
    public float Lookahead = 4.0f;      //Length for raycast

    private Interactable curInteractable;       //Gameobject that the raycast collides with
    private bool isInteracting = false;     //Is the player interacting with something?

    void Update()
    {
        RaycastHit hitInfo;
        //If raycast hits an object
        if(Physics.Raycast(Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f)), out hitInfo, Lookahead, ~(1 << LayerMask.NameToLayer("Puppeteer Interact"))))
        {
            var hit = hitInfo.transform.GetComponent<Interactable>();
            //Sets current interactable object
            if(hit != curInteractable)
            {
                if(curInteractable != null && isInteracting)
                {
                    CmdStopInteracting(new InteractStruct(gameObject, curInteractable.gameObject));
                    isInteracting = false;
                    curInteractable.OnRaycastExit();
                    curInteractable = null;
                }

                curInteractable = hit;
                if(curInteractable != null)
                {
                    curInteractable.OnRaycastEnter();
                }
            }
        }
        //If raycast hits nothing 
        else
        {
           if(curInteractable != null && isInteracting)
            {
                CmdStopInteracting(new InteractStruct(gameObject, curInteractable.gameObject));
                isInteracting = false;
                curInteractable.OnRaycastExit();
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
                //curInteractable.OnInteractEnd(gameObject);
                isInteracting = false;
                curInteractable.OnRaycastExit();
                curInteractable = null;
            }
        }
    }
    [Command]
    private void CmdStopInteracting(InteractStruct info)
    {
        info.Target.GetComponent<Interactable>().OnInteractEnd(info.Source);
    }

    private void StopInteraction()
    {
        //If there isn't any interaction to cancel
       
    }


	[Command]
	public void CmdBeginInteract(InteractStruct info)
	{
		info.Target.GetComponent<Interactable>().OnInteractBegin(info.Source);
	}
}