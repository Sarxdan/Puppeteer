using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Sandra Andersson
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * Used for objects that may be revived upon reaching zero health. 
 * 
 * CODE REVIEWED BY:
 * 
 * 
 * 
 */
public class ReviveComponent : Interactable
{
    // delay until revive is complete
    public int ReviveDelay;
    // delay until the object may no longer be revived
    public int DeathDelay;

    private HealthComponent healthComponent;

    void Start()
    {
        healthComponent = GetComponent<HealthComponent>();
        // register death action
        healthComponent.AddDeathAction(OnZeroHealth);
    }

    public override void OnInteractBegin(GameObject interactor)
    {
        StartCoroutine("ReviveRoutine");
    }

    public override void OnInteractEnd(GameObject interactor)
    {
        StopCoroutine("ReviveRoutine");
    }

    private void OnZeroHealth()
    {
        StartCoroutine("DeathRoutine");
    }

    private IEnumerator DeathRoutine()
    {
        int time = 0;
        while(++time < DeathDelay)
        {
            if (healthComponent.Health != 0)
            {
                yield break;
            }

            yield return new WaitForSeconds(1);
        }
        // TODO: perform death action across network
        Destroy(gameObject);
    }

    private IEnumerator ReviveRoutine()
    {
        int time = 0;
        while(++time < ReviveDelay)
        {
            if (healthComponent.Health != 0)
            {
                yield break;
            }

            yield return new WaitForSeconds(1);
        }
        // revive successful
        healthComponent.Revive();
    }
}
