using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace RPC
{
	public class NetworkingCommon
	{
		// Singleton pattern. It is thread safe and the user can always assume that the object exist.
		// It will also not be instantiated until first use, i.e. it "hold" default values
		static private NetworkingCommon _instance;
		static public NetworkingCommon Instance {
			get {
				if (_instance == null) {
					_instance = new NetworkingCommon ();
				}
				return _instance;
			}
			private set {

			}
		}

		public const string localServer = "localServer";
		public const string localClient = "localClient";
		public const string SERVERTAG = "Server";
		public const string CLIENTTAG = "Client";
		public const string CHATTAG = "";
		public const string NOTAG = "";
		public const short JJMSG_ID_OFFSET = short.MaxValue/2;
		public const byte SERVERID = 0;
		public const byte PlayerIdsLowerBound = 1;
		public const byte PlayerIdsUpperBound = byte.MaxValue/2;

		Action<string> onConsole;

		public void RegisterOnConsole(Action<string> a)
		{
			onConsole += a;
		}
		public void UnregisterOnConsole(Action<string> a)
		{
			onConsole -= a;
		}
		public void printConsole(string tag, string s, bool sendToDebug = false)
		{
			string msg = "[" + tag + "] " + s;
			onConsole (msg);
			if (sendToDebug) {
				Debug.Log (msg);
			}
		}

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

		public bool existPlayerByID (List<Player> L, byte query, out Player player)
		{
			Predicate<Player> pr = p => p.id == query;

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

		public bool existPlayerByID (List<ServerPlayer> L, byte query, out ServerPlayer player)
		{
			Predicate<ServerPlayer> pr = sp => sp.id == query;

			int i = L.FindIndex(pr);
			if (i == -1) {
				player = null;
				return false;
			}
			player = L.Find(pr);
			return true;
		}

		public bool existPlayerByID (List<ServerPlayer> L, byte query)
		{
			ServerPlayer sp = new ServerPlayer();

			return existPlayerByID(L, query, out sp);
		}

		public bool existPlayerByName (List<ServerPlayer> L, string query, out ServerPlayer player)
		{
			Predicate<ServerPlayer> pr = sp => sp.name == query;

			int i = L.FindIndex(pr);
			if (i == -1) {
				player = null;
				return false;
			}
			player = L.Find(pr);
			return true;
		}

		public bool existPlayerByName (List<ServerPlayer> L, string query)
		{
			ServerPlayer sp = new ServerPlayer();

			return existPlayerByName(L, query, out sp);
		}

		public bool existPlayerByConnection (List<ServerPlayer> L, NetworkConnection query, out ServerPlayer player)
		{
			Predicate<ServerPlayer> pr = sp => sp.connection == query;

			int i = L.FindIndex(pr);
			if (i == -1) {
				player = null;
				return false;
			}
			player = L.Find(pr);
			return true;
		}

		public bool existPlayerByConnection (List<ServerPlayer> L, NetworkConnection query)
		{
			ServerPlayer sp = new ServerPlayer();

			return existPlayerByConnection(L, query, out sp);
		}
	}

	public class Player
	{
		public Player() {}
		public Player(Player p) { id = p.id; name = p.name; }
		public Player(byte Id, string Name) { id = Id; name = Name; }

		public byte id;
		public string name;
	}

	public class ServerPlayer : Player
	{
		public ServerPlayer() {}
		public ServerPlayer(Player p, NetworkConnection c) : base(p) { connection = c; }
		public ServerPlayer(byte Id, string Name, NetworkConnection c) : base(Id, Name) { connection = c; }

		public NetworkConnection connection;
		public string dcReason = "Connection closed";
	}

	public class NetworkingInfo
	{
		public string playerName = "";
		public int port = -1;
		public string address = "";
		public bool isServer = false;

		public const int defaultPort = 47624;
		public const string defaultAddress = "127.0.0.1";

		public void reset()
		{
			playerName = "";
			port = -1;
			address = "";
			isServer = false;
		}
	}

	class Channels
	{
		static public byte priority; // For important, reliable one shot events
		static public byte reliable; // For important events, such as player action
		static public byte unreliable; // For slow events, such as camera stream
		static public byte fragmented; // For large events, such as file transfer
		static public byte update; // For spammed events, such as object movement
	}


}
