using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkingTest : MonoBehaviour {

	public bool isAtStartup = true;

	private NetworkClient myClient;
	private PlayerLog eventLog;
	private int localConnectionID;



	public bool isServer { get; protected set; }

	void Start () {
		eventLog = GetComponent<PlayerLog>();
		isServer = false;
	}

	void Update () 
	{
		if (isAtStartup)
		{
			if (Input.GetKeyDown(KeyCode.S))
			{
				SetupServer();
			}

			if (Input.GetKeyDown(KeyCode.C))
			{
				SetupClient();
			}

			if (Input.GetKeyDown(KeyCode.B))
			{
				SetupServer();
				SetupLocalClient();
			}
		}
	}

	void OnGUI()
	{
		if (isAtStartup)
		{
			GUI.Label(new Rect(10, 10, 200, 100), "Press S for server");     
			GUI.Label(new Rect(10, 30, 200, 100), "Press B for both");       
			GUI.Label(new Rect(10, 50, 200, 100), "Press C for client");
			GUI.Label(new Rect(10, 70, 200, 100), "Press Q to toggle console");
			GUI.Label(new Rect(10, 90, 200, 100), "Press M log sample message");
		}
	}

	// Create a server and listen on a port
	public void SetupServer()
	{
		NetworkServer.Listen(47624);
		NetworkServer.RegisterHandler(MsgType.Connect, OnClient);
		isAtStartup = false;
		isServer = true;
	}

	// Create a client and connect to the server port
	public void SetupClient()
	{
		myClient = new NetworkClient();
		myClient.RegisterHandler(MsgType.Connect, OnConnected);
		myClient.Connect("213.114.70.247", 47624);
		isAtStartup = false;
	}

	// Create a local client and connect to the local server
	public void SetupLocalClient()
	{
		myClient = ClientScene.ConnectLocalServer();
		myClient.RegisterHandler(MsgType.Connect, OnConnectedLocal);
		isAtStartup = false;
	}



	// client function
	public void OnConnected(NetworkMessage netMsg)
	{
		eventLog.AddEvent("[Server] A player has connected from " + netMsg.conn.address, true);
		netMsg.conn.RegisterHandler(JJMsgType.BroadcastStringToConsoleControl, cbBroadcastStringToConsoleControl);
	}

	// client function
	public void OnConnectedLocal(NetworkMessage netMsg)
	{
		eventLog.AddEvent("[Server] A player has connected from " + netMsg.conn.address, true);
		netMsg.conn.RegisterHandler(JJMsgType.BroadcastStringToConsoleControl, cbBroadcastStringToConsoleControl);
		localConnectionID = netMsg.conn.connectionId;
	}

	// on client function
	public void OnClient(NetworkMessage netMsg)
	{
		eventLog.AddEvent("[Client] Connected to server " + netMsg.conn.address, true);
		netMsg.conn.RegisterHandler(JJMsgType.BroadcastStringToConsoleControl, cbBroadcastStringToConsoleControl);
	}

	public void BroadcastStringToConsole(string msg, NetworkConnection origin = null)
	{
		if(isServer && NetworkServer.active)
		{
			Debug.Log("[Server] Message received: " + msg );
			NetworkJJMessage nm = new NetworkJJMessage(msg);
			foreach(NetworkConnection nc in NetworkServer.connections)
			{
				if(nc != origin & nc.connectionId != localConnectionID)
					nc.Send(JJMsgType.BroadcastStringToConsoleControl, nm);
			}
		} 
		else if(!isServer && (myClient != null) && (myClient.connection != null))
		{
			NetworkJJMessage nm = new NetworkJJMessage(msg);
			myClient.connection.Send(JJMsgType.BroadcastStringToConsoleControl, nm);
		}
	}

	public void cbBroadcastStringToConsoleControl(NetworkMessage netMsg)
	{
		NetworkJJMessage rcv = netMsg.ReadMessage<NetworkJJMessage>();
		if(isServer)
		{
			BroadcastStringToConsole(rcv.s, netMsg.conn);
			Debug.Log("[Network] " + rcv.s);
		}
		else
		{
			eventLog.AddEvent("[Network] " + rcv.s);
		}

	}

	class JJMsgType
	{
		public const short NetworkMessageOffset = MsgType.Highest;
		public const short BroadcastStringToConsoleControl = NetworkMessageOffset + 1;
	}

	private class NetworkJJMessage : MessageBase
	{
		public NetworkJJMessage() {}
		public NetworkJJMessage(string s) {this.s = s;}

		public string s;
	}
}