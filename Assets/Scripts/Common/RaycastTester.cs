using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * Raycast testing component for player versus in-level objects interactions.
 * The origin of the rays are always from the center of the viewport, sent in
 * direction the camera is facing. Additionally, a lookahead property is used to limit
 * the distance of the casted ray.
 * 
 */
public class RaycastTester : MonoBehaviour
{
    [Range(1.0f, 10.0f)]
    public float Lookahead = 4.0f;

    private Interactable last;
    private RaycastHit hitInfo;

    void Update()
    {
        // performs an interaction if possible
        if (Input.GetKeyDown(KeyCode.E) && last)
        {
            last.OnInteract(gameObject);
        }

        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f)), out hitInfo, Lookahead))
        {
            Interactable next = hitInfo.transform.GetComponent<Interactable>();
            if (next != last)
            {
                SendMessageTo(last, "OnRaycastExit");
                SendMessageTo(next, "OnRaycastEnter");
                last = next;
            }
        }
        else
        {
            SendMessageTo(last, "OnRaycastExit");
            last = null;
        }
    }

    private void SendMessageTo(Interactable target, string methodName)
    {
        if(target)
        {
            target.SendMessage(methodName);
        }
    }
}
