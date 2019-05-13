using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
	private FluidSimulation liquidScript;

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

    public static float RecoilRecovery = 20.0f;
    public Transform HeadTransform;
    public Quaternion HoldRotation;

    //For hold animation
    public int AnimationIndex;

	public GameObject[] HitDecals;

    //Time left until weapon can be used again
    private float cooldown;
    private float recoil;
    private float charge;

    private bool isHeld;

	public void Start()
	{
		sounds = GetComponent<WeaponSounds>();
	}

	//Attemps to fire the weapon
	public void Use()
    {
        
        if(RequireRelease && isHeld) return;
        isHeld = true;

        charge += Time.deltaTime;

        if (cooldown != 0 || charge < ChargeTime)
        {
			return;
        }
		if (LiquidLeft < LiquidPerRound)
		{
			return;
		}

        GetComponentInParent<PlayerController>().AnimController.SetBool("Fire", true);
        sounds.Shoot();

        for(int i = 0; i < NumShots; i++)
        {

            // calculate spread
            Vector3 offset = Random.insideUnitSphere * Spread;

            RaycastHit hitInfo;
            if(Physics.Raycast(Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f)), Camera.main.transform.forward + offset, out hitInfo, Mathf.Infinity, ~(1 << 8)))
            {
                // deal damage to target if possible
                var health = hitInfo.transform.GetComponentInParent<HealthComponent>();
                if(health != null)
                {
                    uint damage = (uint)(this.Damage * Mathf.Pow(DamageDropoff, hitInfo.distance / 10.0f));
                    health.Damage(damage);
                }
                // create hit decal
                // HitDecals[Random.Range(...)] ?
				Instantiate(HitDecals[0], hitInfo.point + hitInfo.normal * 0.001f, Quaternion.FromToRotation(Vector3.forward, hitInfo.normal), hitInfo.transform);

                Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.black, 1.0f);
                Debug.DrawRay(transform.position, -transform.forward * 100.0f, Color.red, 0.2f);

                Noise.MakeNoise(transform.position, NoiseRadius);
            }
        }

        //Adds recoil and cooldown, and subtracts ammo left
        recoil += RecoilAmount;
        cooldown += FiringSpeed;
        LiquidLeft -= LiquidPerRound;
		UpdateAmmoContainer();
    }

    public void Release()
    {
        isHeld = false;
    }

    //Attemps to reload the weapon to its maximum capacity by the given input amount
    public void Reload(ref int liquidInput)
    {
        // reload not possible if recently fired, currently reloading or weapon too charged
        if (cooldown != 0 || (ChargeTime != 0 && charge > ChargeTime))
            return;
        
        int amount = Mathf.Min(Capacity - LiquidLeft, liquidInput);
        liquidInput -= amount;
        LiquidLeft += amount;
		UpdateAmmoContainer();

        // disallow firing while reloading
        cooldown += ReloadTime;
    }

    void Update()
    {
    }

    void FixedUpdate()
    {
        // decrease cooldown constantly
        cooldown = Mathf.Max(0.0f, cooldown -= Time.deltaTime);
        // decrease weapon charge
        charge = Mathf.Max(0.0f, charge -= Time.deltaTime * 0.5f);

        // perform recoil
        if (HeadTransform != null)
        {
            recoil = Mathf.Clamp(recoil - RecoilRecovery * Time.deltaTime, 0.0f, 45.0f);

            // rotate head according to the recoil amount
            var rotation = HeadTransform.localEulerAngles + Vector3.left * recoil;

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
        GUI.Box(new Rect(Screen.width * 0.5f, Screen.height * 0.5f, 10, 10), "");
    }

	public void UpdateAmmoContainer()
	{
		if (currentMagazine == null)
		{
			currentMagazine = Instantiate(MagazinePrefab, MagazineAttach);
			liquidScript = currentMagazine.GetComponent<FluidSimulation>();
			liquidScript.MaxLiquidAmount = Capacity;
		}
		liquidScript.CurrentLiquidAmount = LiquidLeft;
	}

    public override void OnInteractBegin(GameObject interactor)
    {
        Debug.Log(this);

        this.HeadTransform = interactor.GetComponentInChildren<Camera>().transform;
       
        RpcPickupWeapon(gameObject, interactor);

    }

    public override void OnInteractEnd(GameObject interactor)
    {
        // empty
    }

}
