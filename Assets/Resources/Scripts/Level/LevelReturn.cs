using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelReturn : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnTriggerEnter(Collider other)
	{
		other.gameObject.transform.position = GameObject.Find("startRoom").transform.position + new Vector3(0, 1, 0);
	}
}
