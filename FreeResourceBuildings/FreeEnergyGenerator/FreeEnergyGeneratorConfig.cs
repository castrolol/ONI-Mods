using KSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUNING;
using UnityEngine;

namespace FreeResourceBuildings
{

	[SerializationConfig(MemberSerialization.OptIn)]
	public class FreeEnergyGeneratorConfig : IBuildingConfig
	{
        public static string ID = ModStrings.FreeEnergyGeneratorID;

        public override BuildingDef CreateBuildingDef()
		{
            var options = ModBuildingDefs.Instance.GetDefaultOptions(
              BUILDINGS.CONSTRUCTION_MASS_KG.TIER2,
              NOISE_POLLUTION.NOISY.TIER4,
              BUILDINGS.DECOR.PENALTY.TIER2,
              "HollowMetal",
              MATERIALS.REFINED_METALS
          );

            float[] weight = options.Mass;
            string[] resources = options.Resources;
            EffectorValues tieR1 = options.Decor;
            EffectorValues noise = options.Noise;
            var time = options.ConstructionTime;
            var audio = options.AudioCategory;


            BuildingDef def = BuildingTemplates.CreateBuildingDef(
                ID, 
                1, 1, 
                "free_energy_kanim", 
                100,
                time,
                weight,
                resources, 
                2400f, 
                BuildLocationRule.Anywhere, 
                BUILDINGS.DECOR.BONUS.TIER5, 
                noise
            );
            def.Overheatable = false;
            def.Floodable = false;
            def.Entombable = false;
            def.GeneratorWattageRating = 2000f; //267
            def.GeneratorBaseCapacity = 1000f;
            def.ExhaustKilowattsWhenActive = 8f;
            def.SelfHeatKilowattsWhenActive = BUILDINGS.SELF_HEAT_KILOWATTS.TIER0;
            def.ViewMode = OverlayModes.Power.ID;
            def.AudioCategory = audio;
            def.UseStructureTemperature = false;

            //def.UtilityInputOffset = new CellOffset(0, 0);
            //def.UtilityOutputOffset = new CellOffset(0, 1);
            def.RequiresPowerOutput = true;
            def.PowerOutputOffset = new CellOffset(0, 0);
            //def.InputConduitType = ConduitType.Gas;
            //def.OutputConduitType = ConduitType.Gas;
            def.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));
            return def;
        }



 

         public override void DoPostConfigureComplete(GameObject go)
		{
            go.AddOrGet<LogicOperationalController>();
            go.AddOrGetDef<PoweredActiveController.Def>();
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
         

            var generator = go.AddOrGet<FreeEnergyGenerator>();
           

             
            Tinkerable.MakePowerTinkerable(go);
        }

    }
}