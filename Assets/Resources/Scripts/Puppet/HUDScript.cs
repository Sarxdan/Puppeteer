using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDScript : MonoBehaviour
{

    // The actual health "bar"
    public RectTransform HealthBarFill;
    // The med kit icon
    public RectTransform MedKit;
    // The player whose health is to me monitored
    public Transform Owner;
    //The health component to monitor
    private HealthComponent healthComponent;
    // Used to check is player has a med kit
    private PlayerController playerController;

    #region Health UI variables
    // Current health of the player
    private uint health;
    // The max health of the player
    private uint maxHealth;
    // The start scale of the helth bar
    private float xScale;
    // The lerp amount per update
    private float increment = 0;
    private uint previousHP;
    // The hp value to lerp to, changes when the hp is modified
    private uint lerpTo;
    private float healthBarSpeed = 0.8f;
    #endregion

    private bool medKitToggle = true;

    // Start is called before the first frame update
    void Start()
    {
        healthComponent = Owner.GetComponent<HealthComponent>();    
        playerController = Owner.GetComponent<PlayerController>();
        xScale = HealthBarFill.localScale.x;
        previousHP = healthComponent.Health;
    }   

    // Update is called once per frame
    void Update()
    {
        drawHealthBar();

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

        increment += healthBarSpeed * Time.deltaTime;
        maxHealth = healthComponent.MaxHealth;
        if(increment >= 1)
        {
            previousHP = healthComponent.Health;
            lerpTo = previousHP;
        }
    }


}
