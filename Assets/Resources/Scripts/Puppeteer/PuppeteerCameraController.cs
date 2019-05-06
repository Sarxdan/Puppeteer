using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* AUTHOR:
* Filip Renman
*
* DESCRIPTION:
* Moves the camera when the mouse is at the edge of the screen or by using the movement keys
*
* CODE REVIEWED BY:
* Anton Jonsson (30/04-2019)
*
* CONTRIBUTORS:
*
 * Sandra "Sanders" Andersson (MouseMovement is opt.)
*/

public class PuppeteerCameraController : MonoBehaviour
{
    //Movement with Mouse is optional
    public bool MouseMovement;

    //The speed the camera moves in
    public float CameraMovementSpeed;

    //How far to the side you need to move the mouse before the camera starts moving
    public float SideBorderThickness;

    //The playable area. (Lenght of one side of a rectangle)
    public float PlayerArea;

    //The multiplier of how fast the camera is going to zoom
    public float CameraZoomSpeed;

    //Camera zoom limits
    public float FarCameraZoomLimit;
    public float NearCameraZoomLimit;

    //Everything inside these positions are the playable area
    private float RightHorizontalBorder;
    private float LeftHorizontalBorder;
    private float TopVerticalBorder;
    private float BottomVerticalBorder;

	public bool DisableInput;

    void Start()
    {
        //How far from the start position you are able to go before you can move the camera anymore
        float lenghtFromCenter = PlayerArea / 2;

        Vector3 pos = transform.position;
        RightHorizontalBorder = pos.x + lenghtFromCenter;
        LeftHorizontalBorder = pos.x - lenghtFromCenter;
        TopVerticalBorder = pos.z + lenghtFromCenter;
        BottomVerticalBorder = pos.z - lenghtFromCenter;
    }

	void Update()
	{
		if (!DisableInput)
		{
			Vector3 pos = transform.position;

			if ((Input.GetAxis("Vertical") > 0 && Input.GetButton("Vertical") || Input.mousePosition.y >= Screen.height - SideBorderThickness && MouseMovement) && pos.z < TopVerticalBorder)
			{
				pos.z += CameraMovementSpeed * Time.deltaTime;
			}
			if ((Input.GetAxis("Vertical") < 0 && Input.GetButton("Vertical") || Input.mousePosition.y <= SideBorderThickness && MouseMovement) && pos.z > BottomVerticalBorder)
			{
				pos.z -= CameraMovementSpeed * Time.deltaTime;
			}
			if ((Input.GetAxis("Horizontal") > 0 && Input.GetButton("Horizontal") || Input.mousePosition.x >= Screen.width - SideBorderThickness && MouseMovement) && pos.x < RightHorizontalBorder)
			{
				pos.x += CameraMovementSpeed * Time.deltaTime;
			}
			if ((Input.GetAxis("Horizontal") < 0 && Input.GetButton("Horizontal") || Input.mousePosition.x <= SideBorderThickness && MouseMovement) && pos.x > LeftHorizontalBorder)
			{
				pos.x -= CameraMovementSpeed * Time.deltaTime;
			}

			transform.position = pos;
		}

        float deltaScrollWheel = Input.mouseScrollDelta.y;
        if (deltaScrollWheel != 0)
        {
            Vector3 newCameraPosition = new Vector3(transform.position.x, transform.position.y - (deltaScrollWheel * CameraZoomSpeed), transform.position.z);
            if (newCameraPosition.y >= NearCameraZoomLimit && newCameraPosition.y <= FarCameraZoomLimit)
            {
            transform.position = newCameraPosition;
            }
        }
	}
}
