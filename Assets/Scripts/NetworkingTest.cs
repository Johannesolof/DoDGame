using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

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

	List<Player> connectedPlayers, allPlayersClient;
	Dictionary<string, DateTime> whiteList;
	Queue<NetworkConnection> pendingConnections;
	Queue<Action<Delegate>> nextUpdateAction;

	TimeSpan whiteListTimeOut = new TimeSpan(6,0,0); // 6 hours for the whitelist to time out

	bool isAtStartup = true;


	void Start () 
	{
		eventLog = GetComponent<PlayerLog>();
		isServer = false;
		SetupAllVariables();
	}

	void SetupAllVariables()
	{
		connectedPlayers = new List<Player>();
		allPlayersClient = new List<Player>();
		whiteList = new Dictionary<string, DateTime>();
		pendingConnections = new Queue<NetworkConnection>();
		nextUpdateAction = new Queue<Action<Delegate>>();

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
			if (Input.GetKeyDown(KeyCode.N))
			{
				ClientSendMessage(DodNet.MsgId.NameChange,
					new DodNet.NameChange(myID, "JudeJohan"),
					DodChannels.reliable);
			}

//			if (Input.GetKeyDown(KeyCode.D))
//			{
//				if ( myClient != null && myClient.isConnected )
//				{
//					eventLog.AddTaggedEvent(serverTag, "Trying to DC.. ", true);
//					myClient.Disconnect();
//					myClient.Shutdown();
//					myClient = null;
//					NetworkServer.DisconnectAll();
//					foreach( NetworkConnection c in NetworkServer.connections )
//					{
//						c.Disconnect();
//					}
//				}
//			}

			// TODO: Move this stuff to a more appropriate place and handle in a more delicate fashion
			if(isServer && pendingConnections.Count > 0)
			{
				NetworkConnection nc = pendingConnections.Dequeue();
				OnClientAccept(nc);
			}
		}
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
		isServer = true;
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
		Player p;
		if ( existPlayerByConnection(connectedPlayers, netMsg.conn, out p) )
		{
			connectedPlayers.Remove( p );

			ServerSendMessage(DodNet.MsgId.PlayerDisc,
				new DodNet.PlayerDisc(p.playerID, p.dcReason),
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

	void ServerSendMessage(DodNet.MsgId id, MessageBase msg, byte channel, Player player)
	{
		if(player.connection.isConnected)
			player.connection.SendByChannel((short)id, msg, channel);
		else
			eventLog.AddTaggedEvent(serverTag, "Player is not connected on client " + player.connection.address, true);
	}

	void ServerSendMessage(DodNet.MsgId id, MessageBase msg, byte channel, List<Player> players)
	{
		foreach(Player p in players)
		{
			p.connection.SendByChannel((short)id, msg, channel);
			eventLog.AddTaggedEvent(serverTag, "Sending to player conn(id) " + p.connection.address + "(" + p.playerID + ")", true);
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
				if ( existPlayerByName(connectedPlayers, msg.name) )
				{
					ServerSendMessage(DodNet.MsgId.KickReason,
						new DodNet.KickReason("Name already taken!"),
						DodChannels.reliable, netMsg.conn);
					netMsg.conn.Disconnect();
				}
				else // New user connected!
				{
					Player p = new Player(msg.playerID, msg.name, netMsg.conn);
					connectedPlayers.Add(p);

					eventLog.AddTaggedEvent(serverTag, "Player connected: " + printPlayerName(p) + " from " + netMsg.conn.address, true);

					ServerSendMessage(DodNet.MsgId.PlayerCon,
						new DodNet.PlayerCon(p), DodChannels.reliable, connectedPlayers);
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
				Player p;
				if ( existPlayerByID(connectedPlayers, msg.playerID, out p) )
				{
					if ( existPlayerByName(connectedPlayers, msg.name ) ) // Name is already occupied
					{
						ServerSendMessage(DodNet.MsgId.NameChange, new DodNet.NameChange(p.playerID, msg.name, true), DodChannels.reliable, p.connection);
						eventLog.AddTaggedEvent(serverTag, printPlayerName(p) + "'s  name change failed, to: " + msg.name, true);
					}
					else // Acknowledge the name change
					{
						ServerSendMessage(DodNet.MsgId.NameChange, msg, DodChannels.reliable, connectedPlayers);
						eventLog.AddTaggedEvent(serverTag, printPlayerName(p) + " is now known as " + msg.name, true);
					}

					p.name = msg.name;
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
				Player p;
				if ( existPlayerByID(allPlayersClient, msg.playerID, out p) )
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
				Player p;
				if ( existPlayerByID(allPlayersClient, msg.playerID, out p) )
				{
					if( msg.playerID == myID ) // Its me, check if it failed or not
					{
						if ( msg.failed ) // Was not successful
						{
							eventLog.AddTaggedEvent(noTag, "Name change failed. " + msg.name + " is already occupied.", true);
						}
						else
						{
							eventLog.AddTaggedEvent(noTag, "Your new name is " + msg.name, true);
						}
					}
					else // Someone else changed their name
					{
						eventLog.AddTaggedEvent(noTag, printPlayerName(p) + " is now known as " + msg.name, true);
					}

					p.name = msg.name;
				}
			}
			break;

		case (short)DodNet.MsgId.PlayerCon:
			{
				DodNet.PlayerCon msg = netMsg.ReadMessage<DodNet.PlayerCon>();
				Player p;
				allPlayersClient.Add(p = new Player(msg.playerID, msg.name, null));
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
				Player p;
				if ( existPlayerByID(allPlayersClient, msg.playerID, out p) )
				{
					eventLog.AddTaggedEvent(noTag, printPlayerName(p) + " disconnected. Reason: " + msg.reason, true);
					allPlayersClient.Remove(p);
				}
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

	byte giveUniquePlayerID()
	{
		byte newID;
		do
		{
			newID = (byte)UnityEngine.Random.Range(PlayerIdsLowerBound, PlayerIdsUpperBound);
		} while ( existPlayerByID( connectedPlayers, newID ) );
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

	string printPlayerName(Player p)
	{
		if(isServer) return p.name + "(" + p.playerID + ")";
		return p.name;
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

	public bool existPlayerByID (List<Player> L, byte query, out Player player)
	{
		Predicate<Player> pr = p => p.playerID == query;

		int i = L.FindIndex(pr);
		if (i == -1) {
			player = null;
			return false;
		}
		player = L.Find(pr);
		return true;
	}
	public bool existPlayerByID (List<Player> L, byte query)
	{
		Player p = new Player();
		return existPlayerByID(L, query, out p);
	}

	public bool existPlayerByName (List<Player> L, string query, out Player player)
	{
		Predicate<Player> pr = p => p.name == query;

		int i = L.FindIndex(pr);
		if (i == -1) {
			player = null;
			return false;
		}
		player = L.Find(pr);
		return true;
	}
	public bool existPlayerByName (List<Player> L, string query)
	{
		Player p = new Player();
		return existPlayerByName(L, query, out p);
	}

	public bool existPlayerByConnection (List<Player> L, NetworkConnection query, out Player player)
	{
		Predicate<Player> pr = p => p.connection == query;

		int i = L.FindIndex(pr);
		if (i == -1) {
			player = null;
			return false;
		}
		player = L.Find(pr);
		return true;
	}
	public bool existPlayerByConnection (List<Player> L, NetworkConnection query)
	{
		Player p = new Player();
		return existPlayerByConnection(L, query, out p);
	}


	// ===========================================================================
	// ========================= UTILITY CLASSES =========================
	// ===========================================================================

	class DodChannels
	{
		static public byte priority; // For important, reliable one shot events
		static public byte reliable; // For important events, such as player action
		static public byte unreliable; // For slow events, such as camera stream
		static public byte fragmented; // For large events, such as file transfer
		static public byte update; // For spammed events, such as object movement
	}

	public class Player
	{
		public Player() {}
		public Player(byte id, string Name, NetworkConnection conn) { playerID = id; name = Name; connection = conn; }

		public byte playerID;
		public string name;
		public NetworkConnection connection;
		public string dcReason = "Connection closed";
	}

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
		}

		public class UserLogin : MessageBase
		{
			public UserLogin() {}
			public UserLogin(byte id, string Name) { playerID = id; name = Name; }

			public byte playerID;
			public string name;
		}

		public class KickReason : MessageBase
		{
			public KickReason() {}
			public KickReason(string Reason) { reason = Reason; }

			public string reason;
		}

		public class ConsoleBroadcast : MessageBase
		{
			public ConsoleBroadcast() {}
			public ConsoleBroadcast(byte id, string bc) { playerID = id; broadcast = bc; }

			public byte playerID;
			public string broadcast;
		}

		public class NameChange : MessageBase
		{
			public NameChange() {}
			public NameChange(byte id, string Name, bool Failed = false) { playerID = id; name = Name; failed = Failed; }

			public byte playerID;
			public string name;
			public bool failed;
		}

		public class PlayerCon : MessageBase
		{
			public PlayerCon() {}
			public PlayerCon(Player p) { playerID = p.playerID; name = p.name; }

			public byte playerID;
			public string name;
		}

		public class PlayerDisc : MessageBase
		{
			public PlayerDisc() {}
			public PlayerDisc(byte id, string Reason) { playerID = id; reason = Reason; }

			public byte playerID;
			public string reason;
		}
	}
}