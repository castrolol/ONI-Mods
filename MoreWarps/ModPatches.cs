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
using MoreWarps;
using Klei.AI;

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
		//[HarmonyPatch(typeof(GameSpawnData), "IsWarpTeleporter")]
		//public static class GameSpawnData_IsWarpTeleporter
		//{
		//	public static void Postfix(TemplateClasses.Prefab prefab, ref bool __result)
		//	{
		//		if (__result) return;
		//		__result = prefab.id == "WarpPortal2" || prefab.id == WarpReceiver2Config.ID || prefab.id == "WarpConduitSender2" || prefab.id == "WarpConduitReceiver2";
		//	}
		//}

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
		[HarmonyPatch(typeof(ColorSet), "Init")]
		public static class GlobalAssets_Init
		{
			public static void Postfix(ColorSet __instance)
			{
				var colorsert = Traverse.Create(__instance);
				var colors = colorsert.Field("namedLookup").GetValue<Dictionary<string, Color32>>();
				if (!colors.ContainsKey("NewColorX")) colors.Add("NewColorX", new Color32(255, 0, 255, 255));
			}
		}

		[HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
		public static class Db_Initialize_Patch
		{

			public static void Postfix()
			{

				//AddStrings(LuxuryWallConfig.ID, LuxuryWallConfig.NAME, LuxuryWallConfig.DESC, LuxuryWallConfig.EFFECT);
				//ModUtil.AddBuildingToPlanScreen("Utilities", LuxuryWallConfig.ID);
				//Db.Get().Techs.Get("Suits").unlockedItemIDs.Add(LuxuryWallConfig.ID);

				AddStrings(CustomWarpConfig.ID, CustomWarpConfig.Name, CustomWarpConfig.Description, CustomWarpConfig.Effect);
				ModUtil.AddBuildingToPlanScreen("Base", CustomWarpConfig.ID);


				Effect resource1 = null;
				try
				{
					resource1 = new Effect("NewEffect", (string)"Effect new 1", (string)"Effect TOLTIP 1", 600f, true, true, false);
					resource1.Add(new AttributeModifier(Db.Get().CritterAttributes.Happiness.Id, 5f, (string)STRINGS.CREATURES.MODIFIERS.RANCHED.NAME));
					resource1.Add(new AttributeModifier(Db.Get().Amounts.Wildness.deltaAttribute.Id, -0.09166667f, (string)STRINGS.CREATURES.MODIFIERS.RANCHED.NAME));
					resource1.Add(new AttributeModifier(Db.Get().Attributes.Athletics.Id, 300, (string)STRINGS.CREATURES.MODIFIERS.RANCHED.NAME));
				}
				catch (Exception ex)
				{
					Debug.Log("try 1");
				}
				RoomTypeCategory roomTypeCategory = null;
				try
				{
					roomTypeCategory = new RoomTypeCategory("NewEffectRoom", "", "NewColorX");
					Db.Get().RoomTypeCategories.Add(roomTypeCategory);
					Db.Get().effects.Add(resource1);
				}
				catch (Exception ex)
				{
					Debug.Log("try 1");
				}

				var Research = new RoomConstraints.Constraint(
					(KPrefabID bc) => bc.HasTag((Tag)"ResearchCenter"),
					null, 1,
					"Research",
					"Research 2"
				);


				try
				{
					Db.Get().RoomTypes.Add(new RoomType("NOVO",
					(string)"NOME NOVO", //ROOMS.TYPES.MASSAGE_CLINIC.NAME,
					(string)"TOOLTIP NOVO",//ROOMS.TYPES.MASSAGE_CLINIC.TOOLTIP,
					(string)"EFFECT NOVO",//ROOMS.TYPES.MASSAGE_CLINIC.EFFECT,
					roomTypeCategory,
					Research,
					new RoomConstraints.Constraint[3]
					{
						RoomConstraints.DECORATIVE_ITEM,
						RoomConstraints.MINIMUM_SIZE_12,
						RoomConstraints.MAXIMUM_SIZE_64
					},
					new RoomDetails.Detail[2]
					{
						RoomDetails.SIZE,
						RoomDetails.BUILDING_COUNT
					},
					2,
					single_assignee: true,
					priority_building_use: true,
					sortKey: 8,
					effects: new string[1] { "NewEffect" }
					)
					);
				}
				catch (Exception ex)
				{
					Debug.Log("try 1");
				}

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