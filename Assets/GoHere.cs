using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoHere : MonoBehaviour
{
    public GameObject target;

    void Update()
    {
        if(Input.GetKey(KeyCode.P)){
            RaycastHit hit;
            if(Physics.Raycast(transform.position, -transform.up,out hit, 3)){
                target.GetComponent<PathfinderComponent>().MoveTo(hit.point, hit.transform.parent);
            }
        }
        
    }
}
