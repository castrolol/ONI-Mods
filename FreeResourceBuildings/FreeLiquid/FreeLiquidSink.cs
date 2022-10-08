using STRINGS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUNING;
using UnityEngine;


namespace FreeResourceBuildings
{
	public class FreeLiquidSinkConfig : IBuildingConfig
	{
		public const string ID = ModStrings.FreeLiquidSinkID;
		public const string Name = ModStrings.FreeLiquidSinkName;
		public const string Description = ModStrings.FreeLiquidSinkDescription;
		public const string Effect = ModStrings.FreeLiquidSinkEffect;
		
		public override BuildingDef CreateBuildingDef()
		{
			var options = ModBuildingDefs.Instance.GetDefaultOptions(
		  TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2,
		  NOISE_POLLUTION.NOISY.TIER4,
		  TUNING.BUILDINGS.DECOR.PENALTY.TIER2,
		  "HollowMetal",
		  MATERIALS.REFINED_METALS
		);

			float[] weight = options.Mass;
			string[] resources = options.Resources;
			EffectorValues tieR1 = options.Decor;
			EffectorValues noise = options.Noise;
			var time = options.ConstructionTime;
			var audio = options.AudioCategory;

			var buildingDef = BuildingTemplates.CreateBuildingDef(
				id: ID,
				width: 1,
				height: 1,
				anim: "sink_liquids_kanim",
				hitpoints: TUNING.BUILDINGS.HITPOINTS.TIER2,
				construction_time: time,
				construction_mass: weight,
				construction_materials: resources,
				melting_point: TUNING.BUILDINGS.MELTING_POINT_KELVIN.TIER4,
				build_location_rule: BuildLocationRule.Anywhere,
				decor: tieR1,
				noise: noise
				);
			buildingDef.Overheatable = false;
			buildingDef.InputConduitType = ConduitType.Liquid;
			buildingDef.Floodable = false;
			buildingDef.ViewMode = OverlayModes.LiquidConduits.ID;
			buildingDef.AudioCategory = audio;
			buildingDef.PermittedRotations = PermittedRotations.Unrotatable;
			buildingDef.UtilityInputOffset = new CellOffset(0, 0);
			buildingDef.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));
			GeneratedBuildings.RegisterWithOverlay(OverlayScreen.LiquidVentIDs, ID);
			return buildingDef;
		}

		public override void DoPostConfigureComplete(GameObject go)
		{
			go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery, false);

			go.AddOrGet<LogicOperationalController>();

			var sink = go.AddOrGet<FreeSink>();
			sink.Type = ConduitType.Liquid;

			UnityEngine.Object.DestroyImmediate(go.GetComponent<RequireInputs>(), true);
			UnityEngine.Object.DestroyImmediate(go.GetComponent<ConduitConsumer>(), true);

			go.AddOrGetDef<OperationalController.Def>();
			go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayInFrontOfConduits, false);
		}
	}
}
