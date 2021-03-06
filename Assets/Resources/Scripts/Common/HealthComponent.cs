﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
/*
 * AUTHOR:
 * Sandra Andersson
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * Script is placed on entities that will have HP
 * 
 * CODE REVIEWED BY:
 * Ludvig Björk Förare
 * 
 * CONTRIBUTORS:
 * Kristoffer Lundgren (Interact tooltip)
 */
public class HealthComponent : NetworkBehaviour
{
    public static HealthComponent Local;
    //Callback function used when health reaches zero
    public delegate void OnZeroHealth();
    public delegate void OnTakeDamage();

    [SyncVar]
    public uint Health;
    public uint MaxHealth;

    [Range(0.0f, 1.0f)]
    public float MaxRegenRatio;

    [Range(0.0f, 1.0f)]
    public float MaxReviveRatio;

    public uint RegenSpeed;
    public uint RegenDelay;     //Time before regen starts
    public bool AllowRegen;     //Can I regen?
    // (KL) Used for checking if the tooltip should be showed when looking a player
    public bool Downed;

    private OnZeroHealth zeroHealthAction;
    private OnTakeDamage takeDamageAction;

    public CharacterSounds sounds;

    void Start()
    {
        sounds = GetComponent<CharacterSounds>();
        AddOnDamageAction(nothing);
        AddDeathAction(nothing);
    }

    private void nothing(){}

    public override void OnStartLocalPlayer(){
        base.OnStartLocalPlayer();
        Local = this;
    }
    
    public void Damage(uint damage)
    {
        if(isServer)
		{
            if (Health <= 0)
                return;

            StopCoroutine("RegenRoutine");
            
            //Cap the HP so it doesn't go below 0
            Health = (uint)Mathf.Max(0, (int) (Health) - damage);
            if (Health == 0)
            {
                RpcDeath();
                AllowRegen = false;
            }
            else if(Health > 0)
            {
                RpcDamage();         
            }
            else if (AllowRegen)
            {
                StartCoroutine("RegenRoutine");
            }
        }
        else
		{
            Local.CmdDamage(gameObject, damage);
        }
    }
 
    //Starts regenerate HP after delay, up to the max amount of regen
    private IEnumerator RegenRoutine()
    {
        yield return new WaitForSeconds(RegenDelay);

        while(Health < (MaxHealth * MaxRegenRatio))
        {
            Health++;
            yield return new WaitForSeconds(RegenSpeed);
        }
    }

    //Revives the player instantly to the given max revive health
    public void Revive()
    {
        Health = (uint)(MaxHealth * MaxReviveRatio);
        AllowRegen = true;
        PlayerController playerController = gameObject.GetComponent<PlayerController>();
        playerController.UnStunned();
        AddDeathAction(playerController.Downed);
        gameObject.GetComponent<PuppetSounds>().Revive();
        RpcSendRevive();    
    }

    //Registers a new zero health delegate
    public void AddDeathAction(OnZeroHealth action)
    {
        this.zeroHealthAction += action;
    }

    //Unregisters an existing zero health delegate
    public void RemoveDeathAction(OnZeroHealth action)
    {
        this.zeroHealthAction -= action;
    }

    //Registers a new onDamage delegate
    public void AddOnDamageAction(OnTakeDamage action)
    {
        this.takeDamageAction += action;
    }

    //Unregisters an existing onDamage delegate
    public void RemoveOnDamageAction(OnTakeDamage action)
    {
        this.takeDamageAction -= action;
    }

    #region Commands
    [Command]
    public void CmdDamage(GameObject targetObject, uint damage){
        if(!isServer) Debug.LogError("Not server!");
        targetObject.GetComponent<HealthComponent>().Damage(damage);
    }
    #endregion

    #region ClientRpcs
    //Sends damage update to clients
    [ClientRpc]
    public void RpcDamage()
	{
        this.takeDamageAction();
        sounds.Damage(); 
    }

    //Sends death update to clients
    [ClientRpc]
    public void RpcDeath()
    {
		if (isLocalPlayer)
			if(sounds != null) sounds.Death();
        this.zeroHealthAction();
        if(isLocalPlayer)
            gameObject.GetComponent<InteractionController>().InteractionTooltip.enabled = false; 
        Downed = true;
    }

    [ClientRpc]
    public void RpcSendRevive()
    {
        Downed = false;
        if (!isLocalPlayer)
            return;
        gameObject.GetComponent<PuppetSounds>().Revive();
        AllowRegen = true;
        PlayerController playerController = gameObject.GetComponent<PlayerController>();
        playerController.UnStunned();
        AddDeathAction(playerController.Downed);
    }
    #endregion
}
