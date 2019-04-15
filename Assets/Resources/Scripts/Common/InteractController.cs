using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Sandra Andersson
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * Script is placed on the player prefab to allow interaction with interactable objects.
 * 
 * CODE REVIEWED BY:
 * 
 * 
 * 
 */
public class InteractController : MonoBehaviour
{
    public float Lookahead = 4.0f;      //Length for raycast

    private Interactable curInteractable;       //Gameobject that the raycast collides with
    private bool isInteracting = false;     //Is the player interacting with something?

    void Update()
    {
        RaycastHit hitInfo;
        //If raycast hits an object
        if(Physics.Raycast(Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f)), out hitInfo, Lookahead))
        {
            var hit = hitInfo.transform.GetComponent<Interactable>();
            //Sets current interactable object
            if(hit != curInteractable)
            {
                this.StopInteraction();
                curInteractable = hit;
                curInteractable.OnRaycastEnter();
            }
        }
        //If raycast hits nothing
        else
        {
            this.StopInteraction();
        }

        //If the raycast hits something, check for input
        if(curInteractable != null)
        {
            //Attempt to start interaction
            if(Input.GetButtonDown("Use") && !isInteracting)
            {
                curInteractable.OnInteractBegin(gameObject);
                isInteracting = true;
            }

            //Cancel interaction
            if (Input.GetButtonUp("Use") && isInteracting)
            {
                curInteractable.OnInteractEnd(gameObject);
                isInteracting = false;
            }
        }
    }

    private void StopInteraction()
    {
        //If there isn't any interaction to cancel
        if (curInteractable == null)
            return;

        //End interaction
        if(isInteracting)
        {
            curInteractable.OnInteractEnd(gameObject);
            isInteracting = false;
        }

        //Reset interactable
        curInteractable.OnRaycastExit();
        curInteractable = null;
    }
}
