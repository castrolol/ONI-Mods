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
    public class ProgrammableUnitConfig : IBuildingConfig
    {
        public const string ID = "ProgrammableUnit";
        public string _ID = "ProgrammableUnitConfig";
        public string scriptInfo = "";
        public Jint.Engine engine;
        public int outputs;
        public int inputs;

        public ProgrammableUnitConfig()
        {

        }

        public override BuildingDef CreateBuildingDef()
        {
            

            float[] tieR4 = BUILDINGS.CONSTRUCTION_MASS_KG.TIER4;
            string[] allMetals = MATERIALS.ALL_METALS;
            EffectorValues none1 = NOISE_POLLUTION.NONE;
            EffectorValues none2 = BUILDINGS.DECOR.NONE;
            EffectorValues noise = none1;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(_ID, 2, 3, "apothecary_kanim", 30, 120f, tieR4, allMetals, 800f, BuildLocationRule.OnFloor, none2, noise);
            buildingDef.RequiresPowerInput = true;
            buildingDef.EnergyConsumptionWhenActive = 240f;
            buildingDef.ExhaustKilowattsWhenActive = 0.25f;
            buildingDef.SelfHeatKilowattsWhenActive = 0.5f;

            buildingDef.LogicInputPorts = new List<LogicPorts.Port>();


            buildingDef.ViewMode = OverlayModes.Power.ID;
            buildingDef.AudioCategory = "Glass";
            buildingDef.AudioSize = "large";
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            Prioritizable.AddRef(go);
            go.AddOrGet<DropAllWorkable>();
            go.AddOrGet<BuildingComplete>().isManuallyOperated = true;
            Apothecary fabricator = go.AddOrGet<Apothecary>();
            BuildingTemplates.CreateComplexFabricatorStorage(go, (ComplexFabricator)fabricator);
            go.AddOrGet<ComplexFabricatorWorkable>();
            go.AddOrGet<FabricatorIngredientStatusManager>();
            go.AddOrGet<CopyBuildingSettings>();
        }

        public override void DoPostConfigureComplete(GameObject go) => go.AddOrGetDef<PoweredActiveStoppableController.Def>();
    }
}
