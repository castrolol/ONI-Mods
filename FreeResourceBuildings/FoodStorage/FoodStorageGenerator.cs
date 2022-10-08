using FreeResourceBuildingsPatches;
using STRINGS;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUNING;
using UnityEngine;

namespace FreeResourceBuildings
{
	public class FoodStorageGenerator : FreeStorage
	{
		protected override void OnSpawn()
		{
			base.OnSpawn();

			if (Mod.Options.DiscoverAllUsableItems)
			{
				var allFood = EdiblesManager.GetAllFoodTypes();

				foreach (EdiblesManager.FoodInfo food in allFood)
				{
					try
					{
						if ((double)food.CaloriesPerUnit == 0.0)
							DiscoveredResources.Instance.Discover(food.Id.ToTag(), GameTags.CookingIngredient);
						else
							DiscoveredResources.Instance.Discover(food.Id.ToTag(), GameTags.Edible);
					}
					catch (System.Exception e)
					{
						Debug.Log("Unpossible to load " + food.Id);
					}
				}
			}
		}

	}
}

