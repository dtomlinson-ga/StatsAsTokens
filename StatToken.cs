using Force.DeepCloner;
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
	class StatToken
	{
		/*********
		** Fields
		*********/
		/// <summary>The game stats as of the last context update.</summary>
		private Dictionary<string, Stats> statsDict = new(StringComparer.OrdinalIgnoreCase)
		{
			["hostPlayer"] = new Stats(),
			["localPlayer"] = new Stats()
		};

		private readonly FieldInfo[] statFields = typeof(Stats).GetFields();


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

		/// <summary>Get whether the token always returns a value within a bounded numeric range for the given input. Mutually exclusive with <see cref="HasBoundedValues"/>.</summary>
		/// <param name="input">The input arguments, if any.</param>
		/// <param name="min">The minimum value this token may return.</param>
		/// <param name="max">The maximum value this token may return.</param>
		/// <remarks>Default false.</remarks>
		public bool HasBoundedRangeValues(string input, out int min, out int max)
		{
			min = 0;
			max = int.MaxValue;
			return true;
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

				if (!args[1].Contains("stat="))
				{
					error += "Named argument 'stat' not provided. ";
					return false;
				}
				else if (args[1].IndexOf('=') == args[1].Length - 1)
				{
					error += "Named argument 'stat' must be a string consisting of alphanumeric values. ";
				}
				else
				{
					string statArg = args[1].Substring(args[1].IndexOf('=') + 1);
					if (statArg.Any(ch => !char.IsLetterOrDigit(ch) && ch != ' '))
					{
						error += "Only alphanumeric values may be provided to 'stat' argument. ";
					}
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

			Globals.Monitor.Log($"Updating StatToken context - context changed: {hasChanged}");

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
			string stat = args[1].Substring(args[1].IndexOf('=') + 1).Trim().ToLower().Replace(" ", "");

			if (playerType.Equals("host"))
			{
				bool found = TryGetField(stat, "hostPlayer", out string hostStat);

				if (found)
				{
					output.Add(hostStat);
				}
			}
			else if (playerType.Equals("local"))
			{
				bool found = TryGetField(stat, "localPlayer", out string hostStat);

				if (found)
				{
					output.Add(hostStat);
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

			string pType;

			// check cached local player stats against Game1's local player stats
			// only needs to happen if player is local
			if (!Game1.IsMasterGame)
			{
				pType = "localPlayer";

				foreach (var field in statFields)
				{
					if (field.GetType().Equals(typeof(uint)))
					{
						if (!field.GetValue(Game1.stats).Equals(field.GetValue(statsDict[pType])))
						{
							hasChanged = true;
							field.SetValue(statsDict[pType], field.GetValue(Game1.stats));
						}
					}
					else if (field.GetType().Equals(typeof(SerializableDictionary<string, int>)))
					{
						SerializableDictionary<string, int> monStats = (SerializableDictionary<string, int>)field.GetValue(Game1.stats);
						SerializableDictionary<string, int> cachedMonStats = statsDict["localPlayer"].specificMonstersKilled;

						foreach (KeyValuePair<string, int> pair in monStats)
						{
							if (!cachedMonStats.ContainsKey(pair.Key) || !cachedMonStats[pair.Key].Equals(pair.Value))
							{
								hasChanged = true;
								cachedMonStats[pair.Key] = pair.Value;
							}
						}
					}
					else if (field.GetType().Equals(typeof(SerializableDictionary<string, uint>)))
					{
						SerializableDictionary<string, uint> otherStats = (SerializableDictionary<string, uint>)field.GetValue(Game1.stats);
						SerializableDictionary<string, uint> cachedOtherStats = statsDict["localPlayer"].stat_dictionary;

						foreach (KeyValuePair<string, uint> pair in otherStats)
						{
							if (!cachedOtherStats.ContainsKey(pair.Key) || !cachedOtherStats[pair.Key].Equals(pair.Value))
							{
								hasChanged = true;
								cachedOtherStats[pair.Key] = pair.Value;
							}
						}
					}
				}
			}

			pType = "hostPlayer";

			// check cached master player stats against Game1's master player stats
			// needs to happen whether player is host or local
			foreach (var field in statFields)
			{
				if (field.FieldType.Equals(typeof(uint)))
				{
					if (!field.GetValue(Game1.MasterPlayer.stats).Equals(field.GetValue(statsDict[pType])))
					{
						hasChanged = true;
					}
				}
				else if (field.GetType().Equals(typeof(SerializableDictionary<string, uint>)))
				{
					SerializableDictionary<string, uint> otherStats = (SerializableDictionary<string, uint>)field.GetValue(Game1.MasterPlayer.stats);
					SerializableDictionary<string, uint> cachedOtherStats = statsDict[pType].stat_dictionary;

					foreach (KeyValuePair<string, uint> pair in otherStats)
					{
						if (!cachedOtherStats.ContainsKey(pair.Key) || !cachedOtherStats[pair.Key].Equals(pair.Value))
						{
							hasChanged = true;
							cachedOtherStats[pair.Key] = pair.Value;
						}
					}
				}
			}

			return hasChanged;
		}

		private bool TryGetField(string statField, string playerType, out string foundStat)
		{
			bool found = false;
			foundStat = "";

			if (playerType.Equals("localPlayer") && Game1.IsMasterGame)
				playerType = "hostPlayer";

			foreach (FieldInfo field in statFields)
			{
				if (field.Name.ToLower().Equals(statField))
				{
					found = true;
					foundStat = field.GetValue(statsDict[playerType]).ToString();
				}
			}

			if (!found)
			{
				foreach (string key in statsDict[playerType].stat_dictionary.Keys)
				{
					if (key.ToLower().Replace(" ", "").Equals(statField))
					{
						found = true;
						foundStat = statsDict[playerType].stat_dictionary[key].ToString();
					}
				}
			}

			return found;
		}
	}
}
