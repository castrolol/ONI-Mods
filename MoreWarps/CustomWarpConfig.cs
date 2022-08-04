using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreWarps
{
    // Decompiled with JetBrains decompiler
    // Type: WarpPortalConfig
    // Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
    // MVID: 1C7E9CCB-4AA8-44E2-BE45-38990AABF98E
    // Assembly location: C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll

    using TUNING;
    using UnityEngine;

    public class CustomWarpConfig : IBuildingConfig
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
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("CustomWarpPortal", 1, 2, "storagelocker_kanim", 30, 1f, new[] { 1f }, rawMinerals, 1600f, BuildLocationRule.OnFloor, decor, noise);
            buildingDef.Floodable = false;
            buildingDef.Overheatable = false;
            buildingDef.AudioCategory = "Metal";
            buildingDef.PermittedRotations = PermittedRotations.Unrotatable;
            return buildingDef;
        }
        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {

            go.AddOrGet<CustomWarpWorkable>();
             
            var ownable = go.AddOrGet<Ownable>();
            ownable.slotID = Db.Get().AssignableSlots.WarpPortal.Id;
           
           

        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            
        }

         


    }
}