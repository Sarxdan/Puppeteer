using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TestScript : NetworkBehaviour
{
    public GameObject test;

    // Start is called before the first frame update
    void Start()
    {
        NetworkServer.Spawn(test);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
