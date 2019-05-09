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
	private SoundTest soundEmitter;

    public float NoiseRadius;

    //Determines how much liquid this weapon can hold
    public int Capacity;

    public int LiquidLeft;
    public int LiquidPerRound;

    public Transform MagazineTransform;



    //Weapon attributes
    public uint Damage;
    public uint NumShots;
    [Range(0.0f, 1.0f)]
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

    //Time left until weapon can be used again
    private float cooldown;
    private float recoil;
    private float charge;

	public void Start()
	{
		soundEmitter = GetComponent<SoundTest>();
	}

	//Attemps to fire the weapon
	public void Use()
    {
        
        charge += Time.deltaTime;

        if (cooldown != 0 || charge < ChargeTime)
        {
			return;
        }
		if (LiquidLeft < LiquidPerRound)
		{
			// Sound Test
			if (soundEmitter != null)
				soundEmitter.PlaySoundEmptyClip();
			return;
		}

        GetComponentInParent<PlayerController>().AnimController.SetBool("Fire", true);
        for(int i = 0; i < NumShots; i++)
        {
			// Sound Test
			if (soundEmitter != null)
				soundEmitter.PlaySoundGunShot();

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
                Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.black, 1.0f);
                Debug.DrawRay(transform.position, -transform.forward * 100.0f, Color.red, 0.2f);

                Noise.MakeNoise(transform.position, NoiseRadius);
            }
        }

        //Adds recoil and cooldown, and subtracts ammo left
        recoil += RecoilAmount;
        cooldown += FiringSpeed;
        LiquidLeft -= LiquidPerRound;
    }

    //Attemps to reload the weapon to its maximum capacity by the given input amount
    public void Reload(ref int liquidInput)
    {
        // reload not possible if recently fired, currently reloading or weapon too charged
        if (cooldown != 0 || (ChargeTime != 0 && charge > ChargeTime))
            return;

		// Sound Test
		if (soundEmitter != null)
			soundEmitter.PlaySoundReload();

        int amount = Mathf.Min(Capacity - LiquidLeft, liquidInput);
        liquidInput -= amount;
        LiquidLeft += amount;

        // disallow firing while reloading
        cooldown += ReloadTime;
    }

    void Update()
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
            //transform.localEulerAngles = -rotation;
        }
    }

    private void OnGUI()
    {
        // temporary crosshair 
        GUI.Box(new Rect(Screen.width * 0.5f, Screen.height * 0.5f, 10, 10), "");
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
