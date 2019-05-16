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
        if (lookAt != null)
        {
            Vector3 v = lookAt.transform.position - transform.position;

            v.x = v.z = 0.0f;
            Username.transform.LookAt(lookAt.transform.position - v);
            Username.transform.Rotate(45, 180, 0);
        }


        //if (lookAt != null)
        //{
        //    Vector3 objectNormal = Username.transform.rotation * Vector3.forward;
        //    Vector3 cameraToText = Username.transform.position - Camera.main.transform.position;
        //    float f = Vector3.Dot(objectNormal, cameraToText);
        //    if (f < 0.0f)
        //    {
        //        Username.transform.Rotate(0f, 180f, 0f);
        //    }
        //}

        //if (lookAt != null)
        //{
        //    Username.gameObject.transform.LookAt(2 * transform.position - lookAt.transform.position);
        //}
    }

    private void SetNickname()
    {
        Username.text = gameObject.GetComponent<PlayerController>().NickName;

        foreach (var item in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (item.GetComponent<PlayerController>().isLocalPlayer)
            {
                lookAt = item.GetComponentInChildren<Camera>().gameObject;
            }
        }
    }

}
