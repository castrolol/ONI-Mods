using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUNING;
using UnityEngine;

namespace FreeResourceBuildings
{
	public class FreeStorage : KMonoBehaviour, ISim4000ms
	{


		private FilteredStorage filteredStorage;
		private TreeFilterable filter;
		[MyCmpReq]
		public Storage storage;

		public int singleItemCount = 10;
		public int elementCount = 100000;
		public int singleItemPerTick = 1;
		protected override void OnPrefabInit()
		{

			base.OnPrefabInit();

			this.filteredStorage = new FilteredStorage(
				(KMonoBehaviour)this, 
				(Tag[])null, 
				(IUserControlledCapacity)null, 
				false, 
				Db.Get().ChoreTypes.StorageFetch
			);

			filter = FindOrAdd<TreeFilterable>();

			filter.OnFilterChanged = (System.Action<HashSet<Tag>>)System.Delegate.Combine(filter.OnFilterChanged, new System.Action<HashSet<Tag>>(OnFilterChanged));

			storage.allowClearable = true;

		}

		Tag[] selectedTags = new Tag[] { };

		private void OnFilterChanged(HashSet<Tag> tags)
		{
			selectedTags = tags?.ToArray() ?? new Tag[] { };
			var anim = this.GetComponent<KAnimControllerBase>();
			if (anim)
			{
				if (tags == null || selectedTags.Length == 0)
				{
					anim.Play((HashedString)"off");
				}
				else
				{
					anim.Play((HashedString)"on");
				}
			}

			var itemsToRemove = new List<UnityEngine.GameObject>();
			//removing
			foreach (var item in storage.items)
			{
				if (item && !selectedTags.Any(tag => item.HasTag(tag)))
				{
					itemsToRemove.Add(item);
				}

			}
			foreach (var item in itemsToRemove)
				item?.DeleteObject();

			if (storage && storage.items != null)
				foreach (var tag in tags)
				{
					if (!storage.items.Any(item => item.HasTag(tag)))
					{


						var gos = SpawnContents(tag.ToString(), elementCount);
						foreach (var go in gos)
							storage.Store(go);
					}
				}

		}

		private List<GameObject> SpawnContents(string id, float quantity)
		{

			List<GameObject> result = new List<GameObject>();
			try
			{
				GameObject gameObject = (GameObject)null;
				GameObject prefab = Assets.GetPrefab((Tag)id);
				Element element = ElementLoader.GetElement(id.ToTag());
				Vector3 position = this.transform.position + Vector3.up / 2f;

				if (element == null && (Object)prefab != (Object)null)
				{
					for (var i = 0; i < singleItemPerTick; i++)
					{
						gameObject = Util.KInstantiate(prefab, position);
						if ((Object)gameObject != (Object)null)
						{
							//if (!this.facadeID.IsNullOrWhiteSpace())
							//	EquippableFacade.AddFacadeToEquippable(gameObject.GetComponent<Equippable>(), this.facadeID);
							gameObject.SetActive(true);
							result.Add(gameObject);
						}
					}

				}
				else if (element != null)
				{

					gameObject = element.substance.SpawnResource(position, quantity, element.defaultValues.temperature, byte.MaxValue, 0, forceTemperature: true);
					result.Add(gameObject);
				}
				else
					Debug.LogWarning((object)("Can't find spawnable thing from tag " + id));
				if (!((Object)gameObject != (Object)null))
					return null;

				if (gameObject) gameObject.SetActive(true);

			}
			catch (System.Exception e)
			{
				Debug.Log("ERROR SPAWNING ITEM [" + id + "," + quantity + "] (" + e.Message + ")");
			}
			return result;


		}

		protected override void OnSpawn()
		{
			base.OnSpawn();

			this.GetComponent<KAnimControllerBase>().Play((HashedString)"off");
			this.filteredStorage.FilterChanged();
		}


		public bool IsRotten(GameObject item)
		{
			Rottable.Instance smi;
			if (!item) return false;
			smi = item.GetSMI<Rottable.Instance>();
			if (smi == null) return false;

			return smi.RotConstitutionPercentage <= 0.3f;
		}


		public void Sim4000ms(float dt)
		{


			var t = 0f;
			List<Tag> alreadyAdded = new List<Tag>();

			GameObject[] array = new GameObject[storage.items.Count];
			var currentTags = storage.GetAllIDsInStorage();

			storage.items.CopyTo(array);
			var currentItems = array.ToList();
			var itemsToRemove = new List<UnityEngine.GameObject>();
			//removing
			foreach (var item in storage.items)
			{
				if (!selectedTags.Any(tag => item.HasTag(tag)))
				{
					itemsToRemove.Add(item);
				}
				else if (IsRotten(item))
				{
					itemsToRemove.Add(item);
				}
			}

			foreach (var item in itemsToRemove)
				item.DeleteObject();


			foreach (var tag in selectedTags)
			{
				if (alreadyAdded.Contains(tag)) continue;
				alreadyAdded.Add(tag);



				if (!currentTags.Contains(tag))
				{
					var gos = SpawnContents(tag.ToString(), elementCount);
					foreach (var go in gos)
					{
						storage.Store(go);
					}

				}
				else
				{
					Element element = null;
					GameObject item = null;

					var amount = storage.GetAmountAvailable(tag);
					element = ElementLoader.GetElement(tag);
					item = currentItems.Find(x => x.HasTag(tag));
					var qtd = element == null ? singleItemCount : elementCount;
					t += element == null ? singleItemCount * item.GetComponent<PrimaryElement>().MassPerUnit : elementCount;
					if (amount < qtd)
					{
						var gos = SpawnContents(tag.ToString(), qtd);
						foreach (var go in gos)
						{
							storage.Store(go);
						}

					}
				}
			}

			storage.capacityKg = t;
		}




	}
}
