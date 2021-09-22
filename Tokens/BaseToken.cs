using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatsAsTokens
{
	/// <summary>
	/// Base class which all Tokens extend - handles basic token functionality. Tokens must implement their own parsing of input and returning of values.
	/// </summary>
	abstract class BaseToken
	{
		internal static readonly string host = "hostPlayer", loc = "localPlayer";

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
		public abstract bool TryValidateInput(string input, out string error);

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

			return hasChanged;
		}

		/// <summary>
		/// Checks to see if stats changed. Updates cached values if they are out of date.
		/// </summary>
		/// <returns><c>True</c> if stats changed, <c>False</c> otherwise.</returns>
		public abstract bool DidStatsChange();

		/// <summary>Get whether the token is available for use.</summary>
		public bool IsReady()
		{
			return SaveGame.loaded != null || Context.IsWorldReady;
		}

		/// <summary>Get the current values.</summary>
		/// <param name="input">The input arguments, if applicable.</param>
		public abstract IEnumerable<string> GetValues(string input);

	}
}
