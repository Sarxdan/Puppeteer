using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    public GameObject[] Modules;
    public GameObject StartModule;
    public GameObject GoalModule;

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
        // place start module
        var start = Instantiate(StartModule);
        start.transform.SetParent(transform);
        start.transform.position = Vector3.zero;

        CreateDoor(start, 1);

        for(int i = 0; i < NumModules; i++)
        {
            var module = Instantiate(Modules[Random.Range(0, Modules.Length)], transform);
            module.GetComponent<Renderer>().material.SetColor("_Color", new Color(Random.value, Random.value, Random.value));

            module.transform.position = new Vector3(Random.Range(-8, 8), 0.0f, Random.Range(-8, 8));

            CreateDoor(module, Random.Range(2, 4));
        }
    }

    public List<GameObject> GetAllModules()
    {
        return new List<GameObject>(GameObject.FindGameObjectsWithTag("Connectable"));
    }

    public List<AnchorPoint> GetValidDoors()
    {
        var buffer = this.GetComponentsInChildren<AnchorPoint>();
        var result = new List<AnchorPoint>();
        for(int i = 0; i < buffer.Length; i++)
        {
            if(buffer[i].Attachment)
            {
                result.Add(buffer[i]);
            }
        }
        return result;
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
