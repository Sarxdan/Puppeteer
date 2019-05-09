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
 * Filip Renman
 * 
 */
public class SnapFunctionality : MonoBehaviour
{
    [Header("Changed by script")]
    public bool Placed;
    [Header("Where trap can be placed")]
	public bool Floor;
	public bool Roof;
	public bool Wall;
    [Header("Type of trap")]
    public bool FakeItem;
    public bool BearTrap;
    public bool RoofSpike;
    public bool FloorSpike;
	[Header("Cost of placing")]
	public int Cost;
}
