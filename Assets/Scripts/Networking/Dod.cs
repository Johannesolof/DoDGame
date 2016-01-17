using System;
using System.Collections.Generic;

using UnityEngine.Networking;

namespace DodNS
{
	public class Dod
	{
		public static bool existPlayerByID (List<DodPlayer> L, byte query, out DodPlayer player)
		{
			Predicate<DodPlayer> pr = p => p.playerID == query;

			int i = L.FindIndex(pr);
			if (i == -1) {
				player = null;
				return false;
			}
			player = L.Find(pr);
			return true;
		}

		public static bool existPlayerByID (List<DodPlayer> L, byte query)
		{
			DodPlayer p = new DodPlayer();

			return existPlayerByID(L, query, out p);
		}

		public static bool existPlayerByName (List<DodPlayer> L, string query, out DodPlayer player)
		{
			Predicate<DodPlayer> pr = p => p.name == query;

			int i = L.FindIndex(pr);
			if (i == -1) {
				player = null;
				return false;
			}
			player = L.Find(pr);
			return true;
		}

		public static bool existPlayerByName (List<DodPlayer> L, string query)
		{
			DodPlayer p = new DodPlayer();

			return existPlayerByName(L, query, out p);
		}

		public static bool existPlayerByID (List<PlayerConnectionTuple> L, byte query, out PlayerConnectionTuple player)
		{
			Predicate<PlayerConnectionTuple> pr = pct => pct.player.playerID == query;

			int i = L.FindIndex(pr);
			if (i == -1) {
				player = null;
				return false;
			}
			player = L.Find(pr);
			return true;
		}

		public static bool existPlayerByID (List<PlayerConnectionTuple> L, byte query)
		{
			PlayerConnectionTuple pct = new PlayerConnectionTuple();

			return existPlayerByID(L, query, out pct);
		}

		public static bool existPlayerByName (List<PlayerConnectionTuple> L, string query, out PlayerConnectionTuple player)
		{
			Predicate<PlayerConnectionTuple> pr = pct => pct.player.name == query;

			int i = L.FindIndex(pr);
			if (i == -1) {
				player = null;
				return false;
			}
			player = L.Find(pr);
			return true;
		}

		public static bool existPlayerByName (List<PlayerConnectionTuple> L, string query)
		{
			PlayerConnectionTuple pct = new PlayerConnectionTuple();

			return existPlayerByName(L, query, out pct);
		}

		public static bool existPlayerByConnection (List<PlayerConnectionTuple> L, NetworkConnection query, out PlayerConnectionTuple player)
		{
			Predicate<PlayerConnectionTuple> pr = pct => pct.connection == query;

			int i = L.FindIndex(pr);
			if (i == -1) {
				player = null;
				return false;
			}
			player = L.Find(pr);
			return true;
		}

		public static bool existPlayerByConnection (List<PlayerConnectionTuple> L, NetworkConnection query)
		{
			PlayerConnectionTuple pct = new PlayerConnectionTuple();

			return existPlayerByConnection(L, query, out pct);
		}
	}

	class DodChannels
	{
		static public byte priority; // For important, reliable one shot events
		static public byte reliable; // For important events, such as player action
		static public byte unreliable; // For slow events, such as camera stream
		static public byte fragmented; // For large events, such as file transfer
		static public byte update; // For spammed events, such as object movement
	}
}