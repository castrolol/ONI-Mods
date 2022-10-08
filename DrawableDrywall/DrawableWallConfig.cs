
using TUNING;
using UnityEngine;

namespace DrawableDecoration
{
	public class DrawableWallConfig : IBuildingConfig
	{
		public static string ID = "DrawableWall";
		public static string NAME = "Drawable Drywall";
		public static string DESC = "Drawable drywall can be used in conjunction with tiles to build airtight rooms on the surface but with style!.";
		public static string EFFECT = "Prevents " + STRINGS.UI.FormatAsLink("Gas", "ELEMENTS_GAS") + " and " + STRINGS.UI.FormatAsLink("Liquid", "ELEMENTS_LIQUID") + " loss in space.\n\nBuilds an insulating backwall behind buildings.";



		public override BuildingDef CreateBuildingDef()
		{
			var exteriorWall = new ExteriorWallConfig();

			float[] tieR4 = BUILDINGS.CONSTRUCTION_MASS_KG.TIER4;
			string[] rawMinerals = MATERIALS.RAW_MINERALS;
			EffectorValues none1 = NOISE_POLLUTION.NONE;
			EffectorValues none2 = DECOR.NONE;
			EffectorValues noise = none1;
			//BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("DrawableWall", 1, 1, "luxury_walls_kanim", 30, 30f, tieR4, rawMinerals, 1600f, BuildLocationRule.NotInTiles, none2, noise);
			BuildingDef buildingDef = exteriorWall.CreateBuildingDef();
			buildingDef.PrefabID = "DrawableWall";
			buildingDef.AnimFiles = new KAnimFile[1]
			{
				Assets.GetAnim("wall_drawable_kanim")
			};
			//BuildingTemplates.CreateBuildingDef("DrawableWall", 1, 1, "wall_drawable_kanim", 30, 1f, new[] { 1f }, rawMinerals, 1600f, BuildLocationRule.NotInTiles, none2, noise);
			//buildingDef.Entombable = false;
			//buildingDef.Floodable = false;
			//buildingDef.Overheatable = false;
			//buildingDef.AudioCategory = "Metal";
			//buildingDef.AudioSize = "small";
			//buildingDef.BaseTimeUntilRepair = -1f;
			buildingDef.DefaultAnimState = "on";
			//buildingDef.ObjectLayer = ObjectLayer.Backwall;
			//buildingDef.SceneLayer = Grid.SceneLayer.Backwall;
			return buildingDef;
		}

		public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
		{
			GeneratedBuildings.MakeBuildingAlwaysOperational(go);
			go.AddOrGet<AnimTileable>().objectLayer = ObjectLayer.Backwall;
			go.AddOrGet<DrawableWall>();
			go.AddComponent<ZoneTile>();
			BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
		}

		public override void DoPostConfigureComplete(GameObject go) => GeneratedBuildings.RemoveLoopingSounds(go);
	}
}