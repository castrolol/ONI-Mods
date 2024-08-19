// Decompiled with JetBrains decompiler
// Type: SolidConduitInboxConfig
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 93E6D51E-7C50-4F44-83A7-82AAAF7248C3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll

using FreeResourceBuildings;
using FreeResourceBuildingsPatches;
using System.Collections.Generic;
using TUNING;
using UnityEngine;

public class FreeConveyourConfig : IBuildingConfig
{
    public static string ID = ModStrings.FreeConveyourID;
    public static string Name = ModStrings.FreeConveyourName;
    public static string Description = ModStrings.FreeConveyourDescription;
    public static string Effect = ModStrings.FreeConveyourEffect;

    public override BuildingDef CreateBuildingDef()
    {
        float[] tieR3 = BUILDINGS.CONSTRUCTION_MASS_KG.TIER3;
        string[] allMetals = MATERIALS.ALL_METALS;
        EffectorValues none = NOISE_POLLUTION.NONE;
        EffectorValues tieR1 = BUILDINGS.DECOR.PENALTY.TIER1;
        EffectorValues noise = none;
        BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 1, 2, "conveyor_generator_kanim", 100, 60f, tieR3, allMetals, 1600f, BuildLocationRule.Anywhere, tieR1, noise);
        //buildingDef.RequiresPowerInput = true;
        //buildingDef.EnergyConsumptionWhenActive = 120f;
        buildingDef.ExhaustKilowattsWhenActive = 0.0f;
        buildingDef.SelfHeatKilowattsWhenActive = 2f;
        buildingDef.Floodable = false;
        buildingDef.ViewMode = OverlayModes.SolidConveyor.ID;
        buildingDef.AudioCategory = "Metal";
        buildingDef.OutputConduitType = ConduitType.Solid;
        //buildingDef.PowerInputOffset = new CellOffset(0, 1);
        buildingDef.UtilityOutputOffset = new CellOffset(0, 0);
        buildingDef.PermittedRotations = PermittedRotations.R360;
        buildingDef.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 1));
        GeneratedBuildings.RegisterWithOverlay(OverlayScreen.SolidConveyorIDs, "SolidConduitInbox");
        return buildingDef;
    }

    public override void DoPostConfigureUnderConstruction(GameObject go) => go.GetComponent<Constructable>().requiredSkillPerk = Db.Get().SkillPerks.ConveyorBuild.Id;

    public override void DoPostConfigureComplete(GameObject go)
    {
        go.AddOrGet<LogicOperationalController>();
        Prioritizable.AddRef(go);
    
        go.AddOrGet<Automatable>();

        List<Tag> tagList = new List<Tag>();
        tagList.AddRange((IEnumerable<Tag>)STORAGEFILTERS.STORAGE_LOCKERS_STANDARD);
        tagList.AddRange((IEnumerable<Tag>)STORAGEFILTERS.FOOD);

        Storage storage = go.AddOrGet<Storage>();
        storage.capacityKg = 1000f;
        storage.showInUI = true;
        storage.showDescriptor = true;
        storage.storageFilters = tagList;
        storage.allowItemRemoval = false;
        storage.onlyTransferFromLowerPriority = true;
        storage.showCapacityStatusItem = true;
        storage.showCapacityAsMainStatus = true;

        go.AddOrGet<TreeFilterable>();
        go.AddOrGet<SolidConduitInbox>();
        go.AddOrGet<SolidConduitDispenser>(); 

        var filters = new List<Tag>();

        foreach (var tag in STORAGEFILTERS.NOT_EDIBLE_SOLIDS)
        {
            filters.Add(tag);
        }

        filters.Remove(GameTags.Seed);
        filters.Remove(GameTags.MedicalSupplies); // ok
        filters.Remove(GameTags.Clothes); // ok
        filters.Remove(GameTags.Egg);

        storage.storageFilters = filters;

        var gen = go.AddOrGet<StorageGenerator>();
        var modOptions = Mod.Options;
        gen.elementCount = modOptions.freeStorageElementsLimit;
        gen.singleItemCount = modOptions.freeStorageItemsLimit;
        gen.singleItemPerTick = 1;
    }
}
