
using FreeResourceBuildings;
using TUNING;
using UnityEngine;

public class ConveyourTrashcanConfig : IBuildingConfig
{ 

    public static string ID = ModStrings.ConveyourTrashcanID;
    public static string Name = ModStrings.ConveyourTrashcanName;
    public static string Description = ModStrings.ConveyourTrashcanDescription;
    public static string Effect = ModStrings.ConveyourTrashcanEffect;

    public override BuildingDef CreateBuildingDef()
    {
        float[] tieR3 = BUILDINGS.CONSTRUCTION_MASS_KG.TIER3;
        string[] allMetals = MATERIALS.ALL_METALS;
        EffectorValues none = NOISE_POLLUTION.NONE;
        EffectorValues tieR1 = BUILDINGS.DECOR.PENALTY.TIER1;
        EffectorValues noise = none;
        BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 1, 2, "conveyor_trashbin_kanim", 30, 30f, tieR3, allMetals, 1600f, BuildLocationRule.Anywhere, tieR1, noise);
        buildingDef.Floodable = false;
        buildingDef.Overheatable = false;
        buildingDef.ViewMode = OverlayModes.SolidConveyor.ID;
        buildingDef.AudioCategory = "Metal";
        buildingDef.InputConduitType = ConduitType.Solid;
        buildingDef.UtilityInputOffset = new CellOffset(0, 0);
        buildingDef.PermittedRotations = PermittedRotations.R360;
        GeneratedBuildings.RegisterWithOverlay(OverlayScreen.SolidConveyorIDs, "ConveyourTrashcan");
        return buildingDef;
    }

    public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
    {
        GeneratedBuildings.MakeBuildingAlwaysOperational(go);
        go.AddOrGet<ConveyourTrashcan>();
        go.AddOrGet<SolidConduitConsumer>();
        Storage defaultStorage = BuildingTemplates.CreateDefaultStorage(go);
        defaultStorage.capacityKg = 100f;
        defaultStorage.showInUI = true;
        defaultStorage.allowItemRemoval = true;
        go.AddOrGet<SimpleVent>();
    }

    public override void DoPostConfigureUnderConstruction(GameObject go)
    {
        base.DoPostConfigureUnderConstruction(go);
        // go.GetComponent<Constructable>().requiredSkillPerk = Db.Get().SkillPerks.ConveyorBuild.Id;
    }

    public override void DoPostConfigureComplete(GameObject go)
    {
        Prioritizable.AddRef(go);
        go.AddOrGet<Automatable>();
    }
}
