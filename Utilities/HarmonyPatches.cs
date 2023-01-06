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
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewValley.Network;
using Object = StardewValley.Object;

namespace StatsAsTokens
{
	[HarmonyPatch]
	internal class HarmonyPatches
	{
		/*********
		** FoodEatenToken patch
		*********/

		private static Harmony Harmony;
		private static readonly MethodInfo set_RabbitWoolProduced = typeof(Stats).GetMethod("set_RabbitWoolProduced");
		private static readonly MethodInfo incrementAnimalProduceStat = typeof(StatFixes).GetMethod("IncrementAnimalProduceStat");

		public static void PerformHarmonyPatches()
		{
			Harmony = new(Globals.Manifest.UniqueID);
			Harmony.PatchAll();
		}
		
		[HarmonyPatch(typeof(Farmer), nameof(Farmer.eatObject))]
		[HarmonyPrefix]
		public static void Farmer_eatObject_Prefix(Farmer __instance, StardewValley.Object o)
		{
			string foodID = o.ParentSheetIndex.ToString();
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
		
		[HarmonyPatch(typeof(Tree), "performTreeFall")]
		[HarmonyPrefix]
		public static void Tree_performTreeFall_Prefix(Tree __instance, Tool t)
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
		
		[HarmonyPatch(typeof(Object), nameof(Object.performObjectDropInAction))]
		[HarmonyPrefix]
		public static void Object_performObjectDropInAction_Prefix(StardewValley.Object __instance, Item dropInItem, out int? __state)
		{
			int? minsReady = null;
			bool isValidInput = false;

			if (__instance.Name.Equals("Furnace"))
			{
				minsReady = __instance.MinutesUntilReady;
			}
			if (dropInItem is StardewValley.Object)
			{
				StardewValley.Object dropIn = dropInItem as StardewValley.Object;

				if (dropIn.Stack >= 5 && dropIn.ParentSheetIndex is 378 or 380 or 384 or 386)
				{
					isValidInput = true;
				}
			}

			__state = (minsReady is not null && isValidInput) ? minsReady : null;
		}

		[HarmonyPatch(typeof(Object), nameof(Object.performObjectDropInAction))]
		[HarmonyPostfix]
		public static void Object_performObjectDropInAction_Postfix(StardewValley.Object __instance, Farmer who, int? __state)
		{
			if (__instance.Name.Equals("Furnace"))
			{
				if (__state is not null && __state != __instance.MinutesUntilReady)
				{
					who.stats.BarsSmelted++;
				}
			}
		}
		
		[HarmonyPatch(typeof(Object), nameof(Object.checkForAction))]
		[HarmonyPrefix]
		public static void Object_checkForAction_Prefix(StardewValley.Object __instance, bool justCheckingForActivity, out StardewValley.Object __state)
		{
			if (justCheckingForActivity)
			{
				__state = null;
				return;
			}

			__state = __instance.Name.Contains("Mayonnaise") ? __instance.heldObject.Value : null;
		}

		[HarmonyPatch(typeof(Object), nameof(Object.checkForAction))]
		[HarmonyPostfix]
		public static void Object_checkForAction_Postfix(StardewValley.Object __instance, bool justCheckingForActivity, Farmer who, StardewValley.Object __state)
		{
			if (justCheckingForActivity || !who.IsLocalPlayer)
			{
				return;
			}

			if (!__instance.Name.Contains("Mayonnaise")) return;

			if (__state is null || __instance.heldObject.Value is not null) return;

			uint addQuantity = (uint)__state.Stack;

			switch (__state.ParentSheetIndex)
			{
				case 306:
					if (Game1.stats.stat_dictionary.ContainsKey("mayonnaiseMade"))
					{
						Game1.stats.stat_dictionary["mayonnaiseMade"] += addQuantity;
					}
					else
					{
						Game1.stats.stat_dictionary["mayonnaiseMade"] = addQuantity;
					}
					break;

				case 307:
					if (Game1.stats.stat_dictionary.ContainsKey("duckMayonnaiseMade"))
					{
						Game1.stats.stat_dictionary["duckMayonnaiseMade"] += addQuantity;
					}
					else
					{
						Game1.stats.stat_dictionary["duckMayonnaiseMade"] = addQuantity;
					}
					break;

				case 308:
					if (Game1.stats.stat_dictionary.ContainsKey("voidMayonnaiseMade"))
					{
						Game1.stats.stat_dictionary["voidMayonnaiseMade"] += addQuantity;
					}
					else
					{
						Game1.stats.stat_dictionary["voidMayonnaiseMade"] = addQuantity;
					}
					break;

				case 807:
					if (Game1.stats.stat_dictionary.ContainsKey("dinosaurMayonnaiseMade"))
					{
						Game1.stats.stat_dictionary["dinosaurMayonnaiseMade"] += addQuantity;
					}
					else
					{
						Game1.stats.stat_dictionary["dinosaurMayonnaiseMade"] = addQuantity;
					}
					break;

				default:
					return;
			}
		}

		[HarmonyPatch(typeof(Object), nameof(Object.placementAction))]
		[HarmonyPostfix]
		public static void Object_placementAction_Postfix(StardewValley.Object __instance, Farmer who, ref bool __result)
		{
			if (__instance.ParentSheetIndex is 891 or 292 or 310 or 311 && __result)
			{
				Game1.player.stats.stat_dictionary["treesPlanted"] = Game1.player.stats.stat_dictionary.ContainsKey("treesPlanted") ? Game1.player.stats.stat_dictionary["treesPlanted"] + 1 : 1;
			}
		}
		
		[HarmonyPatch(typeof(ResourceClump), nameof(ResourceClump.performToolAction))]
		[HarmonyPostfix]
		public static void ResourceClump_performToolAction_Postfix(ResourceClump __instance, Tool t)
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

		[HarmonyPatch(typeof(FarmAnimal), nameof(FarmAnimal.dayUpdate))]
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> FarmAnimal_dayUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> instructionsList = instructions.ToList();

			// remove everything from immediately after Ldloc_3 (whichProduce is loaded onto the stack) to RabbitWoolProduced is called
			int startIndex = instructionsList.FindIndex(il => il.opcode.Equals(OpCodes.Ldloc_3)) + 1;
			int endIndex = instructionsList.FindIndex(il => il.Calls(set_RabbitWoolProduced)) + 1;

			instructionsList.RemoveRange(startIndex, endIndex - startIndex);

			// add in call to IncrementAnimalProduceStat() just after ldloc_3
			instructionsList.Insert(startIndex, new CodeInstruction(OpCodes.Call, incrementAnimalProduceStat));

			// between ldloc_3 and incrementAnimalProduceStat(), add in ldarg_0 to pass `this` to IncrementAnimalProduceStat()
			instructionsList.Insert(startIndex, new CodeInstruction(OpCodes.Ldarg_0));

			return instructionsList.AsEnumerable();
		}
	}
}
