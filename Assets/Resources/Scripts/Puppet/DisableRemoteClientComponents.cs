using UnityEngine;
using Mirror;

/*
* AUTHOR:
* Filip Renman
*
* DESCRIPTION:
* Disables scripts for remote player gameObjects.
* 
*
* CODE REVIEWED BY:
* = Needs review
*
* CONTRIBUTORS:
*/

public class DisableRemoteClientComponents : NetworkBehaviour
{
    [SerializeField]
    Behaviour[] componentsToDisable;

    Camera sceneCamera;

    void Start()
    {
        if (!isLocalPlayer)
        {
            for (int i = 0; i < componentsToDisable.Length; i++)
            {
                componentsToDisable[i].enabled = false;
            }
        }
    }

    void OnDisable()
    {
        if (sceneCamera != null)
        {
            sceneCamera.gameObject.SetActive(true);
        }
    }
}
