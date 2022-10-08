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
	public class FarmerStorage : FreeStorage
	{


		protected override void OnSpawn()
		{
			base.OnSpawn();

			if (Mod.Options.DiscoverAllUsableItems)
			{
				List<GameObject> eggs = Assets.GetPrefabsWithTag(GameTags.IncubatableEgg);


				foreach (var egg in eggs)
				{
					try
					{
						var prefabId = egg.GetComponent<KPrefabID>();
						if (prefabId != null && egg.HasTag(GameTags.Egg))
						{

							var eggTag = (Tag)"PuftEgg";
							foreach (var tag in prefabId.Tags)
								if (tag.ToString().EndsWith("Egg") && tag != GameTags.IncubatableEgg && tag.ToString().Length > 3)
									eggTag = tag;

							Tag categoryForEntity = DiscoveredResources.GetCategoryForTags(prefabId.Tags);
							DiscoveredResources.Instance.Discover(eggTag, categoryForEntity);
						}
					}
					catch (System.Exception e)
					{
						Debug.Log("Unpossible to load " + egg);
					}
				}
				foreach (var element in ElementLoader.elements)
				{
					if (element.materialCategory == GameTags.Agriculture)
					{
						var tag = element.id.CreateTag();
						Tag categoryForEntity = DiscoveredResources.GetCategoryForEntity(tag.Prefab().GetComponent<KPrefabID>());
						DiscoveredResources.Instance.Discover(element.id.CreateTag(), categoryForEntity);
					}
				}

				var excludedTags = new List<Tag> {
				GameTags.Seed,
				GameTags.UnidentifiedSeed ,
				GameTags.CropSeed ,
				GameTags.DecorSeed ,
				GameTags.WaterSeed ,
				GameTags.MutatedSeed ,
			};

				var seeds = Assets.GetPrefabsWithTag(GameTags.Seed);

				foreach (var seed in seeds)
				{
					try
					{
						var prefabId = seed.GetComponent<KPrefabID>();

						var seedTag = (Tag)"OxyfernSeed";
						foreach (var tag in prefabId.Tags)
							if (tag.ToString().EndsWith("Seed") && !excludedTags.Contains(tag))
								seedTag = tag;

						Tag categoryForEntity = DiscoveredResources.GetCategoryForTags(prefabId.Tags);
						DiscoveredResources.Instance.Discover(seedTag, categoryForEntity);
					}
					catch (System.Exception e)
					{
						Debug.Log("Unpossible to load " + seed);
					}
				}
			}

		}

	}
}
