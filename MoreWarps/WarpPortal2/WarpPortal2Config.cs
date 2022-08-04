// Decompiled with JetBrains decompiler
// Type: WarpPortalConfig
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1C7E9CCB-4AA8-44E2-BE45-38990AABF98E
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll

using TUNING;
using UnityEngine;

public class WarpPortal2Config : IBuildingConfig
{
    public const string ID = "CustomWarpPortal";

    public const string Name = "Name";
    public const string Description = "Description";
    public const string Effect = "Effect";

    public override BuildingDef CreateBuildingDef()
    { 
        EffectorValues tieR0_1 = TUNING.BUILDINGS.DECOR.BONUS.TIER0;
        EffectorValues tieR0_2 = TUNING.NOISE_POLLUTION.NOISY.TIER0; 
        EffectorValues decor = tieR0_1;
        EffectorValues noise = tieR0_2; 
        string[] rawMinerals = MATERIALS.RAW_MINERALS; 
        BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("CustomWarpPortal", 1, 1, "warp2_portal_sender_kanim", 30, 1f, new[] { 1f }, rawMinerals, 1600f, BuildLocationRule.OnFloor, decor, noise);
        buildingDef.Floodable = false;
        buildingDef.Overheatable = false;
        buildingDef.AudioCategory = "Metal";
        buildingDef.PermittedRotations = PermittedRotations.Unrotatable;
        return buildingDef;
    }
    public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
    {
        go.AddOrGet<Operational>();
        go.AddOrGet<Notifier>();
        go.AddOrGet<WarpPortal2>(); 
        go.AddOrGet<LoopingSounds>();
        var ownable = go.AddOrGet<Ownable>();
        ownable.slotID = Db.Get().AssignableSlots.WarpPortal.Id;
        go.AddOrGet<Prioritizable>();
        KBatchedAnimController kbatchedAnimController = go.AddOrGet<KBatchedAnimController>();
        kbatchedAnimController.sceneLayer = Grid.SceneLayer.BuildingBack;
        kbatchedAnimController.fgLayer = Grid.SceneLayer.BuildingFront;
        go.AddTag(GameTags.Gravitas);        
        go.AddTag(GameTags.WarpTech);
        go.GetComponent<Deconstructable>().SetAllowDeconstruction(false);
       

    }

    public override void DoPostConfigureComplete(GameObject go)
    {
        go.AddTag(GameTags.Gravitas);
        go.AddTag(GameTags.WarpTech);
        go.GetComponent<Deconstructable>().SetAllowDeconstruction(false);
    }




    // public string[] GetDlcIds() => DlcManager.AVAILABLE_EXPANSION1_ONLY;

    //public GameObject CreatePrefab()
    //{
    //    string name = (string)STRINGS.BUILDINGS.PREFABS.WARPPORTAL.NAME;
    //    string desc = (string)STRINGS.BUILDINGS.PREFABS.WARPPORTAL.DESC;
    //    EffectorValues tieR0_1 = TUNING.BUILDINGS.DECOR.BONUS.TIER0;
    //    EffectorValues tieR0_2 = TUNING.NOISE_POLLUTION.NOISY.TIER0;
    //    KAnimFile anim = Assets.GetAnim((HashedString)"warp2_portal_sender_kanim");
    //    EffectorValues decor = tieR0_1;
    //    EffectorValues noise = tieR0_2;
    //    GameObject placedEntity = EntityTemplates.CreatePlacedEntity("WarpPortal2", name, desc, 2000f, anim, "idle", Grid.SceneLayer.Building, 3, 3, decor, noise);
    //    placedEntity.AddTag(GameTags.NotRoomAssignable);
    //    placedEntity.AddTag(GameTags.WarpTech);
    //    placedEntity.AddTag(GameTags.Gravitas);
    //    PrimaryElement component = placedEntity.GetComponent<PrimaryElement>();
    //    component.SetElement(SimHashes.Unobtanium);
    //    component.Temperature = 294.15f;
    //    placedEntity.AddOrGet<Operational>();
    //    placedEntity.AddOrGet<Notifier>();
    //    placedEntity.AddOrGet<WarpPortal2>();
    //    placedEntity.AddOrGet<LoreBearer>();
    //    placedEntity.AddOrGet<LoopingSounds>();
    //    placedEntity.AddOrGet<Ownable>().tintWhenUnassigned = false;
    //    placedEntity.AddOrGet<Prioritizable>();
    //    KBatchedAnimController kbatchedAnimController = placedEntity.AddOrGet<KBatchedAnimController>();
    //    kbatchedAnimController.sceneLayer = Grid.SceneLayer.BuildingBack;
    //    kbatchedAnimController.fgLayer = Grid.SceneLayer.BuildingFront;
    //    return placedEntity;
    //}

    //public void OnPrefabInit(GameObject inst)
    //{
    //    inst.GetComponent<WarpPortal2>().workLayer = Grid.SceneLayer.Building;
    //    inst.GetComponent<Ownable>().slotID = Db.Get().AssignableSlots.WarpPortal.Id;
    //    inst.GetComponent<OccupyArea>().objectLayers = new ObjectLayer[1]
    //    {
    //  ObjectLayer.Building
    //    };
    //    inst.GetComponent<Deconstructable>();
    //}


}
