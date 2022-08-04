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
using AnalyzerDoors;
using UnityEngine.UI;
using CrittersCanPass;
using Database;

namespace LuxuryDecoration
{

	// ----------------------------------------------------------------------------



	public class ModPatches
	{
		//[HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
		//public static class SideScreenCreator
		//{
		//	internal static void Postfix()
		//	{
		//		PUIUtils.AddSideScreenContent<AttributesAnalyzerSideScreen>();
		//		PUIUtils.AddSideScreenContent<MasteryAnalyzerSideScreen>();

		//	}
		//}

		//[HarmonyPatch(typeof(AccessControlSideScreen), "SetTarget")]
		//public static class AccessControlSideScreen_SetTarget
		//{

		//	internal static void Postfix(AccessControlSideScreen __instance, GameObject target)
		//	{
		//		var traverse = Traverse.Create(__instance);
		//		var rowGroupField = traverse.Field("rowGroup");
		//		var sortByNameToggleField = traverse.Field("sortByNameToggle");
		//		var rowGroup = rowGroupField.GetValue<GameObject>();
		//		var sortByNameToggle = sortByNameToggleField.GetValue<Toggle>();
		//		var scroll = rowGroup.transform.parent.parent.parent.gameObject;
		//		var header = sortByNameToggle.transform.parent.parent.gameObject;
		//		if (target.GetComponent<AttributesAnalyzerDoor>() != null || target.GetComponent<CrittersPassableDoor>() != null)
		//		{
		//			scroll.SetActive(false);
		//			header.SetActive(false);
		//		}
		//		else
		//		{
		//			scroll.SetActive(true);
		//			header.SetActive(true);
		//		}
		//	}
		//}


		[HarmonyPatch(typeof(ModifierSet), "LoadEffects")]
		public static class ModifierSet_LoadEffects
		{
			public static void Postfix(ModifierSet __instance)
			{
				var effects = __instance.effects;

				foreach (var effect in effects.resources)
				{
					if (effect.Id == "EggSong")
						effect.SelfModifiers.First().SetValue(20f);


					if (effect.Id == "EggHug")
						effect.SelfModifiers.First().SetValue(16f);
				}
			}
		}

		[HarmonyPatch(typeof(MinionResume), "AddExperience")]
		public static class EntityTemplates_ExtendEntityToWildCreature
		{
			public static void Prefix(ref float amount)
			{

				amount *= 100f;

			}
		}

		[HarmonyPatch(typeof(BuildingTemplates), "CreateBuildingDef")]
		public static class BuildingTemplates_CreateBuildingDef
		{
			internal static void Postfix(ref BuildingDef __result)
			{
				
				__result.ConstructionTime = 1f;

				//var currentMaterials = __result.MaterialCategory.ToList();
				//currentMaterials.AddRange(TUNING.MATERIALS.RAW_MINERALS);
				//__result.MaterialCategory = currentMaterials.ToArray();

				__result.Mass = __result.MaterialCategory.Select(x => 1f).ToArray();
			}
		}

		[HarmonyPatch(typeof(Diggable), nameof(Diggable.GetApproximateDigTime))]
		public static class Diggable_CreateDiggable
		{
			internal static void Postfix(ref float __result)
			{
				__result = 1f;
			}
		}

		[HarmonyPatch(typeof(EggIncubator), "UpdateProgress")]
		public static class EggIncubator_UpdateProgress
		{

			public static void Postfix(EggIncubator __instance)
			{

				if (__instance.Occupant != null)
				{
					Uprootable uprootable = __instance.Occupant.GetComponent<Uprootable>();
					if ((UnityEngine.Object)uprootable != (UnityEngine.Object)null && uprootable.IsMarkedForUproot)
					{
					}
					else
					{
						var prefabId = __instance.Occupant.GetComponent<KPrefabID>();

						if (!prefabId.HasTag(GameTags.Egg))
						{
							__instance.OrderRemoveOccupant();

							string tag = prefabId.PrefabTag.ToString();
							tag = tag.Replace("Baby", "Egg");
							//Debug.Log("Remove " + prefabId.PrefabTag);
							__instance.CreateOrder((Tag)tag, Tag.Invalid);

						}

					}


				}
			}
		}



		[HarmonyPatch(typeof(EntityTemplates), "ExtendEntityToFertileCreature")]
		public static class EntityTemplates_ExtendEntityToFertileCreature
		{

			public static void Prefix(ref float incubation_cycles)

			{

				//incubation_cycles /= 4;


			}
		}


		[HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
		public static class Db_Initialize_Patch
		{

			public static void Postfix()
			{

				//AddStrings(AttributesAnalyzerDoorConfig.ID, AttributesAnalyzerDoorConfig.NAME, AttributesAnalyzerDoorConfig.DESC, AttributesAnalyzerDoorConfig.EFFECT);
				//ModUtil.AddBuildingToPlanScreen("Base", AttributesAnalyzerDoorConfig.ID);
				//Db.Get().Techs.Get("DirectedAirStreams").unlockedItemIDs.Add(AttributesAnalyzerDoorConfig.ID);

				AddStrings(CrittersPassableBlockConfig.ID, CrittersPassableBlockConfig.NAME, CrittersPassableBlockConfig.DESC, CrittersPassableBlockConfig.EFFECT);
				ModUtil.AddBuildingToPlanScreen("Base", CrittersPassableBlockConfig.ID);
				Db.Get().Techs.Get("FarmingTech").unlockedItemIDs.Add(CrittersPassableBlockConfig.ID);

				AddStrings(CrittersPassableDoorConfig.ID, CrittersPassableDoorConfig.NAME, CrittersPassableDoorConfig.DESC, CrittersPassableDoorConfig.EFFECT);
				ModUtil.AddBuildingToPlanScreen("Base", CrittersPassableDoorConfig.ID);
				Db.Get().Techs.Get("FarmingTech").unlockedItemIDs.Add(CrittersPassableDoorConfig.ID);
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