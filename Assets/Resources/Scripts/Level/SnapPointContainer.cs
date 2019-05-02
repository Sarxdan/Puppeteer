using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapPointContainer : MonoBehaviour
{
    public List<SnapPointBase> SnapPoints;

    public List<SnapPointBase> FindSnapPoints()
    {
        if (SnapPoints != null)
            SnapPoints.AddRange(GetComponentsInChildren<SnapPointBase>());
        
        return SnapPoints;
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
