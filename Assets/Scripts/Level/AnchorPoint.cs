using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorPoint : MonoBehaviour
{
    // checks if the anchor point is open for anchoring
    public bool Open = true;

    void Update()
    {
        var transform = gameObject.transform;
        if(Open)
        {
            Debug.DrawLine(transform.position, transform.position + transform.up, Color.yellow);
        }
        else
        {
            Debug.DrawLine(transform.position, transform.position + transform.up, Color.red);
        }
    }
}
