using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
/*
 * AUTHOR:
 * Kristoffer Lundgren
 * 
 * 
 * DESCRIPTION:
 * A script for drawing the HUD for the puppets, it handles health, stamina, medkits, and powerups
 * 
 * 
 * 
 * CODE REVIEWED BY:
 * Benjamin "Boris" Vesterlund 
 *
 * CONTRIBUTORS:
 * 
*/
public class HUDScript : NetworkBehaviour
{

    // The player whose health is to me monitored
    public Transform Owner;
    // The actual health "bar"
    public RectTransform HealthBarFill;
    public Text HealthPercentage;
    // The stamina bar
    public RectTransform StaminaBarFill;
    public Text StaminaPercentage;
    // The med kit icon
    public RectTransform MedKit;
    // The mask that shows how much is left on the power up duration
    public RectTransform PowerUpFill;
    public Image PowerUpDisabled;
    public Image PowerUpEnabled;
    // The text for the current ammo
    public Text CurrentAmmo;
    //The health component to monitor
    private HealthComponent healthComponent;
    // Used to check is player has a med kit
    private PlayerController playerController;
    // The base class of the powerup that the player has
    private PowerupBase powerUp;
    // The image that shows the progress of the current interaction
    public Image InteractionProgress;

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
    private uint lerpFromHP;
    // Controls how fast the health bar changes
    public float HealthBarSpeed = 0.8f;

    //Used for making sure the setActive funtion is only run once when the medkit variable changes
    private bool medKitToggle = true;

    // Used for checking if the stamina has changes
    private float previousStamina;
    // The stamina value to lerp to
    private float lerpFromStamina;
    // The lerp increment
    private float StaminaIncrement;
    // The speed of the stamina bar
    public float StaminaBarSpeed = 0.8f;



    // Getting all the components and setting all the constant variables
    void Start()
    {
        healthComponent = Owner.GetComponent<HealthComponent>();    
        playerController = Owner.GetComponent<PlayerController>();
        powerUp = Owner.GetComponent<PowerupBase>();
        xScaleHP = HealthBarFill.localScale.x;
        xScaleStamina = StaminaBarFill.localScale.x;
        previousHP = healthComponent.Health;
        maxHealth = healthComponent.MaxHealth;
        previousStamina = playerController.CurrentStamina;
        if(!GetComponent<NetworkIdentity>().isLocalPlayer)
        {
            this.enabled = false;
            gameObject.SetActive(false);
        }
    }   

    // Draw the HUD
    void Update()
    {
        
        drawHealthBar();
        drawStaminaBar();


        //Powerup Status
        #region powerUp status
        float delta = powerUp.PercentageLeft;
        if(delta == 0)
        {
            PowerUpDisabled.enabled = true;
            PowerUpEnabled.enabled = false;
            PowerUpFill.localScale = new Vector3(1, 0, 1);
        }
        else
        {
            PowerUpEnabled.enabled = true;
            PowerUpDisabled.enabled = false;
            PowerUpFill.localScale = new Vector3(1, 1.0f - delta, 1);
        }
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
            // Calculated the number of magazine left
            float magazines = Mathf.Clamp(((float)playerController.Ammunition / (float)playerController.CurrentWeapon.GetComponent<WeaponComponent>().Capacity), 0, 999); 
            // Changes the number of leading zeroes depending on the number of magazines
            if(magazines >= 100)
            {
                CurrentAmmo.text = Mathf.FloorToInt(magazines).ToString();
            }
            if(magazines >= 10)
            {
                CurrentAmmo.text = "0" + Mathf.FloorToInt(magazines).ToString();
            }
            else
            {
                CurrentAmmo.text = "00" +  Mathf.FloorToInt(magazines).ToString();
            }
        }
        #endregion
    }

    private void drawHealthBar()
    {
        //Check the players health
        health = healthComponent.Health;
        //If the health has been modified reset the lerpTo
        if(previousHP != health)
        {
            HPIncrement = 0;
            lerpFromHP = previousHP;
            previousHP = healthComponent.Health;
        }
        // Scale the health bar
        HealthBarFill.localScale = new Vector3(Mathf.Lerp(xScaleHP * (Mathf.Clamp(lerpFromHP, 0, maxHealth)/maxHealth), xScaleHP * (Mathf.Clamp(health, 0, maxHealth)/maxHealth), HPIncrement),
                                                HealthBarFill.localScale.y, HealthBarFill.localScale.z);
        // Increment the lerp
        HPIncrement += HealthBarSpeed * Time.deltaTime;

        HealthPercentage.text = Mathf.RoundToInt(health/maxHealth).ToString() + "%";

        // runs whe the lerp is complete
        if(HPIncrement >= 1)
        {
            previousHP = healthComponent.Health;
            lerpFromHP = previousHP;
        }
    }

    private void drawStaminaBar()
    {
        // Check if the stamina has changed and start the lerp if it has
        if(playerController.CurrentStamina != previousStamina)
        {
            StaminaIncrement = 0;
            lerpFromStamina = previousStamina;
            previousStamina = playerController.CurrentStamina;
        }

        StaminaBarFill.localScale = new Vector3(Mathf.Lerp(xScaleStamina * (Mathf.Clamp(lerpFromStamina, 0, playerController.MaxStamina)/playerController.MaxStamina), xScaleStamina * (Mathf.Clamp(playerController.CurrentStamina, 0, playerController.MaxStamina)/playerController.MaxStamina), StaminaIncrement),
                                                StaminaBarFill.localScale.y, StaminaBarFill.localScale.z);
        HealthPercentage.text = Mathf.RoundToInt(playerController.CurrentStamina/playerController.MaxStamina).ToString() + "%";
        StaminaIncrement += StaminaBarSpeed*Time.deltaTime;
        if(StaminaIncrement >= 1)
        {
            previousStamina = playerController.CurrentStamina;
            lerpFromStamina = previousStamina;
        }
    }
    [ClientRpc]
    public void RpcScaleZero()
    {
        InteractionProgress.fillAmount = 0;
    }

    public void ScaleInteractionProgress(float percentage)
    {
        InteractionProgress.fillAmount = Mathf.Clamp01(percentage);
    }
}
