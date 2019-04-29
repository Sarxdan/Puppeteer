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
        if(Input.GetAxisRaw("Fire") > 0 && !agent.HasPath)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit) && hit.transform.CompareTag("RoomCollider"))
            {
                agent.MoveTo(hit.point, hit.transform);
            }
        }
    }
}
