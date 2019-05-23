using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

/*
 * AUTHOR:
 * Sandra Andersson
 * Philip Stenmark
 * 
 * DESCRIPTION:
 * Used for objects that may be revived upon reaching zero health. 
 * 
 * CODE REVIEWED BY:
 * Benjamin Vesterlund
 * 
 * CONTRIBUTORS:
 * Kristoffer Lundgren
 *
 * CLEANED
 */

public class ReviveComponent : Interactable
{
    //Delay until revive is complete
    public int ReviveDelay;
    //Delay until the object may no longer be revived
    public int DeathDelay;
    //Determines if the revive requires a medkit
    public bool RequireMedkit = true;

    //Used for drawing the circle on the hud
    private HUDScript hudScript;
    //Used for checking if a player is downed, variable is synced across all clients
    private HealthComponent healthComponent;

    private GameObject downedPanel;
    private RectTransform downedBar;

    private Vector3 DefaultVignette = new Vector3(1,1,1);
    private Vector3 MaxVignette = new Vector3(2,2,1);
    public RectTransform DeathVignette;

    void Start()
    {
        //Register death action
        healthComponent = GetComponent<HealthComponent>();

        healthComponent.AddDeathAction(OnZeroHealth);
        downedPanel = GameObject.Find("DownedPanel");

        downedBar = downedPanel.GetComponentsInChildren<RectTransform>()[1];
        downedPanel.SetActive(false);
    }

    //An object has started to interact this object
    public override void OnInteractBegin(GameObject interactor)
    {
        var interactionController = interactor.GetComponent<InteractionController>();
        hudScript = interactor.GetComponent<HUDScript>();
        if(interactionController.isServer && interactionController.isLocalPlayer)
        {
            
        }
        else
        {
            RpcGetHudScript(interactor);
        }
        StartCoroutine("ReviveRoutine", interactor);
        RpcReviveStart();
    }

    public override void OnInteractEnd(GameObject interactor)
    {
        StopCoroutine("ReviveRoutine");
        RpcReviveEnd();
        var interactionController = interactor.GetComponent<InteractionController>();
        if(interactionController.isServer && interactionController.isLocalPlayer)
        {
            hudScript.ScaleInteractionProgress(0);
        }
        else
        {
            if(hudScript != null)
                hudScript.RpcScaleZero();
        }
    }

    [ClientRpc]
    public void RpcReviveStart()
    {
        gameObject.GetComponent<PuppetSounds>().ReviveStart();
    }

    [ClientRpc]
    public void RpcReviveEnd()
    {
        gameObject.GetComponent<PuppetSounds>().ReviveEnd();
    }

    //Only show the interact tooltip if the player is downed and the interactor has a medkit
    public override void OnRaycastEnter(GameObject interactor)
    {
        bool medKit = interactor.GetComponent<PlayerController>().HasMedkit;
        bool downed = GetComponent<HealthComponent>().Downed;
        if(medKit && downed)
            ShowTooltip(interactor);
    }

    //Called when the health of this object reaches zero
    private void OnZeroHealth()
    {
        StartCoroutine("DeathRoutine");
		RpcStartDown(gameObject);
    }

	[ClientRpc]
	public void RpcStartDown(GameObject puppet)
	{
		if (puppet.GetComponent<InteractionController>().isLocalPlayer)
		{
			StartCoroutine("DownedBar");
		}
	}

	private IEnumerator DownedBar()
	{
		downedPanel.SetActive(true);
		var currentSize = downedBar.sizeDelta;
		var diffInWidth = (downedBar.sizeDelta.x / (DeathDelay));
        var vignetteDelta = 1.0f/(DeathDelay);
		var time = DeathDelay;
		while (time > 0)
		{
			if (healthComponent.Health != 0)
			{
				break;
			}
			downedBar.sizeDelta = new Vector2(downedBar.sizeDelta.x - diffInWidth, downedBar.sizeDelta.y);
			yield return new WaitForSeconds(1);
			time--;
		}
		downedBar.sizeDelta = currentSize;
		downedPanel.SetActive(false);
    }


    private IEnumerator DeathRoutine()
    {
        int time = 0;
        MatchTimer matchTimer = FindObjectOfType<MatchTimer>();
        if (matchTimer.numberOfPuppetsAlive == 1)
            DeathDelay = 3;

        while(++time < DeathDelay)
        {
            if (healthComponent.Health != 0)
            {
                //Someone has revived!
                yield break;
            }
            yield return new WaitForSeconds(1);
        }
        RpcStartSpectating(gameObject);
        Destroy(gameObject);
    }

	[ClientRpc]
	void RpcStartSpectating(GameObject puppet)
	{
		if (puppet.GetComponent<InteractionController>().isLocalPlayer)
		{
			var canvas = GameObject.FindObjectOfType<Spectator>().gameObject;
			canvas.GetComponent<Spectator>().SpectatorScreen.SetActive(true);
			canvas.GetComponent<Spectator>().StartSpectating();
		}
	}

    private IEnumerator ReviveRoutine(GameObject reviver)
    {
        if (RequireMedkit && !reviver.GetComponent<PlayerController>().HasMedkit)
        {
            //No medkit available
            yield break;
        }

        float time = 0;
        while(time < ReviveDelay)
        {
            if (healthComponent.Health != 0)
            {
                //Someone has revived already
                yield break;
            }
            time += 0.1f;
            RpcScaleProgress(time/ReviveDelay);
            yield return new WaitForSeconds(0.1f);
        }

        //Revive successful
        healthComponent.Revive();
        var revivingPlayer = reviver.GetComponent<InteractionController>();
        if(revivingPlayer.isLocalPlayer && revivingPlayer.isServer)
        {
            //Scale the interaction progress to zero
            hudScript.RpcScaleZero();
        }
        hudScript.RpcScaleZero();
        if(RequireMedkit)
        {
            //Consume medkit if required
            reviver.GetComponent<PlayerController>().HasMedkit = false;
            RpcRemoveMedkit(reviver);
        }
    }
    //Sets the correct HUD script for the clients
    [ClientRpc]
    public void RpcGetHudScript(GameObject interactor)
    {
        if(interactor.GetComponent<HealthComponent>().isLocalPlayer)
        {
            hudScript = interactor.GetComponent<HUDScript>();
        }
    }
    //Since the revive action in running on the server the HUD has to be updated over the network
    [ClientRpc]
    public void RpcScaleProgress(float scale)
    {
        if(hudScript != null)
        {
            hudScript.ScaleInteractionProgress(scale);
        }
    }
    //Remove the medkit when a revive is complete
    [ClientRpc]
    public void RpcRemoveMedkit(GameObject interactor)
    {
        interactor.GetComponent<PlayerController>().HasMedkit = false;
    }
}


