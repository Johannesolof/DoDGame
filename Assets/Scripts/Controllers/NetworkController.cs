using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

public class NetworkController : MonoBehaviour {

	RPC.Server server;
	RPC.Client client;

	PlayerLog playerLog;
	RPC.NetworkingCommon common;
	RPC.NetworkingInfo networkingInfo;

	void Start () {
		common = RPC.NetworkingCommon.Instance;

		networkingInfo = GameController.Instance.networkingInfo;
		if(networkingInfo.port == -1) networkingInfo.port = RPC.NetworkingInfo.defaultPort;
		if(networkingInfo.address == "") networkingInfo.address = RPC.NetworkingInfo.defaultAddress;

		playerLog = GetComponent<PlayerLog>();
		common.RegisterOnConsole(playerLog.AddEvent);

		if(networkingInfo.isServer) 
		{
			server = new RPC.Server(networkingInfo);
			server.RegisterOnClient(ClientAvailable);
		}
		client = new RPC.Client(networkingInfo);
		client.RegisterClientShutdown(ShutdownClient);
	}

	void Update () {
		if(server != null)
		{
			server.Tick(Time.time);
		}
		if(client != null)
		{
			client.Tick(Time.time);
		}
	}

	// Various functions for callbacks etc

	void ShutdownClient()
	{
		client = null;
	}

	void ClientAvailable()
	{
		while(server.getPendingConnections().Count > 0)
		{
			server.ClientAccept(server.getPendingConnections().Dequeue());
		}
	}

	public void SendChatMessage(string msg)
	{
		if(client != null)
		{
			client.SendChatMessage(msg);
		}
	}
}
