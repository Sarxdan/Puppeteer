using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartRequierments : MonoBehaviour
{

    public GameObject[] SelectableCharacters;
    private CustomNetworkManager NetworkManager;

    // Start is called before the first frame update
    void Start()
    {
        NetworkManager = GameObject.Find("NetworkManager").GetComponent<CustomNetworkManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
