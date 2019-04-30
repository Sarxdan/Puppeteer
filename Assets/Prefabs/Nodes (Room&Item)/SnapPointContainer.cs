using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapPointContainer : MonoBehaviour
{
    public List<TrapSnapPoint> TrapSnapPoint;

    public void FindTrapSnapPoint()
    {
        TrapSnapPoint.AddRange(GetComponentsInChildren<TrapSnapPoint>());
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
