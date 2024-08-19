// Decompiled with JetBrains decompiler
// Type: BottleVanisherGasConfig
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 93E6D51E-7C50-4F44-83A7-82AAAF7248C3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll

using FreeResourceBuildings;
using TUNING;
using UnityEngine;

public class GasVanisherConfig : IBuildingConfig
{

    public const string ID = ModStrings.GasVanisherID;
    public const string Name = ModStrings.GasVanisherName;
    public const string Description = ModStrings.GasVanisherDescription;
    public const string Effect = ModStrings.GasVanisherEffect;

    public override BuildingDef CreateBuildingDef()
    {
        float[] tieR2_1 = BUILDINGS.CONSTRUCTION_MASS_KG.TIER2;
        string[] rawMinerals = MATERIALS.RAW_MINERALS;
        EffectorValues tieR1 = NOISE_POLLUTION.NOISY.TIER1;
        EffectorValues tieR2_2 = BUILDINGS.DECOR.PENALTY.TIER2;
        EffectorValues noise = tieR1;
        BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 1, 3, "gas_vanisher_kanim", 30, 60f, tieR2_1, rawMinerals, 1600f, BuildLocationRule.OnFloor, tieR2_2, noise);
        buildingDef.Floodable = false;
        buildingDef.AudioCategory = "Metal";
        buildingDef.Overheatable = false;
        buildingDef.PermittedRotations = PermittedRotations.FlipH;
        return buildingDef;
    }

    public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
    {
        Prioritizable.AddRef(go);
        Storage storage = go.AddOrGet<Storage>();
        storage.storageFilters = STORAGEFILTERS.GASES;
        storage.showInUI = true;
        storage.showDescriptor = true;
        storage.capacityKg = 200f;
        go.AddOrGet<TreeFilterable>();
        BottleVanisher bottleVanisher = go.AddOrGet<BottleVanisher>();
        bottleVanisher.isGasEmptier = true;
        bottleVanisher.emptyRate = 0.25f;
    }

    public override void DoPostConfigureComplete(GameObject go)
    {
    }
}
