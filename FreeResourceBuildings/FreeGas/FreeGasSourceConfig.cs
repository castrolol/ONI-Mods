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
	public class FreeGasSourceConfig : IBuildingConfig
	{
		public const string ID = ModStrings.FreeGasSourceID;
		public const string Name = ModStrings.FreeGasSourceName;
		public const string Description = ModStrings.FreeGasSourceDescription;
		public const string Effect = ModStrings.FreeGasSourceEffect;

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
				anim: "free_gases_kanim",
				hitpoints: TUNING.BUILDINGS.HITPOINTS.TIER2,
				construction_time: time,
				construction_mass: weight,
				construction_materials: resources,
				melting_point: TUNING.BUILDINGS.MELTING_POINT_KELVIN.TIER4,
				build_location_rule: BuildLocationRule.Anywhere,
				decor: tieR1,
				noise: noise,
				0.2f
			);
			buildingDef.Overheatable = false;
			buildingDef.Entombable = false;
			buildingDef.Floodable = false;
			buildingDef.OutputConduitType = ConduitType.Gas;
			buildingDef.ViewMode = OverlayModes.GasConduits.ID;
			buildingDef.AudioCategory = audio;
			buildingDef.PermittedRotations = PermittedRotations.Unrotatable;
			buildingDef.UtilityOutputOffset = new CellOffset(0, 0);
			buildingDef.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));
			GeneratedBuildings.RegisterWithOverlay(OverlayScreen.GasVentIDs, ID);
			return buildingDef;
		}

		public override void DoPostConfigureComplete(GameObject go)
		{
			go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery, false);

			go.AddOrGet<LogicOperationalController>();

			var src = go.AddOrGet<FreeSource>();
			src.conduitType = ConduitType.Gas;

			go.AddOrGet<FlowSlider>();
			go.AddOrGet<TemperatureSlider>();
			var filterable = go.AddOrGet<Filterable>();
			filterable.filterElementState = Filterable.ElementState.Gas;

			go.AddOrGetDef<OperationalController.Def>();
			go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayInFrontOfConduits, false);
		}
	}
}
