using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorPoint : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
		transform.LookAt(transform.parent);
		transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y + 180, 0);
    }

    // Update is called once per frame
    void Update()
    {
		Debug.DrawLine(transform.position, transform.position + transform.up * 2, Color.yellow);
		Debug.DrawLine(transform.position, transform.position + transform.forward * 2, Color.blue);

    }
}
