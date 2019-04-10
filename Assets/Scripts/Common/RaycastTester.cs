using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastTester : MonoBehaviour
{
    [Range(1.0f, 10.0f)]
    public float Lookahead = 4.0f;

    private Interactable last;
    private RaycastHit hitInfo;

    void Update()
    {
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

        if(Input.GetKeyDown(KeyCode.E) && last != null)
        {
            last.OnInteract(gameObject);
        }
    }

    private void SendMessageTo(Interactable target, string methodName)
    {
        if(target != null)
        {
            target.SendMessage(methodName);
        }
    }
}
