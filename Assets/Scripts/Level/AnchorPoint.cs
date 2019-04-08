using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorPoint : MonoBehaviour
{
    // the anchor that is attach to this point
    public AnchorPoint Attachment { get; set; }

    void FixedUpdate()
    {
        Debug.DrawLine(transform.position, transform.position + transform.up, Attachment == null ? Color.yellow : Color.red);
    }
}
