using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using PeterHan.PLib.Options;
using FreeResourceBuildings;

namespace FreeResourceBuildingsPatches
{

	// ----------------------------------------------------------------------------

	public class Mod : KMod.UserMod2
	{
		internal static ModConfig Options { get; private set; }

		public override void OnLoad(Harmony harmony)
		{
			harmony.PatchAll();

			// Registering for the config screen
			new POptions().RegisterOptions(this, typeof(ModConfig));

			// Read Settings
			Options = POptions.ReadSettings<ModConfig>() ?? new ModConfig();
		}
	}

	public class FreeEnergyPatch
	{

		private static void SetTechIfNeeded(string id, string early, string mid, string late)
		{
			var howToGetIt = Mod.Options.HowToGetTheseItems;

			switch (howToGetIt)
			{
				case CostOptions.JustSandbox:
				case CostOptions.EasyAndFree:
				case CostOptions.Easy:
					return;
				case CostOptions.EarlyGame:
					Db.Get().Techs.Get(early).unlockedItemIDs.Add(id);
					break;
				case CostOptions.MidGAme:
					Db.Get().Techs.Get(mid).unlockedItemIDs.Add(id);
					break;
				case CostOptions.LateGame:
					Db.Get().Techs.Get(late).unlockedItemIDs.Add(id);
					break;
			}


		}

		[HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
		public static class Db_Initialize_Patch
		{

			public static void Postfix()
			{
				var options = Mod.Options;

				if (options.UseFreeEnergyGenerator)
				{
					AddStrings(FreeEnergyGenerator.ID, FreeEnergyGenerator.Name, FreeEnergyGenerator.Description, FreeEnergyGenerator.Effect);
					ModUtil.AddBuildingToPlanScreen("Power", FreeEnergyGenerator.ID);
					SetTechIfNeeded(FreeEnergyGenerator.ID, "PowerRegulation", "AdvancedPowerRegulation", "ImprovedCombustion");

				}
				if (options.UseFreeStorage)
				{
					AddStrings(StorageGeneratorConfig.ID, StorageGeneratorConfig.Name, StorageGeneratorConfig.Description, StorageGeneratorConfig.Effect);
					ModUtil.AddBuildingToPlanScreen("Base", StorageGeneratorConfig.ID);
					SetTechIfNeeded(StorageGeneratorConfig.ID, "Jobs", "SolidTransport", "SolidManagement");

				}
				if (options.UseMagicRefrigerator)
				{
					AddStrings(FoodStorageGeneratorConfig.ID, FoodStorageGeneratorConfig.Name, FoodStorageGeneratorConfig.Description, FoodStorageGeneratorConfig.Effect);
					ModUtil.AddBuildingToPlanScreen("Food", FoodStorageGeneratorConfig.ID);
					SetTechIfNeeded(FoodStorageGeneratorConfig.ID, "FarmingTech", "Agriculture", "FinerDining");

				}
				if (options.UseFreeGasSource)
				{
					AddStrings(FreeGasSourceConfig.ID, FreeGasSourceConfig.Name, FreeGasSourceConfig.Description, FreeGasSourceConfig.Effect);
					ModUtil.AddBuildingToPlanScreen("HVAC", FreeGasSourceConfig.ID);
					SetTechIfNeeded(FreeGasSourceConfig.ID, "GasPiping", "DirectedAirStreams", "Catalytics");
				 
					AddStrings(FreeGasSinkConfig.ID, FreeGasSinkConfig.Name, FreeGasSinkConfig.Description, FreeGasSinkConfig.Effect);
					ModUtil.AddBuildingToPlanScreen("HVAC", FreeGasSinkConfig.ID);
					SetTechIfNeeded(FreeGasSinkConfig.ID, "GasPiping", "DirectedAirStreams", "Catalytics");
				}
				if (options.UseFreeLiquidSource)
				{
					AddStrings(FreeLiquidSinkConfig.ID, FreeLiquidSinkConfig.Name, FreeLiquidSinkConfig.Description, FreeLiquidSinkConfig.Effect);
					ModUtil.AddBuildingToPlanScreen("Plumbing", FreeLiquidSinkConfig.ID);
					SetTechIfNeeded(FreeLiquidSinkConfig.ID, "LiquidPiping", "LiquidDistribution", "AdvancedSanitation");
				 
					AddStrings(FreeLiquidSourceConfig.ID, FreeLiquidSourceConfig.Name, FreeLiquidSourceConfig.Description, FreeLiquidSourceConfig.Effect);
					ModUtil.AddBuildingToPlanScreen("Plumbing", FreeLiquidSourceConfig.ID);
					SetTechIfNeeded(FreeLiquidSourceConfig.ID, "LiquidPiping", "LiquidDistribution", "AdvancedSanitation");

				}

				if (options.UseMagicFeeder)
				{
					AddStrings(MagicFeederConfig.ID, MagicFeederConfig.Name, MagicFeederConfig.Description, MagicFeederConfig.Effect);
					ModUtil.AddBuildingToPlanScreen("Food", MagicFeederConfig.ID);
					SetTechIfNeeded(MagicFeederConfig.ID, "Ranching", "FoodRepurposing", "FinerDining");

				}

				if (options.UseMagicWardobre)
				{
					AddStrings(WardobreConfig.ID, WardobreConfig.Name, WardobreConfig.Description, WardobreConfig.Effect);
					ModUtil.AddBuildingToPlanScreen("Furniture", WardobreConfig.ID);
					SetTechIfNeeded(WardobreConfig.ID, "InteriorDecor", "Luxury", "GlassFurnishings");

				}
				if (options.UseTrashcan)
				{
					AddStrings(TrashcanConfig.ID, TrashcanConfig.Name, TrashcanConfig.Description, TrashcanConfig.Effect);
					ModUtil.AddBuildingToPlanScreen("Base", TrashcanConfig.ID);
					SetTechIfNeeded(TrashcanConfig.ID, "Jobs", "SolidTransport", "SolidManagement");

				}
				if (options.UseFirstAidBox)
				{
					AddStrings(FirstAidBoxConfig.ID, FirstAidBoxConfig.Name, FirstAidBoxConfig.Description, FirstAidBoxConfig.Effect);
					ModUtil.AddBuildingToPlanScreen("Medical", FirstAidBoxConfig.ID);
					SetTechIfNeeded(FirstAidBoxConfig.ID, "MedicineI", "MedicineIII", "MedicineIV");

				}
				if (DlcManager.IsExpansion1Active() && options.UseFreeRadboltSource)
				{
					AddStrings(FreeRadboltConfig.ID, FreeRadboltConfig.Name, FreeRadboltConfig.Description, FreeRadboltConfig.Effect);
					ModUtil.AddBuildingToPlanScreen("HEP", FreeRadboltConfig.ID);
					SetTechIfNeeded(FreeRadboltConfig.ID, "NuclearResearch", "AdvancedNuclearResearch", "NuclearPropulsion");

				}
				if (options.UseFarmerShelf)
				{
					AddStrings(FarmerStorageConfig.ID, FarmerStorageConfig.Name, FarmerStorageConfig.Description, FarmerStorageConfig.Effect);
					ModUtil.AddBuildingToPlanScreen("Food", FarmerStorageConfig.ID);
					SetTechIfNeeded(FarmerStorageConfig.ID, "FarmingTech", "Ranching", "AnimalControl");
				} 




				Strings.Add(new string[]{
					"STRINGS.BUILDING.STATUSITEMS.COLLECTINGFREEHEP.NAME", "Generating Radbolts ({x}/cycle)"
				});
				Strings.Add(new string[]{
					"STRINGS.BUILDING.STATUSITEMS.COLLECTINGFREEHEP.TOOLTIP", "Generate Radbolts for free"
				});
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


			[HarmonyPatch(typeof(FilterSideScreen))]
			[HarmonyPatch(nameof(FilterSideScreen.IsValidForTarget))]
			public class FilterSideScreen_IsValidForTarget
			{
				private static bool Prefix(GameObject target, FilterSideScreen __instance, ref bool __result)
				{
					if (target.GetComponent<FlowSlider>() != null)
					{
						__result = !__instance.isLogicFilter;
						return false;
					}
					return true;
				}
			}

			[HarmonyPatch(typeof(HighEnergyParticleDirectionSideScreen), nameof(HighEnergyParticleDirectionSideScreen.IsValidForTarget))]
			class HighEnergyParticleDirectionSideScreen_IsValidForTarget
			{
				static void Postfix(GameObject target, ref bool __result)
				{
					if (!__result)
					{
						if (target.GetComponent<FreeRadbolt>() != null)
						{
							__result = true;
						}
					}

				}
			}


			[HarmonyPatch(typeof(SideScreenContent), nameof(SideScreenContent.GetSideScreenSortOrder))]
			public class SideScreenContent_GetSideScreenSortOrder_Patch
			{
				public static void Postfix(SideScreenContent __instance, ref int __result)
				{
					if (__instance is FilterSideScreen)
					{
						__result = 2;
					}
					else if (__instance is IntSliderSideScreen)
					{
						__result = 1;
					}
				}
			}

		}





	}

}