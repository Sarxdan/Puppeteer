using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
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
	[SerializeField]
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
				transform.GetChild(0).gameObject.SetActive(false); // NEEDS NETWORK
				transform.GetChild(1).gameObject.SetActive(true);

				transform.localRotation = Quaternion.Euler(transform.localEulerAngles.x, defaultAngle, transform.localEulerAngles.z);
			}
			else
			{
				transform.GetChild(0).gameObject.SetActive(true);
				transform.GetChild(1).gameObject.SetActive(false);
			}
		}
    }

    
    public float RotationSpeed;
    // How much the door should open
    public float OpenAngle = 90;
	public float defaultAngle = 0;
	// Vector used for offsetting door position into doorframe correctly.
	public Vector3 adjustmentVector = new Vector3(0.6f, 0, 0);

	private float currentAngle = 0;

	[SerializeField]
	private bool open = false;

    // Save the start angle of the door
    void Start()
    {
		currentAngle = defaultAngle;
	}
    // When the used key is pressed the direction the door should open is calculated
    public override void OnInteractBegin(GameObject interactor)
    {
		if (!locked)
		{
			float dotProduct = Vector3.Dot(transform.forward, interactor.transform.forward);
			currentAngle = OpenAngle * Mathf.Sign(dotProduct);
			open = !open;
		}
	}
    public override void OnInteractEnd(GameObject interactor){}
    // Closes and opens the door
    void FixedUpdate()
    {	
        if(!locked)
        {
            if(open)
            {
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(transform.localEulerAngles.x , defaultAngle + currentAngle, transform.localEulerAngles.z), RotationSpeed);
            }
            else
            {
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(transform.localEulerAngles.x , defaultAngle, transform.localEulerAngles.z), RotationSpeed);
            }
        }
    }
}
