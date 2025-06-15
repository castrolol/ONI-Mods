using FreeResourceBuildingsPatches;
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

			if (Mod.Options.DiscoverAllUsableItems)
			{
				List<Tag> tagList = new List<Tag>();
				List<Tag> targetSpecies = new List<Tag>
				{
				  GameTags.Creatures.Species.LightBugSpecies,
				  GameTags.Creatures.Species.HatchSpecies,
				  GameTags.Creatures.Species.MoleSpecies,
				  GameTags.Creatures.Species.CrabSpecies,
				  GameTags.Creatures.Species.StaterpillarSpecies,
				  GameTags.Creatures.Species.DivergentSpecies
				};


				if (DlcManager.IsContentSubscribed(DlcManager.DLC4_ID))
				{
					targetSpecies.Add(GameTags.Creatures.Species.StegoSpecies);
					targetSpecies.Add(GameTags.Creatures.Species.RaptorSpecies);
					targetSpecies.Add(GameTags.Creatures.Species.ChameleonSpecies);
				}

				var collectDiets = DietManager.CollectDiets(targetSpecies.ToArray());

				foreach (KeyValuePair<Tag, Diet> collectDiet in collectDiets)
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
