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
* Benjamin "Boris" Vesterlund 23/4/2019
*
* CONTRIBUTORS:
* Ludvig Björk Förare (added null protection)
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
                if(componentsToDisable[i] == null) continue;
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
