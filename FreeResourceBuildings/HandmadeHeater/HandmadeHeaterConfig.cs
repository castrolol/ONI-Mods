using FreeResourceBuildings;
using TUNING;
using UnityEngine;

public class HandmadeHeaterConfig : IBuildingConfig
{
    public const string ID = ModStrings.HandmadeHeaterID;
    public const string Name = ModStrings.HandmadeHeaterName;
    public const string Description = ModStrings.HandmadeHeaterDescription;
    public const string Effect = ModStrings.HandmadeHeaterEffect;

    public const int RANGE_X = 4;
    public const int RANGE_Y = 3;
    // public static Tag FUEL_TAG = (Tag)SimHashes.WoodLog.ToString();
    //public const float FUEL_CONSUMPTION_RATE = 0.025f;
    //public const float FUEL_CONSTRUCTION_MASS = 5f;
    //public const float FUEL_CAPACITY = 45f;
    //public const float EXHAUST_RATE = 0.004f;
    //public const SimHashes EXHAUST_TAG = SimHashes.CarbonDioxide;
    //private const float EXHAUST_TEMPERATURE = 303.15f;
    public static readonly EffectorValues DECOR_ON = BUILDINGS.DECOR.BONUS.TIER3;
    public static readonly EffectorValues DECOR_OFF = BUILDINGS.DECOR.NONE;

    public override string[] GetDlcIds() => DlcManager.AVAILABLE_DLC_2;

    public override BuildingDef CreateBuildingDef()
    {
        float[] tieR2 = BUILDINGS.CONSTRUCTION_MASS_KG.TIER2;
        string[] rawMetals = MATERIALS.RAW_METALS;
        EffectorValues none = NOISE_POLLUTION.NONE;
        EffectorValues decorOn = HandmadeHeaterConfig.DECOR_ON;
        EffectorValues noise = none;
        BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 1, 2, "handmade_heater_kanim", 100, 10f, tieR2, rawMetals, 9999f, BuildLocationRule.OnFloor, decorOn, noise, 0.1f);
        buildingDef.Floodable = true;
        buildingDef.Entombable = true;
        buildingDef.ExhaustKilowattsWhenActive = 0.0f;
        buildingDef.SelfHeatKilowattsWhenActive = 0.0f;
        buildingDef.ViewMode = OverlayModes.Temperature.ID;
        buildingDef.AudioCategory = "Metal";
        buildingDef.AudioSize = "small";
        buildingDef.UtilityInputOffset = new CellOffset(0, 0);
        buildingDef.UtilityOutputOffset = new CellOffset(0, 0);
        buildingDef.DefaultAnimState = "on";
        buildingDef.OverheatTemperature = 10000f;
        buildingDef.Overheatable = false;
        buildingDef.POIUnlockable = true;
        buildingDef.ShowInBuildMenu = true;
        return buildingDef;
    }

    public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
    {
        go.AddOrGet<LoopingSounds>();
        go.AddOrGet<BuildingComplete>().isManuallyOperated = true;

        KPrefabID component = go.GetComponent<KPrefabID>();
        component.AddTag(RoomConstraints.ConstraintTags.WarmingStation);
        component.AddTag(RoomConstraints.ConstraintTags.Decoration);
        
        //Storage storage = go.AddOrGet<Storage>();
        //storage.capacityKg = 45f;
        //storage.showInUI = true;
        //storage.allowItemRemoval = false;
        //storage.SetDefaultStoredItemModifiers(Storage.StandardInsulatedStorage);
        
        //ManualDeliveryKG manualDeliveryKg = go.AddOrGet<ManualDeliveryKG>();
        //manualDeliveryKg.capacity = 45f;
        //manualDeliveryKg.SetStorage(storage);
        //manualDeliveryKg.requestedItemTag = HandmadeHeaterConfig.FUEL_TAG;
        //manualDeliveryKg.refillMass = 18f;
        //manualDeliveryKg.choreTypeIDHash = Db.Get().ChoreTypes.FetchCritical.IdHash;
        //manualDeliveryKg.MinimumMass = 0.025f;

        //ElementConverter elementConverter = go.AddOrGet<ElementConverter>();
        //elementConverter.consumedElements = new ElementConverter.ConsumedElement[1]
        //{
        //    new ElementConverter.ConsumedElement(HandmadeHeaterConfig.FUEL_TAG, 0.025f)
        //};
        //elementConverter.outputElements = new ElementConverter.OutputElement[1]
        //{
        //    new ElementConverter.OutputElement(0.004f, SimHashes.CarbonDioxide, 303.15f, outputElementOffsety: 1f)
        //};
        
        this.AddVisualizer(go);

        Operational operational = go.AddOrGet<Operational>();
        
        Light2D light2D = go.AddOrGet<Light2D>();
        light2D.Range = 6f;
        light2D.Color = new Color(0.8f, 0.6f, 0.0f, 1f);
        light2D.Lux = 450;

        HandmadeHeater.Def def1 = go.AddOrGetDef<HandmadeHeater.Def>();
        //def1.fuelTag = HandmadeHeaterConfig.FUEL_TAG;
         // def1.initialFuelMass = 5f;
        
        WarmthProvider.Def def2 = go.AddOrGetDef<WarmthProvider.Def>();
        def2.RangeMax = new Vector2I(4, 3);
        def2.RangeMin = new Vector2I(-4, 0);

        go.AddOrGetDef<ColdImmunityProvider.Def>().range = new CellOffset[2][]
        {
            new CellOffset[2]
            {
                new CellOffset(-1, 0),
                new CellOffset(1, 0)
            },
            new CellOffset[1]{ 
                new CellOffset(0, 0) 
            }
        };

        DirectVolumeHeater directVolumeHeater = go.AddOrGet<DirectVolumeHeater>();
        directVolumeHeater.operational = operational;
        directVolumeHeater.DTUs = 20_000f;
        directVolumeHeater.width = 9;
        directVolumeHeater.height = 4;
        directVolumeHeater.maximumExternalTemperature = 343.15f;

   
    }

    public override void DoPostConfigurePreview(BuildingDef def, GameObject go) => this.AddVisualizer(go);

    public override void DoPostConfigureUnderConstruction(GameObject go) => this.AddVisualizer(go);

    public override void DoPostConfigureComplete(GameObject go)
    {
        go.AddOrGet<HeaterTempSlider>();
    }

    private void AddVisualizer(GameObject go)
    {
        RangeVisualizer rangeVisualizer = go.AddOrGet<RangeVisualizer>();
        rangeVisualizer.RangeMax = new Vector2I(4, 3);
        rangeVisualizer.RangeMin = new Vector2I(-4, 0);
        rangeVisualizer.BlockingTileVisible = false;
        go.AddOrGet<EntityCellVisualizer>().AddPort(EntityCellVisualizer.Ports.HeatSource, new CellOffset());
        

     }
}
