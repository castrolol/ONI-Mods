using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUNING;
using UnityEngine;

namespace FreeResourceBuildings
{
	public class TrashcanConfig : IBuildingConfig
	{
		public static string ID = ModStrings.TrashcanID;
		public static string Name = ModStrings.TrashcanName;
		public static string Description = ModStrings.TrashcanDescription;
		public static string Effect = ModStrings.TrashcanEffect;

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

			BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 1, 2, "trash_can_kanim", 30, time, weight, resources, 1600f, BuildLocationRule.OnFloor, tieR1, noise);
			buildingDef.Floodable = false;
			buildingDef.Overheatable = false;
			buildingDef.AudioCategory = audio;
			buildingDef.UtilityInputOffset = new CellOffset(0, 0);
			buildingDef.PermittedRotations = PermittedRotations.FlipH;
			return buildingDef;
		}

		public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
		{
			GeneratedBuildings.MakeBuildingAlwaysOperational(go);
			go.AddOrGet<Trashcan>();
			var defaultStorage = go.AddOrGet<Storage>();
			defaultStorage.showInUI = true;
			defaultStorage.allowItemRemoval = true;
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