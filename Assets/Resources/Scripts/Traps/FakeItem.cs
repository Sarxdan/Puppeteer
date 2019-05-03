using System.Collections;
using UnityEngine;

public class FakeItem : Interactable
{
    public GameObject[] Models; //[0] is default model
    public uint Damage;
    public float DestroyTime;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("Test");
    }

    public IEnumerator Test()
    {
        yield return new WaitForSeconds(5);
        Models[0].SetActive(false);
        Models[Random.Range(1, Models.Length)].SetActive(true);
    }

    public override void OnInteractBegin(GameObject interactor)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 5);

        foreach (Collider hit in hitColliders)
        {
            if (hit.gameObject.tag == "Player")
            {
                hit.GetComponent<HealthComponent>().Damage(Damage);
            }
        }

        StartCoroutine("DestroyTimer");
    }

    public IEnumerator DestroyTimer()
    {
        yield return new WaitForSeconds(DestroyTime);

        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    public override void OnInteractEnd(GameObject interactor)
    {
    }
}