using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

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
 */

public class ReviveComponent : Interactable
{
    // delay until revive is complete
    public int ReviveDelay;
    // delay until the object may no longer be revived
    public int DeathDelay;
    // determines if the revive requires a medkit
    public bool RequireMedkit = true;

    // Used for drawing the circle on the hud
    private HUDScript hudScript;
    // Used for checking if a player is downed, variable is synced across all clients
    private HealthComponent healthComponent;

    private GameObject downedPanel;
    private RectTransform downedBar;

    private float DefaultVignette = 0.1f;
    private float MaxVignette = 0.5f;
    public Image DeathVignette;

    void Start()
    {
        // register death action
        healthComponent = GetComponent<HealthComponent>();

        healthComponent.AddDeathAction(OnZeroHealth);
        downedPanel = GameObject.Find("DownedPanel");
        downedBar = downedPanel.GetComponentsInChildren<RectTransform>()[1];
        downedPanel.SetActive(false);
    }

    // An object has started to interact this object
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
    }

    public override void OnInteractEnd(GameObject interactor)
    {
        StopCoroutine("ReviveRoutine");
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
    // (KL) Only show the interact tooltip if the player is downed and the interactor has a medkit
    public override void OnRaycastEnter(GameObject interactor)
    {
        bool medKit = interactor.GetComponent<PlayerController>().HasMedkit;
        bool downed = GetComponent<HealthComponent>().Downed;
        if(medKit && downed)
            ShowTooltip(interactor);
    }

    // called when the health of this object reaches zero
    private void OnZeroHealth()
    {
        //hudScript.ScaleInteractionProgress(0);
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

    private bool isVignette(VolumeComponent component)
    {
        return(component is UnityEngine.Experimental.Rendering.HDPipeline.Vignette);
    }

	private IEnumerator DownedBar()
	{
        //UnityEngine.Experimental.Rendering.HDPipeline.Vignette vignette;
        //System.Predicate<VolumeComponent> isVig = isVignette;
		downedPanel.SetActive(true);
        //var volume = GameObject.Find("PostProcess").GetComponent<Volume>();
        //vignette = volume.profile.components.Find(isVig) as UnityEngine.Experimental.Rendering.HDPipeline.Vignette;
        
		var currentSize = downedBar.sizeDelta;
		var diffInWidth = (downedBar.sizeDelta.x / (DeathDelay * 10));
		var time = DeathDelay;
        float vignetteDelta = (MaxVignette - DefaultVignette) / (DeathDelay * 10);
		while (time > 0)
		{
			if (healthComponent.Health != 0)
			{
				break;
			}
			downedBar.sizeDelta = new Vector2(downedBar.sizeDelta.x - diffInWidth, downedBar.sizeDelta.y);
            //vignette.intensity =  new UnityEngine.Rendering.ClampedFloatParameter(vignette.intensity.value + vignetteDelta, 0, 1, true);
            
			yield return new WaitForSeconds(0.1f);
			time--;
		}
		downedBar.sizeDelta = currentSize;
		downedPanel.SetActive(false);
        //vignette.intensity = new UnityEngine.Rendering.ClampedFloatParameter(DefaultVignette, 0, 1);
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
                // someone has revived!
                yield break;
            }
            yield return new WaitForSeconds(1);
        }
        // TODO: perform death action across network
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
            // no medkit available
            yield break;
        }

        float time = 0;
        while(time < ReviveDelay)
        {
            if (healthComponent.Health != 0)
            {
                // someone has revived already
                yield break;
            }
            time += 0.1f;
            //hudScript.ScaleInteractionProgress(time/ReviveDelay);
            RpcScaleProgress(time/ReviveDelay);
            yield return new WaitForSeconds(0.1f);
        }

        // revive successful

        healthComponent.Revive();
        var revivingPlayer = reviver.GetComponent<InteractionController>();
        if(revivingPlayer.isLocalPlayer && revivingPlayer.isServer)
        {
            // Scale the interaction progress to zero
            hudScript.RpcScaleZero();
        }
        hudScript.RpcScaleZero();
        if(RequireMedkit)
        {
            // consume medkit if required
            reviver.GetComponent<PlayerController>().HasMedkit = false;
            RpcRemoveMedkit(reviver);
        }
    }
    // (KL) Sets the correct HUD script for the clients
    [ClientRpc]
    public void RpcGetHudScript(GameObject interactor)
    {
        if(interactor.GetComponent<HealthComponent>().isLocalPlayer)
        {
            hudScript = interactor.GetComponent<HUDScript>();
        }
    }
    // (KL) Since the revive action in running on the server the HUD has to be updated over the network
    [ClientRpc]
    public void RpcScaleProgress(float scale)
    {
        if(hudScript != null)
        {
            hudScript.ScaleInteractionProgress(scale);
        }
    }
    // (KL) Remove the medkit when a revive is complete
    [ClientRpc]
    public void RpcRemoveMedkit(GameObject interactor)
    {
        interactor.GetComponent<PlayerController>().HasMedkit = false;
    }
}


