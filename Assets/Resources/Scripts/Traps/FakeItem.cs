using System.Collections;
using UnityEngine;

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
 * 
 */

public class FakeItem : Interactable
{
    public GameObject[] Models; //[0] is default model
    public uint Damage;
    public float DestroyTime;

    // Start is called before the first frame update
    void Start()
    {
        GameObject newModel =
            Instantiate(Models[Random.Range(1, Models.Length)], transform.position, transform.rotation);
        newModel.transform.parent = transform;
        Destroy(Models[0]);
    }
    
    public override void OnInteractBegin(GameObject interactor)
    {
        Debug.Log("Interact");
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 4);

        foreach (Collider hit in hitColliders)
        {
            if (hit.gameObject.tag == "Player")
            {
                hit.GetComponent<HealthComponent>().Damage(Damage);
            }
        }

        StartCoroutine("DestroyTimer");
    }

    public IEnumerator DestroyTimer()
    {
        yield return new WaitForSeconds(DestroyTime);

        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    public override void OnInteractEnd(GameObject interactor)
    {
    }
}