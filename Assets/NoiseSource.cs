using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MinionStates;

/*
 * AUTHOR:
 * Ludvig Björk Förare
 * 
 * DESCRIPTION:
 * A class to declare noises alerting enemies within area
 *
 * CODE REVIEWED BY:
 * 
 */

public class Noise
{

    public static List<StateMachine> Minions = new List<StateMachine>();
    
    public static void MakeNoise(Vector3 position, float amplitude)
    {
        foreach(StateMachine minion in Minions)
        {
            if(minion.CurrentStateName != "Attack" || minion.CurrentStateName != "ReturnToSpawn")
            {

                Physics.Raycast(position, minion.transform.position, out RaycastHit hit, amplitude, ~(1 << LayerMask.NameToLayer("Puppeteer Interact")));
                
                if(hit.transform != null && hit.transform == minion.transform){
                    minion.SetState(new SeekState(minion, position));
                }
            }
        }
    }
}
