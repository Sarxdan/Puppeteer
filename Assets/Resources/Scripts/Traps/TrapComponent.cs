using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Sandra Andersson
 * 
 * DESCRIPTION:
 * Placed on basic traps.
 * 
 * CODE REVIEWED BY:
 * 
 * 
 * 
 */

public class TrapComponent : MonoBehaviour
{ 
    public uint Damage;
    public float ActivateTime;
    public float Countdown = 0;
    public List<GameObject> Puppets;
    public List<int> test;

    public Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        Puppets = new List<GameObject>();
        test = new List<int>();
        anim = gameObject.GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision!");
        if(other.gameObject.tag == "Player")
        {
            if(Puppets.Count <= 0)
            {
                StartCoroutine("TrapTimer");
            }
            
            Puppets.Add(other.gameObject);
        }
    }
    

    private void OnTriggerExit(Collider other)
    {
        Puppets.Remove(other.gameObject);
    }

    private IEnumerator TrapTimer()
    {
        Debug.Log("Start Timer");
        yield return new WaitForSeconds(ActivateTime);
        Debug.Log("Bang #1");
        anim.SetBool("IsActive", true);

    }

    private void ActivateTrap()
    {
        foreach(GameObject puppet in Puppets)
        {
            puppet.GetComponent<HealthComponent>().Damage(Damage);
        }
        Destroy(gameObject);
        Debug.Log("Ultimate Bang");
        //this.gameObject.SetActive(false);
    }

}
