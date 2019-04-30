using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuppeteerCameraController : MonoBehaviour
{
    public float CameraMovementSpeed = 40f;
    public float SideBorderThickness = 10f;

    void Update()
    {
        Vector3 pos = transform.position;

        if (Input.GetAxis("Vertical") > 0 && Input.GetButton("Vertical") || Input.mousePosition.y >= Screen.height - SideBorderThickness)
        {
            pos.z += CameraMovementSpeed * Time.deltaTime;
        }
        if (Input.GetAxis("Vertical") < 0 && Input.GetButton("Vertical") || Input.mousePosition.y <=SideBorderThickness)
        {
            pos.z -= CameraMovementSpeed * Time.deltaTime;
        }
        if (Input.GetAxis("Horizontal") > 0 && Input.GetButton("Horizontal") || Input.mousePosition.x >= Screen.width - SideBorderThickness)
        {
            pos.x += CameraMovementSpeed * Time.deltaTime;
        }
        if (Input.GetAxis("Horizontal") < 0 && Input.GetButton("Horizontal") || Input.mousePosition.x <= SideBorderThickness)
        {
            pos.x -= CameraMovementSpeed * Time.deltaTime;
        }

        transform.position = pos;

    }
}
