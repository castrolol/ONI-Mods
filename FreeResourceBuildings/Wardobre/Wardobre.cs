using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUNING;
using UnityEngine;

namespace FreeResourceBuildings
{
	public class Wardobre : FreeStorage
	{


		protected override void OnSpawn()
		{
			base.OnSpawn();

			var tags = new[]
			{
				GameTags.AtmoSuit,
				GameTags.JetSuit,
				GameTags.LeadSuit,
				GameTags.OxygenMask,
				"Warm_Vest".ToTag(),
				"Cool_Vest".ToTag(),
				"Funky_Vest".ToTag(),
			};

			foreach (var tag in tags)
			{
				var prefab = tag.Prefab();
				if (prefab != null)
				{
					Tag categoryForEntity = DiscoveredResources.GetCategoryForEntity(prefab.GetComponent<KPrefabID>());
					DiscoveredResources.Instance.Discover(tag, categoryForEntity);
				}

			}
			 
		}
	}
}