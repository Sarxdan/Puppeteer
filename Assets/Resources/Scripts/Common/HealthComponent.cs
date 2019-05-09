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
 * Script is placed on entities that will have HP
 * 
 * CODE REVIEWED BY:
 * Ludvig Björk Förare
 * 
 * 
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

    private OnZeroHealth zeroHealthAction;
    private OnTakeDamage takeDamageAction;

    public CharacterSounds sound;

    void Start(){
        this.AddOnDamageAction(dummy);
        this.AddDeathAction(dummy);
        sound = gameObject.GetComponent<CharacterSounds>();
    }

    public override void OnStartLocalPlayer(){
        base.OnStartLocalPlayer();
        Local = this;
    }

    void dummy(){

    }
    
    public void Damage(uint damage)
    {
        if(isServer){
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
        else {
            //Local.CmdDamage(gameObject, damage);
        }
    }

    [Command]
    public void CmdDamage(GameObject targetObject, uint damage){
        targetObject.GetComponent<HealthComponent>().Damage(damage);
    }

    //Sends damage update to clients
    [ClientRpc]
    public void RpcDamage(){
        this.takeDamageAction();
        sound.Damage(); 
    }

    //Sends death update to clients
    [ClientRpc]
    public void RpcDeath()
    {
        sound.Death();
        this.zeroHealthAction();
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
        AddDeathAction(playerController.Stunned);
    
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
}
