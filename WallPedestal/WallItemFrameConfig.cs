using System.Collections.Generic;
using TUNING;
using UnityEngine;

namespace LuxuryDecoration
{
    internal class WallItemFrameConfig : IBuildingConfig
    {
        public const string ID = "WallItemFrame";
        public static LocString NAME ="Wall Pedestal";
        public static LocString DESC = "Perception can be drastically changed by a bit of thoughtful presentation. <b> Can be placed on the wall </b>";
        public static LocString EFFECT = ("Displays a single object, doubling its " + STRINGS.UI.FormatAsLink("Decor", "DECOR") + " value.\n\nObjects with negative Decor will gain some positive Decor when displayed.");


        public override BuildingDef CreateBuildingDef()
        {
            float[] tieR2 = BUILDINGS.CONSTRUCTION_MASS_KG.TIER2;
            string[] rawMinerals = MATERIALS.RAW_MINERALS;
            EffectorValues none = NOISE_POLLUTION.NONE;
            EffectorValues tieR0 = BUILDINGS.DECOR.BONUS.TIER0;
            EffectorValues noise = none;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("WallItemFrame", 1, 1, "wall_pedestal_kanim", 10, 30f, tieR2, rawMinerals, 800f, BuildLocationRule.Anywhere, tieR0, noise);
            buildingDef.DefaultAnimState = "pedestal";
            buildingDef.Floodable = false;
            buildingDef.Overheatable = false;
            buildingDef.ViewMode = OverlayModes.Decor.ID;
            buildingDef.AudioCategory = "Glass";
            buildingDef.AudioSize = "small";
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<Storage>().SetDefaultStoredItemModifiers(new List<Storage.StoredItemModifier>((IEnumerable<Storage.StoredItemModifier>)new Storage.StoredItemModifier[2]
            {
      Storage.StoredItemModifier.Seal,
      Storage.StoredItemModifier.Preserve
            }));
            Prioritizable.AddRef(go);
            SingleEntityReceptacle entityReceptacle = go.AddOrGet<SingleEntityReceptacle>();
            entityReceptacle.AddDepositTag(GameTags.PedestalDisplayable);
            entityReceptacle.occupyingObjectRelativePosition = new Vector3(0.0f, .35f, -1f);
            go.AddOrGet<DecorProvider>();
            go.AddOrGet<ItemPedestal>();
            go.GetComponent<KPrefabID>().AddTag(GameTags.Decoration);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
        }
    }
}
