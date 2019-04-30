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
 * Filip Renman (Animation 190425)
 * 
 * CONTRIBUTORS:
 * Philip Stenmark
 * Ludvig Björk Förare (Animation integration)
*/
public class PlayerController : MonoBehaviour
{
    // Sound Events
    [FMODUnity.EventRef]
    public string Footstep; 

    // Movement
    public float MovementSpeed;
    public float AccelerationRate;
    public float SprintSpeed;
    public float MaxStamina;
    public float SprintAcc;
    public float StaminaRegenDelay;
    public float StaminaRegenSpeed;
    public float CurrentStamina;
    // Added by Krig, used for disabling the input when in the pause menu.
    public bool DisableInput = false;

    // Movement speed modifier used by power up
    public float MovementSpeedMod = 1.0f;

    // Animation
    private Animator animController;

    // Movement private variables
    private float currentMovementSpeed;
    private float accSave;
    private float speedSave;
    private bool isDown;
    private bool reachedZero;

    // Jumping
    public float JumpForce;
    public float JumpRayLength;

    // Looking
    public Transform HeadTransform;
    public float LookVerticalMin, LookVerticalMax;
    public float MouseSensitivity;

    // Revive
    public bool HasMedkit;
    
    // Weapons
    public GameObject CurrentWeapon;
    public int Ammunition;
    public bool CanShoot = true;

    private Rigidbody rigidBody;

    private IEnumerator StaminaRegenRoutine()
    {
        yield return new WaitForSeconds(StaminaRegenDelay);

        while (CurrentStamina < MaxStamina)
        {
            CurrentStamina++;
            yield return new WaitForSeconds(StaminaRegenSpeed);
        }
    }

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        gameObject.GetComponent<HealthComponent>().AddDeathAction(Stunned);
        animController = GetComponent<Animator>();
        // Saves the original input from the variables
        speedSave = MovementSpeed;
        accSave = AccelerationRate;
    }

    private void Update()
    {
        if(!DisableInput)
        {
              if(CurrentWeapon != null && CanShoot)
        {
            // Fire current weapon
            if(Input.GetButton("Fire"))
            {
                CurrentWeapon.GetComponent<WeaponComponent>().Use();
                animController.SetTrigger("Fire");
            }

            // Reload current weapon
            if(Input.GetButton("Reload"))
            {
                CurrentWeapon.GetComponent<WeaponComponent>().Reload(ref Ammunition);
            }
        }

        if(Input.GetKey(KeyCode.F)) // TODO: add input binding for powerup activation
        {
            StartCoroutine(GetComponent<PowerupBase>().Run());
        }

        // Keeps cursor within screen
        if(Input.GetButton("Fire"))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }


        // Escape releases cursor
        if (Input.GetButton("Cancel"))
        {
            Cursor.lockState = CursorLockMode.None;
        }

        // Mouse movement
        if (Input.GetAxis("Mouse X") != 0)
        {
            transform.Rotate(Vector3.up * MouseSensitivity * Input.GetAxis("Mouse X"));
        }
        if (Input.GetAxis("Mouse Y") != 0)
        {
            // Checks if within legal rotation limit
            if (HeadTransform.localEulerAngles.x - MouseSensitivity * Input.GetAxis("Mouse Y") < LookVerticalMax || HeadTransform.localEulerAngles.x - MouseSensitivity * Input.GetAxis("Mouse Y") > LookVerticalMin + 360)
            {
                HeadTransform.localEulerAngles = HeadTransform.localEulerAngles - Vector3.right * MouseSensitivity * Input.GetAxis("Mouse Y");
            }
        }

        
        animController.SetFloat("Forward", Input.GetAxis("Vertical"));
        animController.SetFloat("Strafe", Input.GetAxis("Horizontal"));

        }
      
    }


    private void FixedUpdate()
    {
        if(!DisableInput)
        {
                // Horizontal movement

            if ( Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
            {
                currentMovementSpeed += currentMovementSpeed < MovementSpeed * MovementSpeedMod ? AccelerationRate * Time.deltaTime : 0; //Accelerates to MovementSpeed
                Vector3 direction = (Input.GetAxisRaw("Horizontal") * transform.right + Input.GetAxisRaw("Vertical") * transform.forward).normalized; //Direction to move
                rigidBody.MovePosition(transform.position + direction * currentMovementSpeed * Time.deltaTime);
            }
            else
            {
                currentMovementSpeed = 0;
            }


            // Sprinting
            // Checks the most important task, if the sprint button is released
            if (Input.GetButtonUp("Sprint"))
            {
                animController.SetBool("Sprint", false);
                MovementSpeed = speedSave;
                AccelerationRate = accSave;
                reachedZero = false;
                isDown = false;
            }
            // Makes sure stamina can't be negative
            else if (reachedZero == true && isDown == true)
            {
                animController.SetBool("Sprint", false);
                MovementSpeed = speedSave;
                currentMovementSpeed = MovementSpeed;
                AccelerationRate = accSave;
                StartCoroutine("StaminaRegenRoutine");
            }
            // Checks for sprint key and acts accordingly
            else if (Input.GetButton("Sprint"))
            {
                animController.SetBool("Sprint", true);
                isDown = true;
                MovementSpeed = SprintSpeed;
                AccelerationRate = SprintAcc;
                CurrentStamina--;
                if (isDown == true && CurrentStamina > 0)
                {
                    StopCoroutine("StaminaRegenRoutine");
                }
                else if (CurrentStamina <= 0)
                {
                    MovementSpeed = speedSave;
                    currentMovementSpeed = MovementSpeed;
                    AccelerationRate = accSave;
                    reachedZero = true;
                    StartCoroutine("StaminaRegenRoutine");
                }
            }
            // Basic stamina regen 
            else
            {
                MovementSpeed = speedSave;
                currentMovementSpeed = MovementSpeed;
                AccelerationRate = accSave;
                StartCoroutine("StaminaRegenRoutine");
            }

            Debug.DrawRay(transform.position, -transform.up*JumpRayLength, Color.red, Time.deltaTime);

            // Jumping
            if (Input.GetButtonDown("Jump") && Physics.Raycast(transform.position, -transform.up, JumpRayLength))
            {
                rigidBody.AddForce(transform.up * JumpForce, ForceMode.Impulse);
            }
        }
        
    }

    // Freezes the position of the puppet and disables shooting and interacting
    public void Stunned()
    {
        rigidBody.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        CanShoot = false;
        if(gameObject.GetComponent<HealthComponent>().Health <= 0)
        {
            gameObject.GetComponent<InteractionController>().enabled = false;
        }
    }

    // Unstunns the enemy and enables shooting and interacting
    public void UnStunned()
    {
        rigidBody.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        CanShoot = true;
        gameObject.GetComponent<InteractionController>().enabled = true;
    }

    public void Step()
    {
        FMODUnity.RuntimeManager.PlayOneShot(Footstep, transform.position);
    }
}
