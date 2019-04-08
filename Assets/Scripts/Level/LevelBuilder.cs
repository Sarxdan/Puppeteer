using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    // contains all modules that may be placed in the level
    public GameObject[] Modules;

    // the start module prefab (placed first)
    public GameObject StartModule;

    // the goal module prefab (placed last)
    public GameObject GoalModule;

    // the number of modules that should be placed (not including start and goal modules)
    public int NumModules = 20;

    private Vector3[] directions =
    {
        Vector3.left,
        Vector3.right,
        Vector3.forward,
        Vector3.back,
    };

    void Start()
    {
        // temporary generation
        for(int i = 0; i < NumModules; i++)
        {
            var module = Instantiate(Modules[Random.Range(0, Modules.Length)], transform);
            module.GetComponent<Renderer>().material.SetColor("_Color", new Color(Random.value, Random.value, Random.value));
            module.transform.position = new Vector3(Random.Range(-8, 8), 0.0f, Random.Range(-8, 8));

            CreateDoor(module, Random.Range(2, 4));
        }
    }

    private void CreateDoor(in GameObject obj, int numDoors)
    {
        Debug.Assert(numDoors > 0 && numDoors <= 4);

        var doors = new Stack<Vector3>(directions);
        for(int i = 0; i < numDoors; i++)
        {
            var anchor = new GameObject("Anchor");
            anchor.AddComponent<AnchorPoint>();
            anchor.transform.position = 0.5f * doors.Pop();
            anchor.transform.SetParent(obj.transform, false);
        }
    }
}
