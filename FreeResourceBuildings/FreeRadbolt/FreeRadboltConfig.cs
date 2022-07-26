using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUNING;
using UnityEngine;
namespace FreeResourceBuildings
{
	internal class FreeRadboltConfig : IBuildingConfig
	{
		public const string ID = ModStrings.FreeRadboltID;
		public const string Name = ModStrings.FreeRadboltName;
		public const string Description = ModStrings.FreeRadboltDescription;
		public const string Effect = ModStrings.FreeRadboltEffect;

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

			BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 1, 2, "free_radbolt_kanim", 30, time, weight, resources, 1600f, BuildLocationRule.OnFloor, tieR1, noise);

			buildingDef.Floodable = false;
			buildingDef.AudioCategory = audio;
			buildingDef.Overheatable = false;
			buildingDef.ViewMode = OverlayModes.Radiation.ID;
			buildingDef.PermittedRotations = PermittedRotations.R360;
			buildingDef.UseHighEnergyParticleOutputPort = true;
			buildingDef.HighEnergyParticleOutputOffset = new CellOffset(0, 1);
			buildingDef.RequiresPowerInput = false;
			//buildingDef.PowerInputOffset = new CellOffset(0, 0);
			//buildingDef.EnergyConsumptionWhenActive = 480f;
			//buildingDef.ExhaustKilowattsWhenActive = 1f;
			//buildingDef.SelfHeatKilowattsWhenActive = 4f;
			buildingDef.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));

			GeneratedBuildings.RegisterWithOverlay(OverlayScreen.RadiationIDs, "HighEnergyParticleSpawner");
			buildingDef.Deprecated = !Sim.IsRadiationEnabled();
			return buildingDef;
		}

		public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
		{
			go.AddOrGet<HighEnergyParticleStorage>().capacity = 500f;
			go.AddOrGet<LoopingSounds>();
			go.AddOrGet<RadBoltSilder>();
			go.AddOrGet<LogicOperationalController>();
			FreeRadbolt energyParticleSpawner = go.AddOrGet<FreeRadbolt>();
			energyParticleSpawner.minLaunchInterval = 2f;
			energyParticleSpawner.radiationSampleRate = 0.2f;
			energyParticleSpawner.minSlider = 50;
			energyParticleSpawner.maxSlider = 500;
		}


		public override void DoPostConfigureComplete(GameObject go)
		{
		}
	}
}
