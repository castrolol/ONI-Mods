using FreeResourceBuildingsPatches;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUNING;
using UnityEngine;

namespace FreeResourceBuildings
{
	public class MagicFishFeeder : FreeStorage
	{
		protected override void OnSpawn()
		{
			base.OnSpawn();

			if (Mod.Options.DiscoverAllUsableItems)
			{
				List<Tag> tagList = new List<Tag>();
				Tag[] target_species = new Tag[1]
				   {
					  GameTags.Creatures.Species.PacuSpecies
				   };

				foreach (KeyValuePair<Tag, Diet> collectDiet in DietManager.CollectDiets(target_species))
					tagList.Add(collectDiet.Key);


				foreach (var id in tagList)
				{
					try
					{
						var prefab = Assets.GetPrefab(id);
						if (prefab == null) continue;
						var item = Util.KInstantiate(prefab);
						if (item == null) continue;
						var prefabId = item.GetComponent<KPrefabID>();
						if (prefabId == null) continue;


						var category = DiscoveredResources.GetCategoryForEntity(prefabId);

						DiscoveredResources.Instance.Discover(prefabId.PrefabTag, category);
					}
					catch (System.Exception e)
					{
						Debug.Log("Unpossible to load " + id);
					}
				}


			}

		}
	}
}
