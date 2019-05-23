using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* AUTHOR:
* Benjamin "Boris" Vesterlund
*
* DESCRIPTION:
* Point holding info for placing traps.
*
* CODE REVIEWED BY:
* Sandra "Sanders" Andersson
* 
* CONTRIBUTORS:
* 
* 
* CLEANED
*/

public class TrapSnapPoint : SnapPointBase
{
	// what type of trap can be placed here.
	[Header("Type of Trapnode")]
	public bool Floor;
	public bool Roof;
	public bool Wall;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
