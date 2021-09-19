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

using ContentPatcher;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System;

namespace StatsAsTokens
{
	public class ContentPatcherHelper
	{

		public static IContentPatcherAPI api = null;

		public static bool TryLoadContentPatcherAPI()
		{
			try
			{
				// Check to see if Generic Mod Config Menu is installed
				if (!Globals.Helper.ModRegistry.IsLoaded("Pathoschild.ContentPatcher"))
				{
					Globals.Monitor.Log("Content Patcher not present");
					return false;
				}

				api = Globals.Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");

				return true;
			}
			catch (Exception e)
			{
				Globals.Monitor.Log($"Failed to register ContentPatcher API: {e.Message}", LogLevel.Error);
				return false;
			}
		}

		/// <summary>
		/// Adds all config values as tokens to ContentPatcher so that they can be referenced dynamically by patches
		/// </summary>
		public static void RegisterSimpleTokens()
		{
			if (api == null)
			{
				return;
			}
		}

		/// <summary>
		/// Adds all config values as tokens to ContentPatcher so that they can be referenced dynamically by patches
		/// </summary>
		public static void RegisterAdvancedTokens()
		{
			if (api == null)
			{
				return;
			}

			FoodEatenPatch();

			api.RegisterToken(Globals.Manifest, "Stats", new StatToken());
			api.RegisterToken(Globals.Manifest, "MonstersKilled", new MonstersKilledToken());
			api.RegisterToken(Globals.Manifest, "FoodEaten", new FoodEatenToken());
		}

		public static void FoodEatenPatch()
		{
			try
			{
				Harmony harmony = new(Globals.Manifest.UniqueID);
				harmony.Patch(
					original: typeof(Farmer).GetMethod("eatObject"),
					prefix: new HarmonyMethod(typeof(ContentPatcherHelper), nameof(eatObject_Prefix))
				);

				Globals.Monitor.Log("Patched eatObject() successfully");
			}
			catch (Exception ex)
			{
				Globals.Monitor.Log($"Exception encountered while patching method {nameof(eatObject_Prefix)}: {ex}");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Harmony patch - match original method naming convention")]
		public static void eatObject_Prefix(Farmer __instance, StardewValley.Object o)
		{
			string foodID = o.parentSheetIndex.ToString();

			string pType = __instance.IsMainPlayer ? "hostPlayer" : "localPlayer";
			FoodEatenToken.foodEatenDict[pType][foodID] = FoodEatenToken.foodEatenDict[pType].ContainsKey(foodID) ? FoodEatenToken.foodEatenDict[pType][foodID] + 1 : 1;
		}
	}
}
