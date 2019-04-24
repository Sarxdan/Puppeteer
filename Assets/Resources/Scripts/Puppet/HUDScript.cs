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
    // The stamina bar
    public RectTransform StaminaBarFill;
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
    // The start scale of the health bar
    private float xScaleHP;
    // The start scale of the stamina bar
    private float xScaleStamina;
    // The lerp amount per update
    private float HPIncrement = 0;
    // Used for checking if the hp has changed
    private uint previousHP;
    // The hp value to lerp to, changes when the hp is modified
    private uint lerpToHP;
    // Controls how fast the health bar changes
    public float HealthBarSpeed = 0.8f;

    //Used for making sure the setActive funtion is only run once when the medkit variable changes
    private bool medKitToggle = true;

    //Staminabar
    private float previousStamina;
    private float lerpToStamina;
    private float StaminaIncrement;

    // Start is called before the first frame update
    void Start()
    {
        healthComponent = Owner.GetComponent<HealthComponent>();    
        playerController = Owner.GetComponent<PlayerController>();
        powerUp = Owner.GetComponent<PowerupBase>();
        xScaleHP = HealthBarFill.localScale.x;
        xScaleStamina = StaminaBarFill.localScale.x;
        previousHP = healthComponent.Health;
        previousStamina = playerController.CurrentStamina;
    }   

    // Update is called once per frame
    void Update()
    {
        
        drawHealthBar();



        //Powerup Status
        #region powerUp status
        float delta = powerUp.PercentageLeft;
        PowerUpFill.localScale = new Vector3(1, 1.0f - delta, 1);
        #endregion
        //Shows the medkit icon on the screen
        #region medKit
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
        #endregion
        // Shows the current number of magazines depending on what weapon is currently equipped
        #region magazine counter 
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
        #endregion

        if(playerController.CurrentStamina != previousStamina)
        {
            StaminaIncrement = 0;
            lerpToStamina = previousStamina;
            previousStamina = playerController.CurrentStamina;
        }

        StaminaBarFill.localScale = new Vector3(Mathf.Lerp(xScaleStamina * (Mathf.Clamp(lerpToStamina, 0, playerController.MaxStamina)/playerController.MaxStamina),
                                                xScaleStamina * (Mathf.Clamp(playerController.CurrentStamina, 0, playerController.MaxStamina)/playerController.MaxStamina), StaminaIncrement),
                                                StaminaBarFill.localScale.y, StaminaBarFill.localScale.z);
        StaminaIncrement += 0.8f*Time.deltaTime;

        if(StaminaIncrement >= 1)
        {
            previousStamina = playerController.CurrentStamina;
            lerpToStamina = previousStamina;
        }
    }

    private void drawHealthBar()
    {
        //Check the players health
        health = healthComponent.Health;
        //If the health has been modified reset the lerpTo
        if(previousHP != health)
        {
            HPIncrement = 0;
            lerpToHP = previousHP;
            previousHP = healthComponent.Health;

        }
        // Scale the health bar
        HealthBarFill.localScale = new Vector3(Mathf.Lerp(xScaleHP * (Mathf.Clamp(lerpToHP, 0, maxHealth)/maxHealth), xScaleHP * (Mathf.Clamp(health, 0, maxHealth)/maxHealth), HPIncrement), HealthBarFill.localScale.y, HealthBarFill.localScale.z);

        HPIncrement += HealthBarSpeed * Time.deltaTime;
        maxHealth = healthComponent.MaxHealth;
        if(HPIncrement >= 1)
        {
            previousHP = healthComponent.Health;
            lerpToHP = previousHP;
        }
    }


}
