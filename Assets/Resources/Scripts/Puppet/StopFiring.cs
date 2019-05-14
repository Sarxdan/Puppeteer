using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopFiring : MonoBehaviour
{
    void StopFire()
    {
        GetComponent<Animator>().SetBool("Fire", false);
    }
}
