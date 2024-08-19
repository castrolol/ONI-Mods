// Decompiled with JetBrains decompiler
// Type: BottleEmptierConfig
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 93E6D51E-7C50-4F44-83A7-82AAAF7248C3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll

using FreeResourceBuildings;
using TUNING;
using UnityEngine;

public class BottleVanisherConfig : IBuildingConfig
{
    public const string ID = ModStrings.BottleVanisherID;
    public const string Name = ModStrings.BottleVanisherName;
    public const string Description = ModStrings.BottleVanisherDescription;
    public const string Effect = ModStrings.BottleVanisherEffect;

    public override BuildingDef CreateBuildingDef()
    {
        float[] tieR4 = BUILDINGS.CONSTRUCTION_MASS_KG.TIER4;
        string[] rawMinerals = MATERIALS.RAW_MINERALS;
        EffectorValues none = NOISE_POLLUTION.NONE;
        EffectorValues tieR1 = BUILDINGS.DECOR.PENALTY.TIER1;
        EffectorValues noise = none;
        BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 1, 3, "bottle_vanisher_kanim", 30, 10f, tieR4, rawMinerals, 1600f, BuildLocationRule.OnFloor, tieR1, noise);
        buildingDef.Floodable = false;
        buildingDef.AudioCategory = "Metal";
        buildingDef.Overheatable = false;
       // buildingDef.PermittedRotations = PermittedRotations.FlipH;
        return buildingDef;
    }

    public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
    {
        Prioritizable.AddRef(go);
        Storage storage = go.AddOrGet<Storage>();
        storage.storageFilters = STORAGEFILTERS.LIQUIDS;
        storage.showInUI = true;
        storage.showDescriptor = true;
        storage.capacityKg = 200f;
        go.AddOrGet<TreeFilterable>();
        go.AddOrGet<BottleVanisher>();
    }

    public override void DoPostConfigureComplete(GameObject go)
    {
    }
}
