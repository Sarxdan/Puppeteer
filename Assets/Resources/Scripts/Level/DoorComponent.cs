using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
* AUTHOR:
* Kristoffer Lundgren
*
* DESCRIPTION:
* This class is placed on the parent of a door with a collider attached, the door then be opened with the use key
*
* CODE REVIEWED BY:
* Philip Stenmark
*
* CONTRIBUTORS:
* Anton Jonsson
*/
public class DoorComponent : Interactable
{
    private bool locked;
    public bool Locked
    {
        get
        {
            return locked;
        }
        set
        {
        locked = value;
        if(locked)
        {
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, DefaultAngle, transform.eulerAngles.z);
        }
    }
    }

    public float DefaultAngle = 90;
    public float RotationSpeed;

    // How much the door should open
    private float openAngle = 90;
    private bool open = false;

    // Save the start angle of the door
    void Start()
    {
        DefaultAngle = transform.eulerAngles.y;
    }
    // When the used key is pressed the direction the door should open is calculated
    public override void OnInteractBegin(GameObject interactor)
    {
        if(!locked)
        {
            float dotProduct = Vector3.Dot(transform.forward, interactor.transform.forward);
            openAngle  = -90 * Mathf.Sign(dotProduct);
            Debug.Log(openAngle);
            open = !open;
        }
    }
    public override void OnInteractEnd(GameObject interactor){}
    // Closes and opens the door
    void Update()
    {
        if(!locked)
        {
            if(open)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(transform.eulerAngles.x , DefaultAngle + openAngle, transform.eulerAngles.z), RotationSpeed);

            }
            else
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(transform.eulerAngles.x , DefaultAngle, transform.eulerAngles.z), RotationSpeed);
            }
        }
    }
}
