using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

namespace RPC
{
	public class Client 
	{
		// CONSTANTS
		const int maxConcurrentConnectedUsers = 10;

		// PRIVATES
		byte myID = 0;
		NetworkClient myClient;
		ConnectionConfig config;
		HostTopology hostconfig;

		NetworkingInfo networkingInfo;
		NetworkingCommon common = NetworkingCommon.Instance;
		string tag = NetworkingCommon.CLIENTTAG;

		List<Player> connectedPlayers;
		float lastHeartBeat = 0f; // Last time stamp, for heartbeat checks, in seconds since game start
		float heartBeatTimeOut = 2f; // 2 seconds time out

		Action onShutdown;

		public void RegisterClientShutdown(Action a)
		{
			onShutdown += a;
		}
		public void UnregisterClientShutdown(Action a)
		{
			onShutdown -= a;
		}
		void OnShutdown()
		{
			if (onShutdown != null)
			{
				onShutdown();
			}
		}

		// CONSTRUCTOR
		public Client(NetworkingInfo netinfo)
		{
			networkingInfo = netinfo;
			if(networkingInfo.port == -1) networkingInfo.port = NetworkingInfo.defaultPort;
			if(networkingInfo.address == "") networkingInfo.address = NetworkingInfo.defaultAddress;
			// TODO: Add some kind of DNS pre check for IP?

			connectedPlayers = new List<Player>();

			config = new ConnectionConfig();
			Channels.priority = config.AddChannel(QosType.AllCostDelivery);
			Channels.reliable = config.AddChannel(QosType.ReliableSequenced);
			Channels.unreliable = config.AddChannel(QosType.UnreliableSequenced);
			Channels.fragmented = config.AddChannel(QosType.ReliableFragmented);
			Channels.update = config.AddChannel(QosType.StateUpdate);
			hostconfig = new HostTopology(config, maxConcurrentConnectedUsers);

			if(networkingInfo.isServer)
			{
				myClient = ClientScene.ConnectLocalServer();
				myClient.Configure(hostconfig);

				myClient.RegisterHandler(MsgType.Connect, OnConnected);
				myClient.RegisterHandler(MsgType.Disconnect, ConnectionFailed);
				registerAllCallbacks(myClient, cbClientHandler);
			}
			else
			{
				myClient = new NetworkClient();
				myClient.Configure(hostconfig);

				myClient.RegisterHandler(MsgType.Connect, OnConnected);
				myClient.RegisterHandler(MsgType.Error, ConnectionError);
				myClient.RegisterHandler(MsgType.Disconnect, ConnectionFailed);
				registerAllCallbacks(myClient, cbClientHandler);

				common.printConsole (tag, "Setup complete", true);

				myClient.Connect(networkingInfo.address, networkingInfo.port);
				common.printConsole (tag, "Connecting to " + networkingInfo.address + ":" + networkingInfo.port, true);
			}
		}

		// DESTRUCTOR
		~Client()
		{
			if(myClient != null)
			{
				myClient.Disconnect();
				myClient.Shutdown();
				myClient = null;
			}
		}

		public void Tick (float ackumulativeTimeSinceStart)
		{
			if( ackumulativeTimeSinceStart > lastHeartBeat + heartBeatTimeOut )
			{
				lastHeartBeat = ackumulativeTimeSinceStart;
				// TODO: Check this muditrucker
				if(isConnectedAndAuthenticated())
				{
					ClientSendMessage(Message.ID.HeartBeat,
						new Message.HeartBeat(), Channels.update);
				}
			}
		}


		// ===========================================================================
		// ========================= NETWORKING CALLBACK FUNCTIONS =========================
		// ===========================================================================

		void OnConnected(NetworkMessage netMsg)
		{
			myClient.UnregisterHandler(MsgType.Disconnect);
			myClient.RegisterHandler(MsgType.Disconnect, OnDisconnected);
		}
		void ConnectionError(NetworkMessage netMsg)
		{
			common.printConsole(tag, "Unable to connect to server " + networkingInfo.address + ":" + networkingInfo.port + ". Please check the adress and port and try again.", true);
			OnShutdown();
		}
		void ConnectionFailed(NetworkMessage netMsg)
		{
			common.printConsole(tag, "Unable to connect to server, connection timed out: " + netMsg.conn.address + ":" + networkingInfo.port, true);
			OnShutdown();
		}
		void OnDisconnected(NetworkMessage netMsg)
		{
			common.printConsole(tag, "Disconnected from server " + printServerAdress(), true);
			OnShutdown();
		}


		// ===========================================================================
		// ========================= SEND FUNCTIONS =========================
		// ===========================================================================

		void ClientSendMessage(Message.ID id, MessageBase msg, byte channel)
		{
			if(isConnectedAndAuthenticated())
				myClient.connection.SendByChannel((short)id, msg, channel);
			else
				common.printConsole(tag, "Is not connected to any server", true);
		}


		// ===========================================================================
		// ========================= DOD CALLBACK FUNCTIONS =========================
		// ===========================================================================

		void cbClientHandler (NetworkMessage netMsg) 
		{
			switch(netMsg.msgType)
			{
			case (short)Message.ID.UserLogin:
				{
					Message.UserLogin msg = netMsg.ReadMessage<Message.UserLogin>();
					myID = msg.playerID;
					ClientSendMessage(Message.ID.UserLogin,
						new Message.UserLogin(myID, networkingInfo.playerName),
						Channels.reliable);
				}
				break;

			case (short)Message.ID.KickReason:
				{
					Message.KickReason msg = netMsg.ReadMessage<Message.KickReason>();
					common.printConsole(tag, "Kicked from server. Reason: " + msg.reason, true);
				}
				break;

			case (short)Message.ID.ConsoleBroadcast:
				{
					Message.ConsoleBroadcast msg = netMsg.ReadMessage<Message.ConsoleBroadcast>();
					Player p;
					if ( common.existPlayerByID(connectedPlayers, msg.playerID, out p) )
					{
						common.printConsole(NetworkingCommon.CHATTAG, p.name +  ": " + msg.broadcast, true);
					}
					else if ( msg.playerID == NetworkingCommon.SERVERID )
					{
						common.printConsole(NetworkingCommon.CHATTAG, "SERVER" +  " ~ " + msg.broadcast, true);
					}
				}
				break;

			case (short)Message.ID.NameChange:
				{
					Message.NameChange msg = netMsg.ReadMessage<Message.NameChange>();
					Player p;
					if ( common.existPlayerByID(connectedPlayers, msg.playerID, out p) )
					{
						if( msg.playerID == myID ) // Its me, check if it failed or not
						{
							if ( msg.failed ) // Was not successful
							{
								common.printConsole(NetworkingCommon.NOTAG, "Name change failed. " + msg.newName + " is already occupied.", true);
							}
							else
							{
								common.printConsole(NetworkingCommon.NOTAG, "Your new name is " + msg.newName, true);
								p.name = msg.newName;
							}
						}
						else // Someone else changed their name
						{
							if ( !msg.failed )
							{
								common.printConsole(NetworkingCommon.NOTAG, printPlayerName(p) + " is now known as " + msg.newName, true);
								p.name = msg.newName;
							}
						}
					}
				}
				break;

			case (short)Message.ID.PlayerCon:
				{
					Message.PlayerCon msg = netMsg.ReadMessage<Message.PlayerCon>();
					Player p;
					connectedPlayers.Add(p = new Player(msg.playerID, msg.name));
					if(msg.playerID != myID)
					{
						common.printConsole(NetworkingCommon.NOTAG, "Player connected: " + printPlayerName(p), true);
					}
					else
					{
						common.printConsole(NetworkingCommon.NOTAG, "Connected to " + printServerAdress() + " with ID: " + myID, true);
					}
				}
				break;

			case (short)Message.ID.PlayerDisc:
				{
					Message.PlayerDisc msg = netMsg.ReadMessage<Message.PlayerDisc>();
					Player p;
					if ( common.existPlayerByID(connectedPlayers, msg.playerID, out p) )
					{
						common.printConsole(NetworkingCommon.NOTAG, printPlayerName(p) + " disconnected. Reason: " + msg.reason, true);
						connectedPlayers.Remove(p);
					}
				}
				break;

			case (short)Message.ID.PlayerList:
				{
					Message.PlayerList msg = netMsg.ReadMessage<Message.PlayerList>();
					connectedPlayers = msg.GetArrayAsList();
				}
				break;

			case (short)Message.ID.HeartBeat:
				{
					// Do nothing, since this is not interresting for us
				}
				break;

			default:
				common.printConsole(tag, "Unknown message id received: " + netMsg.msgType, true);
				break;
			}
		}


		// ===========================================================================
		// ========================= UTILITY FUNCTIONS =========================
		// ===========================================================================

		void registerAllCallbacks ( NetworkClient nc, NetworkMessageDelegate nmd )
		{
			foreach (Message.ID MsgId in System.Enum.GetValues(typeof(Message.ID))) // Register all the callbacks
				nc.RegisterHandler((short)MsgId, nmd);
		}

		string printPlayerName(Player p)
		{
			if(networkingInfo.isServer) return p.name + "(" + p.id + ")";
			return p.name;
		}

		string printServerAdress ()
		{
			if ( networkingInfo.address == NetworkingCommon.localServer) return networkingInfo.address;
			return networkingInfo.address + ":" + networkingInfo.port;
		}


		// ===========================================================================
		// ========================= PUBLIC HOOK FUNCTIONS =========================
		// ===========================================================================

		public void SendChatMessage(string s)
		{
			ClientSendMessage(RPC.Message.ID.ConsoleBroadcast,
				new RPC.Message.ConsoleBroadcast(myID, s), Channels.reliable);
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

		class Channels
		{
			static public byte priority; // For important, reliable one shot events
			static public byte reliable; // For important events, such as player action
			static public byte unreliable; // For slow events, such as camera stream
			static public byte fragmented; // For large events, such as file transfer
			static public byte update; // For spammed events, such as object movement
		}
	}
}