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

using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatsAsTokens
{
	internal class FoodEatenToken
	{
		/*********
		** Fields
		*********/
		/// <summary>Stores item ID number as Key, number eaten as Value.</summary>
		private static readonly string host = "hostPlayer", loc = "localPlayer";
		public static readonly Dictionary<string, SerializableDictionary<string, int>> foodEatenDict, cachedFoodEatenDict;
		public static readonly Dictionary<int, string> objectData;

		/*********
		** Public methods
		*********/

		/****
		** Static Constructor
		****/
		static FoodEatenToken()
		{
			objectData = Globals.Helper.Content.Load<Dictionary<int, string>>("Data/ObjectInformation", ContentSource.GameContent);

			foodEatenDict = new(StringComparer.OrdinalIgnoreCase)
			{
				[host] = InitializeFoodEatenStats(),
				[loc] = InitializeFoodEatenStats()
			};

			cachedFoodEatenDict = new(StringComparer.OrdinalIgnoreCase)
			{
				[host] = InitializeFoodEatenStats(),
				[loc] = InitializeFoodEatenStats()
			};
		}


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

				if (!args[1].Contains("food="))
				{
					error += "Named argument 'food' not provided. Must be a string consisting of alphanumeric characters. ";
					return false;
				}
				else if (args[1].IndexOf('=') == args[1].Length - 1)
				{
					error += "Named argument 'food' must be a string consisting of alphanumeric characters. ";
				}
			}
			else
			{
				error += "Incorrect number of arguments provided. A 'player' argument and 'food' argument should be provided. ";
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

			Globals.Monitor.Log($"Updating FoodEatenToken context - context changed: {hasChanged}");

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

			// sanitize inputs
			string playerType = args[0].Substring(args[0].IndexOf('=') + 1).Trim().ToLower().Replace("player", "").Replace(" ", "");
			string food = args[1].Substring(args[1].IndexOf('=') + 1).Trim().ToLower().Replace(" ", "");

			string pType = playerType.Equals("host") ? host : loc;

			if (TryGetFoodEaten(food, pType, out string monsterNum))
			{
				output.Add(monsterNum);
			}

			return output;
		}

		/*********
		** Private methods
		*********/

		private static SerializableDictionary<string, int> InitializeFoodEatenStats()
		{
			SerializableDictionary<string, int> foodEaten = new();
			Dictionary<int, string> objData = objectData;

			foreach (KeyValuePair<int, string> obj in objData)
			{
				string[] objDescription = obj.Value.Split('/');

				// anything with edibility that is not -300 is edible
				if (objDescription.Length > 2 && !objDescription[2].Equals("-300"))
				{
					foodEaten[obj.Key.ToString()] = 0;
				}
			}

			// check for DGA food?
			// can't be done without hard dependency - mention to users that they will need to check if FoodEaten has value for DGA food before querying with it

			return foodEaten;
		}

		/// <summary>
		/// Checks to see if stats changed. Updates cached values if they are out of date.
		/// </summary>
		/// <returns></returns>
		private bool DidStatsChange()
		{
			bool hasChanged = false;

			string pType = loc;

			SerializableDictionary<string, int> foodEaten = foodEatenDict[pType];
			SerializableDictionary<string, int> cachedFoodEaten = cachedFoodEatenDict[pType];

			// check cached local player stats against Game1's local player stats
			// only needs to happen if player is local
			if (!Game1.IsMasterGame)
			{
				foreach (KeyValuePair<string, int> pair in foodEaten)
				{
					if (!cachedFoodEaten.ContainsKey(pair.Key))
					{
						hasChanged = true;
						cachedFoodEaten[pair.Key] = pair.Value;
					}
					else if (!cachedFoodEaten[pair.Key].Equals(pair.Value))
					{
						hasChanged = true;
						cachedFoodEaten[pair.Key] = pair.Value;
					}
				}
			}

			pType = host;

			// check cached master player stats against Game1's master player stats
			// needs to happen whether player is host or local
			foodEaten = foodEatenDict[pType];
			cachedFoodEaten = cachedFoodEatenDict[pType];

			foreach (KeyValuePair<string, int> pair in foodEaten)
			{
				if (!cachedFoodEaten.ContainsKey(pair.Key))
				{
					hasChanged = true;
					cachedFoodEaten[pair.Key] = pair.Value;
				}
				else if (!cachedFoodEaten[pair.Key].Equals(pair.Value))
				{
					hasChanged = true;
					cachedFoodEaten[pair.Key] = pair.Value;
				}
			}

			return hasChanged;
		}

		private bool TryGetFoodEaten(string foodNameOrId, string playerType, out string foodEatenNum)
		{
			string pType = playerType;
			string foodId = "";
			foodEatenNum = "";

			bool found = false;
			bool isNumericId = int.TryParse(foodNameOrId, out _);

			// string passed in is not a number - try matching with object entry to find ID
			if (!isNumericId)
			{
				// "any" is special case - otherwise, try to match
				if (!foodNameOrId.Equals("any"))
				{
					string fuzzyName = Utility.fuzzyItemSearch(foodNameOrId)?.Name.Trim().Replace(" ", "").ToLower() ?? foodNameOrId;

					// logging
					Globals.Monitor.Log($"Parsed 'food' value {foodNameOrId} to {fuzzyName}");

					Dictionary<int, string> objData = objectData;
					foreach (KeyValuePair<int, string> pair in objData)
					{
						if (pair.Value.Split('/')[0].Replace(" ", "").ToLower().Equals(fuzzyName))
						{
							foodId = pair.Key.ToString();
							break;
						}
					}
				}
			}
			else
			{
				foodId = foodNameOrId;
			}

			if (playerType.Equals(loc) && Game1.IsMasterGame)
			{
				pType = host;
			}

			if (pType.Equals(host) || pType.Equals(loc))
			{
				if (foodNameOrId.Equals("any"))
				{
					found = true;
					foodEatenNum = cachedFoodEatenDict[pType].Values.Sum().ToString();
				}
				else
				{
					foreach (string key in cachedFoodEatenDict[pType].Keys)
					{
						if (key.Equals(foodId))
						{
							found = true;
							foodEatenNum = cachedFoodEatenDict[pType][key].ToString();
						}
					}
				}
			}

			return found;
		}

	}
}
