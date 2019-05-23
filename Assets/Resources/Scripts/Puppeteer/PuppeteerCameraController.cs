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
* Philip Stenmark
* Sandra "Sanders" Andersson (MouseMovement is opt.)
*/
//CLEANED

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

    public bool SmoothMovement;

	public bool SpectatorMode = false;

    private float zoomAmount;
	private float mouseSpeedX = 0;
	private float mouseSpeedY = 0;

    void Start()
    {
        //How far from the start position you are able to go before you can move the camera anymore
        float lengthFromCenter = PlayerArea / 2;

        Vector3 pos = transform.position;
        RightHorizontalBorder = pos.x + lengthFromCenter;
        LeftHorizontalBorder = pos.x - lengthFromCenter;
        TopVerticalBorder = pos.z + lengthFromCenter;
        BottomVerticalBorder = pos.z - lengthFromCenter;

        if (PlayerPrefs.GetInt("MouseCameraMovementPuppeteer") == 1)
            MouseMovement = true;
        else
            MouseMovement = false;

        // zoom to height on start
        zoomAmount = 12;
    }

    void Update()
    {
        if (!DisableInput)
        {
			if (SpectatorMode)
			{
				mouseSpeedX += Input.GetAxis("Mouse X");
				transform.RotateAround(transform.position, Vector3.up, 0.1f * mouseSpeedX);

				mouseSpeedY += Input.GetAxis("Mouse Y");
				transform.Rotate(new Vector3(-0.1f * mouseSpeedY, 0, 0));

				if (Input.GetAxis("Mouse X") == 0)
				{
					mouseSpeedX /= 1.3f;
				}

				if (Input.GetAxis("Mouse Y") == 0)
				{
					mouseSpeedY /= 1.3f;
				}

				float x = Input.GetAxis("Horizontal") * 3;
				float z = Input.GetAxis("Vertical") * 3;

				// move camera
				transform.position = Vector3.Lerp(transform.position, transform.position + transform.forward * z + transform.right * x, Time.deltaTime);
			}
			else
			{
				if (SmoothMovement)
				{
					float x = Input.GetAxis("Horizontal") * CameraMovementSpeed;
					float z = Input.GetAxis("Vertical") * CameraMovementSpeed;
					zoomAmount = Mathf.Clamp(zoomAmount -= Input.mouseScrollDelta.y, NearCameraZoomLimit, FarCameraZoomLimit);

					Vector3 tmp;
					tmp = Vector3.Lerp(transform.position, transform.position + new Vector3(x, 0, z), Time.deltaTime);
					tmp.y = Mathf.Lerp(tmp.y, zoomAmount, Time.deltaTime * CameraZoomSpeed * 2.0f);

					// clamp to player area
					transform.position = Vector3.ClampMagnitude(tmp, PlayerArea);
				}
				else
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
        }
    }
}
