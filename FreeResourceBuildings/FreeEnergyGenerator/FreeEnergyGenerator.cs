using FreeResourceBuildingsPatches;
using KSerialization;
using STRINGS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static EventSystem;

namespace FreeResourceBuildings
{
	public class FreeEnergyGenerator : Generator, ISliderControl, ISingleSliderControl, ISaveLoadable
	{

		public static string ID = ModStrings.FreeEnergyGeneratorID;
		public static string Name = ModStrings.FreeEnergyGeneratorName;
		public static string Description = ModStrings.FreeEnergyGeneratorDescription;
		public static string Effect = ModStrings.FreeEnergyGeneratorEffect;

		private HandleVector<int>.Handle structureTemperature;

		[SerializeField]
		[Serialize]
		public float currentCapacity = Mod.Options.defaultEnergyWattage;
		[MyCmpGet]
		private OccupyArea occupyArea;
		public override float Capacity => currentCapacity;
		public float lastEnvTemp { get; private set; }

		public float lastGasTemp { get; private set; }

		private float lastSampleTime = -1f;

		private int cooledAirOutputCell = -1;
		private float envTemp;
		private int cellCount;
		private float lowTempLag;

		protected const int OnActivateChangeFlag = 824508782;
		protected static readonly IntraObjectHandler<FreeEnergyGenerator> OnActivateChangeDelegate = new IntraObjectHandler<FreeEnergyGenerator>(OnActivateChangedStatic);
		private static readonly Func<int, object, bool> UpdateStateCbDelegate = (Func<int, object, bool>)((cell, data) => FreeEnergyGenerator.UpdateStateCb(cell, data));

		public string SliderTitleKey => "-";

		public string SliderUnits => UI.UNITSUFFIXES.ELECTRICAL.WATT;
		[MyCmpAdd]
		private CopyBuildingSettings copyBuildingSettings;
		private static readonly EventSystem.IntraObjectHandler<FreeEnergyGenerator> OnCopySettingsDelegate = new EventSystem.IntraObjectHandler<FreeEnergyGenerator>((System.Action<FreeEnergyGenerator, object>)((component, data) => component.OnCopySettings(data)));

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			this.Subscribe<FreeEnergyGenerator>(-905833192, FreeEnergyGenerator.OnCopySettingsDelegate);

		}

		private void OnCopySettings(object data)
		{
			FreeEnergyGenerator component = ((GameObject)data).GetComponent<FreeEnergyGenerator>();
			if (!((UnityEngine.Object)component != (UnityEngine.Object)null))
				return;
			this.currentCapacity = component.currentCapacity;
		}

		private static void OnActivateChangedStatic(FreeEnergyGenerator gen, object data) =>
		  gen.OnActivateChanged(data as Operational);


		protected virtual void OnActivateChanged(Operational op)
		{

			if (op.IsActive && currentCapacity > 0)
			{
				this.GetComponent<KAnimControllerBase>().Play((HashedString)"on");
			}
			else
			{
				this.GetComponent<KAnimControllerBase>().Play((HashedString)"off");
			}

			selectable.SetStatusItem(Db.Get().StatusItemCategories.Power, op.IsActive ?
				Db.Get().BuildingStatusItems.Wattage :
				Db.Get().BuildingStatusItems.GeneratorOffline, this);
		}

		protected override void OnSpawn()
		{
			base.OnSpawn();
			this.structureTemperature = GameComps.StructureTemperatures.GetHandle(this.gameObject);
			this.cooledAirOutputCell = this.building.GetUtilityOutputCell();
		}

		private static bool UpdateStateCb(int cell, object data)
		{
			FreeEnergyGenerator airConditioner = data as FreeEnergyGenerator;
			++airConditioner.cellCount;
			airConditioner.envTemp += Grid.Temperature[cell];
			return true;
		}


 

		//this.occupyArea.TestArea(Grid.PosToCell(this.gameObject), (object) this, AirConditioner.UpdateStateCbDelegate);
		private void UpdateState(float dt)
		{ 
			this.envTemp = 0.0f;
			this.cellCount = 0;
			if ((UnityEngine.Object)this.occupyArea != (UnityEngine.Object)null && (UnityEngine.Object)this.gameObject != (UnityEngine.Object)null)
			{
				this.occupyArea.TestArea(Grid.PosToCell(this.gameObject), (object)this, FreeEnergyGenerator.UpdateStateCbDelegate);
				this.envTemp /= (float)this.cellCount;
			}
			this.lastEnvTemp = this.envTemp;
			
			if ((double)Time.time - (double)this.lastSampleTime > 2.0)
			{
				GameComps.StructureTemperatures.ProduceEnergy(this.structureTemperature, 0.0f, (string)BUILDING.STATUSITEMS.OPERATINGENERGY.PIPECONTENTS_TRANSFER, Time.time - this.lastSampleTime);
				this.lastSampleTime = Time.time;
			}

			//this.UpdateStatus();
		}

		public void Sim200ms(float dt)
		{
			if ((UnityEngine.Object)this.operational != (UnityEngine.Object)null && !this.operational.IsOperational)
				this.operational.SetActive(false);
			else
				this.UpdateState(dt);
		}

		public override void EnergySim200ms(float dt)
		{
			base.EnergySim200ms(dt);

			operational.SetFlag(wireConnectedFlag, CircuitID != ushort.MaxValue);
			operational.SetActive(operational.IsOperational);

			if (!operational.IsOperational)
			{
				this.GetComponent<KAnimControllerBase>().Play((HashedString)"off");
				return;
			}


			if (currentCapacity > 0)
			{
				this.GetComponent<KAnimControllerBase>().Play((HashedString)"on");
			}
			else
			{
				this.GetComponent<KAnimControllerBase>().Play((HashedString)"off");

			}

			GenerateJoules(WattageRating * dt, true);
			selectable.SetStatusItem(Db.Get().StatusItemCategories.Power, Db.Get().BuildingStatusItems.Wattage, this);
		}

		public int SliderDecimalPlaces(int index)
		{
			return 0;
		}

		public float GetSliderMin(int index)
		{
			return 0;
		}

		public float GetSliderMax(int index)
		{
			return 40000;
		}

		public float GetSliderValue(int index)
		{
			return currentCapacity;
		}

		public void SetSliderValue(float value, int index)
		{
			currentCapacity = value;
			building.Def.GeneratorWattageRating = value;
		}


		public string GetSliderTooltipKey(int index) => "STRINGS.UI.UISIDESCREENS.MANUALDELIVERYGENERATORSIDESCREEN.TOOLTIP";

		public string GetSliderTooltip()
		{
			return "Tooltip";
		}
	}
}
