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
 * CLEANED
 */

public class Noise
{
    public static void MakeNoise(Vector3 position, float amplitude)
    {
        foreach(StateMachine minion in EnemySpawner.AllMinions)
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
