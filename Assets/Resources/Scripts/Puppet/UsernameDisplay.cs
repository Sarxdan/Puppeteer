using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UsernameDisplay : NetworkBehaviour
{

    public TextMesh Username;
    private GameObject lookAt;

    // Start is called before the first frame update
    void Start()
    {
        if (isLocalPlayer || GameObject.FindGameObjectWithTag("GameController").GetComponent<NetworkIdentity>().isLocalPlayer)
        {
            Username.gameObject.SetActive(false);
            return;
        }

        Invoke("SetNickname", 1.0f);
    }

    void Update()
    {
        Username.gameObject.transform.LookAt(lookAt.transform);
        Username.gameObject.transform.eulerAngles = Username.gameObject.transform.eulerAngles + 180f * Vector3.up;
    }

    private void SetNickname()
    {
        Username.text = gameObject.GetComponent<PlayerController>().NickName;

        foreach (var item in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (item.GetComponent<PlayerController>().isLocalPlayer)
            {
                lookAt = item;
            }
        }
    }

}
