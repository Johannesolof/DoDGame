using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkingTest : MonoBehaviour {

	public bool isAtStartup = true;

	private NetworkClient myClient;
	private PlayerLog eventLog;

	void Start () {
		eventLog = GetComponent<PlayerLog>();
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
			GUI.Label(new Rect(2, 10, 150, 100), "Press S for server");     
			GUI.Label(new Rect(2, 30, 150, 100), "Press B for both");       
			GUI.Label(new Rect(2, 50, 150, 100), "Press C for client");
		}
	}

	// Create a server and listen on a port
	public void SetupServer()
	{
		NetworkServer.Listen(47624);
		NetworkServer.RegisterHandler(MsgType.Connect, OnClient);
		isAtStartup = false;
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
		myClient.RegisterHandler(MsgType.Connect, OnConnected);
		isAtStartup = false;
	}

	// client function
	public void OnConnected(NetworkMessage netMsg)
	{
		eventLog.AddEvent("[Server] A player has connected from " + netMsg.conn.address, true);
	}

	// on client function
	public void OnClient(NetworkMessage netMsg)
	{
		eventLog.AddEvent("[Client] Connected to server " + netMsg.conn.address, true);
	}
}

