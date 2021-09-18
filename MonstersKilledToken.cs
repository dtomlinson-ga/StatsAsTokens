// Copyright (C) 2021 Vertigon
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see https://www.gnu.org/licenses/.

using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatsAsTokens
{
	internal class MonstersKilledToken
	{
		/*********
		** Fields
		*********/
		/// <summary>The game stats as of the last context update.</summary>
		private readonly Dictionary<string, SerializableDictionary<string, int>> monsterStatsDict;

		/*********
		** Constructor
		*********/
		public MonstersKilledToken()
		{
			monsterStatsDict = new(StringComparer.OrdinalIgnoreCase)
			{
				["hostPlayer"] = InitializeMonstersKilledStats(),
				["localPlayer"] = InitializeMonstersKilledStats()
			};
		}

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

			if (SaveGame.loaded != null || Context.IsWorldReady)
			{
				hasChanged = DidStatsChange();
			}

			Globals.Monitor.Log($"Updating MonstersKilledToken context - context Changed: {hasChanged}");

			return hasChanged;
		}

		/// <summary>Get whether the token is available for use.</summary>
		public bool IsReady()
		{
			return SaveGame.loaded != null || Context.IsWorldReady;
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

		private SerializableDictionary<string, int> InitializeMonstersKilledStats()
		{
			SerializableDictionary<string, int> monstersKilled = new();
			Dictionary<string, string> monsterData = Globals.Helper.Content.Load<Dictionary<string, string>>("Data/Monsters", ContentSource.GameContent);

			foreach (KeyValuePair<string, string> monster in monsterData)
			{
				monstersKilled[monster.Key] = 0;
			}

			return monstersKilled;
		}

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
					if (!cachedMonStats.ContainsKey(pair.Key))
					{
						hasChanged = true;
						cachedMonStats[pair.Key] = pair.Value;
					}
					else if (!cachedMonStats[pair.Key].Equals(pair.Value))
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
				if (!cachedMonStats.ContainsKey(pair.Key))
				{
					hasChanged = true;
					cachedMonStats[pair.Key] = pair.Value;
				}
				else if (!cachedMonStats[pair.Key].Equals(pair.Value))
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
			{
				playerType = "hostPlayer";
			}

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
