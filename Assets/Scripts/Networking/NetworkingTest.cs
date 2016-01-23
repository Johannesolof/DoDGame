using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

using DodNS;


public class NetworkingTest : MonoBehaviour {
	
	// Public fields
	public bool isServer { get; protected set; }

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
	string myName = "Steve"; // TODO: Make user able to choose their own name..
	NetworkClient myClient;
	PlayerLog eventLog;
	ConnectionConfig config;
	HostTopology hostconfig;
	int port = 47624; // TODO: Make user able to choose the port and ip
	string adress = "213.114.70.247";
	const string localServer = "localServer";
	const string localClient = "localClient";

	List<PlayerConnectionTuple> connectedPlayers;
	List<DodPlayer> allPlayersClient;

	Dictionary<string, DateTime> whiteList;
	Queue<NetworkConnection> pendingConnections;

	TimeSpan whiteListTimeOut = new TimeSpan(6,0,0); // 6 hours for the whitelist to time out
	float lastHeartBeat = 0f; // Last time stamp, for heartbeat checks, in seconds since game start
	float heartBeatTimeOut = 2f; // 2 seconds time out

	bool isAtStartup = true;

	GameController gameController;


	void Start ()
	{
		gameController = GameController.Instance;
		myName = gameController.playerName;
		port = gameController.port;
		adress = gameController.adress;
		isServer = gameController.isServer;

		eventLog = GetComponent<PlayerLog>();
		SetupAllVariables();

		if(isServer) SetupServer();
		SetupClient();
	}

	void SetupAllVariables()
	{
		connectedPlayers = new List<PlayerConnectionTuple>();
		allPlayersClient = new List<DodPlayer>();
		whiteList = new Dictionary<string, DateTime>();
		pendingConnections = new Queue<NetworkConnection>();

		config = new ConnectionConfig();
		DodChannels.priority = config.AddChannel(QosType.AllCostDelivery);
		DodChannels.reliable = config.AddChannel(QosType.ReliableSequenced);
		DodChannels.unreliable = config.AddChannel(QosType.UnreliableSequenced);
		DodChannels.fragmented = config.AddChannel(QosType.ReliableFragmented);
		DodChannels.update = config.AddChannel(QosType.StateUpdate);
		hostconfig = new HostTopology(config, maxConcurrentConnectedUsers);
	}

	void Update () 
	{
		// TODO: Move this stuff to a more appropriate place
		if (isAtStartup)
		{
			if (Input.GetKeyDown(KeyCode.C))
			{
				SetupClient();
			}

			if (Input.GetKeyDown(KeyCode.H))
			{
				SetupServer();
				SetupClient();
			}
		}
		else
		{
			if (Input.GetKeyDown(KeyCode.T))
			{
				ResetClientAndServerAndRestart();
				Start();
			}
//			if (Input.GetKeyDown(KeyCode.N))
//			{
//				ClientSendMessage(DodNet.MsgId.NameChange,
//					new DodNet.NameChange(myID, "JudeJohan"),
//					DodChannels.reliable);
//			}

//			if (Input.GetKeyDown(KeyCode.D))
//			{
//				if ( myClient != null && myClient.isConnected )
//				{
//					eventLog.AddTaggedEvent(serverTag, "Trying to DC.. ", true);
//					myClient.Disconnect();
//					myClient.Shutdown();
//					myClient = null;
////					NetworkServer.DisconnectAll();
////					foreach( NetworkConnection c in NetworkServer.connections )
////					{
////						c.Disconnect();
////					}
//				}
//			}

			// TODO: Move this stuff to a more appropriate place and handle in a more delicate fashion
			if(isServer && pendingConnections.Count > 0)
			{
				NetworkConnection nc = pendingConnections.Dequeue();
				OnClientAccept(nc);
			}
		}

		doHeartBeatChecks();
	}

	void OnGUI()
	{
		//  TODO: Move this stuff to a more appropriate place
		if (isAtStartup)
		{
			GUI.Label(new Rect(10, 30, 200, 100), "Press H for host");
			GUI.Label(new Rect(10, 50, 200, 100), "Press C for client");
			GUI.Label(new Rect(10, 70, 200, 100), "Press Q to toggle console");
			GUI.Label(new Rect(10, 90, 200, 100), "Press M log sample message");
		}
		else
		{
//			GUI.Label(new Rect(10, 30, 200, 100), "Press D to disconnect");
			GUI.Label(new Rect(10, 50, 200, 100), "Press T to terminate networking");
			GUI.Label(new Rect(10, 70, 200, 100), "Press Q to toggle console");
			GUI.Label(new Rect(10, 90, 200, 100), "Press M log sample message");
		}
	}


	// ===========================================================================
	// ========================= SETUP FUNCTIONS =========================
	// ===========================================================================

	// Create a server and listen on a port
	void SetupServer()
	{
		NetworkServer.Configure(hostconfig);
		NetworkServer.Listen(port);
		NetworkServer.RegisterHandler(MsgType.Connect, OnClientConnected);
//		isServer = true;
		eventLog.AddTaggedEvent(serverTag, "Setup complete", true);
		adress = localServer;
	}

	// Create a client and connect to the server port
	void SetupClient()
	{
		if(isServer)
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
			myClient.RegisterHandler(MsgType.Disconnect, ConnectionFailed);
			registerAllDodCallbacks(myClient, cbClientHandler);

			eventLog.AddTaggedEvent(clientTag, "Setup complete", true);

			myClient.Connect(adress, port);
		}


		isAtStartup = false;
	}

	void ResetClientAndServerAndRestart ()
	{
		if(isServer)
		{
			NetworkServer.DisconnectAll();
			NetworkServer.Shutdown();
			NetworkServer.Reset();
		}
		myClient.Shutdown();
		myClient = null;

		isAtStartup = true;
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
		PlayerConnectionTuple p;
		if ( Dod.existPlayerByConnection(connectedPlayers, netMsg.conn, out p) )
		{
			connectedPlayers.Remove( p );

			ServerSendMessage(DodNet.MsgId.PlayerDisc,
				new DodNet.PlayerDisc(p.player.playerID, p.player.dcReason),
				DodChannels.reliable, connectedPlayers);
		}
	}

	// CLIENT SIDE
	void OnConnected(NetworkMessage netMsg)
	{
		myClient.UnregisterHandler(MsgType.Disconnect);
		myClient.RegisterHandler(MsgType.Disconnect, OnDisconnected);
	}
	void ConnectionFailed(NetworkMessage netMsg)
	{
		eventLog.AddTaggedEvent(clientTag, "Unable to connect to server " + netMsg.conn.address, true);
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

	void ServerSendMessage(DodNet.MsgId id, MessageBase msg, byte channel, NetworkConnection conn)
	{
		if(conn != null)
			conn.SendByChannel((short)id, msg, channel);
		else
			eventLog.AddTaggedEvent(serverTag, "Is not connected to client " + conn.address, true);
	}

	void ServerSendMessage(DodNet.MsgId id, MessageBase msg, byte channel, List<NetworkConnection> playerConnectionList)
	{
		foreach(NetworkConnection nc in playerConnectionList)
		{
			nc.SendByChannel((short)id, msg, channel);
		}
	}

	void ServerSendMessage(DodNet.MsgId id, MessageBase msg, byte channel, PlayerConnectionTuple player)
	{
		if(player.connection.isConnected)
			player.connection.SendByChannel((short)id, msg, channel);
		else
			eventLog.AddTaggedEvent(serverTag, "Player is not connected on client " + player.connection.address, true);
	}

	void ServerSendMessage(DodNet.MsgId id, MessageBase msg, byte channel, List<PlayerConnectionTuple> players)
	{
		foreach(PlayerConnectionTuple p in players)
		{
			p.connection.SendByChannel((short)id, msg, channel);
		}
	}


	void ClientSendMessage(DodNet.MsgId id, MessageBase msg, byte channel)
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
		case (short)DodNet.MsgId.UserLogin:  // Last part of user connecting to the server. Check if name is free
			{
				DodNet.UserLogin msg = netMsg.ReadMessage<DodNet.UserLogin>();
				if ( Dod.existPlayerByName(connectedPlayers, msg.name) )
				{
					ServerSendMessage(DodNet.MsgId.KickReason,
						new DodNet.KickReason("Name already taken!"),
						DodChannels.reliable, netMsg.conn);
					netMsg.conn.Disconnect();
				}
				else // New user connected!
				{
					PlayerConnectionTuple p = new PlayerConnectionTuple(msg.playerID, msg.name, netMsg.conn);
					connectedPlayers.Add(p);

					eventLog.AddTaggedEvent(serverTag, "Player connected: " + printPlayerName(p) + " from " + netMsg.conn.address, true);

					ServerSendMessage(DodNet.MsgId.PlayerCon,
						new DodNet.PlayerCon(p.player), DodChannels.reliable, connectedPlayers); // Inform everyone of the new player

					ServerSendMessage(DodNet.MsgId.PlayerList,
						new DodNet.PlayerList(getAllDodPlayers(connectedPlayers)), 
						DodChannels.reliable, netMsg.conn); // Let the new player know the current state of the server
				}
			}
			break;
		case (short)DodNet.MsgId.KickReason:
			{
				DodNet.KickReason msg = netMsg.ReadMessage<DodNet.KickReason>();
				eventLog.AddTaggedEvent(serverTag, "Tried to kick server. Reason: " + msg.reason, true);
			}
			break;

		case (short)DodNet.MsgId.ConsoleBroadcast:
			{
				DodNet.ConsoleBroadcast msg = netMsg.ReadMessage<DodNet.ConsoleBroadcast>();
				ServerSendMessage(DodNet.MsgId.ConsoleBroadcast, msg, DodChannels.reliable, connectedPlayers);
			}
			break;

		case (short)DodNet.MsgId.NameChange:
			{
				DodNet.NameChange msg = netMsg.ReadMessage<DodNet.NameChange>();
				PlayerConnectionTuple p;
				if ( Dod.existPlayerByID(connectedPlayers, msg.playerID, out p) )
				{
					if ( Dod.existPlayerByName(connectedPlayers, msg.newName ) ) // Name is already occupied
					{
						ServerSendMessage(DodNet.MsgId.NameChange, new DodNet.NameChange(p.player.playerID, msg.newName, true), DodChannels.reliable, p.connection);
						eventLog.AddTaggedEvent(serverTag, printPlayerName(p) + "'s name change failed, to: " + msg.newName, true);
					}
					else // Acknowledge the name change
					{
						ServerSendMessage(DodNet.MsgId.NameChange, msg, DodChannels.reliable, connectedPlayers);
						eventLog.AddTaggedEvent(serverTag, printPlayerName(p) + " is now known as " + msg.newName, true);
					}

					p.player.name = msg.newName;
				}
			}
			break;

		case (short)DodNet.MsgId.PlayerCon:
			{
				eventLog.AddTaggedEvent(serverTag, "PlayerCon received: " + netMsg.msgType, true);
			}
			break;

		case (short)DodNet.MsgId.PlayerDisc:
			{
				eventLog.AddTaggedEvent(serverTag, "PlayerDisc received: " + netMsg.msgType, true);
			}
			break;

		case (short)DodNet.MsgId.PlayerList:
			{
				// TODO: See this as a request for the player list ?
				eventLog.AddTaggedEvent(serverTag, "PlayerList received: " + netMsg.msgType, true);
			}
			break;

		case (short)DodNet.MsgId.HeartBeat:
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
		case (short)DodNet.MsgId.UserLogin:
			{
				DodNet.UserLogin msg = netMsg.ReadMessage<DodNet.UserLogin>();
				myID = msg.playerID;
				ClientSendMessage(DodNet.MsgId.UserLogin,
					new DodNet.UserLogin(myID, myName),
					DodChannels.reliable);
			}
			break;

		case (short)DodNet.MsgId.KickReason:
			{
				DodNet.KickReason msg = netMsg.ReadMessage<DodNet.KickReason>();
				eventLog.AddTaggedEvent(clientTag, "Kicked from server. Reason: " + msg.reason, true);
			}
			break;

		case (short)DodNet.MsgId.ConsoleBroadcast:
			{
				DodNet.ConsoleBroadcast msg = netMsg.ReadMessage<DodNet.ConsoleBroadcast>();
				DodPlayer p;
				if ( Dod.existPlayerByID(allPlayersClient, msg.playerID, out p) )
				{
					eventLog.AddTaggedEvent(chatTag, p.name +  ": " + msg.broadcast, true);
				}
				else if ( msg.playerID == SERVERID )
				{
					eventLog.AddTaggedEvent(chatTag, "SERVER" +  " ~ " + msg.broadcast, true);
				}
			}
			break;

		case (short)DodNet.MsgId.NameChange:
			{
				DodNet.NameChange msg = netMsg.ReadMessage<DodNet.NameChange>();
				DodPlayer p;
				if ( Dod.existPlayerByID(allPlayersClient, msg.playerID, out p) )
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

		case (short)DodNet.MsgId.PlayerCon:
			{
				DodNet.PlayerCon msg = netMsg.ReadMessage<DodNet.PlayerCon>();
				DodPlayer p;
				allPlayersClient.Add(p = new DodPlayer(msg.playerID, msg.name));
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

		case (short)DodNet.MsgId.PlayerDisc:
			{
				DodNet.PlayerDisc msg = netMsg.ReadMessage<DodNet.PlayerDisc>();
				DodPlayer p;
				if ( Dod.existPlayerByID(allPlayersClient, msg.playerID, out p) )
				{
					eventLog.AddTaggedEvent(noTag, printPlayerName(p) + " disconnected. Reason: " + msg.reason, true);
					allPlayersClient.Remove(p);
				}
			}
			break;

		case (short)DodNet.MsgId.PlayerList:
			{
				DodNet.PlayerList msg = netMsg.ReadMessage<DodNet.PlayerList>();
				allPlayersClient = msg.GetArrayAsList();
			}
			break;

		case (short)DodNet.MsgId.HeartBeat:
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
		foreach (DodNet.MsgId MsgId in System.Enum.GetValues(typeof(DodNet.MsgId))) // Register all the callbacks
			nc.RegisterHandler((short)MsgId, nmd);
	}

	void registerAllDodCallbacks ( NetworkClient nc, NetworkMessageDelegate nmd )
	{
		foreach (DodNet.MsgId MsgId in System.Enum.GetValues(typeof(DodNet.MsgId))) // Register all the callbacks
			nc.RegisterHandler((short)MsgId, nmd);
	}

	List<DodPlayer> getAllDodPlayers(List<PlayerConnectionTuple> L)
	{
		List<DodPlayer> newList = new List<DodPlayer>();
		foreach(PlayerConnectionTuple pct in connectedPlayers)
		{
			newList.Add(pct.player);
		}
		return newList;
	}

	byte giveUniquePlayerID()
	{
		// TODO: Maybe not having this being random?
		byte newID;
		do
		{
			newID = (byte)UnityEngine.Random.Range(PlayerIdsLowerBound, PlayerIdsUpperBound);
		} while ( Dod.existPlayerByID( connectedPlayers, newID ) );
		return newID;
	}

	IEnumerator serverDelayBeforeSendingHandshake( byte newID, NetworkConnection nc )
	{
		yield return new WaitForSeconds(0.01f);

		ServerSendMessage(DodNet.MsgId.UserLogin,
			new DodNet.UserLogin(newID, ""),
			DodChannels.reliable, nc);
		// return null;
	}

	void doHeartBeatChecks()
	{
		if( Time.time > lastHeartBeat + heartBeatTimeOut )
		{
			lastHeartBeat = Time.time;
			if( isServer )
			{
				if(NetworkServer.active)
				{
					ServerSendMessage(DodNet.MsgId.HeartBeat, new DodNet.HeartBeat(), DodChannels.update, connectedPlayers);
				}
			}
			else
			{
				if(isConnected())
				{
					ClientSendMessage(DodNet.MsgId.HeartBeat,
						new DodNet.HeartBeat(), DodChannels.update);
				}
			}
		}
	}

	string printPlayerName(DodPlayer p)
	{
		if(isServer) return p.name + "(" + p.playerID + ")";
		return p.name;
	}
	string printPlayerName(PlayerConnectionTuple p)
	{
		if(isServer) return p.player.name + "(" + p.player.playerID + ")";
		return p.player.name;
	}

	string printServerAdress ()
	{
		if ( adress == localServer) return adress;
		return adress + ":" + port;
	}


	// ===========================================================================
	// ========================= PUBLIC HOOK FUNCTIONS =========================
	// ===========================================================================

	public void SendChatMessage(string s)
	{
		ClientSendMessage(DodNet.MsgId.ConsoleBroadcast,
			new DodNet.ConsoleBroadcast(myID, s), DodChannels.reliable);
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

	// ===========================================================================
	// ========================= UTILITY CLASSES =========================
	// ===========================================================================

	class DodNet
	{
		public enum MsgId 
		{
			UserLogin = JJMSG_ID_OFFSET,
			KickReason,
			ConsoleBroadcast,
			NameChange,
			PlayerCon,
			PlayerDisc,
			PlayerList,
			HeartBeat,
		}

		public class UserLogin : MessageBase
		{
			// From Server to Client:
			//  • Second-to-last part of handshake; Tell the user what his PlayerID is
			//  • 
			// From Client to Server:
			//  • Last part of handshake; Tell server what the user's name is
			//  • 
			public UserLogin() {}
			public UserLogin(byte id, string Name) { playerID = id; name = Name; }

			public byte playerID;
			public string name;
		}

		public class KickReason : MessageBase
		{
			// From Server to Client:
			//  • Kick a user and tell them the reason
			//  • 
			// From Client to Server:
			//  • N/A
			//  • 
			public KickReason() {}
			public KickReason(string Reason) { reason = Reason; }

			public string reason;
		}

		public class ConsoleBroadcast : MessageBase
		{
			// From Server to Client:
			//  • Relaying chat messages
			//  • Broadcasting any useful information to users' consoles
			//  •
			// From Client to Server:
			//  • Chat messages
			//  • 
			public ConsoleBroadcast() {}
			public ConsoleBroadcast(byte id, string bc) { playerID = id; broadcast = bc; }

			public byte playerID;
			public string broadcast;
		}

		public class NameChange : MessageBase
		{
			// From Server to Client:
			//  • Tell the user that the name change failed, since the name is already occupied
			//  • Tell the receiver that this user has changed his name
			//  • 
			// From Client to Server:
			//  • Inform the server that the user wants to change his name
			//  • 
			public NameChange() {}
			public NameChange(byte id, string Name, bool Failed = false) { playerID = id; newName = Name; failed = Failed; }

			public byte playerID;
			public string newName;
			public bool failed;
		}

		public class PlayerCon : MessageBase
		{
			// From Server to Client:
			//  • Tell the receiver that this user has connected
			//  • 
			// From Client to Server:
			//  • N/A
			//  • 
			public PlayerCon() {}
			public PlayerCon(DodPlayer p) { playerID = p.playerID; name = p.name; }

			public byte playerID;
			public string name;
		}

		public class PlayerDisc : MessageBase
		{
			// From Server to Client:
			//  • Tell the receiver that this user has disconnected
			//  • 
			// From Client to Server:
			//  • N/A
			//  • 
			public PlayerDisc() {}
			public PlayerDisc(byte id, string Reason) { playerID = id; reason = Reason; }

			public byte playerID;
			public string reason;
		}

		public class PlayerList : MessageBase
		{
			// From Server to Client:
			//  • Relay the list of currently connected users to the receiver
			//  • 
			// From Client to Server:
			//  • N/A
			//  • TODO: Request the list of currently connected users ? 
			//    (This should never be required, since all player list updates are sent when a player connects or disconnects, over the reliable channel)
			//  • 
			public PlayerList() {}
			public PlayerList(List<DodPlayer> List) { array = List.ToArray(); }

			public DodPlayer[] array;


			public List<DodPlayer> GetArrayAsList ()
			{
				List<DodPlayer> L = new List<DodPlayer>();
				foreach(DodPlayer p in array)
				{
					L.Add(p);
				}
				return L;
			}
		}

		public class HeartBeat : MessageBase
		{
			// From Server to Client:
			//  • Probe the connection (Right now, these message are not handled at the receiver)
			//  • 
			// From Client to Server:
			//  • Probe the connection (Right now, these message are not handled at the receiver)
			//  • 
			public HeartBeat() {}
		}
	}
}