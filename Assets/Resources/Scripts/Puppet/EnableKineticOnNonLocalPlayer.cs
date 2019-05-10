using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
 * AUTHOR:
 * Filip Renman
 * 
 * DESCRIPTION:
 * Makes non local player objects kinematic which prevents them from being moved by others.
 * 
 * CODE REVIEWED BY:
 * 
 * 
 * 
 */

public class EnableKineticOnNonLocalPlayer : NetworkBehaviour
{
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (!isLocalPlayer)
        {
            rb.isKinematic = true;
        }
    }
}
