using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapSnapPoint : SnapPointBase
{
	[Header("Type of Trapnode")]
	public bool Floor;
	public bool Roof;
	public bool Wall;

	[Header("Is node in use")]
	public bool Used;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
