﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
/*
 * AUTHOR:
 * Sandra "Sanders" Andersson
 * 
 * DESCRIPTION:
 * This script is placed on the fake item for specific attributes.
 * 
 * CODE REVIEWED BY:
 * 
 * 
 * CONTRIBUTORS:
 * Kristoffer Lundgren
 */

public class FakeItem : Interactable
{
    public ParticleSystem Explosion;
    public uint Damage;
    public float Radius;
    public bool Activated = false;
    
    // Destroy the whole trap if the trap is activated and particle system is done playing
    private void Update()
    {
        if (Activated && Explosion)
        {
            if (!Explosion.IsAlive())
            {
                Destroy(gameObject);
            }
        }
    }


    // (KL) Used to show the interact tooltip
    public override void OnRaycastEnter(GameObject interactor)
    {
        ShowTooltip(interactor);
    }
    
    // Activate the trap and explode and damage puppet
    public override void OnInteractBegin(GameObject interactor)
    {
        // Activate trap and create explosion
        Activated = true;
        Explosion = Instantiate(Explosion, transform.position, transform.rotation);
        Explosion.transform.parent = gameObject.transform;

        // Damage all players in the explosion area
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, Radius);
        foreach (Collider hit in hitColliders)
        {
            if (hit.gameObject.tag == "Player")
            {
                hit.GetComponent<HealthComponent>().Damage(Damage);
            }
        }
        CmdDie(gameObject);       
    }

    public override void OnInteractEnd(GameObject interactor)
    {
    }

    [Command]
    public void CmdDie(GameObject thing)
    {
        NetworkServer.Destroy(thing);
    }

}