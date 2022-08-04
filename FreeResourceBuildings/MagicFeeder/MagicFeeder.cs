using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUNING;
using UnityEngine;

namespace FreeResourceBuildings
{
	public class MagicFeeder : FreeStorage
	{
		protected override void OnSpawn()
		{
			base.OnSpawn();


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
				}catch(System.Exception e)
				{
					Debug.Log("Unpossible to load " + id);
				}
			}

			

			var elements = ElementLoader.elements;

			 
		}
	}
}
