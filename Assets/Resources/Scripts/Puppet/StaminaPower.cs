using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * Increases maximum stamina and overall movement speed when activated.
 * 
 * CODE REVIEWED BY:
 * 
 */
public class StaminaPower : PowerupBase
{
    public float SpeedModifier = 2.0f;

    public override void OnActivate()
    {
        var player = GetComponent<PlayerController>();
        player.CurrentStamina = Mathf.Infinity;
        player.SprintAcc *= this.SpeedModifier;
        player.SprintSpeed *= this.SpeedModifier;
    }

    public override void OnComplete()
    {
        var player = GetComponent<PlayerController>();
        player.CurrentStamina = player.MaxStamina;
        player.SprintAcc /= this.SpeedModifier;
        player.SprintSpeed /= this.SpeedModifier;
    }
}
