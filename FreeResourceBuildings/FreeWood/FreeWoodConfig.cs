// Decompiled with JetBrains decompiler
// Type: WoodStorageConfig
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 93E6D51E-7C50-4F44-83A7-82AAAF7248C3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll

using FreeResourceBuildings;
using FreeResourceBuildingsPatches;
using System.Collections.Generic;
using TUNING;
using UnityEngine;

public class FreeWoodConfig : IBuildingConfig
{

    public static string ID = ModStrings.FreeWoodID;
    public static string Name = ModStrings.FreeWoodName;
    public static string Description = ModStrings.FreeWoodDescription;
    public static string Effect = ModStrings.FreeWoodEffect;

    public override string[] GetRequiredDlcIds() => DlcManager.DLC2;

    public override BuildingDef CreateBuildingDef()
    {
        float[] tieR4 = BUILDINGS.CONSTRUCTION_MASS_KG.TIER4;
        string[] woods = MATERIALS.WOODS;
        EffectorValues none = NOISE_POLLUTION.NONE;
        EffectorValues tieR1 = BUILDINGS.DECOR.BONUS.TIER1;
        EffectorValues noise = none;
        BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 3, 2, "storageWood_kanim", 30, 10f, tieR4, woods, 1600f, BuildLocationRule.OnFloor, tieR1, noise);
        buildingDef.Floodable = false;
        buildingDef.AudioCategory = "Metal";
        buildingDef.Overheatable = false;
        //buildingDef.ShowInBuildMenu = false;
        return buildingDef;
    }

    public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
    {
        SoundEventVolumeCache.instance.AddVolume("storagelocker_kanim", "StorageLocker_Hit_metallic_low", NOISE_POLLUTION.NOISY.TIER1);
        Prioritizable.AddRef(go);
        Storage storage = go.AddOrGet<Storage>();
        storage.showInUI = true;
        storage.allowItemRemoval = true;
        storage.showDescriptor = true;
        storage.storageFilters = new List<Tag>()
    {
      GameTags.Organics
    };
        storage.storageFullMargin = STORAGE.STORAGE_LOCKER_FILLED_MARGIN;
        storage.fetchCategory = Storage.FetchCategory.GeneralStorage;
        storage.showCapacityStatusItem = true;
        storage.showCapacityAsMainStatus = true;
        storage.capacityKg = 40000f;
        go.AddOrGet<StorageMeter>();
        go.AddOrGetDef<RocketUsageRestriction.Def>();
        //ManualDeliveryKG manualDeliveryKg = go.AddOrGet<ManualDeliveryKG>();
        //manualDeliveryKg.SetStorage(storage);
        //manualDeliveryKg.RequestedItemTag = "WoodLog".ToTag();
        //manualDeliveryKg.capacity = storage.Capacity();
        //manualDeliveryKg.refillMass = storage.Capacity();
        //manualDeliveryKg.choreTypeIDHash = Db.Get().ChoreTypes.StorageFetch.IdHash;


        go.AddOrGet<FreeWoodStorage>();

    }

    public override void DoPostConfigureComplete(GameObject go) => go.AddOrGetDef<StorageController.Def>();
}
