using System.Collections;
using System.Collections.Generic;
using Mirror;
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
 * Ludvig Björk Förare (08/05-2019 Velocity instead of move)
 * 
 * CONTRIBUTORS:
 * Philip Stenmark
 * Ludvig Björk Förare (Animation integration)
 * Sandra Andersson (Sound Impl.)
 * Filip Renman (Velocity)
*/
public class PlayerController : NetworkBehaviour
{
    public static float SpawnRadius = 3.0f;
	[SyncVar]
	public string NickName;

    // Movement
    [Header("Movement")]
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
    [HideInInspector]
    public Animator AnimController, FPVAnimController;

    // Movement private variables
    private float currentMovementSpeed;
    private float accSave;
    private float speedSave;
    private bool isDown;
    private bool reachedZero;

    [Header("Jumping")]
    public float JumpForce;
    public float JumpRayLength;

    private bool hasLeftGround;
    private bool isGrounded = true;

    // Looking
    [Header("Mouse")]
    public Transform HeadTransform;
    public float LookVerticalMin, LookVerticalMax;
    public float MouseSensitivity;

    // Revive
    public bool HasMedkit;

    // Weapons
    [Header("Weapon")]
    public GameObject CurrentWeapon;
    public int Ammunition;
    public bool CanShoot = true;

    //References
    [Header("References")]
    public Transform HandTransform;
    public Transform FPVHandTransform;
    public GameObject FPVArms;
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

    void Awake()
    {
        gameObject.GetComponent<HealthComponent>().AddDeathAction(Downed);
    }
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        AnimController = GetComponent<Animator>();
        FPVAnimController = FPVArms.GetComponent<Animator>();
        // Saves the original input from the variables
        speedSave = MovementSpeed;
        accSave = AccelerationRate;

        // try to move to spawn position (physics enabled)
        rigidBody.MovePosition(new Vector3(Random.Range(-SpawnRadius, SpawnRadius), 0.0f, Random.Range(-SpawnRadius, SpawnRadius)));

        
        if(isLocalPlayer)
        {
            CurrentWeapon.transform.SetParent(FPVHandTransform);
            CurrentWeapon.transform.localPosition = Vector3.zero;
            CurrentWeapon.transform.localRotation = CurrentWeapon.GetComponent<WeaponComponent>().HoldRotation;
            //transform.Find("Mesh").gameObject.SetActive(false);
        }
        else
        {
            FPVArms.SetActive(false);
        }

        // configure compass
        var compass = GetComponentInChildren<Compass>();
        if (compass)
        {
            foreach (var player in GameObject.FindObjectsOfType<PlayerController>())
            {
                if (player.transform == transform || player.isLocalPlayer)
                {
                    continue;
                }
                compass.AddTarget(player.transform);
            }
        }
		var CNLP = FindObjectsOfType<CustomNetworkLobbyPlayer>();
		foreach (var LP in CNLP)
		{
			if (LP.LocalPlayer)
			{
				CmdSetName(LP.Nickname);
			}
		}
    }

	[Command]
	void CmdSetName(string nickName)
	{
		NickName = nickName;
	}

    private void Update()
    {
        if (!DisableInput)
        {
            if (CurrentWeapon != null && CanShoot)
            {
                // Fire current weapon
                if (Input.GetButton("Fire"))
                {
                    CurrentWeapon.GetComponent<WeaponComponent>().Use();
                }
                // Reload current weapon
                if (Input.GetButton("Reload"))
                {
                    CurrentWeapon.GetComponent<WeaponComponent>().Reload(ref Ammunition);
                }
            } 

            if(Input.GetButtonUp("Fire") && CurrentWeapon != null)
            {
                CurrentWeapon.GetComponent<WeaponComponent>().Release();
            }

            if (Input.GetButtonDown("UsePowerup"))
            {
                StartCoroutine(GetComponent<PowerupBase>().Run());
            }

            // Keeps cursor within screen
            if (Input.GetButton("Fire"))
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

            AnimController.SetFloat("Forward", Input.GetAxis("Vertical"));
            AnimController.SetFloat("Strafe", Input.GetAxis("Horizontal"));

        }

        // Jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            if(Physics.Raycast(transform.position, -transform.up, JumpRayLength))
            {
                isGrounded = false;
                rigidBody.velocity = transform.up * JumpForce;
                AnimController.SetBool("Jump", true);
                FPVAnimController.SetBool("Jump", true);
            }
        }
        else if(!isGrounded)
        {
            if(Physics.Raycast(transform.position, -transform.up, JumpRayLength))
            {
                if(hasLeftGround)
                {
                    isGrounded = true;
                    hasLeftGround = false;
                    AnimController.SetBool("Jump", false);
                    FPVAnimController.SetBool("Jump", false);
                }
            }
            else if(!hasLeftGround)
            {
                hasLeftGround = true;
            }
        }
        float weight = AnimController.GetFloat("JumpWeight");
        AnimController.SetLayerWeight(2, AnimController.GetFloat("JumpWeight") / AnimController.GetLayerWeight(2) + .01f);
        FPVAnimController.SetLayerWeight(2, FPVAnimController.GetFloat("JumpWeight") / FPVAnimController.GetLayerWeight(2)+ .01f);
    }


    private void FixedUpdate()
    {
        // Horizontal movement
        if ((Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) && !DisableInput)
        {
            currentMovementSpeed += currentMovementSpeed < MovementSpeed * MovementSpeedMod ? AccelerationRate * Time.deltaTime : 0; //Accelerates to MovementSpeed
            currentMovementSpeed = Mathf.Clamp(currentMovementSpeed, 0, SprintSpeed); //Clamp the movementspeed so you dont run faster than the sprint speed
            Vector3 direction = (Input.GetAxisRaw("Horizontal") * transform.right + Input.GetAxisRaw("Vertical") * transform.forward).normalized; //Direction to move
            direction.x *= currentMovementSpeed; //Add Movementspeed multiplier 
            direction.y = rigidBody.velocity.y; //Add your y velocity
            direction.z *= currentMovementSpeed; //Add Movementspeed multiplier 
            rigidBody.velocity = direction;
        }
        else
        {
            rigidBody.velocity = new Vector3(0, rigidBody.velocity.y, 0);
        }

        // Sprinting
        // Checks the most important task, if the sprint button is released
        if (Input.GetButtonUp("Sprint") && (!Input.GetButton("Horizontal") || !Input.GetButton("Vertical")))
        {
            AnimController.SetBool("Sprint", false);
            MovementSpeed = speedSave;
            AccelerationRate = accSave;
            reachedZero = false;
            isDown = false;
        }
        // Makes sure stamina can't be negative
        else if (reachedZero == true && isDown == true)
        {
            AnimController.SetBool("Sprint", false);
            MovementSpeed = speedSave;
            currentMovementSpeed = MovementSpeed;
            AccelerationRate = accSave;
            StartCoroutine("StaminaRegenRoutine");
        }

        // Checks for sprint key and acts accordingly
        else if (!DisableInput && (Input.GetButton("Sprint") && (Input.GetButton("Horizontal") || Input.GetButton("Vertical"))))
        {
            AnimController.SetBool("Sprint", true);
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

        Debug.DrawRay(transform.position, -transform.up * JumpRayLength, Color.red, Time.deltaTime);

    }

    //Animation
    public void SetWeaponAnimation(int animationIndex)
    {
        AnimController.SetInteger("Weapon", animationIndex);
        FPVAnimController.SetInteger("Weapon", animationIndex);
    }

    public void StopFire()
    {
        AnimController.SetBool("Fire", false);
    }

    // Freezes the position of the puppet and disables shooting and interacting
    public void Stunned()
    {

        rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        CanShoot = false;
    }

    public void Downed()
    {
        rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        CanShoot = false;
        gameObject.GetComponent<InteractionController>().enabled = false;
        AnimController.SetBool("Downed", true);
        AnimController.SetLayerWeight(1,0);
    }

    // Unstunns the enemy and enables shooting and interacting
    public void UnStunned()
    {
        rigidBody.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
        CanShoot = true;
        gameObject.GetComponent<InteractionController>().enabled = true;
        AnimController.SetBool("Downed", false);
        AnimController.SetLayerWeight(1,1);
    }

    [ClientRpc]
    public void RpcAddAmmo(int liquid)
    {
        if (isLocalPlayer)
        {
            Ammunition += liquid;
        }
    }

    [ClientRpc]
    public void RpcAddMedkit()
    {
        if (isLocalPlayer && !HasMedkit)
        {
            HasMedkit = true;
        }
    }
}
