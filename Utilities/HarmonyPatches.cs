using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace StatsAsTokens
{
	internal class HarmonyPatches
	{
		/*********
		** FoodEatenToken patch
		*********/

		static HarmonyInstance harmony;
		const string HarmonyJustification = "Harmony patch - match original method naming convention";

		public static void PerformHarmonyPatches()
		{
			harmony = HarmonyInstance.Create(Globals.Manifest.UniqueID);
			FoodEatenPatch();
			TreesFelledPatch();
			BarsSmeltedPatch();
			BouldersCrackedPatch();
		}

		public static void FoodEatenPatch()
		{
			try
			{
				System.Reflection.MethodBase eatObject = typeof(Farmer).GetMethod("eatObject");

				harmony.Patch(
					original: eatObject,
					prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(eatObject_Prefix))
				);

				Globals.Monitor.Log($"Patched {eatObject.Name} successfully");
			}
			catch (Exception ex)
			{
				Globals.Monitor.Log($"Exception encountered while patching method {nameof(eatObject_Prefix)}: {ex}", LogLevel.Error);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = HarmonyJustification)]
		public static void eatObject_Prefix(Farmer __instance, StardewValley.Object o)
		{
			string foodID = o.parentSheetIndex.ToString();
			Farmer f = __instance;

			string pType;
			if (f.IsMainPlayer && f.IsLocalPlayer)
			{
				pType = "hostPlayer";
			}
			else if (f.IsLocalPlayer)
			{
				pType = "localPlayer";
			}
			else
			{
				return;
			}

			Dictionary<string, Dictionary<string, int>> foodDict = FoodEatenToken.foodEatenDict.Value;
			foodDict[pType][foodID] = foodDict[pType].ContainsKey(foodID) ? foodDict[pType][foodID] + 1 : 1;
		}


		/*********
		** TreesFelledToken patch
		*********/

		public static void TreesFelledPatch()
		{
			try
			{
				System.Reflection.MethodBase treeFall = AccessTools.Method(typeof(Tree), "performTreeFall");

				harmony.Patch(
					original: treeFall,
					prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(performTreeFall_Prefix))
				);

				Globals.Monitor.Log($"Patched {treeFall.Name} successfully");
			}
			catch (Exception ex)
			{
				Globals.Monitor.Log($"Exception encountered while patching method {nameof(performTreeFall_Prefix)}: {ex}", LogLevel.Error);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = HarmonyJustification)]
		public static void performTreeFall_Prefix(Tree __instance, Tool t)
		{
			Farmer owner = t?.getLastFarmerToUse();

			if (owner == null)
			{
				return;
			}

			string pType;
			if (owner.IsMainPlayer)
			{
				pType = "hostPlayer";
			}
			else if (owner.IsLocalPlayer)
			{
				pType = "localPlayer";
			}
			else
			{
				return;
			}

			string treeType = __instance.treeType.ToString();

			// condense palm trees (palm and palm2) into one entry
			if (treeType == "9")
			{
				treeType = "6";
			}

			Dictionary<string, Dictionary<string, int>> treeDict = TreesFelledToken.treesFelledDict.Value;
			treeDict[pType][treeType] = treeDict[pType].ContainsKey(treeType) ? treeDict[pType][treeType] + 1 : 1;
		}

		public static void BarsSmeltedPatch()
		{
			try
			{
				System.Reflection.MethodBase dropIn = typeof(StardewValley.Object).GetMethod("performObjectDropInAction");

				harmony.Patch(
					original: dropIn,
					prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(performObjectDropInAction_Prefix))
				);

				harmony.Patch(
					original: dropIn,
					postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(performObjectDropInAction_Postfix))
				);

				Globals.Monitor.Log($"Patched {dropIn.Name} successfully");
			}
			catch (Exception ex)
			{
				Globals.Monitor.Log($"Exception encountered while patching methods: {nameof(performObjectDropInAction_Prefix)}, {nameof(performObjectDropInAction_Postfix)}: {ex}", LogLevel.Error);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = HarmonyJustification)]
		public static void performObjectDropInAction_Prefix(StardewValley.Object __instance, Item dropInItem, bool probe, Farmer who, out Vector2? __state)
		{
			int? minsReady = null;
			int isValidInput = 0;

			if (__instance.Name.Equals("Furnace"))
			{
				minsReady = __instance.MinutesUntilReady;
			}
			if (dropInItem is StardewValley.Object)
			{
				StardewValley.Object dropIn = dropInItem as StardewValley.Object;

				if (dropIn.Stack >= 5 && dropIn.ParentSheetIndex is 378 or 380 or 384 or 386)
				{
					isValidInput = 1;
				}
			}

			if (minsReady is not null && isValidInput is 1)
			{
				__state = new Vector2((int)minsReady, isValidInput);
			}
			else
			{
				__state = null;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = HarmonyJustification)]
		public static void performObjectDropInAction_Postfix(StardewValley.Object __instance, Item dropInItem, bool probe, Farmer who, Vector2? __state)
		{
			if (__instance.Name.Equals("Furnace"))
			{
				if (__state is not null && (int)__state?.X != __instance.MinutesUntilReady)
				{
					who.stats.BarsSmelted++;
				}
			}
		}

		public static void BouldersCrackedPatch()
		{
			try
			{
				System.Reflection.MethodBase performToolAction = typeof(ResourceClump).GetMethod("performToolAction");

				harmony.Patch(
					original: performToolAction,
					postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(performToolAction_Postfix))
				);

				Globals.Monitor.Log($"Patched {performToolAction.Name} successfully");
			}
			catch (Exception ex)
			{
				Globals.Monitor.Log($"Exception encountered while patching method {nameof(performToolAction_Postfix)}: {ex}", LogLevel.Error);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = HarmonyJustification)]
		public static void performToolAction_Postfix(ResourceClump __instance, Tool t)
		{
			if (__instance.health.Value <= 0f)
			{
				if (__instance.parentSheetIndex.Value is 672 or 752 or 754 or 756 or 758)
				{
					if (t is not null && t.getLastFarmerToUse() is not null)
					{
						t.getLastFarmerToUse().stats.BouldersCracked++;
					}
				}
			}
		}
	}
}
