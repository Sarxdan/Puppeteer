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
 * CONTRIBUTORS:
 * Kristoffer Lundgren
 */

public class FakeItem : Interactable
{
    public GameObject[] Models; //[0] is default model
    public ParticleSystem Explosion;
    public GameObject NewModel;
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

    // Switch model to a random one of some specific items
    public string SwitchModel()
    {
        NewModel =
            Instantiate(Models[Random.Range(1, Models.Length)], transform.position, transform.rotation);
        NewModel.transform.parent = transform;
        Destroy(Models[0]);
        return NewModel.name;
    }
    
    // Switch model to a random one of some specific items
    public void MakeModel(string name, TransformStruct placingTransform)
    {
        foreach (GameObject model in Models)
        {
            if (model != null && model.name + "(Clone)" == name)
            {
                NewModel =
                    Instantiate(model, placingTransform.Position, placingTransform.Rotation);
                NewModel.transform.parent = transform;
                Destroy(Models[0]);
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

        Destroy(NewModel);
        
    }

    public override void OnInteractEnd(GameObject interactor)
    {
    }
}