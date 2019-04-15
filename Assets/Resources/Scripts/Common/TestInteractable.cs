using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInteractable : Interactable
{
    public void PlayerDowned()
    {
        Debug.Log("kill");
    }

    private void Start()
    {
        GetComponent<HealthComponent>().AddDeathAction(PlayerDowned);
    }

    public override void OnInteractBegin(GameObject interactor)
    {
        this.GetComponent<HealthComponent>().Damage(2);
    }

    public override void OnInteractEnd(GameObject interactor)
    {
        
    }
}
