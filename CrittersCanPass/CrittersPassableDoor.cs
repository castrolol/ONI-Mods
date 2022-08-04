using HarmonyLib;
using Klei.AI;
using KSerialization;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AnalyzerDoors
{


	[SerializationConfig(MemberSerialization.OptIn)]
	public class CrittersPassableDoor : Workable, ICustomDoor, ISaveLoadable, ISim200ms, INavDoor, IGameObjectEffectDescriptor
	{
		[MyCmpReq]
		private Operational operational;
		[MyCmpGet]
		private Rotatable rotatable;
		[MyCmpReq]
		private KBatchedAnimController animController;
		[MyCmpReq]
		public Building building;
		[MyCmpGet]
		private EnergyConsumer consumer;
		[MyCmpGet]
		private AccessControl accessControl;
		[MyCmpAdd]
		private LoopingSounds loopingSounds;
		public Orientation verticalOrientation;
		[SerializeField]
		public bool hasComplexUserControls;
		[SerializeField]
		public float unpoweredAnimSpeed = 0.25f;
		[SerializeField]
		public float poweredAnimSpeed = 1f;
		[SerializeField]
		public Door.DoorType doorType;
		[SerializeField]
		public bool allowAutoControl = true;
		[SerializeField]
		public string doorClosingSoundEventName;
		[SerializeField]
		public string doorOpeningSoundEventName;
		private string doorClosingSound;
		private string doorOpeningSound;
		private static readonly HashedString SOUND_POWERED_PARAMETER = (HashedString)"doorPowered";
		private static readonly HashedString SOUND_PROGRESS_PARAMETER = (HashedString)"doorProgress";
		[Serialize]
		private bool hasBeenUnsealed;
		[Serialize]
		private Door.ControlState controlState;
		private bool on;
		private bool do_melt_check;
		private int openCount;
		private Door.ControlState requestedState;
		private Chore changeStateChore;
		private CrittersPassableDoor.Controller.Instance controller;
		private LoggerFSS log;
		private const float REFRESH_HACK_DELAY = 1f;
		private bool doorOpenLiquidRefreshHack;
		private float doorOpenLiquidRefreshTime;
		private static readonly EventSystem.IntraObjectHandler<CrittersPassableDoor> OnCopySettingsDelegate = new EventSystem.IntraObjectHandler<CrittersPassableDoor>((System.Action<CrittersPassableDoor, object>)((component, data) => component.OnCopySettings(data)));
		public static readonly HashedString OPEN_CLOSE_PORT_ID = new HashedString("DoorOpenClose");
		private static readonly KAnimFile[] OVERRIDE_ANIMS = new KAnimFile[1]
		{
			Assets.GetAnim((HashedString) "anim_use_remote_kanim")
		};
		private static readonly EventSystem.IntraObjectHandler<CrittersPassableDoor> OnOperationalChangedDelegate = new EventSystem.IntraObjectHandler<CrittersPassableDoor>((System.Action<CrittersPassableDoor, object>)((component, data) => component.OnOperationalChanged(data)));
		private static readonly EventSystem.IntraObjectHandler<CrittersPassableDoor> OnLogicValueChangedDelegate = new EventSystem.IntraObjectHandler<CrittersPassableDoor>((System.Action<CrittersPassableDoor, object>)((component, data) => component.OnLogicValueChanged(data)));
		private bool applyLogicChange;


		private void OnCopySettings(object data)
		{
			CrittersPassableDoor component = ((GameObject)data).GetComponent<CrittersPassableDoor>();
			if (!((UnityEngine.Object)component != (UnityEngine.Object)null))
				return;
			this.QueueStateChange(component.requestedState);
		}


		public CrittersPassableDoor() => this.SetOffsetTable(OffsetGroups.InvertedStandardTable);

		public Door.ControlState CurrentState => this.controlState;

		public Door.ControlState RequestedState => this.requestedState;

		public bool ShouldBlockFallingSand => this.rotatable.GetOrientation() != this.verticalOrientation;

		public bool isSealed => this.controller.sm.isSealed.Get(this.controller);

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			this.overrideAnims = CrittersPassableDoor.OVERRIDE_ANIMS;
			this.synchronizeAnims = false;
			this.SetWorkTime(3f);
			 

			if (!string.IsNullOrEmpty(this.doorClosingSoundEventName))
				this.doorClosingSound = GlobalAssets.GetSound(this.doorClosingSoundEventName);
			if (!string.IsNullOrEmpty(this.doorOpeningSoundEventName))
				this.doorOpeningSound = GlobalAssets.GetSound(this.doorOpeningSoundEventName);
			this.Subscribe<CrittersPassableDoor>(-905833192, CrittersPassableDoor.OnCopySettingsDelegate);
		}

		private Door.ControlState GetNextState(Door.ControlState wantedState) => (Door.ControlState)((int)(wantedState + 1) % 3);

		private static bool DisplacesGas(Door.DoorType type) => type != Door.DoorType.Internal;

		protected override void OnSpawn()
		{
			base.OnSpawn();
			if ((UnityEngine.Object)this.GetComponent<KPrefabID>() != (UnityEngine.Object)null)
				this.log = new LoggerFSS(nameof(CrittersPassableDoor));
			if (!this.allowAutoControl && this.controlState == Door.ControlState.Auto)
				this.controlState = Door.ControlState.Locked;
			StructureTemperatureComponents structureTemperatures = GameComps.StructureTemperatures;
			HandleVector<int>.Handle handle = structureTemperatures.GetHandle(this.gameObject);
			if (CrittersPassableDoor.DisplacesGas(this.doorType))
				structureTemperatures.Bypass(handle);
			this.controller = new CrittersPassableDoor.Controller.Instance(this);
			this.controller.StartSM();
			if (this.doorType == Door.DoorType.Sealed && !this.hasBeenUnsealed)
				this.Seal();
			this.UpdateDoorSpeed(this.operational.IsOperational);
			this.Subscribe<CrittersPassableDoor>(-592767678, CrittersPassableDoor.OnOperationalChangedDelegate);
			this.Subscribe<CrittersPassableDoor>(824508782, CrittersPassableDoor.OnOperationalChangedDelegate);
			this.Subscribe<CrittersPassableDoor>(-801688580, CrittersPassableDoor.OnLogicValueChangedDelegate);
			this.requestedState = this.CurrentState;
			this.ApplyRequestedControlState(true);
			int num1 = this.rotatable.GetOrientation() == Orientation.Neutral ? this.building.Def.WidthInCells * (this.building.Def.HeightInCells - 1) : 0;
			int num2 = this.rotatable.GetOrientation() == Orientation.Neutral ? this.building.Def.WidthInCells : this.building.Def.HeightInCells;
			for (int index = 0; index != num2; ++index)
			{
				int placementCell = this.building.PlacementCells[num1 + index];
				Grid.FakeFloor.Add(placementCell);
				Pathfinding.Instance.AddDirtyNavGridCell(placementCell);
			}
			List<int> intList = new List<int>();
			foreach (int placementCell in this.building.PlacementCells)
			{
				Grid.HasDoor[placementCell] = true;
				if (this.rotatable.IsRotated)
				{
					intList.Add(Grid.CellAbove(placementCell));
					intList.Add(Grid.CellBelow(placementCell));
				}
				else
				{
					intList.Add(Grid.CellLeft(placementCell));
					intList.Add(Grid.CellRight(placementCell));
				}
				SimMessages.SetCellProperties(placementCell, (byte)8);
				if (CrittersPassableDoor.DisplacesGas(this.doorType))
					Grid.RenderedByWorld[placementCell] = false;
			}
		}

		protected override void OnCleanUp()
		{
			this.UpdateDoorState(true);
			List<int> intList = new List<int>();
			foreach (int placementCell in this.building.PlacementCells)
			{
				SimMessages.ClearCellProperties(placementCell, (byte)12);
				var substance = Traverse.Create(Grid.Element[placementCell].substance);
				Grid.RenderedByWorld[placementCell] = substance.Field("renderedByWorld").GetValue<bool>();
				Grid.FakeFloor.Remove(placementCell);
				if (Grid.Element[placementCell].IsSolid)
					SimMessages.ReplaceAndDisplaceElement(placementCell, SimHashes.Vacuum, CellEventLogger.Instance.DoorOpen, 0.0f);
				Pathfinding.Instance.AddDirtyNavGridCell(placementCell);
				if (this.rotatable.IsRotated)
				{
					intList.Add(Grid.CellAbove(placementCell));
					intList.Add(Grid.CellBelow(placementCell));
				}
				else
				{
					intList.Add(Grid.CellLeft(placementCell));
					intList.Add(Grid.CellRight(placementCell));
				}
			}
			foreach (int placementCell in this.building.PlacementCells)
			{
				Grid.HasDoor[placementCell] = false;
				Game.Instance.SetDupePassableSolid(placementCell, false, Grid.Solid[placementCell]);
				//Grid.CritterImpassable[placementCell] = false;
				Grid.DupeImpassable[placementCell] = false;
				Pathfinding.Instance.AddDirtyNavGridCell(placementCell);
			}
			base.OnCleanUp();
		}

		public void Seal() => this.controller.sm.isSealed.Set(true, this.controller);

		public void OrderUnseal() => this.controller.GoTo((StateMachine.BaseState)this.controller.sm.Sealed.awaiting_unlock);

		private void RefreshControlState()
		{
			switch (this.controlState)
			{
				case Door.ControlState.Auto:
					this.controller.sm.isLocked.Set(false, this.controller);
					break;
				case Door.ControlState.Opened:
					this.controller.sm.isLocked.Set(false, this.controller);
					break;
				case Door.ControlState.Locked:
					this.controller.sm.isLocked.Set(true, this.controller);
					break;
			}
			this.Trigger(279163026, (object)this.controlState);
			this.SetWorldState();
			this.GetComponent<KSelectable>().SetStatusItem(Db.Get().StatusItemCategories.Main, ModBuildingStatus.ChangeDoorControlState, (object)this);



		}

		private void OnOperationalChanged(object data)
		{
			bool isOperational = this.operational.IsOperational;
			if (isOperational == this.on)
				return;
			this.UpdateDoorSpeed(isOperational);
			if (this.on && this.GetComponent<KPrefabID>().HasTag(GameTags.Transition))
				this.SetActive(true);
			else
				this.SetActive(false);
		}

		private void UpdateDoorSpeed(bool powered)
		{
			this.on = powered;
			this.UpdateAnimAndSoundParams(powered);
			float positionPercent = this.animController.GetPositionPercent();
			this.animController.Play(this.animController.CurrentAnim.hash, this.animController.PlayMode);
			this.animController.SetPositionPercent(positionPercent);
		}

		private void UpdateAnimAndSoundParams(bool powered)
		{
			if (powered)
			{
				this.animController.PlaySpeedMultiplier = this.poweredAnimSpeed;
				if (this.doorClosingSound != null)
					this.loopingSounds.UpdateFirstParameter(this.doorClosingSound, CrittersPassableDoor.SOUND_POWERED_PARAMETER, 1f);
				if (this.doorOpeningSound == null)
					return;
				this.loopingSounds.UpdateFirstParameter(this.doorOpeningSound, CrittersPassableDoor.SOUND_POWERED_PARAMETER, 1f);
			}
			else
			{
				this.animController.PlaySpeedMultiplier = this.unpoweredAnimSpeed;
				if (this.doorClosingSound != null)
					this.loopingSounds.UpdateFirstParameter(this.doorClosingSound, CrittersPassableDoor.SOUND_POWERED_PARAMETER, 0.0f);
				if (this.doorOpeningSound == null)
					return;
				this.loopingSounds.UpdateFirstParameter(this.doorOpeningSound, CrittersPassableDoor.SOUND_POWERED_PARAMETER, 0.0f);
			}
		}

		private void SetActive(bool active)
		{
			if (!this.operational.IsOperational)
				return;
			this.operational.SetActive(active);
		}

		private void SetWorldState()
		{
			int[] placementCells = this.building.PlacementCells;
			bool is_door_open = this.IsOpen();
			this.SetPassableState(is_door_open, (IList<int>)placementCells);
			this.SetSimState(is_door_open, (IList<int>)placementCells);
		}

		private void SetPassableState(bool is_door_open, IList<int> cells)
		{
			for (int index = 0; index < cells.Count; ++index)
			{
				int cell = cells[index];
				switch (this.doorType)
				{
					case Door.DoorType.Pressure:
					case Door.DoorType.ManualPressure:
					case Door.DoorType.Sealed:
						Grid.CritterImpassable[cell] = false;// this.controlState != Door.ControlState.Opened;
						bool solid = !is_door_open;
						bool passable = this.controlState != Door.ControlState.Locked;
						Game.Instance.SetDupePassableSolid(cell, passable, solid);
						if (this.controlState == Door.ControlState.Opened)
						{
							this.doorOpenLiquidRefreshHack = true;
							this.doorOpenLiquidRefreshTime = 1f;
							break;
						}
						break;
					case Door.DoorType.Internal:
						Grid.CritterImpassable[cell] = false;// this.controlState != Door.ControlState.Opened;
						Grid.DupeImpassable[cell] = this.controlState == Door.ControlState.Locked;
						break;
				}
				Pathfinding.Instance.AddDirtyNavGridCell(cell);
			}
		}

		private void SetSimState(bool is_door_open, IList<int> cells)
		{
			PrimaryElement component = this.GetComponent<PrimaryElement>();
			float mass = component.Mass / (float)cells.Count;
			for (int index = 0; index < cells.Count; ++index)
			{
				int cell = cells[index];
				switch (this.doorType)
				{
					case Door.DoorType.Pressure:
					case Door.DoorType.ManualPressure:
					case Door.DoorType.Sealed:
						World.Instance.groundRenderer.MarkDirty(cell);
						if (is_door_open)
						{
							HandleVector<Game.CallbackInfo>.Handle handle = Game.Instance.callbackManager.Add(new Game.CallbackInfo(new System.Action(this.OnSimDoorOpened)));
							SimMessages.Dig(cell, handle.index, true);
							if (this.ShouldBlockFallingSand)
							{
								SimMessages.ClearCellProperties(cell, (byte)4);
								break;
							}
							SimMessages.SetCellProperties(cell, (byte)4);
							break;
						}
						HandleVector<Game.CallbackInfo>.Handle handle1 = Game.Instance.callbackManager.Add(new Game.CallbackInfo(new System.Action(this.OnSimDoorClosed)));
						float temperature = component.Temperature;
						if ((double)temperature <= 0.0)
							temperature = component.Temperature;
						SimMessages.ReplaceAndDisplaceElement(cell, component.ElementID, CellEventLogger.Instance.DoorClose, mass, temperature, callbackIdx: handle1.index);
						SimMessages.SetCellProperties(cell, (byte)4);
						break;
				}
			}
		}

		private void UpdateDoorState(bool cleaningUp)
		{
			foreach (int placementCell in this.building.PlacementCells)
			{
				if (Grid.IsValidCell(placementCell))
					Grid.Foundation[placementCell] = !cleaningUp;
			}
		}

		public void QueueStateChange(Door.ControlState nextState)
		{
			this.requestedState = this.requestedState == nextState ? this.controlState : nextState;
			if (this.requestedState == this.controlState)
			{
				if (this.changeStateChore == null)
					return;
				this.changeStateChore.Cancel("Change state");
				this.changeStateChore = (Chore)null;
				this.GetComponent<KSelectable>().RemoveStatusItem(Db.Get().BuildingStatusItems.ChangeDoorControlState);
			}
			else if (DebugHandler.InstantBuildMode)
			{
				this.controlState = this.requestedState;
				this.RefreshControlState();
				this.OnOperationalChanged((object)null);
				this.GetComponent<KSelectable>().RemoveStatusItem(Db.Get().BuildingStatusItems.ChangeDoorControlState);
				this.Open();
				this.Close();
			}
			else
			{
				if (this.changeStateChore != null)
					this.changeStateChore.Cancel("Change state");
				this.GetComponent<KSelectable>().AddStatusItem(Db.Get().BuildingStatusItems.ChangeDoorControlState, (object)this);
				this.changeStateChore = (Chore)new WorkChore<CrittersPassableDoor>(Db.Get().ChoreTypes.Toggle, (IStateMachineTarget)this, only_when_operational: false);
			}
		}

		private void OnSimDoorOpened()
		{
			if ((UnityEngine.Object)this == (UnityEngine.Object)null || !CrittersPassableDoor.DisplacesGas(this.doorType))
				return;
			StructureTemperatureComponents structureTemperatures = GameComps.StructureTemperatures;
			structureTemperatures.UnBypass(structureTemperatures.GetHandle(this.gameObject));
			this.do_melt_check = false;
		}

		private void OnSimDoorClosed()
		{
			if ((UnityEngine.Object)this == (UnityEngine.Object)null || !CrittersPassableDoor.DisplacesGas(this.doorType))
				return;
			StructureTemperatureComponents structureTemperatures = GameComps.StructureTemperatures;
			structureTemperatures.Bypass(structureTemperatures.GetHandle(this.gameObject));
			this.do_melt_check = true;
		}

		protected override void OnCompleteWork(Worker worker)
		{
			base.OnCompleteWork(worker);
			this.changeStateChore = (Chore)null;
			this.ApplyRequestedControlState();
		}

		public void Open()
		{
			if (this.openCount == 0 && CrittersPassableDoor.DisplacesGas(this.doorType))
			{
				StructureTemperatureComponents structureTemperatures = GameComps.StructureTemperatures;
				HandleVector<int>.Handle handle = structureTemperatures.GetHandle(this.gameObject);
				if (handle.IsValid() && structureTemperatures.IsBypassed(handle))
				{
					int[] placementCells = this.building.PlacementCells;
					float num1 = 0.0f;
					int num2 = 0;
					for (int index = 0; index < placementCells.Length; ++index)
					{
						int i = placementCells[index];
						if ((double)Grid.Mass[i] > 0.0)
						{
							++num2;
							num1 += Grid.Temperature[i];
						}
					}
					if (num2 > 0)
					{
						float num3 = num1 / (float)placementCells.Length;
						PrimaryElement component = this.GetComponent<PrimaryElement>();
						KCrashReporter.Assert((double)num3 > 0.0, "AttributeAnalyzerDoor has calculated an invalid temperature");
						double num4 = (double)num3;
						component.Temperature = (float)num4;
					}
				}
			}
			++this.openCount;
			switch (this.controlState)
			{
				case Door.ControlState.Auto:
				case Door.ControlState.Opened:
					this.controller.sm.isOpen.Set(true, this.controller);
					break;
			}
		}

		public void Close()
		{
			this.openCount = Mathf.Max(0, this.openCount - 1);
			if (this.openCount == 0 && CrittersPassableDoor.DisplacesGas(this.doorType))
			{
				StructureTemperatureComponents structureTemperatures = GameComps.StructureTemperatures;
				HandleVector<int>.Handle handle = structureTemperatures.GetHandle(this.gameObject);
				PrimaryElement component = this.GetComponent<PrimaryElement>();
				if (handle.IsValid() && !structureTemperatures.IsBypassed(handle))
				{
					float temperature = structureTemperatures.GetPayload(handle).Temperature;
					component.Temperature = temperature;
				}
			}
			switch (this.controlState)
			{
				case Door.ControlState.Auto:
					if (this.openCount != 0)
						break;
					this.controller.sm.isOpen.Set(false, this.controller);
					Game.Instance.userMenu.Refresh(this.gameObject);
					break;
				case Door.ControlState.Locked:
					this.controller.sm.isOpen.Set(false, this.controller);
					break;
			}
		}

		public bool IsOpen() => this.controller.IsInsideState((StateMachine.BaseState)this.controller.sm.open) || this.controller.IsInsideState((StateMachine.BaseState)this.controller.sm.closedelay) || this.controller.IsInsideState((StateMachine.BaseState)this.controller.sm.closeblocked);

		private void ApplyRequestedControlState(bool force = false)
		{
			if (this.requestedState == this.controlState && !force)
				return;
			this.controlState = this.requestedState;
			this.RefreshControlState();
			this.OnOperationalChanged((object)null);
			this.GetComponent<KSelectable>().RemoveStatusItem(Db.Get().BuildingStatusItems.ChangeDoorControlState);
			this.Trigger(1734268753, (object)this);
			if (force)
				return;
			this.Open();
			this.Close();
		}

		public void OnLogicValueChanged(object data)
		{
			LogicValueChanged logicValueChanged = (LogicValueChanged)data;
			if (logicValueChanged.portID != CrittersPassableDoor.OPEN_CLOSE_PORT_ID)
				return;
			int newValue = logicValueChanged.newValue;
			if (this.changeStateChore != null)
			{
				this.changeStateChore.Cancel("Change state");
				this.changeStateChore = (Chore)null;
			}
			this.requestedState = LogicCircuitNetwork.IsBitActive(0, newValue) ? Door.ControlState.Opened : Door.ControlState.Locked;
			this.applyLogicChange = true;
		}

		public void Sim200ms(float dt)
		{
			if ((UnityEngine.Object)this == (UnityEngine.Object)null)
				return;
			if (this.doorOpenLiquidRefreshHack)
			{
				this.doorOpenLiquidRefreshTime -= dt;
				if ((double)this.doorOpenLiquidRefreshTime <= 0.0)
				{
					this.doorOpenLiquidRefreshHack = false;
					foreach (int placementCell in this.building.PlacementCells)
						Pathfinding.Instance.AddDirtyNavGridCell(placementCell);
				}
			}
			if (this.applyLogicChange)
			{
				this.applyLogicChange = false;
				this.ApplyRequestedControlState();
			}
			if (!this.do_melt_check)
				return;
			StructureTemperatureComponents structureTemperatures = GameComps.StructureTemperatures;
			HandleVector<int>.Handle handle = structureTemperatures.GetHandle(this.gameObject);
			if (!handle.IsValid() || !structureTemperatures.IsBypassed(handle))
				return;
			foreach (int placementCell in this.building.PlacementCells)
			{
				if (!Grid.Solid[placementCell])
				{
					Util.KDestroyGameObject((Component)this);
					break;
				}
			}
		}


		[SpecialName]
		bool INavDoor.isSpawned => this.isSpawned;

		public class Controller : GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor>
		{
			public GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State open;
			public GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State opening;
			public GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State closed;
			public GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State closing;
			public GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State closedelay;
			public GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State closeblocked;
			public GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State locking;
			public GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State locked;
			public GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State unlocking;
			public CrittersPassableDoor.Controller.SealedStates Sealed;
			public StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.BoolParameter isOpen;
			public StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.BoolParameter isLocked;
			public StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.BoolParameter isBlocked;
			public StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.BoolParameter isSealed;
			public StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.BoolParameter sealDirectionRight;

			public override void InitializeStates(out StateMachine.BaseState default_state)
			{
				this.serializable = StateMachine.SerializeType.Both_DEPRECATED;
				default_state = (StateMachine.BaseState)this.closed;
				this.root.Update("RefreshIsBlocked", (System.Action<CrittersPassableDoor.Controller.Instance, float>)((smi, dt) => smi.RefreshIsBlocked())).ParamTransition<bool>((StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.Parameter<bool>)this.isSealed, this.Sealed.closed, GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.IsTrue);
				this.closeblocked.PlayAnim("open").ParamTransition<bool>((StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.Parameter<bool>)this.isOpen, this.open, GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.IsTrue).ParamTransition<bool>((StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.Parameter<bool>)this.isBlocked, this.closedelay, GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.IsFalse);
				this.closedelay.PlayAnim("open").ScheduleGoTo(0.5f, (StateMachine.BaseState)this.closing).ParamTransition<bool>((StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.Parameter<bool>)this.isOpen, this.open, GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.IsTrue).ParamTransition<bool>((StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.Parameter<bool>)this.isBlocked, this.closeblocked, GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.IsTrue);
				this.closing.ParamTransition<bool>((StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.Parameter<bool>)this.isBlocked, this.closeblocked, GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.IsTrue).ToggleTag(GameTags.Transition).ToggleLoopingSound("Closing loop", (Func<CrittersPassableDoor.Controller.Instance, string>)(smi => smi.master.doorClosingSound), (Func<CrittersPassableDoor.Controller.Instance, bool>)(smi => !string.IsNullOrEmpty(smi.master.doorClosingSound))).Enter("SetParams", (StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State.Callback)(smi => smi.master.UpdateAnimAndSoundParams(smi.master.on))).Update((System.Action<CrittersPassableDoor.Controller.Instance, float>)((smi, dt) =>
				{
					if (smi.master.doorClosingSound == null)
						return;
					smi.master.loopingSounds.UpdateSecondParameter(smi.master.doorClosingSound, CrittersPassableDoor.SOUND_PROGRESS_PARAMETER, smi.Get<KBatchedAnimController>().GetPositionPercent());
				}), UpdateRate.SIM_33ms).Enter("SetActive", (StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State.Callback)(smi => smi.master.SetActive(true))).Exit("SetActive", (StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State.Callback)(smi => smi.master.SetActive(false))).PlayAnim("closing").OnAnimQueueComplete(this.closed);
				this.open.PlayAnim("open").ParamTransition<bool>((StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.Parameter<bool>)this.isOpen, this.closeblocked, GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.IsFalse).Enter("SetWorldStateOpen", (StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State.Callback)(smi => smi.master.SetWorldState()));
				this.closed.PlayAnim("closed").ParamTransition<bool>((StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.Parameter<bool>)this.isOpen, this.opening, GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.IsTrue).ParamTransition<bool>((StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.Parameter<bool>)this.isLocked, this.locking, GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.IsTrue).Enter("SetWorldStateClosed", (StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State.Callback)(smi => smi.master.SetWorldState()));
				this.locking.PlayAnim("locked_pre").OnAnimQueueComplete(this.locked).Enter("SetWorldStateClosed", (StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State.Callback)(smi => smi.master.SetWorldState()));
				this.locked.PlayAnim("locked").ParamTransition<bool>((StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.Parameter<bool>)this.isLocked, this.unlocking, GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.IsFalse);
				this.unlocking.PlayAnim("locked_pst").OnAnimQueueComplete(this.closed);
				this.opening.ToggleTag(GameTags.Transition).ToggleLoopingSound("Opening loop", (Func<CrittersPassableDoor.Controller.Instance, string>)(smi => smi.master.doorOpeningSound), (Func<CrittersPassableDoor.Controller.Instance, bool>)(smi => !string.IsNullOrEmpty(smi.master.doorOpeningSound))).Enter("SetParams", (StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State.Callback)(smi => smi.master.UpdateAnimAndSoundParams(smi.master.on))).Update((System.Action<CrittersPassableDoor.Controller.Instance, float>)((smi, dt) =>
				{
					if (smi.master.doorOpeningSound == null)
						return;
					smi.master.loopingSounds.UpdateSecondParameter(smi.master.doorOpeningSound, CrittersPassableDoor.SOUND_PROGRESS_PARAMETER, smi.Get<KBatchedAnimController>().GetPositionPercent());
				}), UpdateRate.SIM_33ms).Enter("SetActive", (StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State.Callback)(smi => smi.master.SetActive(true))).Exit("SetActive", (StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State.Callback)(smi => smi.master.SetActive(false))).PlayAnim("opening").OnAnimQueueComplete(this.open);
				this.Sealed.Enter((StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State.Callback)(smi =>
				{
					OccupyArea component = smi.master.GetComponent<OccupyArea>();
					for (int index = 0; index < component.OccupiedCellsOffsets.Length; ++index)
						Grid.PreventFogOfWarReveal[Grid.OffsetCell(Grid.PosToCell(smi.master.gameObject), component.OccupiedCellsOffsets[index])] = false;
					smi.sm.isLocked.Set(true, smi);
					smi.master.controlState = Door.ControlState.Locked;
					smi.master.RefreshControlState();
					if (!smi.master.GetComponent<Unsealable>().facingRight)
						return;
					smi.master.GetComponent<KBatchedAnimController>().FlipX = true;
				})).Enter("SetWorldStateClosed", (StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State.Callback)(smi => smi.master.SetWorldState())).Exit((StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State.Callback)(smi =>
				{
					smi.sm.isLocked.Set(false, smi);
					smi.master.GetComponent<AccessControl>().controlEnabled = true;
					smi.master.controlState = Door.ControlState.Opened;
					smi.master.RefreshControlState();
					smi.sm.isOpen.Set(true, smi);
					smi.sm.isLocked.Set(false, smi);
					smi.sm.isSealed.Set(false, smi);
				}));
				this.Sealed.closed.PlayAnim("sealed", KAnim.PlayMode.Once);
				this.Sealed.awaiting_unlock.ToggleChore((Func<CrittersPassableDoor.Controller.Instance, Chore>)(smi => this.CreateUnsealChore(smi, true)), this.Sealed.chore_pst);
				this.Sealed.chore_pst.Enter((StateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State.Callback)(smi =>
				{
					smi.master.hasBeenUnsealed = true;
					if (smi.master.GetComponent<Unsealable>().unsealed)
					{
						smi.GoTo((StateMachine.BaseState)this.opening);
						FogOfWarMask.ClearMask(Grid.CellRight(Grid.PosToCell(smi.master.gameObject)));
						FogOfWarMask.ClearMask(Grid.CellLeft(Grid.PosToCell(smi.master.gameObject)));
					}
					else
						smi.GoTo((StateMachine.BaseState)this.Sealed.closed);
				}));
			}




			private Chore CreateUnsealChore(CrittersPassableDoor.Controller.Instance smi, bool approach_right) => (Chore)new WorkChore<Unsealable>(Db.Get().ChoreTypes.Toggle, (IStateMachineTarget)smi.master);

			public class SealedStates :
			  GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State
			{
				public GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State closed;
				public CrittersPassableDoor.Controller.SealedStates.AwaitingUnlock awaiting_unlock;
				public GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State chore_pst;

				public class AwaitingUnlock :
				  GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State
				{
					public GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State awaiting_arrival;
					public GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.State unlocking;
				}
			}

			public new class Instance :
			  GameStateMachine<CrittersPassableDoor.Controller, CrittersPassableDoor.Controller.Instance, CrittersPassableDoor, object>.GameInstance
			{
				public Instance(CrittersPassableDoor AttributeAnalyzerDoor)
				  : base(AttributeAnalyzerDoor)
				{
				}

				public void RefreshIsBlocked()
				{
					bool flag = false;
					foreach (int placementCell in this.master.GetComponent<Building>().PlacementCells)
					{
						if ((UnityEngine.Object)Grid.Objects[placementCell, 40] != (UnityEngine.Object)null)
						{
							flag = true;
							break;
						}
					}
					this.sm.isBlocked.Set(flag, this.smi);
				}
			}
		}
	}
	 

}