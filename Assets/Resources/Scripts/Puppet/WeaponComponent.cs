using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
 * AUTHOR:
 * Sandra Andersson
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * Script is placed on the weapons for all weapon logic.
 * 
 * CODE REVIEWED BY:
 * Benjamin Vesterlund
 * 
 * 
 * CONTRIBUTORS:
 * Kristoffer Lundgren
 */
public class WeaponComponent : Interactable
{
	// Sound Test
    private WeaponSounds sounds;

    public float NoiseRadius;

    //Determines how much liquid this weapon can hold
    public int Capacity;

    public int LiquidLeft;
    public int LiquidPerRound;

	public GameObject MagazinePrefab;
    public Transform MagazineAttach;
	private GameObject currentMagazine;
    [HideInInspector]
	public FluidSimulation LiquidScript;

    private PlayerController user;

    //Weapon attributes
    public uint Damage;
    public uint NumShots;
    public bool RequireRelease;
    [Range(0.0f, 5.0f)]
    public float FiringSpeed;
    [Range(0.0f, 4.0f)]
    public float ReloadTime;
    [Range(0.0f, 0.2f)]
    public float Spread;
    [Range(0.0f, 4.0f)]
    public float RecoilAmount;
    [Range(0.0f, 1.0f)]
    public float DamageDropoff;

    // time required before weapon is ready to fire (i.e gatling gun spinning up)
    [Range(0.0f, 4.0f)]
    public float ChargeTime;

    public static float RecoilRecovery = 16.0f;
    public Transform HeadTransform;
    public Quaternion HoldRotation;

    //For hold animation
    public int AnimationIndex;

	public GameObject[] HitDecals;

    //Time left until weapon can be used again
    private bool onCooldown = true;
    private float recoil;
    private float charge;
    private float rotX;
    private bool resetRecoil;

    private bool isHeld;

    private bool showingMagazine;

	public void Start()
	{
		sounds = GetComponent<WeaponSounds>();
        ShowMagazine();
        onCooldown = false;
	}

	//Attemps to fire the weapon
	public void Use()
    {
        
        if(RequireRelease && isHeld) return;
        isHeld = true;

        charge = Mathf.Min(charge + Time.deltaTime, ChargeTime);

        if (onCooldown || charge < ChargeTime)
        {
			return;
        }

        sounds.Shoot(LiquidLeft / LiquidPerRound);    //Send amount of "bullets" left in mag to sound man.

        if (LiquidLeft < LiquidPerRound)
		{
			return;
		}

        PlayerController pc = GetComponentInParent<PlayerController>();
        DecalHandler decalHandler = pc.GetComponent<DecalHandler>();
        
        for(int i = 0; i < NumShots; i++)
        {
            // calculate spread
            Vector3 offset = Random.insideUnitSphere * Spread;

            RaycastHit hitInfo;
            if(Physics.Raycast(Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f)), Camera.main.transform.forward + offset, out hitInfo, Mathf.Infinity, ~(1 << 8 | 1 << 2)))
            {
                // deal damage to target if possible
                var health = hitInfo.transform.GetComponentInParent<HealthComponent>();
                if(health != null)
                {
                    uint damage = (uint)(this.Damage * Mathf.Pow(DamageDropoff, hitInfo.distance / 10.0f));
                    health.Damage(damage);
                    Debug.LogFormat("Dealt {0} damage in range {1}", damage, hitInfo.distance);
                }
				// create hit decal
				decalHandler.AddDecal(Instantiate(HitDecals[Random.Range(0, HitDecals.Length)], hitInfo.point, Quaternion.FromToRotation(Vector3.forward, hitInfo.normal), hitInfo.transform));

                Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.black, 1.0f);
                Debug.DrawRay(transform.position, -transform.forward * 100.0f, Color.red, 0.2f);

                Noise.MakeNoise(transform.position, NoiseRadius);
            }
        }

        rotX = HeadTransform.localEulerAngles.x;
        resetRecoil = true;

        pc.AnimController.SetBool("Fire", true);
        pc.FPVAnimController.SetTrigger("Fire");

        //Adds recoil and cooldown, and subtracts ammo left
        recoil += RecoilAmount;
        LiquidLeft -= LiquidPerRound;
		LiquidScript.CurrentLiquidAmount = LiquidLeft;
    }

    public void Release()
    {
        isHeld = false;
    }

    //Attemps to reload the weapon to its maximum capacity by the given input amount
    public void Reload(ref int liquidInput)
    {
        if(!CanReload()) return;

        int amount = Mathf.Min(Capacity - LiquidLeft, liquidInput);
        liquidInput -= amount;
        LiquidLeft += amount;
    }

    public bool CanReload()
    {
         return !onCooldown && (ChargeTime == 0 || charge == 0);
    }

    void FixedUpdate()
    {
        // decrease weapon charge
        charge = Mathf.Max(0.0f, charge -= Time.deltaTime * 0.5f);

        // perform recoil
        if (HeadTransform != null)
        {
            recoil = Mathf.Max(Mathf.Lerp(recoil, recoil - RecoilRecovery, Time.fixedDeltaTime), 0.0f);

            // rotate head according to the recoil amount
            var rotation = HeadTransform.localEulerAngles + Vector3.left * recoil;
            if(resetRecoil)
            {
                rotation.x = Mathf.MoveTowardsAngle(rotation.x, rotX, Time.fixedDeltaTime * RecoilRecovery * RecoilAmount);
                if(rotation.x == rotX || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.1f)
                {
                    resetRecoil = false;
                }
            }

            //Prevents the recoil to go to far and making the camera turn upside down.
            if (rotation.x < 270 && rotation.x > 90)
            {
                if (rotation.x > 180)
                    rotation.x = 270;
                else
                    rotation.x = 90;
            }

            HeadTransform.localEulerAngles = rotation;
            rotation.y = 180.0f;
        }
    }

    private void OnGUI()
    {
        // temporary crosshair 
        GUI.Box(new Rect(Screen.width * 0.5f - 5, Screen.height * 0.5f - 5, 10, 10), "");
    }


    public override void OnInteractBegin(GameObject interactor)
    {

        this.HeadTransform = interactor.GetComponentInChildren<Camera>().transform;
       
        RpcPickupWeapon(gameObject, interactor);

        sounds.Pickup(); // Send pickup trigger to sound

    }

    public override void OnInteractEnd(GameObject interactor)
    {
        // empty
    }
    // (KL) Used to show the interact tooltip
    public override void OnRaycastEnter(GameObject interactor)
    {
        ShowTooltip(interactor);
    }


    [ClientRpc]
    public void RpcPickupWeapon(GameObject weaponObject, GameObject userObject)
    {

        WeaponComponent newWeapon = weaponObject.GetComponent<WeaponComponent>();
        PlayerController user = userObject.GetComponent<PlayerController>();


        //Disables new weapons collider
        newWeapon.GetComponent<CapsuleCollider>().enabled = false;

        GameObject CurrentWeaponObject = user.CurrentWeapon;
        //If carrying a weapon, detach it and place it on new weapons location
        if (CurrentWeaponObject != null && CurrentWeaponObject.transform != transform)
        {
            WeaponComponent CurrentWeapon = CurrentWeaponObject.GetComponent<WeaponComponent>();
            CurrentWeapon.HeadTransform = null;
            CurrentWeapon.transform.SetPositionAndRotation(transform.position, transform.rotation);
            CurrentWeapon.transform.SetParent(null);
            CurrentWeapon.GetComponent<Collider>().enabled = true;
        }

        //Attaches new weapon to player
        user.CurrentWeapon = newWeapon.gameObject;
        newWeapon.HeadTransform = user.HeadTransform;
        user.CurrentWeaponComponent = newWeapon;
        user.CurrentLiquidPerRound = newWeapon.LiquidPerRound;
        user.SetWeaponAnimation(newWeapon.AnimationIndex);

        if (userObject.GetComponent<PlayerController>().isLocalPlayer)
        {
            newWeapon.transform.SetParent(user.FPVArms.Right);
        }
        else
        {
            newWeapon.transform.SetParent(user.FullBody.Right);
        }

        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = newWeapon.HoldRotation;

        if (user.isLocalPlayer)
        {
            newWeapon.ShowMagazine();
        }
    }

    //Doesn't actually remove or add, just hides or shows
    public void HideMagazine()
    {
        if(onCooldown) return;

        if(currentMagazine != null)
        {
            currentMagazine.SetActive(false);
        }

        onCooldown = true;
    }

    public void ShowMagazine()
    {
        if(!onCooldown) return;

        if(currentMagazine == null)
        {
			currentMagazine = Instantiate(MagazinePrefab, MagazineAttach);
			LiquidScript = currentMagazine.GetComponent<FluidSimulation>();
			LiquidScript.MaxLiquidAmount = Capacity;
        }
		LiquidScript.CurrentLiquidAmount = LiquidLeft;
        currentMagazine.SetActive(true);
    }

    public void StopCooldown()
    {
        onCooldown = false;
    }
}
