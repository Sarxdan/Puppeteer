using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

/*
 * AUTHOR:
 * Filip Renman
 * 
 * 
 * DESCRIPTION:
 * Displays the username above the other puppets in the game. This is the name they had in the lobby.
 * 
 * CODE REVIEWED BY:
 * Anton Jonsson (16/05-2019)
 * 
 * CONTRIBUTORS:
 * 
*/

public class UsernameDisplay : NetworkBehaviour
{
    //The text above a puppets head
    public TextMesh Username;
    //The target the text will rotate towards
    private GameObject lookAt;

    // Start is called before the first frame update
    void Start()
    {
        //Disable this script for the local puppet or for all if we are the puppeteer
        if (isLocalPlayer || GameObject.FindGameObjectWithTag("GameController").GetComponent<NetworkIdentity>().isLocalPlayer)
        {
            Username.gameObject.SetActive(false);
            return;
        }

        Invoke("SetNickname", 1.0f);
    }

    void Update()
    {
        if (lookAt != null)
        {
            //Rotate the text towards the localplayer
            Vector3 v = lookAt.transform.position - transform.position;
            v.x = v.z = 0.0f;
            Username.transform.LookAt(lookAt.transform.position - v);
            Username.transform.Rotate(45, 180, 0);
        }
    }

    private void SetNickname()
    {
        //Get the username
        Username.text = gameObject.GetComponent<PlayerController>().NickName;

        //Find the localplayer
        foreach (var item in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (item.GetComponent<PlayerController>().isLocalPlayer)
            {
                lookAt = item.GetComponentInChildren<Camera>().gameObject;
            }
        }
    }
}