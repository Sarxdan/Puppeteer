using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Sandra Andersson
 * 
 * DESCRIPTION:
 * Is placed on the following objects prefabs for sound:
 * Enemy Spawner
 * 
 * CODE REVIEWED BY:
 * Kristoffer Lundgren
 * 
 */

public class EnemySpawnerSounds : MonoBehaviour
{
    // Sound Events
    [FMODUnity.EventRef] public string s_Spawn;
    [FMODUnity.EventRef] public string s_Destroy;

    public void Spawn()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Spawn, transform.position);
    }

    public void Destroy()
    {
        FMODUnity.RuntimeManager.PlayOneShot(s_Destroy, transform.position);
    }
}
