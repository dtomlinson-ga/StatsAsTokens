using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StatsAsTokens
{
	class MonstersKilledToken
	{
		/*********
		** Fields
		*********/
		/// <summary>The game stats as of the last context update.</summary>
		private Dictionary<string, SerializableDictionary<string, int>> monsterStatsDict = new(StringComparer.OrdinalIgnoreCase)
		{
			["hostPlayer"] = new Stats().specificMonstersKilled,
			["localPlayer"] = new Stats().specificMonstersKilled
		};

		/*********
		** Public methods
		*********/

		/****
		** Metadata
		****/
		/// <summary>Get whether the token allows input arguments (e.g. an NPC name for a relationship token).</summary>
		public bool AllowsInput()
		{
			return true;
		}


		/// <summary>Whether the token requires input arguments to work, and does not provide values without it (see <see cref="AllowsInput"/>).</summary>
		/// <remarks>Default false.</remarks>
		public bool RequiresInput()
		{
			return true;
		}

		/// <summary>Whether the token may return multiple values for the given input.</summary>
		/// <param name="input">The input arguments, if applicable.</param>
		public bool CanHaveMultipleValues(string input = null)
		{
			return false;
		}

		/// <summary>Validate that the provided input arguments are valid.</summary>
		/// <param name="input">The input arguments, if any.</param>
		/// <param name="error">The validation error, if any.</param>
		/// <returns>Returns whether validation succeeded.</returns>
		/// <remarks>Default true.</remarks>
		public bool TryValidateInput(string input, out string error)
		{
			error = "";
			string[] args = input.ToLower().Trim().Split('|');

			if (args.Count() == 2)
			{
				if (!args[0].Contains("player="))
				{
					error += "Named argument 'player' not provided. ";
				}
				else if (args[0].IndexOf('=') == args[0].Length - 1)
				{
					error += "Named argument 'player' not provided a value. Must be one of the following values: 'host', 'local'. ";
				}
				else
				{
					// accept hostplayer or host, localplayer or local
					string playerType = args[0].Substring(args[0].IndexOf('=') + 1).Trim().Replace("player", "");
					if (!(playerType.Equals("host") || playerType.Equals("local")))
					{
						error += "Named argument 'player' must be one of the following values: 'host', 'local'. ";
					}
				}

				if (!args[1].Contains("monster="))
				{
					error += "Named argument 'monster' not provided. ";
					return false;
				}
				else if (args[1].IndexOf('=') == args[1].Length - 1)
				{
					error += "Named argument 'monster' must be a string consisting of alphanumeric characters. ";
				}
			}
			else
			{
				error += "Incorrect number of arguments provided. A 'player' argument and 'stat' argument should be provided. ";
			}

			return error.Equals("");
		}

		/****
		** State
		****/
		/// <summary>Update the values when the context changes.</summary>
		/// <returns>Returns whether the value changed, which may trigger patch updates.</returns>
		public bool UpdateContext()
		{
			bool hasChanged = false;

			if (Game1.stats != null)
			{
				hasChanged = DidStatsChange();
			}

			Globals.Monitor.Log($"Updating MonstersKilledToken context - context Changed: {hasChanged}");

			return hasChanged;
		}

		/// <summary>Get whether the token is available for use.</summary>
		public bool IsReady()
		{
			return Game1.stats != null && Context.IsWorldReady;
		}

		/// <summary>Get the current values.</summary>
		/// <param name="input">The input arguments, if applicable.</param>
		public IEnumerable<string> GetValues(string input)
		{
			List<string> output = new();

			string[] args = input.Split('|');

			string playerType = args[0].Substring(args[0].IndexOf('=') + 1).Trim().ToLower().Replace("player", "").Replace(" ", "");
			string monster = args[1].Substring(args[1].IndexOf('=') + 1).Trim().ToLower().Replace(" ", "");

			if (playerType.Equals("host"))
			{
				bool found = TryGetMonsterStat(monster, "hostPlayer", out string monsterNum);

				if (found)
				{
					output.Add(monsterNum);
				}
			}
			else if (playerType.Equals("local"))
			{
				bool found = TryGetMonsterStat(monster, "localPlayer", out string monsterNum);

				if (found)
				{
					output.Add(monsterNum);
				}
			}

			return output;
		}
		/*********
		** Private methods
		*********/

		/// <summary>
		/// Checks to see if stats changed. Updates cached values if they are out of date.
		/// </summary>
		/// <returns></returns>
		private bool DidStatsChange()
		{
			bool hasChanged = false;

			string pType = "localPlayer";

			SerializableDictionary<string, int> monStats = Game1.stats.specificMonstersKilled;
			SerializableDictionary<string, int> cachedMonStats = monsterStatsDict[pType];

			// check cached local player stats against Game1's local player stats
			// only needs to happen if player is local
			if (!Game1.IsMasterGame)
			{
				foreach (KeyValuePair<string, int> pair in monStats)
				{
					if (!cachedMonStats.ContainsKey(pair.Key) || !cachedMonStats[pair.Key].Equals(pair.Value))
					{
						hasChanged = true;
						cachedMonStats[pair.Key] = pair.Value;
					}
				}
			}

			pType = "hostPlayer";

			// check cached master player stats against Game1's master player stats
			// needs to happen whether player is host or local
			monStats = Game1.MasterPlayer.stats.specificMonstersKilled;
			cachedMonStats = monsterStatsDict[pType];

			foreach (KeyValuePair<string, int> pair in monStats)
			{
				if (!cachedMonStats.ContainsKey(pair.Key) || !cachedMonStats[pair.Key].Equals(pair.Value))
				{
					hasChanged = true;
					cachedMonStats[pair.Key] = pair.Value;
				}
			}

			return hasChanged;
		}
		private bool TryGetMonsterStat(string monsterName, string playerType, out string monsterNum)
		{
			bool found = false;
			monsterNum = "";

			if (playerType.Equals("localPlayer") && Game1.IsMasterGame)
				playerType = "hostPlayer";

			if (playerType.Equals("hostPlayer") || playerType.Equals("localPlayer"))
			{
				foreach (string key in monsterStatsDict[playerType].Keys)
				{
					if (key.ToLower().Replace(" ", "").Equals(monsterName))
					{
						found = true;
						monsterNum = monsterStatsDict[playerType][key].ToString();
					}
				}
			}

			return found;
		}

	}
}
