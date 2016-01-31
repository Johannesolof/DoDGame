using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

namespace RPC
{
	public class Server 
	{
		// CONSTANTS
		const int maxConcurrentConnectedUsers = 10;

		// PRIVATES
		ConnectionConfig config;
		HostTopology hostconfig;
		NetworkingInfo networkingInfo;
		NetworkingCommon common = NetworkingCommon.Instance;
		string tag = NetworkingCommon.SERVERTAG;

		List<ServerPlayer> connectedPlayers;
		Queue<NetworkConnection> pendingConnections;
		Dictionary<string, DateTime> whiteList; // IP to DateTime dictionary.

		TimeSpan whiteListTimeOut = new TimeSpan(6,0,0); // 6 hours for the whitelist to time out
		// TODO: Make sure these heartbeats go out as expected
		float lastHeartBeat = 0f; // Last time stamp, for heartbeat checks, in seconds since game start
		float heartBeatTimeOut = 2f; // 2 seconds time out
		int handshakeTimeoutMs = 10;

		Action onClient;

		public void RegisterOnClient(Action a)
		{
			onClient += a;
		}
		public void UnregisterOnClient(Action a)
		{
			onClient -= a;
		}
		void OnClient()
		{
			if(onClient != null)
			{
				onClient();
			}
		}
		public Queue<NetworkConnection> getPendingConnections()
		{
			return pendingConnections;
		}

		// CONSTRUCTOR
		public Server(NetworkingInfo netinfo)
		{
			networkingInfo = netinfo;
			if(networkingInfo.port == -1) networkingInfo.port = NetworkingInfo.defaultPort;
			if(networkingInfo.address == "") networkingInfo.address = NetworkingInfo.defaultAddress;
			// TODO: Add some kind of DNS pre check for IP?

			connectedPlayers = new List<ServerPlayer>();
			whiteList = new Dictionary<string, DateTime>();
			pendingConnections = new Queue<NetworkConnection>();

			config = new ConnectionConfig();
			Channels.priority = config.AddChannel(QosType.AllCostDelivery);
			Channels.reliable = config.AddChannel(QosType.ReliableSequenced);
			Channels.unreliable = config.AddChannel(QosType.UnreliableSequenced);
			Channels.fragmented = config.AddChannel(QosType.ReliableFragmented);
			Channels.update = config.AddChannel(QosType.StateUpdate);
			hostconfig = new HostTopology(config, maxConcurrentConnectedUsers);

			NetworkServer.Configure(hostconfig);
			NetworkServer.Listen(networkingInfo.port);
			NetworkServer.RegisterHandler(MsgType.Connect, OnClientConnected);
			common.printConsole (tag, "Setup complete", true);
			networkingInfo.address = NetworkingCommon.localServer;
		}

		// DESTRUCTOR
		~Server()
		{
			NetworkServer.DisconnectAll();
			NetworkServer.Shutdown();
			NetworkServer.Reset();
		}

		public void Tick (float ackumulativeTimeSinceStart)
		{
			if( ackumulativeTimeSinceStart > lastHeartBeat + heartBeatTimeOut )
			{
				lastHeartBeat = ackumulativeTimeSinceStart;
				// TODO: Check this muditrucker
				if( networkingInfo.isServer )
				{
					if(NetworkServer.active)
					{
						ServerSendMessage(RPC.Message.ID.HeartBeat, new RPC.Message.HeartBeat(), 
							Channels.update, connectedPlayers);
					}
				}
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
				ClientAccept(netMsg.conn);
			}
			else
			{
				common.printConsole (tag, "A peer has connected from " + netMsg.conn.address, true);

				if ( whiteList.ContainsKey( netMsg.conn.address ) )
				{
					if ( whiteList[netMsg.conn.address] > DateTime.Now )
					{
						ClientAccept(netMsg.conn);
						return;
					}
					whiteList.Remove(netMsg.conn.address);
				}
				pendingConnections.Enqueue(netMsg.conn);
				OnClient (); // Signal to all listeners that a new client is ready to be accepted
			}
		}

		public void ClientAccept(NetworkConnection nc)
		{
			if ( !whiteList.ContainsKey( nc.address ) ) // White list him for default amount of time, if he is not already white listed
			{
				whiteList.Add(nc.address, DateTime.Now + whiteListTimeOut);
			}

			registerAllCallbacks(nc, cbServerHandler);

			byte newID = giveUniquePlayerID();

			// TODO: Check this Modifacka
			new Timer(((data) => {serverDelayBeforeSendingHandshake(newID, nc);}), 
				null, handshakeTimeoutMs, Timeout.Infinite);

//			StartCoroutine( serverDelayBeforeSendingHandshake(newID, nc) );
		}

		public void ClientRefuse(NetworkConnection nc)
		{
			// TODO: Add anti-spam support
			common.printConsole (tag, "Refusing peer: " + nc.address, true);
			nc.Disconnect();
		}

		void OnClientDisconnected(NetworkMessage netMsg)
		{
			common.printConsole (tag, "Peer disconnected: " + netMsg.conn.address, true);
			ServerPlayer sp;
			if ( common.existPlayerByConnection(connectedPlayers, netMsg.conn, out sp) )
			{
				connectedPlayers.Remove( sp );

				ServerSendMessage(Message.ID.PlayerDisc,
					new Message.PlayerDisc(sp.id, sp.dcReason),
					Channels.reliable, connectedPlayers);
			}
		}


		// ===========================================================================
		// ========================= SEND FUNCTIONS =========================
		// ===========================================================================

		void ServerSendMessage(Message.ID id, MessageBase msg, byte channel, NetworkConnection conn)
		{
			if(conn != null)
				conn.SendByChannel((short)id, msg, channel);
			else
				common.printConsole (tag, "Is not connected to client " + conn.address, true);
		}

		void ServerSendMessage(Message.ID id, MessageBase msg, byte channel, List<NetworkConnection> playerConnectionList)
		{
			foreach(NetworkConnection nc in playerConnectionList)
			{
				nc.SendByChannel((short)id, msg, channel);
			}
		}

		void ServerSendMessage(Message.ID id, MessageBase msg, byte channel, ServerPlayer sp)
		{
			if(sp.connection.isConnected)
				sp.connection.SendByChannel((short)id, msg, channel);
			else
				common.printConsole (tag, "Player is not connected on client " + sp.connection.address, true);
		}

		void ServerSendMessage(Message.ID id, MessageBase msg, byte channel, List<ServerPlayer> players)
		{
			foreach(var p in players)
			{
				p.connection.SendByChannel((short)id, msg, channel);
			}
		}


		// ===========================================================================
		// ========================= CALLBACK FUNCTIONS =========================
		// ===========================================================================

		// SERVER SIDE DOD MESSAGE CALLBACK
		void cbServerHandler (NetworkMessage netMsg) 
		{
			if ( !whiteList.ContainsKey(netMsg.conn.address) )
			{
				// User is not yet white listed, or should not be. Just kick and forget about him
				common.printConsole (tag, "Kicked chatty not white-listed peer: " + netMsg.conn.address + "(" + netMsg.msgType + ")", true);
				netMsg.ReadMessage<Message.NameChange>();
				netMsg.conn.Disconnect();
				return;
			}


			switch(netMsg.msgType)
			{
			case (short)Message.ID.UserLogin:  // Last part of user connecting to the server. Check if name is free
				{
					Message.UserLogin msg = netMsg.ReadMessage<Message.UserLogin>();
					if ( common.existPlayerByName(connectedPlayers, msg.name) )
					{
						ServerSendMessage(Message.ID.KickReason,
							new Message.KickReason("Name already taken!"),
							Channels.reliable, netMsg.conn);
						netMsg.conn.Disconnect();
					}
					else // New user connected!
					{
						ServerPlayer sp = new ServerPlayer(msg.playerID, msg.name, netMsg.conn);
						connectedPlayers.Add(sp);

						common.printConsole (tag, "Player connected: " + printPlayerName(sp) + " from " + netMsg.conn.address, true);

						ServerSendMessage(Message.ID.PlayerCon,
							new Message.PlayerCon(sp), Channels.reliable, connectedPlayers); // Inform everyone of the new player

						ServerSendMessage(Message.ID.PlayerList,
							new Message.PlayerList(connectedPlayers), 
							Channels.reliable, netMsg.conn); // Let the new player know the current state of the server
					}
				}
				break;
			case (short)Message.ID.KickReason:
				{
					Message.KickReason msg = netMsg.ReadMessage<Message.KickReason>();
					common.printConsole (tag, "Tried to kick server. Reason: " + msg.reason, true);
				}
				break;

			case (short)Message.ID.ConsoleBroadcast:
				{
					Message.ConsoleBroadcast msg = netMsg.ReadMessage<Message.ConsoleBroadcast>();
					ServerSendMessage(Message.ID.ConsoleBroadcast, msg, Channels.reliable, connectedPlayers);
				}
				break;

			case (short)Message.ID.NameChange:
				{
					Message.NameChange msg = netMsg.ReadMessage<Message.NameChange>();
					ServerPlayer sp;
					if ( common.existPlayerByID(connectedPlayers, msg.playerID, out sp) )
					{
						if ( common.existPlayerByName(connectedPlayers, msg.newName ) ) // Name is already occupied
						{
							ServerSendMessage(Message.ID.NameChange, new Message.NameChange(sp.id, msg.newName, true), 
								Channels.reliable, sp.connection);
							common.printConsole (tag, printPlayerName(sp) + "'s name change failed, to: " + msg.newName,
								true);
						}
						else // Acknowledge the name change
						{
							ServerSendMessage(Message.ID.NameChange, msg, Channels.reliable, connectedPlayers);
							common.printConsole (tag, printPlayerName(sp) + " is now known as " + msg.newName, true);
						}

						sp.name = msg.newName;
					}
				}
				break;

			case (short)Message.ID.PlayerCon:
				{
					common.printConsole (tag, "PlayerCon received: " + netMsg.msgType, true);
				}
				break;

			case (short)Message.ID.PlayerDisc:
				{
					common.printConsole (tag, "PlayerDisc received: " + netMsg.msgType, true);
				}
				break;

			case (short)Message.ID.PlayerList:
				{
					// TODO: See this as a request for the player list ?
					common.printConsole (tag, "PlayerList received: " + netMsg.msgType, true);
				}
				break;

			case (short)Message.ID.HeartBeat:
				{
					// Do nothing, since this is not interresting for us
				}
				break;

			default:
				common.printConsole (tag, "Unknown message id received: " + netMsg.msgType, true);
				break;
			}
		}


		// ===========================================================================
		// ========================= UTILITY FUNCTIONS =========================
		// ===========================================================================

		void registerAllCallbacks ( NetworkConnection nc, NetworkMessageDelegate nmd )
		{
			foreach (Message.ID MsgId in System.Enum.GetValues(typeof(Message.ID))) // Register all the callbacks
				nc.RegisterHandler((short)MsgId, nmd);
		}

		List<Player> getAllPlayers(List<ServerPlayer> L)
		{
			List<Player> newList = new List<Player>();
			foreach(ServerPlayer sp in connectedPlayers)
			{
				Player p = new Player (sp.id, sp.name);
				newList.Add(p);
			}
			return newList;
		}

		byte giveUniquePlayerID()
		{
			byte newID = 0;
			do
			{
				++newID;
			} while ( common.existPlayerByID( connectedPlayers, newID ) );
			return newID;
		}

		string printPlayerName(Player p)
		{
			return p.name + "(" + p.id + ")";
		}

		void serverDelayBeforeSendingHandshake( byte newID, NetworkConnection nc )
		{
			ServerSendMessage(Message.ID.UserLogin,
				new Message.UserLogin(newID, ""),
				Channels.reliable, nc);
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