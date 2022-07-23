using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUNING;
using UnityEngine;

namespace FreeResourceBuildings
{
	public class Trashcan : KMonoBehaviour, IUserControlledCapacity
	{


		private FilteredStorage filteredStorage;
		[MyCmpReq]
		private Storage storage;

		private float userMaxCapacity;

		protected override void OnPrefabInit()
		{

			base.OnPrefabInit();


			storage.storageFilters = STORAGEFILTERS.NOT_EDIBLE_SOLIDS;
			storage.capacityKg = 999999;
			this.filteredStorage = new FilteredStorage((KMonoBehaviour)this, (Tag[])null, (Tag[])null, (IUserControlledCapacity)null, false, Db.Get().ChoreTypes.StorageFetch);

			storage.OnStorageIncreased += Storage_OnStorageIncreased;

		}


		public float UserMaxCapacity
		{
			get => Mathf.Min(this.userMaxCapacity, this.storage.capacityKg);
			set
			{
				this.userMaxCapacity = value;
				this.filteredStorage.FilterChanged();
			}
		}

		protected override void OnCleanUp()
		{
			var mass = storage.MassStored();

			var itemsToRemove = new List<UnityEngine.GameObject>();
			//removing
			foreach (var item in storage.items)
			{
				itemsToRemove.Add(item);
			}
			foreach (var item in itemsToRemove)
				storage.Remove(item, false);


			storage.capacityKg -= mass;

		}

		public float AmountStored => storage.MassStored();

		public float MinCapacity => 0;

		public float MaxCapacity => 100000;

		public bool WholeValues => false;

		public LocString CapacityUnits => GameUtil.GetCurrentMassUnit();

		private void Storage_OnStorageIncreased()
		{
			var mass = storage.MassStored();

			var itemsToRemove = new List<UnityEngine.GameObject>();
			//removing
			foreach (var item in storage.items)
			{
				itemsToRemove.Add(item);
			}
			foreach (var item in itemsToRemove)
				storage.Remove(item, false);


			storage.capacityKg -= mass;

		}

		protected override void OnSpawn()
		{
			base.OnSpawn();
			this.filteredStorage.FilterChanged();
		}



	}
}
