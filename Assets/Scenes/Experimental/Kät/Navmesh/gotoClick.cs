using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class gotoClick : MonoBehaviour
{
    PathfinderComponent agent;
    void Start()
    {
        agent = transform.GetComponent<PathfinderComponent>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.P) && !agent.HasPath)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit))
            {
                Vector3 pos = hit.point;
                pos.y = 0;
                Debug.DrawRay(pos, transform.up, Color.red, 2);
                agent.MoveTo(pos, hit.transform);
            }
        }
    }
}
