
using TUNING;
using UnityEngine;

namespace LuxuryDecoration
{
	internal class LuxuryWallConfig : IBuildingConfig
	{
		public static string ID = "LuxuryWall";
		public static string NAME = "Luxury Drywall";
		public static string DESC = "Luxury drywall can be used in conjunction with tiles to build airtight rooms on the surface but with style!.";
		public static string EFFECT = "Prevents " + STRINGS.UI.FormatAsLink("Gas", "ELEMENTS_GAS") + " and " + STRINGS.UI.FormatAsLink("Liquid", "ELEMENTS_LIQUID") + " loss in space.\n\nBuilds an insulating backwall behind buildings.";



		public override BuildingDef CreateBuildingDef()
		{
			//float[] tieR4 = BUILDINGS.CONSTRUCTION_MASS_KG.TIER4;
			//string[] rawMinerals = MATERIALS.RAW_MINERALS;
			//EffectorValues none1 = NOISE_POLLUTION.NONE;
			//EffectorValues none2 = DECOR.NONE;
			//EffectorValues noise = none1;
			////BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("LuxuryWall", 1, 1, "luxury_walls_kanim", 30, 30f, tieR4, rawMinerals, 1600f, BuildLocationRule.NotInTiles, none2, noise);
			//BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("LuxuryWall", 1, 1, "luxury_walls_kanim", 30, 1f, new[] { 1f }, rawMinerals, 1600f, BuildLocationRule.NotInTiles, none2, noise);
			//buildingDef.Entombable = false;
			//buildingDef.Floodable = false;
			//buildingDef.Overheatable = false;
			//buildingDef.AudioCategory = "Metal";
			//buildingDef.AudioSize = "small";
			//buildingDef.BaseTimeUntilRepair = -1f;
			//buildingDef.DefaultAnimState = "mosaic_white";
			//buildingDef.ObjectLayer = ObjectLayer.Backwall;
			//buildingDef.SceneLayer = Grid.SceneLayer.Backwall;
			var exteriorWall = new ExteriorWallConfig();

			BuildingDef buildingDef = exteriorWall.CreateBuildingDef();
			buildingDef.PrefabID = "LuxuryWall";
			buildingDef.AnimFiles = new KAnimFile[1]
			{
				Assets.GetAnim("luxury_walls_kanim")
			};

			return buildingDef;
		}

		public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
		{
			GeneratedBuildings.MakeBuildingAlwaysOperational(go);
			go.AddOrGet<AnimTileable>().objectLayer = ObjectLayer.Backwall;
			go.AddOrGet<LuxuryTypeSelectorWall>();
			go.AddComponent<ZoneTile>();
			BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
		}

		public override void DoPostConfigureComplete(GameObject go) => GeneratedBuildings.RemoveLoopingSounds(go);
	}
}