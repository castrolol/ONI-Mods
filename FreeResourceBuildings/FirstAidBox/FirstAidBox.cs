using FreeResourceBuildingsPatches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeResourceBuildings
{
	public class FirstAidBox : FreeStorage
	{

		protected override void OnSpawn()
		{
			base.OnSpawn();

			if (Mod.Options.DiscoverAllUsableItems)
			{

				//BasicBooster
				//IntermediateBooster
				//BasicCure
				//Antihistamine
				//IntermediateCure
				//AdvancedCure
				//BasicRadPill
				//IntermediateRadPill

				foreach (var prefab in Assets.Prefabs)
				{

					if (prefab.HasTag(GameTags.Medicine) || prefab.HasTag(GameTags.MedicalSupplies))
					{
						var category = DiscoveredResources.GetCategoryForEntity(prefab);
						DiscoveredResources.Instance.Discover(prefab.PrefabTag, category);
					}
				}

			}

		}

	}
}
