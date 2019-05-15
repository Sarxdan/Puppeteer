using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Ludvig Björk Förare
 * 
 * DESCRIPTION:
 * A script that updates jump layer weight by fetching the value from the animation
 * 
 * CODE REVIEWED BY:
 * Carl Apppelkvist
*/
public class WeightSync : MonoBehaviour
{
    private PlayerController playerController;
    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        float weight = playerController.AnimController.GetFloat("JumpWeight");
        playerController.AnimController.SetLayerWeight(2, weight / playerController.AnimController.GetLayerWeight(2) + .01f);
        playerController.FPVAnimController.SetLayerWeight(2, weight / playerController.FPVAnimController.GetLayerWeight(2)+ .01f);
     
    }
}
