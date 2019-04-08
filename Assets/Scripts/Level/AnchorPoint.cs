using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorPoint : MonoBehaviour
{
    // the anchor that is attach to this point
    public AnchorPoint Attachment { get; set; }

    void Start()
    {
        Attachment = null;   
    }

    void Update()
    {
        if(!Attachment)
        {
            Debug.DrawLine(transform.position, transform.position + transform.up, Color.yellow);
        }
        else
        {
            Debug.DrawLine(transform.position, transform.position + transform.up, Color.red);
        }
    }
}
