using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorPoint : MonoBehaviour
{
    void Update()
    {
        Debug.DrawLine(transform.position, transform.position + transform.up, Color.yellow);
    }
}
