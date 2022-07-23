using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUNING;
using UnityEngine;

namespace FreeResourceBuildings
{
	public class StorageGenerator : FreeStorage
	{
		protected override void OnSpawn()
		{
			base.OnSpawn();

			var ids = new List<string>
			{
				BasicFabricConfig.ID,
				WoodLogConfig.ID,
				SwampLilyFlowerConfig.ID,
				CrabShellConfig.ID,
				CrabWoodShellConfig.ID,
				GingerConfig.ID,
				GeneShufflerRechargeConfig.ID,
				HeatCubeConfig.ID,
				BabyCrabShellConfig.ID,
				BabyCrabWoodShellConfig.ID,
				OrbitalResearchDatabankConfig.ID,
				GasGrassHarvestedConfig.ID,
				TableSaltConfig.ID,
				EggShellConfig.ID,
				RotPileConfig.ID,
			};
			foreach (var id in ids)
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

			

			var elements = ElementLoader.elements;



			foreach (var element in elements)
			{

				if (element.tag.ProperName().Contains("MISSING")) continue;
				DiscoveredResources.Instance.Discover(element.tag, element.GetMaterialCategoryTag());
			}
		}
	}
}
