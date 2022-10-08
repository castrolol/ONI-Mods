using FreeResourceBuildingsPatches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUNING;
using UnityEngine;

namespace FreeResourceBuildings
{
	public class MagicFeederConfig : IBuildingConfig
	{
		public const string ID = ModStrings.MagicFeederID;
		public const string Name = ModStrings.MagicFeederName;
		public const string Description = ModStrings.MagicFeederDescription;
		public const string Effect = ModStrings.MagicFeederEffect;

		public override BuildingDef CreateBuildingDef()
		{
			var options = ModBuildingDefs.Instance.GetDefaultOptions(
			  TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER3,
			  NOISE_POLLUTION.NONE,
			  TUNING.BUILDINGS.DECOR.PENALTY.TIER2,
			  "Metal",
			  MATERIALS.RAW_METALS
			);

			float[] weight = options.Mass;
			string[] resources = options.Resources;
			EffectorValues tieR1 = options.Decor;
			EffectorValues noise = options.Noise;
			var time = options.ConstructionTime;
			var audio = options.AudioCategory;

			BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 1, 2, "magic_feeder_kanim", 30, time, weight, resources, 1600f, BuildLocationRule.OnFloor, tieR1, noise);
			buildingDef.AudioCategory = audio;
			return buildingDef;
		}


		public override void DoPostConfigureUnderConstruction(GameObject go)
		{
		}

		public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
		{
			Prioritizable.AddRef(go);
		//	go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.CreatureFeeder);
			Storage storage = go.AddOrGet<Storage>();
			storage.capacityKg = 2000f;
			storage.showInUI = true;
			storage.showDescriptor = true;
			storage.allowItemRemoval = false;
			storage.allowSettingOnlyFetchMarkedItems = false;
			storage.showCapacityStatusItem = true;
			storage.showCapacityAsMainStatus = true;
			go.AddOrGet<StorageLocker>().choreTypeID = Db.Get().ChoreTypes.RanchingFetch.Id;
			var gen = go.AddOrGet<FreeStorage>();
			go.AddOrGet<UserNameable>();
			go.AddOrGet<TreeFilterable>();
			go.AddOrGet<CreatureFeeder>();

			var modOptions = Mod.Options;
			gen.elementCount = modOptions.magicFeederElementsLimit;
			gen.singleItemCount = modOptions.magicFeederItemsLimit;
			gen.singleItemPerTick = 1;
		}

		public override void DoPostConfigureComplete(GameObject go) => go.AddOrGetDef<StorageController.Def>();

		public override void ConfigurePost(BuildingDef def)
		{
			List<Tag> tagList = new List<Tag>();
			Tag[] target_species = new Tag[6]
			{
			  GameTags.Creatures.Species.LightBugSpecies,
			  GameTags.Creatures.Species.HatchSpecies,
			  GameTags.Creatures.Species.MoleSpecies,
			  GameTags.Creatures.Species.CrabSpecies,
			  GameTags.Creatures.Species.StaterpillarSpecies,
			  GameTags.Creatures.Species.DivergentSpecies
			};

			foreach (KeyValuePair<Tag, Diet> collectDiet in DietManager.CollectDiets(target_species))
				tagList.Add(collectDiet.Key);



			def.BuildingComplete.GetComponent<Storage>().storageFilters = tagList;
		}
	}
}