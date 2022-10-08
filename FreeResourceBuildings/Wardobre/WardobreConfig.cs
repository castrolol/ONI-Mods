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
	public class WardobreConfig : IBuildingConfig
	{
		public static string ID = ModStrings.WardobreID;
		public static string Name = ModStrings.WardobreName;
		public static string Description = ModStrings.WardobreDescription;
		public static string Effect = ModStrings.WardobreEffect;

		public override BuildingDef CreateBuildingDef()
		{
			var options = ModBuildingDefs.Instance.GetDefaultOptions(
			TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2,
			NOISE_POLLUTION.NOISY.TIER4,
			TUNING.BUILDINGS.DECOR.PENALTY.TIER2,
			 "Metal",
			  MATERIALS.ALL_MINERALS
		  );

			float[] weight = options.Mass;
			string[] resources = options.Resources;
			EffectorValues tieR1 = options.Decor;
			EffectorValues noise = options.Noise;
			var time = options.ConstructionTime;
			var audio = options.AudioCategory;

			BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 2, 2, "wardobre_kanim", 30, time, weight, resources, 1600f, BuildLocationRule.OnFloor, tieR1, noise);
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
				GameTags.Clothes,
				GameTags.Suit,
			};



			var gen = go.AddOrGet<Wardobre>();
			var modOptions = Mod.Options;
			gen.elementCount = 100000;
			gen.singleItemCount = modOptions.wardobreItemsLimit;
			gen.singleItemPerTick = 1;
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