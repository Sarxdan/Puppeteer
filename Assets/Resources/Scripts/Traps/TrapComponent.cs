using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
 * AUTHOR:
 * Sandra "Sanders" Andersson
 * 
 * DESCRIPTION:
 * This script may be inherited by scripts for traps.
 * 
 * CODE REVIEWED BY:
 * Philip Stenmark
 * 
 */

public abstract class TrapComponent : NetworkBehaviour
{ 
    public uint Damage;
    public float ActivationTime;
    public float DestroyTime;   //Time until trap gets destroyed after activated
    public List<GameObject> Puppets;
    public Animator Anim;


    // Start is called before the first frame update
    void Start()
    {
        Puppets = new List<GameObject>();
        Anim = gameObject.GetComponent<Animator>();
    }

    //Start timer for activating the trap and start the animation
    private IEnumerator TrapTimer()
    {
        yield return new WaitForSeconds(ActivationTime);
        ActivateAnim();
    }

    private IEnumerator DestroyTimer()
    {
        yield return new WaitForSeconds(DestroyTime);
        DestroyTrap();
    }
    public void DestroyTrap()
    {
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    public void ActivateAnim()
    {
        Anim.SetBool("IsActive", true);
    }

    public abstract void OnTriggerEnter(Collider other);

    public abstract void OnTriggerExit(Collider other);

    public abstract void ActivateTrap();

}
