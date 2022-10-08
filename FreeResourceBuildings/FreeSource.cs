using FreeResourceBuildingsPatches;
using KSerialization;
using STRINGS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FreeResourceBuildings
{
	[SerializationConfig(MemberSerialization.OptIn)]
	public class FreeSource : KMonoBehaviour
	{
		private static StatusItem filterStatusItem = null;

		[SerializeField]
		public ConduitType conduitType = ConduitType.None;

		[SerializeField]
		[Serialize]
		public virtual float Flow
		{ get; set; } = Mod.Options.defaultGasFlowRate;

		[SerializeField]
		[Serialize]
		public float Temp
		{ get; set; } = 300f;

		[MyCmpGet]
		public KSelectable selectable;

		[MyCmpGet]
		public Building building;

		[MyCmpGet]
		public Operational operational;

		[MyCmpGet]
		public KBatchedAnimController anim;

		[MyCmpGet]
		public Filterable filterable;

		private int outputCell = -1;
		private SimHashes filteredElement = SimHashes.Vacuum;
		private Element filteredElementItem;
		private float minTemp;
		private float maxTemp;
		private Operational.Flag filterFlag = new Operational.Flag("filter", Operational.Flag.Type.Requirement);

		[MyCmpAdd]
		private CopyBuildingSettings copyBuildingSettings;
		private static readonly EventSystem.IntraObjectHandler<FreeSource> OnCopySettingsDelegate = new EventSystem.IntraObjectHandler<FreeSource>((System.Action<FreeSource, object>)((component, data) => component.OnCopySettings(data)));

		private void OnCopySettings(object data)
		{
			FreeSource component = ((GameObject)data).GetComponent<FreeSource>();
			if (!((UnityEngine.Object)component != (UnityEngine.Object)null))
				return;
			this.Flow = component.Flow;
			this.Temp = component.Temp;
		}

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			InitializeStatusItems();
			this.Subscribe<FreeSource>(-905833192, FreeSource.OnCopySettingsDelegate);

		}

		private void InitializeStatusItems()
		{
			if (filterStatusItem != null) return;

			filterStatusItem = new StatusItem("Filter", "BUILDING", "", StatusItem.IconType.Info, NotificationType.Neutral, false, OverlayModes.LiquidConduits.ID, true, 129022);
			filterStatusItem.resolveStringCallback = (str, data) =>
			{
				FreeSource infiniteSource = (FreeSource)data;
				if (infiniteSource.filteredElement == SimHashes.Void)
				{
					str = string.Format(BUILDINGS.PREFABS.GASFILTER.STATUS_ITEM, BUILDINGS.PREFABS.GASFILTER.ELEMENT_NOT_SPECIFIED);
				}
				else
				{
					Element elementByHash = ElementLoader.FindElementByHash(infiniteSource.filteredElement);
					str = string.Format(BUILDINGS.PREFABS.GASFILTER.STATUS_ITEM, elementByHash.name);
				}
				return str;
			};
			filterStatusItem.conditionalOverlayCallback = new Func<HashedString, object, bool>(ShowInUtilityOverlay);
		}

		protected override void OnSpawn()
		{
			base.OnSpawn();

			outputCell = building.GetUtilityOutputCell();

			Conduit.GetFlowManager(conduitType).AddConduitUpdater(ConduitUpdate);

			filterable.onFilterChanged += new Action<Tag>(OnFilterChanged);
			OnFilterChanged(filterable.SelectedTag);

			selectable.SetStatusItem(Db.Get().StatusItemCategories.Main, filterStatusItem, this);
		}

		protected override void OnCleanUp()
		{
			Conduit.GetFlowManager(conduitType).RemoveConduitUpdater(ConduitUpdate);
			base.OnCleanUp();
		}

		private bool refreshing = false;

		private void OnFilterChanged(Tag tag)
		{
			if (refreshing) return;

			refreshing = true;

			Element element = ElementLoader.GetElement(tag);
			if (element != null)
			{
				filteredElement = element.id;
				filteredElementItem = element;
				anim.Play("on");
				anim.SetSymbolTint("place_color", element.substance.uiColour);

				minTemp = element.lowTemp;
				maxTemp = element.highTemp;
				var newTemp = Mathf.Clamp(Temp, element.lowTemp, element.highTemp);
				if (Temp != newTemp)
				{
					var cell = building.PlacementCells[0];
					newTemp = Grid.Temperature[cell];
					Temp = Mathf.Clamp(newTemp, element.lowTemp, element.highTemp);
				}
			}


			bool invalidElement = (!tag.IsValid || tag == GameTags.Void);
			selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.NoFilterElementSelected, invalidElement, null);
			operational.SetFlag(filterFlag, !invalidElement);

			RefreshSideScreen();

			refreshing = false;
		}

		private void RefreshSideScreen()
		{
			if (refreshing)

				if (base.GetComponent<KSelectable>().IsSelected)
				{
					refreshing = true;
					DetailsScreen.Instance.Refresh(base.gameObject);
				}
		}

		private bool ShowInUtilityOverlay(HashedString mode, object data)
		{
			bool flag = false;
			switch (conduitType)
			{
				case ConduitType.Gas:
					flag = mode == OverlayModes.GasConduits.ID;
					break;
				case ConduitType.Liquid:
					flag = mode == OverlayModes.LiquidConduits.ID;
					break;
			}
			return flag;
		}

		private void ConduitUpdate(float dt)
		{
			if (operational.IsOperational)
			{
				if (filteredElement == SimHashes.Void || filteredElement == SimHashes.Vacuum)
				{

					anim.Play("off");
					return;
				}

				anim.Play("on");

				if (filteredElementItem != null)
				{
					anim.SetSymbolTint("place_color", filteredElementItem.substance.uiColour);
				}

				var flowManager = Conduit.GetFlowManager(conduitType);
				if (flowManager == null) return;
				if (flowManager.HasConduit(outputCell))
				{
					flowManager.AddElement(outputCell, filteredElement, Flow / 1000, Temp, 0, 0);
				}
			}
			else
			{
				anim.Play("off");
			}
		}
	}
}
