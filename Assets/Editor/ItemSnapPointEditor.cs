using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*
 * AUTHOR:
 * Ludvig Björk Förare
 * 
 * DESCRIPTION:
 * A custom editor that allows previewing of item spawning
 *
 * CODE REVIEWED BY:
 * 
 * 
 * CLEANED
 */

#if UNITY_EDITOR
[CustomEditor(typeof(ItemSnapPoint))]
public class ItemSnapPointEditor : Editor
{
    private GameObject currentSpawned;
    private List<GameObject> spawnables = new List<GameObject>();
    private int index;
    private ItemSnapPoint snapPoint;
    private ItemSpawner spawner;
    public override void OnInspectorGUI()
    {
        snapPoint = (ItemSnapPoint)target;
        spawner = snapPoint.GetComponentInParent<ItemSpawner>();

        DrawDefaultInspector();

        if(GUILayout.Button("Hide")) 
        {
            if(currentSpawned != null)
            {
                DestroyImmediate(currentSpawned);
                currentSpawned = null;
            }
            //Destroys children
            foreach(Transform child in snapPoint.transform)
            {
                DestroyImmediate(child.gameObject);
            }
            index = 0;
            spawnables = new List<GameObject>();
        }
        if(GUILayout.Button("Next"))
            next();
    }

    private void next()
    {
        //Destroys existing children
        foreach(Transform child in snapPoint.transform)
        {
            DestroyImmediate(child.gameObject);
        }
        //Nullprotection
        if(spawnables == null) spawnables = new List<GameObject>();

        //Fetches spawnable list
        if(spawnables.Count == 0)
        {
            if(snapPoint.SpawningWeapons)
            {
                spawnables.Add(spawner.Pistol);
                spawnables.Add(spawner.Rifle);
                spawnables.Add(spawner.Shotgun);
                spawnables.Add(spawner.GatlingGun);
            }

            if(snapPoint.SpawningMedKit)
                spawnables.Add(spawner.MedKitItem);

            if(snapPoint.SpawningAmmo)
                spawnables.Add(spawner.AmmoItem);

            if(snapPoint.SpawningPowerUps)
                spawnables.Add(spawner.PowerUpItem);
        }

        //Loops index
        if(index >= spawnables.Count) index = 0;

        //Spawns object
        currentSpawned = PrefabUtility.InstantiatePrefab(spawnables[index]) as GameObject;
        currentSpawned.transform.SetParent(snapPoint.transform);
        currentSpawned.transform.localPosition = Vector3.zero;
		SpawnOffset spawnOffset = currentSpawned.GetComponent<SpawnOffset>();
		if (spawnOffset != null)
		{
			currentSpawned.transform.localPosition += spawnOffset.Offset;
		}
		currentSpawned.transform.localEulerAngles = Vector3.zero; 
        index++;
    }
}
#endif