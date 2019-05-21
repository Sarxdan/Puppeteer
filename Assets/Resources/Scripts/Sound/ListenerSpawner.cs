using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ListenerSpawner : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		if (isLocalPlayer)
		{
			GetComponentInChildren<Camera>().gameObject.AddComponent<FMODUnity.StudioListener>();
		}
    }
}
