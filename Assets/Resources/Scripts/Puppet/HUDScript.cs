using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDScript : MonoBehaviour
{

    public RectTransform HealthBarFill;
    public HealthComponent healthComponent;
    public Transform Owner;

    private uint health;
    private uint maxHealth;
    private float xScale;
    private float increment = 0;

    // Start is called before the first frame update
    void Start()
    {
        healthComponent = Owner.GetComponent<HealthComponent>();    



        xScale = HealthBarFill.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        health = healthComponent.Health;
        maxHealth = healthComponent.MaxHealth;
        HealthBarFill.localScale = new Vector3(Mathf.Lerp(xScale * Mathf.Clamp(health, 0, maxHealth)/maxHealth, xScale, 0.0001f), 0.1f, 1.0f);
    }
}
