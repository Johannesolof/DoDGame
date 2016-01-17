using System;

using UnityEngine.Networking;

namespace DodNS
{
	public class PlayerConnectionTuple
	{
		public PlayerConnectionTuple() {}
		public PlayerConnectionTuple(DodPlayer p, NetworkConnection c) { player = p; connection = c; }
		public PlayerConnectionTuple(byte id, string Name, NetworkConnection c) { player = new DodPlayer(id, Name); connection = c; }

		public DodPlayer player;
		public NetworkConnection connection;
	}
}