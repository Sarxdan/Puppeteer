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

    public override void OnActivate()
    {
        Debug.Log("POWER ACTIVATED");
    }

    public override void OnComplete()
    {
        Debug.Log("POWER DEACTIVATED");
    }
}
