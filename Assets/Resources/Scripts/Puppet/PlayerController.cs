using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * AUTHOR:
 * Ludvig Björk Förare
 * Carl Appelkvist 
 * 
 * 
 * DESCRIPTION:
 * A simple player controller allowing horizontal, jump and mouse movement.
 * 
 * Sprint done, made by Carl Appelkvist
 * 
 * CODE REVIEWED BY:
 * Anton Jonsson (Player Movement done)
 * Ludvig Björk Förare (Sprint function)
 * 
 * CONTRIBUTORS:
 * 
*/
public class PlayerController : MonoBehaviour
{
    //Movement
    public float MovementSpeed;
    public float AccelerationRate;
    public float SprintSpeed;
    public float MaxStamina;
    public float SprintAcc;
    public float StaminaRegenDelay;
    public float RegenSpeed;

    //Movement private variables
    private float currentMovementSpeed;
    [SerializeField]
    private float currentStamina;
    private float accSave;
    private float speedSave;
    private bool isDown;
    private bool reachedZero;

    //Jumping
    public float jumpForce;
    public float jumpRayLength;

    //Looking
    public Transform HeadTransform;
    public float LookVerticalMin, LookVerticalMax;
    public float MouseSensitivity;

    //Revive
    public bool HasMedkit;
    public float ReviveTime;
    

    //Weapons + powerups
    public bool PowerupReady;
    public GameObject CurrentWeapon;
    public int Ammunition;

    private Rigidbody rigidBody;


    private IEnumerator StaminaRegenRoutine()
    {
        yield return new WaitForSeconds(StaminaRegenDelay);

        while (currentStamina < MaxStamina)
        {
            currentStamina++;
            yield return new WaitForSeconds(RegenSpeed);
        }
    }

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        //Saves the original input from the variables
        speedSave = MovementSpeed;
        accSave = AccelerationRate;
    }

    private void Update()
    {
        //Keeps cursor within screen
        if(Input.GetAxis("Fire") == 1)
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

        //Sprinting
        //Checks the most important task, if the sprint button is released
        if (Input.GetKeyUp("left shift"))
        {
            MovementSpeed = speedSave;
            AccelerationRate = accSave;
            reachedZero = false;
            isDown = false;
        }
        //Makes sure stamina can't be negative
        else if (reachedZero == true && isDown == true)
        {
            MovementSpeed = speedSave;
            currentMovementSpeed = MovementSpeed;
            AccelerationRate = accSave;
            StartCoroutine("StaminaRegenRoutine");
        }
        //Checks for sprint key and acts accordingly
        else if (Input.GetKey("left shift"))
        {
            isDown = true;
            MovementSpeed = SprintSpeed;
            AccelerationRate = SprintAcc;
            currentStamina--;
            if (isDown == true && currentStamina > 0)
            {
                StopCoroutine("StaminaRegenRoutine");
            }
            else if (currentStamina <= 0)
            {
                MovementSpeed = speedSave;
                currentMovementSpeed = MovementSpeed;
                AccelerationRate = accSave;
                reachedZero = true;
                StartCoroutine("StaminaRegenRoutine");
            }
        }
        //Basic stamina regen 
        else
        {
            MovementSpeed = speedSave;
            currentMovementSpeed = MovementSpeed;
            AccelerationRate = accSave;
            StartCoroutine("StaminaRegenRoutine");
        }


        //Jumping
        if (Input.GetAxisRaw("Jump") > 0 && Physics.Raycast(transform.position, -transform.up, jumpRayLength))
        {
            rigidBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }
}
