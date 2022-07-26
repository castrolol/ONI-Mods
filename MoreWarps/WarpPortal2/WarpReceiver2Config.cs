// Decompiled with JetBrains decompiler
// Type: WarpReceiverConfig
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1C7E9CCB-4AA8-44E2-BE45-38990AABF98E
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class WarpReceiver2Config : IEntityConfig
{
    public static string ID = "WarpReceiver2";

    public string[] GetDlcIds() => DlcManager.AVAILABLE_EXPANSION1_ONLY;

    public GameObject CreatePrefab()
    {
        string id = WarpReceiver2Config.ID;
        string name = (string)STRINGS.BUILDINGS.PREFABS.WARPRECEIVER.NAME;
        string desc = (string)STRINGS.BUILDINGS.PREFABS.WARPRECEIVER.DESC;
        EffectorValues tieR0_1 = TUNING.BUILDINGS.DECOR.BONUS.TIER0;
        EffectorValues tieR0_2 = TUNING.NOISE_POLLUTION.NOISY.TIER0;
        KAnimFile anim = Assets.GetAnim((HashedString)"warp2_portal_receiver_kanim");
        EffectorValues decor = tieR0_1;
        EffectorValues noise = tieR0_2;
        GameObject placedEntity = EntityTemplates.CreatePlacedEntity(id, name, desc, 2000f, anim, "idle", Grid.SceneLayer.Building, 3, 3, decor, noise);
        placedEntity.AddTag(GameTags.NotRoomAssignable);
        placedEntity.AddTag(GameTags.WarpTech);
        placedEntity.AddTag(GameTags.Gravitas);
        PrimaryElement component = placedEntity.GetComponent<PrimaryElement>();
        component.SetElement(SimHashes.Unobtanium);
        component.Temperature = 294.15f;
        placedEntity.AddOrGet<Operational>();
        placedEntity.AddOrGet<Notifier>();
        placedEntity.AddOrGet<WarpReceiver2>();
        placedEntity.AddOrGet<LoreBearer>();
        placedEntity.AddOrGet<LoopingSounds>();
        placedEntity.AddOrGet<Prioritizable>();
        KBatchedAnimController kbatchedAnimController = placedEntity.AddOrGet<KBatchedAnimController>();
        kbatchedAnimController.sceneLayer = Grid.SceneLayer.BuildingBack;
        kbatchedAnimController.fgLayer = Grid.SceneLayer.BuildingFront;
        return placedEntity;
    }

    public void OnPrefabInit(GameObject inst)
    {
        inst.GetComponent<WarpReceiver2>().workLayer = Grid.SceneLayer.Building;
        inst.GetComponent<OccupyArea>().objectLayers = new ObjectLayer[1]
        {
      ObjectLayer.Building
        };
        inst.GetComponent<Deconstructable>();
    }

    public void OnSpawn(GameObject inst)
    {
    }
}
