using FreeResourceBuildingsPatches;
using Klei.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUNING;
using UnityEngine;

namespace FreeResourceBuildings
{
    public class MagicFishFeederConfig : IBuildingConfig
    {
        public const string ID = ModStrings.MagicFishFeederID;
        public const string Name = ModStrings.MagicFishFeederName;
        public const string Description = ModStrings.MagicFishFeederDescription;
        public const string Effect = ModStrings.MagicFishFeederEffect;

        public override BuildingDef CreateBuildingDef()
        {
            var options = ModBuildingDefs.Instance.GetDefaultOptions(
              TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER3,
              NOISE_POLLUTION.NONE,
              TUNING.BUILDINGS.DECOR.PENALTY.TIER2,
              "Metal",
              MATERIALS.RAW_METALS
            );

            float[] weight = options.Mass;
            string[] resources = options.Resources;
            EffectorValues tieR1 = options.Decor;
            EffectorValues noise = options.Noise;
            var time = options.ConstructionTime;
            var audio = options.AudioCategory;

            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 1, 3, "magic_fish_feeder_kanim", 30, time, weight, resources, 1600f, BuildLocationRule.Anywhere, tieR1, noise);
            buildingDef.AudioCategory = "Metal";
            buildingDef.Entombable = true;
            buildingDef.Floodable = true;
            buildingDef.ForegroundLayer = Grid.SceneLayer.TileMain;
            return buildingDef;
        }

        public override void DoPostConfigureUnderConstruction(GameObject go)
        {
        }


        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            Prioritizable.AddRef(go);
            Storage storage1 = go.AddOrGet<Storage>();
            storage1.capacityKg = 200f;
            storage1.showInUI = true;
            storage1.showDescriptor = true;
            storage1.allowItemRemoval = false;
            storage1.allowSettingOnlyFetchMarkedItems = false;
            storage1.showCapacityStatusItem = true;
            storage1.showCapacityAsMainStatus = true;
            Storage storage2 = go.AddComponent<Storage>();
            storage2.capacityKg = 200f;
            storage2.showInUI = true;
            storage2.showDescriptor = true;
            storage2.allowItemRemoval = false;
            go.AddOrGet<StorageLocker>().choreTypeID = Db.Get().ChoreTypes.RanchingFetch.Id;
            go.AddOrGet<UserNameable>();
            Effect resource = new Effect("AteFromFeeder", (string)STRINGS.CREATURES.MODIFIERS.ATE_FROM_FEEDER.NAME, (string)STRINGS.CREATURES.MODIFIERS.ATE_FROM_FEEDER.TOOLTIP, 600f, true, false, false);
            resource.Add(new AttributeModifier(Db.Get().Amounts.Wildness.deltaAttribute.Id, -0.03333334f, (string)STRINGS.CREATURES.MODIFIERS.ATE_FROM_FEEDER.NAME));
            resource.Add(new AttributeModifier(Db.Get().CritterAttributes.Happiness.Id, 2f, (string)STRINGS.CREATURES.MODIFIERS.ATE_FROM_FEEDER.NAME));
            Db.Get().effects.Add(resource);
            go.AddOrGet<TreeFilterable>();
            go.AddOrGet<CreatureFeeder>().effectId = resource.Id;
            var gen = go.AddOrGet<MagicFishFeeder>();

            //gen.storage = storage2;

            var modOptions = Mod.Options;
            gen.elementCount = modOptions.magicFeederElementsLimit;
            gen.singleItemCount = modOptions.magicFeederItemsLimit;
            gen.singleItemPerTick = 1;
        }
         

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGetDef<StorageController.Def>();
            go.AddOrGetDef<FishFeeder.Def>();
            go.AddOrGetDef<MakeBaseSolid.Def>().solidOffsets = new CellOffset[1]
            {
      new CellOffset(0, 0)
            };
            SymbolOverrideControllerUtil.AddToPrefab(go);



        }

        public override void ConfigurePost(BuildingDef def)
        {
            List<Tag> tagList = new List<Tag>();
            Tag[] target_species = new Tag[1]
            {
      GameTags.Creatures.Species.PacuSpecies
            };
            foreach (KeyValuePair<Tag, Diet> collectDiet in DietManager.CollectDiets(target_species))
                tagList.Add(collectDiet.Key);
            def.BuildingComplete.GetComponent<Storage>().storageFilters = tagList;



            var feeder = def.BuildingComplete.GetComponent<FishFeeder>();

            Debug.Log($"FEEDER {feeder}");
        }
    }
}