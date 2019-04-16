using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInteractable : PowerupBase
{
    void Start()
    {
        GetComponent<HealthComponent>().AddDeathAction(Die);   
    }

    void Die()
    {
        Debug.Log("Dead");
    }

    public override void OnActivate(GameObject owner)
    {
        Debug.Log("POWER ACTIVATED");
    }

    public override void OnComplete(GameObject owner)
    {
        Debug.Log("POWER DEACTIVATED");
    }
}
