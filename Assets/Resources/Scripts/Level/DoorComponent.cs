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
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, defaultAngle, transform.eulerAngles.z);
        }
    }
    }

    
    public float RotationSpeed;
    // How much the door should open
    public float OpenAngle = 90;

	private float currentAngle = 0;
	private float defaultAngle = 90;
	private bool open = false;

    // Save the start angle of the door
    void Start()
    {
        defaultAngle = transform.eulerAngles.y;
		currentAngle = defaultAngle;
    }
    // When the used key is pressed the direction the door should open is calculated
    public override void OnInteractBegin(GameObject interactor)
    {
        if(!locked)
        {
            float dotProduct = Vector3.Dot(transform.forward, interactor.transform.forward);
            currentAngle  = OpenAngle * Mathf.Sign(dotProduct);
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
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(transform.eulerAngles.x , defaultAngle + currentAngle, transform.eulerAngles.z), RotationSpeed);
            }
            else
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(transform.eulerAngles.x , defaultAngle, transform.eulerAngles.z), RotationSpeed);
            }
        }
    }
}
