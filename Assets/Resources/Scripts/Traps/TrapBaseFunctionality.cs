using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * AUTHOR:
 * Kristoffer "Krig" Lundgren
 * 
 * DESCRIPTION:
 * Script for checking what type a trap is
 * 
 * CODE REVIEWED BY:
 * 
 * 
 */
public class TrapBaseFunctionality : MonoBehaviour
{
    [Header("Changed by script")]
    public bool Placed;
    [Header("Type of trap")]
	public bool Floor;
	public bool Roof;
	public bool Wall;
}
