using System;

namespace DodNS
{
	public class DodPlayer
	{
		public DodPlayer() {}
		public DodPlayer(byte id, string Name) { playerID = id; name = Name; }

		public byte playerID;
		public string name;
		public string dcReason = "Connection closed";
	}
}