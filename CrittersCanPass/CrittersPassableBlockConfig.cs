using CrittersCanPass;
using System.Collections.Generic;
using TUNING;
using UnityEngine;

namespace AnalyzerDoors
{

	public class CrittersPassableBlockConfig : IBuildingConfig
	{
		public const string ID = "CrittersPassableBlock";
		public static string NAME = "Farm Block";
		public static string DESC = "A block that works as a tile but allows critters to pass through";
		public static string EFFECT = "A block that works as a tile but allows critters to pass through";



		public override BuildingDef CreateBuildingDef()
		{
			float[] tieR2 = TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2;
			string[] allMetals = MATERIALS.ALL_METALS;
			EffectorValues none1 = TUNING.NOISE_POLLUTION.NONE;
			EffectorValues none2 = TUNING.BUILDINGS.DECOR.NONE;
			EffectorValues noise = none1;
			BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("CrittersPassableBlock", 1, 1, "critter_passable_kanim", 30, 10f, tieR2, allMetals, 1600f, BuildLocationRule.Tile, none2, noise, 1f);
			buildingDef.Entombable = true;
			buildingDef.Floodable = false;
			buildingDef.AudioCategory = "Metal";
			buildingDef.ConstructionOffsetFilter = BuildingDef.ConstructionOffsetFilter_OneDown;

			buildingDef.IsFoundation = true;
			buildingDef.TileLayer = ObjectLayer.FoundationTile;
			buildingDef.ReplacementLayer = ObjectLayer.ReplacementTile;
			SoundEventVolumeCache.instance.AddVolume("door_internal_kanim", "Open_DoorInternal", TUNING.NOISE_POLLUTION.NOISY.TIER2);
			SoundEventVolumeCache.instance.AddVolume("door_internal_kanim", "Close_DoorInternal", TUNING.NOISE_POLLUTION.NOISY.TIER2);
			BuildingTemplates.CreateFoundationTileDef(buildingDef);
			return buildingDef;
		}

		public static List<LogicPorts.Port> CreateSingleInputPortList(CellOffset offset) => new List<LogicPorts.Port>()
  {
	LogicPorts.Port.InputPort(Door.OPEN_CLOSE_PORT_ID, offset, (string) STRINGS.BUILDINGS.PREFABS.DOOR.LOGIC_OPEN, (string) STRINGS.BUILDINGS.PREFABS.DOOR.LOGIC_OPEN_ACTIVE, (string) STRINGS.BUILDINGS.PREFABS.DOOR.LOGIC_OPEN_INACTIVE)
  };

		public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
		{ 
			GeneratedBuildings.MakeBuildingAlwaysOperational(go);
			BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
			go.AddTag(GameTags.FloorTiles);
			go.AddOrGet<ZoneTile>();
			go.AddOrGet<KBoxCollider2D>();
			go.AddOrGet<CritterPassable>();
			//var simCellOccupier = go.AddOrGet<SimCellOccupier>();
			//simCellOccupier.setGasImpermeable = false;
			//simCellOccupier.setOpaque = false;
			//simCellOccupier.setTransparent = true;
			//simCellOccupier.setLiquidImpermeable = false;
		}
		public override void DoPostConfigureComplete(GameObject go) => GeneratedBuildings.RemoveLoopingSounds(go);

		public override void DoPostConfigureUnderConstruction(GameObject go)
		{

		}
	}

}