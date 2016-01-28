using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

using RPC;

public class NetworkingTest : MonoBehaviour {

	// Private fields
	const string serverTag = "Server";
	const string clientTag = "Client";
	const string chatTag = "";
	const string noTag = "";
	const short JJMSG_ID_OFFSET = short.MaxValue/2;
	const int maxConcurrentConnectedUsers = 10;
	const byte SERVERID = 0;
	const byte PlayerIdsLowerBound = 1;
	const byte PlayerIdsUpperBound = byte.MaxValue/2;
	byte myID = 0;
	NetworkClient myClient;
	PlayerLog eventLog;
	ConnectionConfig config;
	HostTopology hostconfig;

	RPC.NetworkingInfo networkingInfo;
	RPC.NetworkingCommon common;

	const string localServer = "localServer";
	const string localClient = "localClient";

	List<ServerPlayer> connectedPlayers;
	List<Player> allPlayersClient;

	Dictionary<string, DateTime> whiteList;
	Queue<NetworkConnection> pendingConnections;

	TimeSpan whiteListTimeOut = new TimeSpan(6,0,0); // 6 hours for the whitelist to time out
	float lastHeartBeat = 0f; // Last time stamp, for heartbeat checks, in seconds since game start
	float heartBeatTimeOut = 2f; // 2 seconds time out

//	bool isAtStartup = true;

	void Start ()
	{
		networkingInfo = GameController.Instance.networkingInfo;
		if(networkingInfo.port == -1) networkingInfo.port = RPC.NetworkingInfo.defaultPort;
		if(networkingInfo.address == "") networkingInfo.address = RPC.NetworkingInfo.defaultAddress;

		common = NetworkingCommon.Instance;

		eventLog = GetComponent<PlayerLog>();

		SetupAllVariables();

		if(networkingInfo.isServer) SetupServer();
		SetupClient();
	}

	void SetupAllVariables()
	{
		connectedPlayers = new List<ServerPlayer>();
		allPlayersClient = new List<Player>();
		whiteList = new Dictionary<string, DateTime>();
		pendingConnections = new Queue<NetworkConnection>();

		config = new ConnectionConfig();
		RPC.Channels.priority = config.AddChannel(QosType.AllCostDelivery);
		RPC.Channels.reliable = config.AddChannel(QosType.ReliableSequenced);
		RPC.Channels.unreliable = config.AddChannel(QosType.UnreliableSequenced);
		RPC.Channels.fragmented = config.AddChannel(QosType.ReliableFragmented);
		RPC.Channels.update = config.AddChannel(QosType.StateUpdate);
		hostconfig = new HostTopology(config, maxConcurrentConnectedUsers);
	}

	void Update () 
	{
		// TODO: Move this stuff to a more appropriate place
//		if (isAtStartup)
//		{
//			if (Input.GetKeyDown(KeyCode.C))
//			{
//				SetupClient();
//			}
//
//			if (Input.GetKeyDown(KeyCode.H))
//			{
//				SetupServer();
//				SetupClient();
//			}
//		}
//		else
//		{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			ResetClientAndServerAndRestart();
			GameController.Instance.SceneLoader("MenuScene");
		}

		// TODO: Move this stuff to a more appropriate place and handle in a more delicate fashion
		if(networkingInfo.isServer && pendingConnections.Count > 0)
		{
			NetworkConnection nc = pendingConnections.Dequeue();
			OnClientAccept(nc);
		}
//		}

		doHeartBeatChecks();
	}

	void OnGUI()
	{
//		//  TODO: Move this stuff to a more appropriate place
//		if (isAtStartup)
//		{
//			GUI.Label(new Rect(10, 30, 200, 100), "Press H for host");
//			GUI.Label(new Rect(10, 50, 200, 100), "Press C for client");
//			GUI.Label(new Rect(10, 70, 200, 100), "Press Q to toggle console");
//			GUI.Label(new Rect(10, 90, 200, 100), "Press M log sample message");
//		}
//		else
//		{
//			GUI.Label(new Rect(10, 30, 200, 100), "Press D to disconnect");
		GUI.Label(new Rect(10, 50, 250, 100), "Press ESC to terminate networking");
		GUI.Label(new Rect(10, 70, 250, 100), "Press Q to toggle console");
		GUI.Label(new Rect(10, 90, 250, 100), "Press M log sample message");
//		}
	}


	// ===========================================================================
	// ========================= SETUP FUNCTIONS =========================
	// ===========================================================================

	// Create a server and listen on a port
	void SetupServer()
	{
		NetworkServer.Configure(hostconfig);
		NetworkServer.Listen(networkingInfo.port);
		NetworkServer.RegisterHandler(MsgType.Connect, OnClientConnected);
		eventLog.AddTaggedEvent(serverTag, "Setup complete", true);
		networkingInfo.address = localServer;
	}

	// Create a client and connect to the server port
	void SetupClient()
	{
		if(networkingInfo.isServer)
		{
			myClient = ClientScene.ConnectLocalServer();
			myClient.Configure(hostconfig);

			myClient.RegisterHandler(MsgType.Connect, OnConnected);
			myClient.RegisterHandler(MsgType.Disconnect, ConnectionFailed);
			registerAllDodCallbacks(myClient, cbClientHandler);
		}
		else
		{
			myClient = new NetworkClient();
			myClient.Configure(hostconfig);

			myClient.RegisterHandler(MsgType.Connect, OnConnected);
			myClient.RegisterHandler(MsgType.Error, ConnectionError);
			myClient.RegisterHandler(MsgType.Disconnect, ConnectionFailed);
			registerAllDodCallbacks(myClient, cbClientHandler);

			eventLog.AddTaggedEvent(clientTag, "Setup complete", true);

			myClient.Connect(networkingInfo.address, networkingInfo.port);
			eventLog.AddTaggedEvent(clientTag, "Connecting to " + networkingInfo.address + ":" + networkingInfo.port, true);
		}
	}

	void ResetClientAndServerAndRestart ()
	{
		if(networkingInfo.isServer)
		{
			NetworkServer.DisconnectAll();
			NetworkServer.Shutdown();
			NetworkServer.Reset();
		}
		if(myClient != null)
		{
			myClient.Shutdown();
			myClient = null;
		}
	}


	// ===========================================================================
	// ========================= NETWORKING CALLBACK FUNCTIONS =========================
	// ===========================================================================

	// SERVER SIDE
	void OnClientConnected(NetworkMessage netMsg)
	{
		netMsg.conn.RegisterHandler(MsgType.Disconnect, OnClientDisconnected);

		if ( netMsg.conn.address == "localClient" )
		{
			OnClientAccept(netMsg.conn);
		}
		else
		{
			eventLog.AddTaggedEvent(serverTag, "A peer has connected from " + netMsg.conn.address, true);

			if ( whiteList.ContainsKey( netMsg.conn.address ) )
			{
				if ( whiteList[netMsg.conn.address] > DateTime.Now )
				{
					OnClientAccept(netMsg.conn);
					return;
				}
				whiteList.Remove(netMsg.conn.address);
			}
			pendingConnections.Enqueue(netMsg.conn);
		}
	}

	void OnClientAccept(NetworkConnection nc)
	{
		if ( !whiteList.ContainsKey( nc.address ) ) // White list him for default amount of time, if he is not already white listed
		{
			whiteList.Add(nc.address, DateTime.Now + whiteListTimeOut);
		}

		registerAllDodCallbacks(nc, cbServerHandler);

		byte newID = giveUniquePlayerID();

		StartCoroutine( serverDelayBeforeSendingHandshake(newID, nc) );
	}

	void OnClientRefuse(NetworkConnection nc)
	{
		// TODO: Add anti-spam support
		eventLog.AddTaggedEvent(serverTag, "Refusing peer: " + nc.address, true);
		nc.Disconnect();
	}

	void OnClientDisconnected(NetworkMessage netMsg)
	{
		eventLog.AddTaggedEvent(serverTag, "Peer disconnected: " + netMsg.conn.address, true);
		ServerPlayer sp;
		if ( common.existPlayerByConnection(connectedPlayers, netMsg.conn, out sp) )
		{
			connectedPlayers.Remove( sp );

			ServerSendMessage(RPC.Message.ID.PlayerDisc,
				new RPC.Message.PlayerDisc(sp.id, sp.dcReason),
				RPC.Channels.reliable, connectedPlayers);
		}
	}

	// CLIENT SIDE
	void OnConnected(NetworkMessage netMsg)
	{
		myClient.UnregisterHandler(MsgType.Disconnect);
		myClient.RegisterHandler(MsgType.Disconnect, OnDisconnected);
	}
	void OnFailedToConnect(NetworkConnectionError error)
	{
		eventLog.AddTaggedEvent(clientTag, "Unable to connect to server " + error.ToString(), true);
	}
	void ConnectionError(NetworkMessage netMsg)
	{
		eventLog.AddTaggedEvent(clientTag, "Unable to connect to server " + networkingInfo.address + ":" + networkingInfo.port + ". Please check the adress and port and try again.", true);
		ResetClientAndServerAndRestart();
	}
	void ConnectionFailed(NetworkMessage netMsg)
	{
		eventLog.AddTaggedEvent(clientTag, "Unable to connect to server, connection timed out: " + netMsg.conn.address + ":" + networkingInfo.port, true);
		ResetClientAndServerAndRestart();
	}
	void OnDisconnected(NetworkMessage netMsg)
	{
		eventLog.AddTaggedEvent(clientTag, "Disconnected from server " + printServerAdress(), true);
		ResetClientAndServerAndRestart();
	}


	// ===========================================================================
	// ========================= SEND FUNCTIONS =========================
	// ===========================================================================

	void ServerSendMessage(RPC.Message.ID id, MessageBase msg, byte channel, NetworkConnection conn)
	{
		if(conn != null)
			conn.SendByChannel((short)id, msg, channel);
		else
			eventLog.AddTaggedEvent(serverTag, "Is not connected to client " + conn.address, true);
	}

	void ServerSendMessage(RPC.Message.ID id, MessageBase msg, byte channel, List<NetworkConnection> playerConnectionList)
	{
		foreach(NetworkConnection nc in playerConnectionList)
		{
			nc.SendByChannel((short)id, msg, channel);
		}
	}

	void ServerSendMessage(RPC.Message.ID id, MessageBase msg, byte channel, ServerPlayer player)
	{
		if(player.connection.isConnected)
			player.connection.SendByChannel((short)id, msg, channel);
		else
			eventLog.AddTaggedEvent(serverTag, "Player is not connected on client " + player.connection.address, true);
	}

	void ServerSendMessage(RPC.Message.ID id, MessageBase msg, byte channel, List<ServerPlayer> players)
	{
		foreach(ServerPlayer p in players)
		{
			p.connection.SendByChannel((short)id, msg, channel);
		}
	}


	void ClientSendMessage(RPC.Message.ID id, MessageBase msg, byte channel)
	{
		if(isConnectedAndAuthenticated())
			myClient.connection.SendByChannel((short)id, msg, channel);
		else
			eventLog.AddTaggedEvent(clientTag, "Is not connected to any server", true);
	}


	// ===========================================================================
	// ========================= DOD CALLBACK FUNCTIONS =========================
	// ===========================================================================

	// SERVER SIDE DOD MESSAGE CALLBACK
	void cbServerHandler (NetworkMessage netMsg) 
	{
		if ( !whiteList.ContainsKey(netMsg.conn.address) )
		{
			// User is not yet white listed, or should not be. Just kick and forget about him
			eventLog.AddTaggedEvent(serverTag, "Kicked chatty not white-listed peer: " + netMsg.conn.address, true);
			netMsg.conn.Disconnect();
			return;
		}


		switch(netMsg.msgType)
		{
		case (short)RPC.Message.ID.UserLogin:  // Last part of user connecting to the server. Check if name is free
			{
				RPC.Message.UserLogin msg = netMsg.ReadMessage<RPC.Message.UserLogin>();
				if ( common.existPlayerByName(connectedPlayers, msg.name) )
				{
					ServerSendMessage(RPC.Message.ID.KickReason,
						new RPC.Message.KickReason("Name already taken!"),
						RPC.Channels.reliable, netMsg.conn);
					netMsg.conn.Disconnect();
				}
				else // New user connected!
				{
					ServerPlayer p = new ServerPlayer(msg.playerID, msg.name, netMsg.conn);
					connectedPlayers.Add(p);

					eventLog.AddTaggedEvent(serverTag, "Player connected: " + printPlayerName(p) + " from " + netMsg.conn.address, true);

					ServerSendMessage(RPC.Message.ID.PlayerCon,
						new RPC.Message.PlayerCon(p), RPC.Channels.reliable, connectedPlayers); // Inform everyone of the new player

					ServerSendMessage(RPC.Message.ID.PlayerList,
						new RPC.Message.PlayerList(connectedPlayers), 
						RPC.Channels.reliable, netMsg.conn); // Let the new player know the current state of the server
				}
			}
			break;
		case (short)RPC.Message.ID.KickReason:
			{
				RPC.Message.KickReason msg = netMsg.ReadMessage<RPC.Message.KickReason>();
				eventLog.AddTaggedEvent(serverTag, "Tried to kick server. Reason: " + msg.reason, true);
			}
			break;

		case (short)RPC.Message.ID.ConsoleBroadcast:
			{
				RPC.Message.ConsoleBroadcast msg = netMsg.ReadMessage<RPC.Message.ConsoleBroadcast>();
				ServerSendMessage(RPC.Message.ID.ConsoleBroadcast, msg, RPC.Channels.reliable, connectedPlayers);
			}
			break;

		case (short)RPC.Message.ID.NameChange:
			{
				RPC.Message.NameChange msg = netMsg.ReadMessage<RPC.Message.NameChange>();
				ServerPlayer p;
				if ( common.existPlayerByID(connectedPlayers, msg.playerID, out p) )
				{
					if ( common.existPlayerByName(connectedPlayers, msg.newName ) ) // Name is already occupied
					{
						ServerSendMessage(RPC.Message.ID.NameChange, new RPC.Message.NameChange(p.id, msg.newName, true), RPC.Channels.reliable, p.connection);
						eventLog.AddTaggedEvent(serverTag, printPlayerName(p) + "'s name change failed, to: " + msg.newName, true);
					}
					else // Acknowledge the name change
					{
						ServerSendMessage(RPC.Message.ID.NameChange, msg, RPC.Channels.reliable, connectedPlayers);
						eventLog.AddTaggedEvent(serverTag, printPlayerName(p) + " is now known as " + msg.newName, true);
					}

					p.name = msg.newName;
				}
			}
			break;

		case (short)RPC.Message.ID.PlayerCon:
			{
				eventLog.AddTaggedEvent(serverTag, "PlayerCon received: " + netMsg.msgType, true);
			}
			break;

		case (short)RPC.Message.ID.PlayerDisc:
			{
				eventLog.AddTaggedEvent(serverTag, "PlayerDisc received: " + netMsg.msgType, true);
			}
			break;

		case (short)RPC.Message.ID.PlayerList:
			{
				// TODO: See this as a request for the player list ?
				eventLog.AddTaggedEvent(serverTag, "PlayerList received: " + netMsg.msgType, true);
			}
			break;

		case (short)RPC.Message.ID.HeartBeat:
			{
				// Do nothing, since this is not interresting for us
			}
			break;

		default:
			eventLog.AddTaggedEvent(serverTag, "Unknown message id received: " + netMsg.msgType, true);
			break;
		}
	}


	// CLIENT SIDE DOD MESSAGE CALLBACK
	void cbClientHandler (NetworkMessage netMsg) 
	{
		switch(netMsg.msgType)
		{
		case (short)RPC.Message.ID.UserLogin:
			{
				RPC.Message.UserLogin msg = netMsg.ReadMessage<RPC.Message.UserLogin>();
				myID = msg.playerID;
				ClientSendMessage(RPC.Message.ID.UserLogin,
					new RPC.Message.UserLogin(myID, networkingInfo.playerName),
					RPC.Channels.reliable);
			}
			break;

		case (short)RPC.Message.ID.KickReason:
			{
				RPC.Message.KickReason msg = netMsg.ReadMessage<RPC.Message.KickReason>();
				eventLog.AddTaggedEvent(clientTag, "Kicked from server. Reason: " + msg.reason, true);
			}
			break;

		case (short)RPC.Message.ID.ConsoleBroadcast:
			{
				RPC.Message.ConsoleBroadcast msg = netMsg.ReadMessage<RPC.Message.ConsoleBroadcast>();
				Player p;
				if ( common.existPlayerByID(allPlayersClient, msg.playerID, out p) )
				{
					eventLog.AddTaggedEvent(chatTag, p.name +  ": " + msg.broadcast, true);
				}
				else if ( msg.playerID == SERVERID )
				{
					eventLog.AddTaggedEvent(chatTag, "SERVER" +  " ~ " + msg.broadcast, true);
				}
			}
			break;

		case (short)RPC.Message.ID.NameChange:
			{
				RPC.Message.NameChange msg = netMsg.ReadMessage<RPC.Message.NameChange>();
				Player p;
				if ( common.existPlayerByID(allPlayersClient, msg.playerID, out p) )
				{
					if( msg.playerID == myID ) // Its me, check if it failed or not
					{
						if ( msg.failed ) // Was not successful
						{
							eventLog.AddTaggedEvent(noTag, "Name change failed. " + msg.newName + " is already occupied.", true);
						}
						else
						{
							eventLog.AddTaggedEvent(noTag, "Your new name is " + msg.newName, true);
							p.name = msg.newName;
						}
					}
					else // Someone else changed their name
					{
						if ( !msg.failed )
						{
							eventLog.AddTaggedEvent(noTag, printPlayerName(p) + " is now known as " + msg.newName, true);
							p.name = msg.newName;
						}
					}
				}
			}
			break;

		case (short)RPC.Message.ID.PlayerCon:
			{
				RPC.Message.PlayerCon msg = netMsg.ReadMessage<RPC.Message.PlayerCon>();
				Player p;
				allPlayersClient.Add(p = new Player(msg.playerID, msg.name));
				if(msg.playerID != myID)
				{
					eventLog.AddTaggedEvent(noTag, "Player connected: " + printPlayerName(p), true);
				}
				else
				{
					eventLog.AddTaggedEvent(noTag, "Connected to " + printServerAdress() + " with ID: " + myID, true);
				}
			}
			break;

		case (short)RPC.Message.ID.PlayerDisc:
			{
				RPC.Message.PlayerDisc msg = netMsg.ReadMessage<RPC.Message.PlayerDisc>();
				Player p;
				if ( common.existPlayerByID(allPlayersClient, msg.playerID, out p) )
				{
					eventLog.AddTaggedEvent(noTag, printPlayerName(p) + " disconnected. Reason: " + msg.reason, true);
					allPlayersClient.Remove(p);
				}
			}
			break;

		case (short)RPC.Message.ID.PlayerList:
			{
				RPC.Message.PlayerList msg = netMsg.ReadMessage<RPC.Message.PlayerList>();
				allPlayersClient = msg.GetArrayAsList();
			}
			break;

		case (short)RPC.Message.ID.HeartBeat:
			{
				// Do nothing, since this is not interresting for us
			}
			break;

		default:
			eventLog.AddTaggedEvent(clientTag, "Unknown message id received: " + netMsg.msgType, true);
			break;
		}
	}

	// ===========================================================================
	// ========================= UTILITY FUNCTIONS =========================
	// ===========================================================================

	void registerAllDodCallbacks ( NetworkConnection nc, NetworkMessageDelegate nmd )
	{
		foreach (RPC.Message.ID MsgId in System.Enum.GetValues(typeof(RPC.Message.ID))) // Register all the callbacks
			nc.RegisterHandler((short)MsgId, nmd);
	}

	void registerAllDodCallbacks ( NetworkClient nc, NetworkMessageDelegate nmd )
	{
		foreach (RPC.Message.ID MsgId in System.Enum.GetValues(typeof(RPC.Message.ID))) // Register all the callbacks
			nc.RegisterHandler((short)MsgId, nmd);
	}

	byte giveUniquePlayerID()
	{
		// TODO: Maybe not having this being random?
		byte newID;
		do
		{
			newID = (byte)UnityEngine.Random.Range(PlayerIdsLowerBound, PlayerIdsUpperBound);
		} while ( common.existPlayerByID( connectedPlayers, newID ) );
		return newID;
	}

	IEnumerator serverDelayBeforeSendingHandshake( byte newID, NetworkConnection nc )
	{
		yield return new WaitForSeconds(0.01f);

		ServerSendMessage(RPC.Message.ID.UserLogin,
			new RPC.Message.UserLogin(newID, ""),
			RPC.Channels.reliable, nc);
		// return null;
	}

	void doHeartBeatChecks()
	{
		if( Time.time > lastHeartBeat + heartBeatTimeOut )
		{
			lastHeartBeat = Time.time;
			if( networkingInfo.isServer )
			{
				if(NetworkServer.active)
				{
					ServerSendMessage(RPC.Message.ID.HeartBeat, new RPC.Message.HeartBeat(), 
						RPC.Channels.update, connectedPlayers);
				}
			}
			else
			{
				if(isConnectedAndAuthenticated())
				{
					ClientSendMessage(RPC.Message.ID.HeartBeat,
						new RPC.Message.HeartBeat(), RPC.Channels.update);
				}
			}
		}
	}

	string printPlayerName(Player p)
	{
		if(networkingInfo.isServer) return p.name + "(" + p.id + ")";
		return p.name;
	}
	string printPlayerName(ServerPlayer p)
	{
		if(networkingInfo.isServer) return p.name + "(" + p.id + ")";
		return p.name;
	}

	string printServerAdress ()
	{
		if ( networkingInfo.address == localServer) return networkingInfo.address;
		return networkingInfo.address + ":" + networkingInfo.port;
	}


	// ===========================================================================
	// ========================= PUBLIC HOOK FUNCTIONS =========================
	// ===========================================================================

	public void SendChatMessage(string s)
	{
		ClientSendMessage(RPC.Message.ID.ConsoleBroadcast,
			new RPC.Message.ConsoleBroadcast(myID, s), RPC.Channels.reliable);
	}

	public bool isConnected()
	{
		return ( myClient != null && myClient.connection != null && myClient.isConnected );
	}
	public bool isAuthenticated()
	{
		return ( myID > 0 );
	}
	public bool isConnectedAndAuthenticated()
	{
		return ( isConnected() && isAuthenticated() );
	}
}