using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupRecharge : Interactable
{
    public override void OnInteractBegin(GameObject interactor)
    {
        var power = interactor.GetComponent<PowerupBase>();

        // attempt to pickup recharge
        if (power != null && !power.Charged)
        {
            power.Charged = true;
            Destroy(gameObject);
        }
    }

    public override void OnInteractEnd(GameObject interactor)
    {
        // empty
    }
}
