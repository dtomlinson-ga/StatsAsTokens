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

		private List<FieldInfo> fields = new();


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
				// if this instance is main player, update both stats dicts to this instance's stats
				if (Game1.player.IsMainPlayer)
				{
					hasChanged = !Game1.stats.Equals(statsDict["hostPlayer"]);
					if (hasChanged)
					{
						statsDict["hostPlayer"] = Game1.stats;
						statsDict["localPlayer"] = Game1.stats;
					}
				}
				// otherwise, update local player's and main player's stats separately
				else
				{
					hasChanged = !Game1.stats.Equals(statsDict["localPlayer"]);
					if (hasChanged) statsDict["localPlayer"] = Game1.stats;

					hasChanged = !Game1.MasterPlayer.stats.Equals(statsDict["hostPlayer"]);
					if (hasChanged) statsDict["hostPlayer"] = Game1.MasterPlayer.stats;
				}
			}

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

			string playerType = args[0].Substring(args[0].IndexOf('=') + 1).Trim().ToLower().Replace("player", "");
			string stat = args[1].Substring(args[1].IndexOf('=') + 1).Trim().ToLower();

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

		private bool TryGetField(string statField, string playerType, out string foundStat)
		{
			bool found = false;
			foundStat = "";

			if (fields.Count() == 0)
			{
				fields = typeof(Stats).GetFields().ToList();
			}

			foreach (FieldInfo field in fields)
			{
				if (field.Name.ToLower().Equals(statField))
				{
					found = true;
					foundStat = field.GetValue(statsDict[playerType]).ToString();
				}
			}

			if (!found)
			{
				uint statGet = Game1.stats.getStat(statField);

				foreach (string key in Game1.stats.stat_dictionary.Keys)
				{
					if (key.ToLower().Equals(statField))
					{
						found = true;
						foundStat = Game1.stats.stat_dictionary[key].ToString();
					}
				}
			}

			return found;
		}
	}
}
