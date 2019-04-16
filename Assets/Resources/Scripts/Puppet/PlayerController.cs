using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * AUTHOR:
 * Ludvig Björk Förare
 * Carl Appelkvist
 * 
 * DESCRIPTION:
 * A simple player controller allowing horizontal, jump and mouse movement.
 * 
 * CODE REVIEWED BY:
 * Anton Jonsson (Player Movement done)
 * 
 * CONTRIBUTORS:
 * Philip Stenmark
*/
public class PlayerController : MonoBehaviour
{
    //Movement
    public float MovementSpeed;
    public float AccelerationRate;
    private float currentMovementSpeed;
    
    //Jumping
    public float JumpForce;
    public float JumpRayLength;

    //Looking
    public Transform HeadTransform;
    public float LookVerticalMin, LookVerticalMax;
    public float MouseSensitivity;

    //Revive
    public bool HasMedkit;
    public float ReviveTime;

    //Weapon and ammunition storage
    public GameObject CurrentWeapon;
    public int Ammunition;

    private PowerupBase power;
    private Rigidbody rigidBody;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        power = GetComponent<PowerupBase>();
    }

    private void Update()
    {
        if(CurrentWeapon != null)
        {
            if(Input.GetButton("Fire"))
            {
                CurrentWeapon.GetComponent<WeaponComponent>().Use();
            }

            if(Input.GetButtonDown("Reload"))
            {
                CurrentWeapon.GetComponent<WeaponComponent>().Reload(ref this.Ammunition);
            }
        }

        if (Input.GetKeyDown(KeyCode.F))  // FIXME: input mapping
        {
            // launch powerup
            StartCoroutine(power.Run());
        }

        //Keeps cursor within screen
        if (Input.GetAxis("Fire") == 1)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        //Escape releases cursor
        if (Input.GetAxisRaw("Cancel") > 0)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        //Mouse movement
        if (Input.GetAxis("Mouse X") != 0)
        {
            transform.Rotate(Vector3.up * MouseSensitivity * Input.GetAxis("Mouse X"));
        }
        if (Input.GetAxis("Mouse Y") != 0)
        {
            //Checks if within legal rotation limit
            if (HeadTransform.localEulerAngles.x - MouseSensitivity * Input.GetAxis("Mouse Y") < LookVerticalMax || HeadTransform.localEulerAngles.x - MouseSensitivity * Input.GetAxis("Mouse Y") > LookVerticalMin + 360)
            {
                HeadTransform.localEulerAngles = HeadTransform.localEulerAngles - Vector3.right * MouseSensitivity * Input.GetAxis("Mouse Y");
            }
        }
    }

    private void FixedUpdate()
    {
        //Horizontal movement
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            currentMovementSpeed += currentMovementSpeed < MovementSpeed ? AccelerationRate * Time.deltaTime : 0; //Accelerates to MovementSpeed
            Vector3 direction = (Input.GetAxisRaw("Horizontal") * transform.right + Input.GetAxisRaw("Vertical") * transform.forward).normalized; //Direction to move
            rigidBody.MovePosition(transform.position + direction * currentMovementSpeed * Time.deltaTime);
        }
        else
        {
            currentMovementSpeed = 0;
        }

        //Jumping
        if (Input.GetAxisRaw("Jump") > 0 && Physics.Raycast(transform.position, -transform.up, JumpRayLength))
        {
            rigidBody.AddForce(transform.up * JumpForce, ForceMode.Impulse);
        }

    }
    
    
}
