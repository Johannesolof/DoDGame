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
	const short JJMSG_ID_OFFSET = short.MaxValue/2;
	const int maxConcurrentConnectedUsers = 10;
	const int SERVERID = 0;
	byte myID = 0;
	// TODO: Make user able to choose their own name..
	string myName = "Steve";
	NetworkClient myClient;
	PlayerLog eventLog;
	ConnectionConfig config;
	HostTopology hostconfig;
	// TODO: Make user able to choose the port and ip
	int port = 47624;
	string adress = "213.114.70.200"; // "213.114.70.247";

	List<Player> connectedPlayers;
	Dictionary<string, DateTime> whiteList;
	Queue<NetworkConnection> pendingConnections;

	TimeSpan whiteListTimeOut = new TimeSpan(6,0,0); // 6 hours for the whitelist to time out

	bool isAtStartup = true;


	void Start () {
		eventLog = GetComponent<PlayerLog>();
		isServer = false;

		connectedPlayers = new List<Player>();
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

		// TODO: Move this stuff to a more appropriate place and handle in a more delicate fashion
		if(pendingConnections.Count > 0)
		{
			NetworkConnection nc = pendingConnections.Dequeue();
			OnClientAccept(nc);
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
		}
		else
		{
			myClient = new NetworkClient();
			myClient.Configure(hostconfig);
			myClient.RegisterHandler(MsgType.Connect, OnConnected);
			// TODO: ConnectionFailed does not seem to work?
			myClient.RegisterHandler(MsgType.Disconnect, ConnectionFailed);
			myClient.Connect(adress, port);
		}


		isAtStartup = false;
	}


	// ===========================================================================
	// ========================= NETWORKING CALLBACK FUNCTIONS =========================
	// ===========================================================================

	// SERVER SIDE
	void OnClientConnected(NetworkMessage netMsg)
	{
		eventLog.AddTaggedEvent(serverTag, "A peer has connected from " + netMsg.conn.address, true);
		netMsg.conn.RegisterHandler(MsgType.Disconnect, OnClientDisconnected);

		if ( whiteList.ContainsKey( netMsg.conn.address ) )
		{
			if ( whiteList[netMsg.conn.address] + whiteListTimeOut > DateTime.Now )
			{
				OnClientAccept(netMsg.conn);
				return;
			}
			whiteList.Remove(netMsg.conn.address);
		}
		pendingConnections.Enqueue(netMsg.conn);
	}

	void OnClientAccept(NetworkConnection nc)
	{
		if ( !whiteList.ContainsKey( nc.address ) ) // White list him for default amount of time, if he is not already white listed
		{
			whiteList.Add(nc.address, DateTime.Now);
		}

		foreach (DodNet.MsgId MsgId in System.Enum.GetValues(typeof(DodNet.MsgId))) // Register all the callbacks
			nc.RegisterHandler((short)MsgId, cbServerHandler);

		byte newID = 0;
		int searchResult = 0;
		do
		{
			newID = (byte)UnityEngine.Random.Range(1, byte.MaxValue);
			searchResult = connectedPlayers.FindIndex(p => p.playerID == newID);
		} while ( searchResult != -1 );

		ServerSendMessage(DodNet.MsgId.UserLogin,
			new DodNet.UserLogin(newID, ""),
			DodChannels.reliable, nc);
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
		if( connectedPlayers.FindIndex(p => p.connection.Equals(netMsg.conn)) != -1 )
		{
			Player P = connectedPlayers.Find(p => p.connection.Equals(netMsg.conn));
			connectedPlayers.Remove( P );

			ServerSendMessage(DodNet.MsgId.PlayerDisc,
				new DodNet.PlayerDisc(P.playerID, P.dcReason),
				DodChannels.reliable, connectedPlayers);
		}
	}

	// CLIENT SIDE
	void OnConnected(NetworkMessage netMsg)
	{
		eventLog.AddTaggedEvent(clientTag, "Connected to server " + netMsg.conn.address, true);
		netMsg.conn.RegisterHandler(MsgType.Disconnect, OnDisconnected);

		foreach (DodNet.MsgId MsgId in System.Enum.GetValues(typeof(DodNet.MsgId))) // Register all the callbacks
			netMsg.conn.RegisterHandler((short)MsgId, cbClientHandler);
	}
	void ConnectionFailed(NetworkMessage netMsg)
	{
		eventLog.AddTaggedEvent(clientTag, "Unable to connect to server " + netMsg.conn.address, true);
	}
	void OnDisconnected(NetworkMessage netMsg)
	{
		eventLog.AddTaggedEvent(clientTag, "Disconnected from server " + adress + ":" + port, true);
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
	// ========================= UTILITY FUNCTIONS =========================
	// ===========================================================================

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
				if ( connectedPlayers.FindIndex(p => p.name == msg.name) != -1)
				{
					ServerSendMessage(DodNet.MsgId.KickReason,
						new DodNet.KickReason("Name already taken!"),
						DodChannels.reliable, netMsg.conn);
					netMsg.conn.Disconnect();
				}
				else // New user connected!
				{
					Player P = new Player(msg.playerID, msg.name, netMsg.conn);
					connectedPlayers.Add(P);

					ServerSendMessage(DodNet.MsgId.PlayerCon,
						new DodNet.PlayerCon(P), DodChannels.reliable, connectedPlayers);
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

			}
			break;

		case (short)DodNet.MsgId.PlayerCon:
			{

			}
			break;

		case (short)DodNet.MsgId.PlayerDisc:
			{

			}
			break;

		default:
			eventLog.AddTaggedEvent(serverTag, "Unknown message id received: " + netMsg.msgType, true);
			break;
		}
	}

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
				Player P = connectedPlayers.Find(p => p.playerID == msg.playerID);
				eventLog.AddTaggedEvent(clientTag, P.name +  ": " + msg.broadcast, true);
			}
			break;

		case (short)DodNet.MsgId.NameChange:
			{
				
			}
			break;

		case (short)DodNet.MsgId.PlayerCon:
			{
				DodNet.PlayerCon msg = netMsg.ReadMessage<DodNet.PlayerCon>();
				Player P = new Player(msg.playerID, msg.name, null);
//				connectedPlayers.Add(P);
				eventLog.AddTaggedEvent(clientTag, "Player connected: " + P.name, true);
			}
			break;

		case (short)DodNet.MsgId.PlayerDisc:
			{
				DodNet.PlayerDisc msg = netMsg.ReadMessage<DodNet.PlayerDisc>();
				Player P = connectedPlayers.Find(p => p.playerID == msg.playerID);
				connectedPlayers.Remove(P);
				eventLog.AddTaggedEvent(clientTag, "Player disconnected: " + P.name, true);
			}
			break;

		default:
			eventLog.AddTaggedEvent(clientTag, "Unknown message id received: " + netMsg.msgType, true);
			break;
		}
	}

//	public void BroadcastStringToConsole(string s, string playerName = "Stig")
//	{
//		if (!isConnected())
//		{
//			Debug.Log("Client is not active to send: " + s );
//			return;
//		}
//
////		Debug.Log("[Client] Sending message: " + s );
//		JJNetMsgConsoleBroadcast msg = new JJNetMsgConsoleBroadcast(myID, s);
//		myClient.connection.SendByChannel((short)JJNetMsgId.ConsoleBroadcast, msg, JJChannels.reliable);
//	}

	// SERVER SIDE
//	void cbNameChangeServer (NetworkMessage netMsg) 
//	{
//		JJNetMsgNameChange msg = netMsg.ReadMessage<JJNetMsgNameChange>();
//
//		if(!dictionary_PlayerID_Player.ContainsKey(msg.playerID))
//		{
//			// Kick the player because this is not correct
//			netMsg.conn.Disconnect();
//			eventLog.AddTaggedEvent(serverTag, "Kicked connection from " + netMsg.conn.address + ". PlayerID does not exist.", true);
//			return;
//		}
//
//		Player p = dictionary_PlayerID_Player[msg.playerID];
//		p.name = msg.playerName;
//		dictionary_PlayerID_Player[msg.playerID] = p;
//	}

//	void cbJJMessageReceivedFromClient (NetworkMessage netMsg)
//	{
//		JJNetMsg msg = netMsg.ReadMessage<JJNetMsg>();
//
//		switch(msg.msgId)
//		{
//
//		case JJNetMsgId.ConsoleBroadcast:
////			Debug.Log("[Server] Received chat broadcast from " + netMsg.conn.address + ": " + msg.s);
//			ServerBroadcastString(msg);
//			break;
//
//		default:
//			eventLog.AddTaggedEvent(serverTag, "Unknown message id received: " + msg.msgId, true);
//			break;
//		}
//	}

//	void ServerSendString(JJNetMsgId reason, string s)
//	{
//		JJNetMsg msg = new JJNetMsg(reason, s);
//
//		foreach(NetworkConnection nc in NetworkServer.connections)
//		{
//			nc.SendByChannel(JJMSGID, msg, JJChannels.reliable);
//		}
//	}
//
//	void ServerSendString(JJNetMsg msg)
//	{
//		foreach(NetworkConnection nc in NetworkServer.connections)
//		{
//			nc.SendByChannel(JJMSGID, msg, JJChannels.reliable);
//		}
//	}



//	List<NetworkConnection> AllPlayers ()
//	{
//		List<NetworkConnection> L = new List<NetworkConnection>();
//		L.Find()
//		foreach(NetworkConnection nc in dictionary_Connection_PlayerID.Keys)
//		{
//			L.Add(nc);
//		}
//		return L;
//	}
//
//	// CLIENT SIDE
//	void cbIDExchangeClient(NetworkMessage netMsg)
//	{
//		
//	}
//	void cbJJMessageReceivedFromServer (NetworkMessage netMsg)
//	{
//		JJNetMsg msg = netMsg.ReadMessage<JJNetMsg>();
//
//		switch(msg.msgId)
//		{
//
//		case JJNetMsgId.ConsoleBroadcast:
//			eventLog.AddTaggedEvent(clientTag, msg.s, true);
//			break;
//
//		default:
//			eventLog.AddTaggedEvent(clientTag, "Unknown message id received: " + msg.msgId, true);
//			break;
//		}
//	}

//	void RegisterOnServer(string phrase, string playerName)
//	{
//		JJNetMsgUserRegister msg = new JJNetMsgUserRegister(phrase, myID, playerName);
//		ClientSendMessage(JJNetMsgId.UserRegister, msg, JJChannels.reliable);
//	}
//
//	void ChangePlayerName(string PlayerName) 
//	{
//		JJNetMsgNameChange msg = new JJNetMsgNameChange(myID, PlayerName);
//		ClientSendMessage(JJNetMsgId.NameChange, msg, JJChannels.reliable);
//	}




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

	class DodChannels
	{
		static public byte priority; // For important, reliable one shot events
		static public byte reliable; // For important events, such as player action
		static public byte unreliable; // For slow events, such as camera stream
		static public byte fragmented; // For large events, such as file transfer
		static public byte update; // For spammed events, such as object movement
	}

	class Player
	{
		public Player(byte id, string Name, NetworkConnection conn) { playerID = id; name = Name; connection = conn; }

		public byte playerID;
		public string name;
		public NetworkConnection connection;

		public string dcReason = "Unknown reason";
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
			public NameChange(byte id, string Name) { playerID = id; name = Name; }

			public byte playerID;
			public string name;
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