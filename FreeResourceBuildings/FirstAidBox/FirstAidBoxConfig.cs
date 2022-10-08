using FreeResourceBuildingsPatches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUNING;
using UnityEngine;

namespace FreeResourceBuildings
{
	public class FirstAidBoxConfig : IBuildingConfig
	{
		public static string ID = ModStrings.FirstAidBoxID;
		public static string Name = ModStrings.FirstAidBoxName;
		public static string Description = ModStrings.FirstAidBoxDescription;
		public static string Effect = ModStrings.FirstAidBoxEffect;

		public override BuildingDef CreateBuildingDef()
		{

			var options = ModBuildingDefs.Instance.GetDefaultOptions(
				BUILDINGS.CONSTRUCTION_MASS_KG.TIER2,
				NOISE_POLLUTION.DAMPEN.TIER1,
				BUILDINGS.DECOR.PENALTY.TIER1,
				"Plastic",
				MATERIALS.PLASTICS
			);

			float[] weight = options.Mass;
			string[] resources = options.Resources;
			EffectorValues tieR1 = options.Decor;
			EffectorValues noise = options.Noise;
			var time = options.ConstructionTime;
			var audio = options.AudioCategory;

			BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 2, 2, "firstaid_box_kanim", 30, time, weight, resources, 1600f, BuildLocationRule.OnFloor, tieR1, noise);
			buildingDef.Floodable = false;
			buildingDef.Overheatable = false;
			buildingDef.AudioCategory = audio;
			buildingDef.UtilityInputOffset = new CellOffset(0, 0);
			buildingDef.PermittedRotations = PermittedRotations.Unrotatable;
			return buildingDef;
		}

		public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
		{
			GeneratedBuildings.MakeBuildingAlwaysOperational(go);
			var defaultStorage = go.AddOrGet<Storage>();
			defaultStorage.capacityKg = float.MaxValue;
			defaultStorage.showInUI = true;
			defaultStorage.allowItemRemoval = true;

			defaultStorage.storageFilters = new List<Tag> {
				GameTags.MedicalSupplies,
				GameTags.Medicine,
			};



			var gen = go.AddOrGet<FirstAidBox>();

			var modOptions = Mod.Options;
			gen.elementCount = 100000;
			gen.singleItemCount = modOptions.farmerFirstAidBoxItemsLimit;
			gen.singleItemPerTick = Mathf.Min(modOptions.farmerFirstAidBoxItemsLimit, 10);
		}

		public override void DoPostConfigureUnderConstruction(GameObject go)
		{
			base.DoPostConfigureUnderConstruction(go);
		}

		public override void DoPostConfigureComplete(GameObject go)
		{
			Prioritizable.AddRef(go);
		}
	}
}