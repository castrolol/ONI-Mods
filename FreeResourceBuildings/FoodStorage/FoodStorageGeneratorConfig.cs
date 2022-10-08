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
	public class FoodStorageGeneratorConfig : IBuildingConfig
	{
		public static string ID = ModStrings.FoodStorageGeneratorID;
		public static string Name = ModStrings.FoodStorageGeneratorName;
		public static string Description = ModStrings.FoodStorageGeneratorDescription;
		public static string Effect = ModStrings.FoodStorageGeneratorEffect;

		public override BuildingDef CreateBuildingDef()
		{
			var options = ModBuildingDefs.Instance.GetDefaultOptions(
				BUILDINGS.CONSTRUCTION_MASS_KG.TIER2,
				NOISE_POLLUTION.NOISY.TIER2,
				BUILDINGS.DECOR.PENALTY.TIER1,
				"Metal",
				MATERIALS.REFINED_METALS
			);

			float[] weight = options.Mass;
			string[] resources = options.Resources;
			EffectorValues tieR1 = options.Decor;
			EffectorValues noise = options.Noise;
			var time = options.ConstructionTime;
			var audio = options.AudioCategory;

			BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 1, 2, "food_box_kanim", 30, time, weight, resources, 1600f, BuildLocationRule.OnFloor, tieR1, noise);
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

			defaultStorage.storageFilters = new List<Tag>();
			defaultStorage.storageFilters.AddRange(STORAGEFILTERS.FOOD);

			defaultStorage.storageFilters.Remove(GameTags.Medicine);


			var gen = go.AddOrGet<FoodStorageGenerator>();
			var modOptions = Mod.Options;
			gen.elementCount = 100000;
			gen.singleItemCount = modOptions.foodStorageItemsLimit;
			gen.singleItemPerTick = Mathf.Min(modOptions.foodStorageItemsLimit, 10);
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