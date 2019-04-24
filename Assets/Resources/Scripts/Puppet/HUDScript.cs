using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDScript : MonoBehaviour
{

    // The player whose health is to me monitored
    public Transform Owner;
    // The actual health "bar"
    public RectTransform HealthBarFill;
    // The med kit icon
    public RectTransform MedKit;
    // The mask that shows how much is left on the power up duration
    public RectTransform PowerUpFill;
    // The text for the current ammo
    public Text CurrentAmmo;
    //The health component to monitor
    private HealthComponent healthComponent;
    // Used to check is player has a med kit
    private PlayerController playerController;

    private PowerupBase powerUp;

    // Current health of the player
    private uint health;
    // The max health of the player
    private uint maxHealth;
    // The start scale of the helth bar
    private float xScale;
    // The lerp amount per update
    private float increment = 0;
    // Used for checking if the hp has changed
    private uint previousHP;
    // The hp value to lerp to, changes when the hp is modified
    private uint lerpTo;
    // Controls how fast the health bar changes
    public float HealthBarSpeed = 0.8f;


    private bool medKitToggle = true;

    // Start is called before the first frame update
    void Start()
    {
        healthComponent = Owner.GetComponent<HealthComponent>();    
        playerController = Owner.GetComponent<PlayerController>();
        powerUp = Owner.GetComponent<PowerupBase>();
        xScale = HealthBarFill.localScale.x;
        previousHP = healthComponent.Health;
    }   

    // Update is called once per frame
    void Update()
    {
        drawHealthBar();
        int duration = powerUp.Duration;
        float delta = powerUp.PercentageLeft;
        PowerUpFill.localScale = new Vector3(1, 1.0f-Mathf.Lerp(0.0f, 1, delta), 1);

        if(playerController.HasMedkit && medKitToggle)
        {
            MedKit.gameObject.SetActive(true);
            medKitToggle = false;
        }
        else if(!playerController.HasMedkit && !medKitToggle)
        {
            MedKit.gameObject.SetActive(false);
            medKitToggle = true;
        }
        

        if(playerController.CurrentWeapon != null)
        {
            float magazines = playerController.Ammunition / playerController.CurrentWeapon.GetComponent<WeaponComponent>().Capacity; 
            
            if(magazines > 100)
            {
                CurrentAmmo.text = Mathf.FloorToInt(magazines).ToString();
            }
            if(magazines > 10)
            {
                CurrentAmmo.text = "00" + Mathf.FloorToInt(magazines).ToString();
            }else
            {
                CurrentAmmo.text = "0" +  Mathf.FloorToInt(magazines).ToString();
            }
        }

    }

    private void drawHealthBar()
    {
        //Check the players health
        health = healthComponent.Health;
        //If the health has been modified reset the lerpTo
        if(previousHP != health)
        {
            increment = 0;
            lerpTo = previousHP;
            previousHP = healthComponent.Health;

        }
        // Scale the health bar
        HealthBarFill.localScale = new Vector3(Mathf.Lerp(xScale * (Mathf.Clamp(lerpTo, 0, maxHealth)/maxHealth), xScale * (Mathf.Clamp(health, 0, maxHealth)/maxHealth), increment), 0.1f, 1.0f);

        increment += HealthBarSpeed * Time.deltaTime;
        maxHealth = healthComponent.MaxHealth;
        if(increment >= 1)
        {
            previousHP = healthComponent.Health;
            lerpTo = previousHP;
        }
    }


}
