using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUNING;
using UnityEngine;

namespace FreeResourceBuildings
{
	public class Trashcan : KMonoBehaviour, ISim4000ms
	{


		private FilteredStorage filteredStorage;
		[MyCmpReq]
		private Storage storage;

		private float userMaxCapacity;

		protected override void OnPrefabInit()
		{

			base.OnPrefabInit();

			var filters = new List<Tag>();

			filters.AddRange(STORAGEFILTERS.NOT_EDIBLE_SOLIDS);
			filters.AddRange(STORAGEFILTERS.LIQUIDS);
			filters.AddRange(STORAGEFILTERS.FOOD);
			filters.AddRange(STORAGEFILTERS.GASES); 

			storage.storageFilters = filters;
			storage.capacityKg = 999999;
			this.filteredStorage = new FilteredStorage((KMonoBehaviour)this, (Tag[])null, (IUserControlledCapacity)null, false, Db.Get().ChoreTypes.StorageFetch);


		}


		  

		public float AmountStored => storage.MassStored();

		public float MinCapacity => 0;

		public float MaxCapacity => 100000;

		public bool WholeValues => false;

		public LocString CapacityUnits => GameUtil.GetCurrentMassUnit();

		public void Sim4000ms(float dt)
		{
			foreach (GameObject current in storage.items)
			{
				current.DeleteObject();
			}
		}

		protected override void OnSpawn()
		{
			base.OnSpawn();
			this.filteredStorage.FilterChanged();
		}



	}
}
