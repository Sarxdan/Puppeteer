using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Networking;

public class CustomNetworkDiscovery : NetworkDiscoveryBase

{
	List<string> ips = new List<string>(), serverName = new List<string>();
	public GameObject Content;
	public Button ItemTemplate;
	public InputField Ip;

	public override void OnReceivedBroadcast(string fromAddress, string data)
	{
		if (ips.Contains(fromAddress) || serverName.Contains(data))
			return;

		ips.Add(fromAddress);
		serverName.Add(data);
		var copy = Instantiate(ItemTemplate);
		copy.transform.parent = Content.transform;
		var gameNameText = copy.GetComponentsInChildren<Text>()[0];
		var ipAddressText = copy.GetComponentsInChildren<Text>()[1];

		ipAddressText.text = fromAddress.Substring(7);
		gameNameText.text = data;

		copy.onClick.AddListener(()=> this.AddressChoice(fromAddress.Substring(7)));
	}


	void AddressChoice(string ip)
	{
		Ip.text = ip;
	}
}
