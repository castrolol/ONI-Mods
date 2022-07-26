using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using PeterHan.PLib.Options;
using PeterHan.PLib.UI;
using ProcGenGame;
using ProcGen;
using TemplateClasses;

namespace LuxuryDecoration
{

	// ----------------------------------------------------------------------------



	public class ModPatches
	{
		[HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
		public static class SideScreenCreator
		{
			internal static void Postfix()
			{
				PUIUtils.AddSideScreenContent<WarpPortal2SideScreen>();

			}
		}
		[HarmonyPatch(typeof(GameSpawnData), "IsWarpTeleporter")]
		public static class GameSpawnData_IsWarpTeleporter
		{
			public static void Postfix(TemplateClasses.Prefab prefab, ref bool __result)
			{
				if (__result) return;
				__result = prefab.id == "WarpPortal2" || prefab.id == WarpReceiver2Config.ID || prefab.id == "WarpConduitSender2" || prefab.id == "WarpConduitReceiver2";
			}
		}

		//[HarmonyPatch(typeof(GameSpawnData), "AddTemplate")]
		//public static class TemplateSpawning_DetermineTemplatesForWorld
		//{
		//	public static void Postfix(
		//			TemplateContainer template,
	 // Vector2I position,
	 // ref Dictionary<int, int> claimedCells
		//		)
		//	{
		//		Debug.Log("AddTemplate");

		//		foreach(var tag in template.info.tags)
		//		{
		//			Debug.Log("Tag: " + tag);

		//		}
		//	}
		//}



		//[HarmonyPatch(typeof(WorldGen), "PlaceTemplateSpawners")]
		//public static class WorldGen_PlaceTemplateSpawners
		//{
		//	public static void Postfix(
		//		   Vector2I position,
	 // TemplateContainer template,
	 // ref Dictionary<int, int> claimedCells
		//		)
		//	{
		//		Debug.Log("TemplateContainer:");
		//		Debug.Log(" - name: " + template.name);
		//		if (template.info.tags != null)
		//		{
		//			Debug.Log(" - tags: ");
		//			foreach (var tag in template.info.tags)
		//				Debug.Log("   - " + tag);
		//		}

		//		if (template.buildings != null)
		//		{
		//			Debug.Log(" - buildings: ");
		//			foreach (var building in template.buildings)
		//			{
		//				Debug.Log("   - " + building?.id);
		//				Debug.Log("   - " + building?.type);
		//				if (building?.other_values != null)
		//				{
		//					Debug.Log("   - other_values");

		//					foreach (var other_values in building?.other_values)
		//					{
		//						Debug.Log("      - " + other_values.id + '=' + other_values.value);
		//					}
		//				}
		//			}
		//		}

		//		if (template.otherEntities != null)
		//		{
		//			Debug.Log(" - otherEntities:");
		//			foreach (var building in template.otherEntities)
		//			{
		//				Debug.Log("   - " + building?.id);
		//				Debug.Log("   - " + building?.type);
		//				if (building?.other_values != null)
		//				{
		//					Debug.Log("   - other_values");

		//					foreach (var other_values in building?.other_values)
		//					{
		//						Debug.Log("      - " + other_values.id + '=' + other_values.value);
		//					}
		//				}
		//			}

		//		}
		//	}
		//}


		[HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
		public static class Db_Initialize_Patch
		{

			public static void Postfix()
			{

				//AddStrings(LuxuryWallConfig.ID, LuxuryWallConfig.NAME, LuxuryWallConfig.DESC, LuxuryWallConfig.EFFECT);
				//ModUtil.AddBuildingToPlanScreen("Utilities", LuxuryWallConfig.ID);
				//Db.Get().Techs.Get("Suits").unlockedItemIDs.Add(LuxuryWallConfig.ID);

			}

			private static void AddStrings(string ID, string Name, string Description, string Effect)
			{
				Strings.Add(new string[]{
					"STRINGS.BUILDINGS.PREFABS." + ID.ToUpperInvariant() + ".NAME", Name
			});

				Strings.Add(new string[]{
					"STRINGS.BUILDINGS.PREFABS." + ID.ToUpperInvariant() + ".DESC",Description
			});

				Strings.Add(new string[]{
					"STRINGS.BUILDINGS.PREFABS." + ID.ToUpperInvariant() + ".EFFECT", Effect
			});

			}


		}





	}

}