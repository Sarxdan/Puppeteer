using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
 * // 2019-04-30 Krig added placed bool
 */

public abstract class TrapComponent : MonoBehaviour
{ 
    public uint Damage;
    public float ActivationTime;
    public float DestroyTime;   //Time until trap gets destroyed after activated
    public List<GameObject> Puppets;
    public Animator Anim;
    public bool Placed;

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
        //TODO: Set correct conditions for future animations
        Anim.SetBool("IsActive", true);
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

    public abstract void OnTriggerEnter(Collider other);

    public abstract void OnTriggerExit(Collider other);

    public abstract void ActivateTrap();

}
