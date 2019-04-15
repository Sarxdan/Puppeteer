using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponComponent : Interactable
{
    //Determines how much liquid this weapon can hold
    public int Capacity;

    public int LiquidLeft;
    public int LiquidPerRound;
    public uint Damage;
    public uint NumShots;

    [Range(0.0f, 1.0f)]
    public float FiringSpeed;
    [Range(0.0f, 4.0f)]
    public float ReloadTime;
    [Range(0.0f, 1.0f)]
    public float Spread;
    [Range(0.0f, 1.0f)]
    public float RecoilAmount;

    private float cooldown;

    public void Use()
    {
        if (cooldown != 0 || LiquidLeft == 0)
            return;

        for(int i = 0; i < NumShots; i++)
        {
            // calculate spread
            Vector3 offset = Random.insideUnitCircle * Spread;

            RaycastHit hitInfo;
            if(Physics.Raycast(Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f)), Camera.main.transform.forward + offset, out hitInfo))
            {
                var health = hitInfo.transform.GetComponent<HealthComponent>();
                if(health != null)
                {
                    health.Damage(this.Damage);
                }
                Debug.DrawLine(hitInfo.point, Camera.main.transform.forward * -1.0f, Color.black, 2.0f);
                Debug.Log("hit");
            }
        }

        cooldown += FiringSpeed;
        LiquidLeft -= LiquidPerRound;
    }

    public void Reload(ref int liquidInput)
    {
        if (cooldown != 0)
            return;

        int amount = Mathf.Min(Capacity - LiquidLeft, liquidInput);
        liquidInput -= amount;
        LiquidLeft += amount;

        cooldown += ReloadTime;
    }

    void Update()
    { 
        if(Input.GetKey(KeyCode.F))
        {
            this.Use();
        }

        cooldown = Mathf.Max(0.0f, cooldown -= Time.deltaTime);
    }

    public override void OnInteractBegin(GameObject interactor)
    {
        throw new System.NotImplementedException();
    }

    public override void OnInteractEnd(GameObject interactor)
    {
        throw new System.NotImplementedException();
    }
}
