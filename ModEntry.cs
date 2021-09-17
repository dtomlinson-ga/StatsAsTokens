using StardewModdingAPI;
using StardewValley;
using System;

namespace StatsAsTokens
{
	/// <summary>The mod entry point.</summary>
	public class ModEntry : Mod
	{

		/// <summary>The mod entry point.</summary>
		/// <param name="helper" />
		public override void Entry(IModHelper helper)
		{
			SetUpGlobals(helper);
			SetUpEventHooks();

			//testing
			//SetUpConsoleCommands();
		}
		
		private void SetUpEventHooks()
		{
			Globals.Helper.Events.GameLoop.GameLaunched += (sender, args) =>
			{
				ContentPatcherHelper.TryLoadContentPatcherAPI();
				ContentPatcherHelper.RegisterSimpleTokens();
				ContentPatcherHelper.RegisterAdvancedTokens();
			};
		}

		private void SetUpConsoleCommands()
		{
			Globals.Helper.ConsoleCommands.Add("lm", "List monster currently in monsters killed dict", (name, args) =>
			{
				Globals.Monitor.Log(string.Join("\n", Game1.stats.specificMonstersKilled.Keys));
			});
		}

		private void SetUpGlobals(IModHelper helper)
		{
			Globals.Helper = helper;
			Globals.Monitor = Monitor;
			Globals.Manifest = ModManifest;
		}
	}
}
