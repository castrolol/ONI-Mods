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
				PUIUtils.AddSideScreenContent<AttributesAnalyzerSideScreen>();
				PUIUtils.AddSideScreenContent<MasteryAnalyzerSideScreen>();

			}
		}

		[HarmonyPatch(typeof(AccessControlSideScreen), "SetTarget")]
		public static class AccessControlSideScreen_SetTarget
		{

			internal static void Postfix(AccessControlSideScreen __instance, GameObject target)
			{
				var traverse = Traverse.Create(__instance);
				var rowGroupField = traverse.Field("rowGroup");
				var sortByNameToggleField = traverse.Field("sortByNameToggle");
				var rowGroup = rowGroupField.GetValue<GameObject>();
				var sortByNameToggle = sortByNameToggleField.GetValue<Toggle>();
				var scroll = rowGroup.transform.parent.parent.parent.gameObject;
				var header = sortByNameToggle.transform.parent.parent.gameObject;
				if (target.GetComponent<AttributesAnalyzerDoor>() != null || target.GetComponent<MasteryAnalyzerDoor>() != null)
				{
					scroll.SetActive(false);
					header.SetActive(false);
				}
				else
				{
					scroll.SetActive(true);
					header.SetActive(true);
				}
			}
		}



		[HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
		public static class Db_Initialize_Patch
		{

			public static void Postfix()
			{

				AddStrings(AttributesAnalyzerDoorConfig.ID, AttributesAnalyzerDoorConfig.NAME, AttributesAnalyzerDoorConfig.DESC, AttributesAnalyzerDoorConfig.EFFECT);
				ModUtil.AddBuildingToPlanScreen("Base", AttributesAnalyzerDoorConfig.ID);
				//Db.Get().Techs.Get("Suits").unlockedItemIDs.Add(LuxuryWallConfig.ID);


				AddStrings(MasteryAnalyzerDoorConfig.ID, MasteryAnalyzerDoorConfig.NAME, MasteryAnalyzerDoorConfig.DESC, MasteryAnalyzerDoorConfig.EFFECT);
				ModUtil.AddBuildingToPlanScreen("Base", MasteryAnalyzerDoorConfig.ID);
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