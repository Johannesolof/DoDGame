using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace RPC
{
	public class Message 
	{
		public enum ID 
		{
			UserLogin = NetworkingCommon.JJMSG_ID_OFFSET,
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
			public PlayerCon(Player p) { playerID = p.id; name = p.name; }

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
			public PlayerList(List<Player> List) { array = List.ToArray(); }
			public PlayerList(List<ServerPlayer> List) 
			{
				List<Player> newList = new List<Player>();
				foreach(var sp in List)
				{
					newList.Add(new Player(sp.id, sp.name));
				}

				array = newList.ToArray();
			}

			public Player[] array;


			public List<Player> GetArrayAsList ()
			{
				List<Player> L = new List<Player>();
				foreach(Player p in array)
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