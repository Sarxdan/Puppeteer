using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

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
    public ParticleSystem Explosion;
    public GameObject NewModel;
    public uint Damage;
    public float DestroyTime;
    public float Radius;
    public bool Activated = false;

    // Start is called before the first frame update
    void Start()
    {
        NewModel =
            Instantiate(Models[Random.Range(1, Models.Length)], transform.position, transform.rotation);
        NewModel.transform.parent = transform;
        Destroy(Models[0]);
    }

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

    public override void OnInteractBegin(GameObject interactor)
    {
        Debug.Log("Interact");
        Activated = true;
        Explosion = Instantiate(Explosion, transform.position, transform.rotation);
        Explosion.transform.parent = gameObject.transform;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, Radius);

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

        if (NewModel != null)
        {
            Destroy(NewModel);
        }
    }

    public override void OnInteractEnd(GameObject interactor)
    {
    }
}