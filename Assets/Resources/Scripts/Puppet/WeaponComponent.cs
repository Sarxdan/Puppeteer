using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using MinionStates;

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
 * CONTRIBUTORS:
 * Kristoffer Lundgren
 * Ludvig Björk Förare (Reload animation, gatling gun spin, bullet effects)
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

    //Flash and trail
    private MuzzleFlash muzzleFlash;
    public bool SpawnTrail;
    public BulletTrail TrailObject;
    private Transform muzzlePoint;

    //Splat particles

    public WallSplat WallSplatObject;

    // time required before weapon is ready to fire (i.e gatling gun spinning up)
    [Range(0.0f, 4.0f)]
    public float ChargeTime;

    public static float RecoilRecovery = 16.0f;
    public Transform HeadTransform;
    public Quaternion HoldRotation;

    //For hold animation
    public int AnimationIndex;

    public GatlingGunAnimator GatlingAnimator;
    public GatlingGunSounds gatlingGunSounds;

    public GameObject[] HitDecals;

    //Time left until weapon can be used again
    private bool reloading = true;

    private float cooldown;
    private float recoil;
    private float charge;
    private float rotX;
    private bool resetRecoil;

    private bool isHeld;

    private bool showingMagazine;

    public void Start()
    {
        sounds = GetComponent<WeaponSounds>();
        gatlingGunSounds = GetComponent<GatlingGunSounds>();
        muzzleFlash = GetComponentInChildren<MuzzleFlash>();
        ShowMagazine();
        reloading = false;
        muzzlePoint = transform.Find("MuzzlePoint");
    }


    //Attemps to fire the weapon
    public void Use()
    {
        if (!isHeld && gatlingGunSounds != null)
        {
            gatlingGunSounds.SpinUp();
        }

        if (RequireRelease && isHeld) return;
        isHeld = true;

        charge = Mathf.Min(charge + Time.deltaTime, ChargeTime);

        PlayerController pc = GetComponentInParent<PlayerController>();

        if (reloading || charge < ChargeTime || cooldown > 0)
        {
            pc.AnimController.SetBool("Fire", false);
            pc.FPVAnimController.SetBool("Fire", false);
            return;
        }
        cooldown = FiringSpeed;

        CmdShotSound(LiquidLeft / LiquidPerRound);

        if (LiquidLeft < LiquidPerRound)
        {
            return;
        }

        DecalHandler decalHandler = pc.GetComponent<DecalHandler>();

        muzzleFlash.Fire();

        for (int i = 0; i < NumShots; i++)
        {

            // calculate spread
            Vector3 offset = Random.insideUnitSphere * Spread;

            RaycastHit hitInfo;
            if (Physics.Raycast(Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f)), Camera.main.transform.forward + offset, out hitInfo, Mathf.Infinity, ~(1 << 8 | 1 << 2)))
            {
                // deal damage to target if possible
                var health = hitInfo.transform.GetComponentInParent<HealthComponent>();
                if (health != null)
                {
                    if(hitInfo.transform.tag == "Enemy"){
                        CmdAggro(hitInfo.transform.gameObject);
                    }
                    uint damage = (uint)(this.Damage * Mathf.Pow(DamageDropoff, hitInfo.distance / 10.0f));
                    health.Damage(damage);
                }
                // create hit decal
                //decalHandler.AddDecal(Instantiate(HitDecals[Random.Range(0, HitDecals.Length)], hitInfo.point, Quaternion.FromToRotation(Vector3.forward, hitInfo.normal), hitInfo.transform));
                Instantiate(WallSplatObject.gameObject, hitInfo.point, Quaternion.LookRotation(hitInfo.normal, Vector3.up), hitInfo.transform);

                if (SpawnTrail)
                {
                    BulletTrail spawnedTrail = Instantiate(TrailObject.gameObject, muzzlePoint.position, muzzlePoint.rotation).GetComponent<BulletTrail>();
                    spawnedTrail.hit(hitInfo.point);
                }

                Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.black, 1.0f);
                Debug.DrawRay(transform.position, -transform.forward * 100.0f, Color.red, 0.2f);

                Noise.MakeNoise(transform.position, NoiseRadius);
            }
        }

        rotX = HeadTransform.localEulerAngles.x;
        resetRecoil = true;

        pc.AnimController.SetBool("Fire", true);
        pc.FPVAnimController.SetTrigger("Fire");

        float animationSpeed = 0.367f / FiringSpeed;
        pc.AnimController.SetFloat("FireSpeed", animationSpeed);
        pc.FPVAnimController.SetFloat("FireSpeed", animationSpeed);

        //Adds recoil and cooldown, and subtracts ammo left
        recoil += RecoilAmount;
        LiquidLeft -= LiquidPerRound;
        LiquidScript.CurrentLiquidAmount = LiquidLeft;
    }

    public void Release()
    {
        PlayerController pc = GetComponentInParent<PlayerController>();
        pc.AnimController.SetBool("Fire", false);
        pc.FPVAnimController.SetBool("Fire", false);
        if (gatlingGunSounds != null)
        {
            gatlingGunSounds.SpinDown();
        }
        isHeld = false;
    }

    //Attemps to reload the weapon to its maximum capacity by the given input amount
    public void Reload(ref int liquidInput)
    {
        if (!CanReload()) return;

        CmdReloadSound();
        int amount = Mathf.Min(Capacity - LiquidLeft, liquidInput);
        liquidInput -= amount;
        LiquidLeft += amount;
    }

    public bool CanReload()
    {
        return !reloading && cooldown == 0;
    }

    void Update()
    {
        // decrease weapon charge
        charge = Mathf.Max(0.0f, charge -= Time.deltaTime * 0.5f);

        cooldown = Mathf.Max(0.0f, cooldown - Time.deltaTime);

        // perform recoil
        if (HeadTransform != null)
        {
            recoil = Mathf.Max(Mathf.Lerp(recoil, recoil - RecoilRecovery, Time.deltaTime), 0.0f);

            // rotate head according to the recoil amount
            var rotation = HeadTransform.localEulerAngles + Vector3.left * recoil;
            if (resetRecoil)
            {
                rotation.x = Mathf.MoveTowardsAngle(rotation.x, rotX, Time.deltaTime * RecoilRecovery * RecoilAmount);
                if (rotation.x == rotX || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.1f)
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

        if (GatlingAnimator != null)
        {
            if (reloading)
                GatlingAnimator.CurrentSpeed = 0;
            else
                GatlingAnimator.CurrentSpeed = charge / ChargeTime;

        }
    }

    public override void OnInteractBegin(GameObject interactor)
    {

        this.HeadTransform = interactor.GetComponentInChildren<Camera>().transform;
        GetComponent<NetworkIdentity>().AssignClientAuthority(interactor.GetComponent<NetworkIdentity>().connectionToClient);

        RpcPickupWeapon(gameObject, interactor);
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

    [Command]
    public void CmdAggro(GameObject target)
    {
        StateMachine machine = target.GetComponent<StateMachine>();
        if(machine.TargetEntity == null)
        {
            machine.TargetEntity = transform.GetComponentInParent<HealthComponent>();
            machine.SetState(new AttackState(machine));
        }
    }

    [Command]
    public void CmdShotSound(float ammoleft)
    {
        RpcShotSound(ammoleft);
    }

    [Command]
    public void CmdReloadSound()
    {
        RpcReloadSound();
    }

    [ClientRpc]
    public void RpcPickupWeapon(GameObject weaponObject, GameObject userObject)
    {

        WeaponComponent newWeapon = weaponObject.GetComponent<WeaponComponent>();
        PlayerController user = userObject.GetComponent<PlayerController>();
        // plays sound.
        sounds.Pickup(); // Send pickup trigger to sound

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
            CmdSetWeaponLayer(CurrentWeaponObject, 0);
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

    [ClientRpc]
    public void RpcShotSound(float ammoleft)
    {
        sounds.Shoot(ammoleft);    //Send amount of "bullets" left in mag to sound man.
    }

    [ClientRpc]
    public void RpcReloadSound()
    {
        sounds.Reload();
    }

    //Doesn't actually remove or add, just hides or shows
    public void HideMagazine()
    {
        if (reloading) return;

        if (currentMagazine != null)
        {
            currentMagazine.SetActive(false);
        }

        reloading = true;
    }

    public void ShowMagazine()
    {
        if (!reloading) return;

        if (currentMagazine == null)
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
        reloading = false;
    }

    [Command]
    public void CmdSetWeaponLayer(GameObject obj, int value)
    {
        RpcSetWeaponLayer(obj, value);
    }

    [ClientRpc]
    public void RpcSetWeaponLayer(GameObject obj, int value)
    {
        setWeaponLayer(obj, value);
    }

    public void setWeaponLayer(GameObject obj, int layer)
    {
        //Exit condidtion
        if (null == obj)
        {
            return;
        }

        //Set the layer of the gameObject
        obj.layer = layer;

        //For each child in the gameObject, Call this function again.
        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }
            setWeaponLayer(child.gameObject, layer);
        }
    }
}
