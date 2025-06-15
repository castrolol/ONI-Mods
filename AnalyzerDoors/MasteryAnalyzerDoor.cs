using HarmonyLib;
using Klei.AI;
using KSerialization;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static AnalyzerDoors.AttributesAnalyzerSideScreen;

namespace AnalyzerDoors
{


	[SerializationConfig(MemberSerialization.OptIn)]
	public class MasteryAnalyzerDoor : Workable, ISaveLoadable, ICustomDoor, ISim200ms, ISim1000ms, INavDoor, IGameObjectEffectDescriptor
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
		private MasteryAnalyzerDoor.Controller.Instance controller;
		private LoggerFSS log;
		private const float REFRESH_HACK_DELAY = 1f;
		private bool doorOpenLiquidRefreshHack;
		private float doorOpenLiquidRefreshTime;
		private static readonly EventSystem.IntraObjectHandler<MasteryAnalyzerDoor> OnCopySettingsDelegate = new EventSystem.IntraObjectHandler<MasteryAnalyzerDoor>((System.Action<MasteryAnalyzerDoor, object>)((component, data) => component.OnCopySettings(data)));
		public static readonly HashedString OPEN_CLOSE_PORT_ID = new HashedString("DoorOpenClose");
		private static readonly KAnimFile[] OVERRIDE_ANIMS = new KAnimFile[1]
		{
			Assets.GetAnim((HashedString) "anim_use_remote_kanim")
		};
		private static readonly EventSystem.IntraObjectHandler<MasteryAnalyzerDoor> OnOperationalChangedDelegate = new EventSystem.IntraObjectHandler<MasteryAnalyzerDoor>((System.Action<MasteryAnalyzerDoor, object>)((component, data) => component.OnOperationalChanged(data)));
		private static readonly EventSystem.IntraObjectHandler<MasteryAnalyzerDoor> OnLogicValueChangedDelegate = new EventSystem.IntraObjectHandler<MasteryAnalyzerDoor>((System.Action<MasteryAnalyzerDoor, object>)((component, data) => component.OnLogicValueChanged(data)));
		private bool applyLogicChange;


		private void OnCopySettings(object data)
		{
			MasteryAnalyzerDoor component = ((GameObject)data).GetComponent<MasteryAnalyzerDoor>();
			if (!((UnityEngine.Object)component != (UnityEngine.Object)null))
				return;
			this.QueueStateChange(component.requestedState);
		}


		public MasteryAnalyzerDoor() => this.SetOffsetTable(OffsetGroups.InvertedStandardTable);

		public Door.ControlState CurrentState => this.controlState;

		public Door.ControlState RequestedState => this.requestedState;

		public bool ShouldBlockFallingSand => this.rotatable.GetOrientation() != this.verticalOrientation;

		public bool isSealed => this.controller.sm.isSealed.Get(this.controller);

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			this.overrideAnims = MasteryAnalyzerDoor.OVERRIDE_ANIMS;
			this.synchronizeAnims = false;
			this.SetWorkTime(3f);

			if (allowedSkills == null)
				allowedSkills = new List<string>();

			if (!string.IsNullOrEmpty(this.doorClosingSoundEventName))
				this.doorClosingSound = GlobalAssets.GetSound(this.doorClosingSoundEventName);
			if (!string.IsNullOrEmpty(this.doorOpeningSoundEventName))
				this.doorOpeningSound = GlobalAssets.GetSound(this.doorOpeningSoundEventName);
			this.Subscribe<MasteryAnalyzerDoor>(-905833192, MasteryAnalyzerDoor.OnCopySettingsDelegate);
		}

		private Door.ControlState GetNextState(Door.ControlState wantedState) => (Door.ControlState)((int)(wantedState + 1) % 3);

		private static bool DisplacesGas(Door.DoorType type) => type != Door.DoorType.Internal;

		protected override void OnSpawn()
		{
			base.OnSpawn();
			if ((UnityEngine.Object)this.GetComponent<KPrefabID>() != (UnityEngine.Object)null)
				this.log = new LoggerFSS(nameof(MasteryAnalyzerDoor));
			if (!this.allowAutoControl && this.controlState == Door.ControlState.Auto)
				this.controlState = Door.ControlState.Locked;
			StructureTemperatureComponents structureTemperatures = GameComps.StructureTemperatures;
			HandleVector<int>.Handle handle = structureTemperatures.GetHandle(this.gameObject);
			if (MasteryAnalyzerDoor.DisplacesGas(this.doorType))
				structureTemperatures.Bypass(handle);
			this.controller = new MasteryAnalyzerDoor.Controller.Instance(this);
			this.controller.StartSM();
			if (this.doorType == Door.DoorType.Sealed && !this.hasBeenUnsealed)
				this.Seal();
			this.UpdateDoorSpeed(this.operational.IsOperational);
			this.Subscribe<MasteryAnalyzerDoor>(-592767678, MasteryAnalyzerDoor.OnOperationalChangedDelegate);
			this.Subscribe<MasteryAnalyzerDoor>(824508782, MasteryAnalyzerDoor.OnOperationalChangedDelegate);
			this.Subscribe<MasteryAnalyzerDoor>(-801688580, MasteryAnalyzerDoor.OnLogicValueChangedDelegate);
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
				if (MasteryAnalyzerDoor.DisplacesGas(this.doorType))
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
				Grid.CritterImpassable[placementCell] = false;
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
			this.GetComponent<KSelectable>().SetStatusItem(Db.Get().StatusItemCategories.Main, ModBuildingStatus.CurrentDoorControlState, (object)this);



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
					this.loopingSounds.UpdateFirstParameter(this.doorClosingSound, MasteryAnalyzerDoor.SOUND_POWERED_PARAMETER, 1f);
				if (this.doorOpeningSound == null)
					return;
				this.loopingSounds.UpdateFirstParameter(this.doorOpeningSound, MasteryAnalyzerDoor.SOUND_POWERED_PARAMETER, 1f);
			}
			else
			{
				this.animController.PlaySpeedMultiplier = this.unpoweredAnimSpeed;
				if (this.doorClosingSound != null)
					this.loopingSounds.UpdateFirstParameter(this.doorClosingSound, MasteryAnalyzerDoor.SOUND_POWERED_PARAMETER, 0.0f);
				if (this.doorOpeningSound == null)
					return;
				this.loopingSounds.UpdateFirstParameter(this.doorOpeningSound, MasteryAnalyzerDoor.SOUND_POWERED_PARAMETER, 0.0f);
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
						Grid.CritterImpassable[cell] = this.controlState != Door.ControlState.Opened;
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
						Grid.CritterImpassable[cell] = this.controlState != Door.ControlState.Opened;
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
				this.changeStateChore = (Chore)new WorkChore<MasteryAnalyzerDoor>(Db.Get().ChoreTypes.Toggle, (IStateMachineTarget)this, only_when_operational: false);
			}
		}

		private void OnSimDoorOpened()
		{
			if ((UnityEngine.Object)this == (UnityEngine.Object)null || !MasteryAnalyzerDoor.DisplacesGas(this.doorType))
				return;
			StructureTemperatureComponents structureTemperatures = GameComps.StructureTemperatures;
			structureTemperatures.UnBypass(structureTemperatures.GetHandle(this.gameObject));
			this.do_melt_check = false;
		}

		private void OnSimDoorClosed()
		{
			if ((UnityEngine.Object)this == (UnityEngine.Object)null || !MasteryAnalyzerDoor.DisplacesGas(this.doorType))
				return;
			StructureTemperatureComponents structureTemperatures = GameComps.StructureTemperatures;
			structureTemperatures.Bypass(structureTemperatures.GetHandle(this.gameObject));
			this.do_melt_check = true;
		}

		protected override void OnCompleteWork(WorkerBase worker)
		{
			base.OnCompleteWork(worker);
			this.changeStateChore = (Chore)null;
			this.ApplyRequestedControlState();
		}

		public void Open()
		{
			if (this.openCount == 0 && MasteryAnalyzerDoor.DisplacesGas(this.doorType))
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
			if (this.openCount == 0 && MasteryAnalyzerDoor.DisplacesGas(this.doorType))
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
			if (logicValueChanged.portID != MasteryAnalyzerDoor.OPEN_CLOSE_PORT_ID)
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

		public void Sim1000ms(float dt)
		{
			UpdateMasteryAccess();
		}


		public List<MinionIdentity> allowedMinions = new List<MinionIdentity>();
		public List<MinionIdentity> disallowedMinions = new List<MinionIdentity>();
		[Serialize]
		public List<string> allowedSkills;
		[Serialize]
		public bool needsAll;

		public void UpdateMasteryAccess(bool forceUpdate = true)
		{
			var identities = Components.MinionAssignablesProxy;
			allowedMinions.Clear();
			disallowedMinions.Clear();

			var groups = Db.Get().SkillGroups.resources;
			var skills = Db.Get().Skills.resources;


			foreach (var obj in identities)
			{

				var assignablesIdentity = (MinionAssignablesProxy)obj;

				var minionObject = assignablesIdentity.GetTargetGameObject();

				if (minionObject.GetMyWorldId() != ClusterManager.Instance.activeWorldId)
					continue;

				var resume = minionObject.GetComponent<MinionResume>();
				var identity = minionObject.GetComponent<MinionIdentity>();


				var permission = accessControl.DefaultPermission;

				var allowed = true;
				var orConditions = new List<bool>();
				var masteries = new List<string>();

				foreach (var skillId in resume.MasteryBySkillID)
					if (skillId.Value)
						masteries.Add(skillId.Key);


				foreach (var allowedSkill in allowedSkills)
				{

					if (!masteries.Contains(allowedSkill))
					{

						orConditions.Add(false);
						allowed = false;

						if (needsAll)
							break;

					}
					else
					{
						orConditions.Add(true);

					}

				}

				if (!needsAll)
				{
					if (orConditions.Count <= 0)
					{
						allowed = true;
					}
					else
					{
						allowed = false;
						foreach (var b in orConditions)
						{
							if (b)
							{
								allowed = true;
								break;
							}
						}

					}
				}

				if (allowed)
				{
					allowedMinions.Add(identity);
					accessControl.SetPermission(assignablesIdentity, permission);
				}
				else
				{
					disallowedMinions.Add(identity);
					accessControl.SetPermission(assignablesIdentity, AccessControl.Permission.Neither);
				}


			}

			//if (forceUpdate)
			//{
			//	var detail = Traverse.Create(DetailsScreen.Instance);
			//	var tabHeader = detail.Field("tabHeader");//.GetValue<DetailTabHeader>();
			//	var tabGo = tabHeader.Field("simpleInfoScreen").GetValue<GameObject>();
			//	var tab = tabGo.GetComponent<SimpleInfoScreen>();

			//	Traverse.Create(tab).Method("RefreshWorldPanel").GetValue();
			//}

		}


		public override List<Descriptor> GetDescriptors(GameObject go)
		{
			var descriptors = base.GetDescriptors(go);

			if (allowedMinions.Count > 0)
			{

				var enabledTitle = new Descriptor("<b>" + Strings.Get(new StringKey("STRINGS.UI.FRONTEND.MODS.TOOLTIPS.ENABLED")) + "</b>", "", Descriptor.DescriptorType.Effect);
				descriptors.Add(enabledTitle);
				foreach (var identity in allowedMinions)
				{
					var identitDescriptor = new Descriptor(
						"• <color=#228f39>" + identity.GetProperName() + "</color>",
						"",
						Descriptor.DescriptorType.Effect
					);
					identitDescriptor.IncreaseIndent();
					descriptors.Add(identitDescriptor);
				}
			}

			if (disallowedMinions.Count > 0)
			{
				var disabledTitle = new Descriptor("<b>" + Strings.Get(new StringKey("STRINGS.UI.FRONTEND.MODS.TOOLTIPS.DISABLED")) + "</b>", "", Descriptor.DescriptorType.Effect);
				descriptors.Add(disabledTitle);
				foreach (var identity in disallowedMinions)
				{
					var identitDescriptor = new Descriptor(
						"• <color=#F44A47>" + identity.GetProperName() + "</color>",
						"",
						Descriptor.DescriptorType.Effect
					);
					identitDescriptor.IncreaseIndent();
					descriptors.Add(identitDescriptor);
				}

			}
			return descriptors;
		}



		[SpecialName]
		bool INavDoor.isSpawned => this.isSpawned;

		public class Controller : GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor>
		{
			public GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State open;
			public GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State opening;
			public GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State closed;
			public GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State closing;
			public GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State closedelay;
			public GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State closeblocked;
			public GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State locking;
			public GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State locked;
			public GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State unlocking;
			public MasteryAnalyzerDoor.Controller.SealedStates Sealed;
			public StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.BoolParameter isOpen;
			public StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.BoolParameter isLocked;
			public StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.BoolParameter isBlocked;
			public StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.BoolParameter isSealed;
			public StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.BoolParameter sealDirectionRight;

			public override void InitializeStates(out StateMachine.BaseState default_state)
			{
				this.serializable = StateMachine.SerializeType.Both_DEPRECATED;
				default_state = (StateMachine.BaseState)this.closed;
				this.root.Update("RefreshIsBlocked", (System.Action<MasteryAnalyzerDoor.Controller.Instance, float>)((smi, dt) => smi.RefreshIsBlocked())).ParamTransition<bool>((StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.Parameter<bool>)this.isSealed, this.Sealed.closed, GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.IsTrue);
				this.closeblocked.PlayAnim("open").ParamTransition<bool>((StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.Parameter<bool>)this.isOpen, this.open, GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.IsTrue).ParamTransition<bool>((StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.Parameter<bool>)this.isBlocked, this.closedelay, GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.IsFalse);
				this.closedelay.PlayAnim("open").ScheduleGoTo(0.5f, (StateMachine.BaseState)this.closing).ParamTransition<bool>((StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.Parameter<bool>)this.isOpen, this.open, GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.IsTrue).ParamTransition<bool>((StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.Parameter<bool>)this.isBlocked, this.closeblocked, GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.IsTrue);
				this.closing.ParamTransition<bool>((StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.Parameter<bool>)this.isBlocked, this.closeblocked, GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.IsTrue).ToggleTag(GameTags.Transition).ToggleLoopingSound("Closing loop", (Func<MasteryAnalyzerDoor.Controller.Instance, string>)(smi => smi.master.doorClosingSound), (Func<MasteryAnalyzerDoor.Controller.Instance, bool>)(smi => !string.IsNullOrEmpty(smi.master.doorClosingSound))).Enter("SetParams", (StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State.Callback)(smi => smi.master.UpdateAnimAndSoundParams(smi.master.on))).Update((System.Action<MasteryAnalyzerDoor.Controller.Instance, float>)((smi, dt) =>
				{
					if (smi.master.doorClosingSound == null)
						return;
					smi.master.loopingSounds.UpdateSecondParameter(smi.master.doorClosingSound, MasteryAnalyzerDoor.SOUND_PROGRESS_PARAMETER, smi.Get<KBatchedAnimController>().GetPositionPercent());
				}), UpdateRate.SIM_33ms).Enter("SetActive", (StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State.Callback)(smi => smi.master.SetActive(true))).Exit("SetActive", (StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State.Callback)(smi => smi.master.SetActive(false))).PlayAnim("closing").OnAnimQueueComplete(this.closed);
				this.open.PlayAnim("open").ParamTransition<bool>((StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.Parameter<bool>)this.isOpen, this.closeblocked, GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.IsFalse).Enter("SetWorldStateOpen", (StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State.Callback)(smi => smi.master.SetWorldState()));
				this.closed.PlayAnim("closed").ParamTransition<bool>((StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.Parameter<bool>)this.isOpen, this.opening, GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.IsTrue).ParamTransition<bool>((StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.Parameter<bool>)this.isLocked, this.locking, GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.IsTrue).Enter("SetWorldStateClosed", (StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State.Callback)(smi => smi.master.SetWorldState()));
				this.locking.PlayAnim("locked_pre").OnAnimQueueComplete(this.locked).Enter("SetWorldStateClosed", (StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State.Callback)(smi => smi.master.SetWorldState()));
				this.locked.PlayAnim("locked").ParamTransition<bool>((StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.Parameter<bool>)this.isLocked, this.unlocking, GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.IsFalse);
				this.unlocking.PlayAnim("locked_pst").OnAnimQueueComplete(this.closed);
				this.opening.ToggleTag(GameTags.Transition).ToggleLoopingSound("Opening loop", (Func<MasteryAnalyzerDoor.Controller.Instance, string>)(smi => smi.master.doorOpeningSound), (Func<MasteryAnalyzerDoor.Controller.Instance, bool>)(smi => !string.IsNullOrEmpty(smi.master.doorOpeningSound))).Enter("SetParams", (StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State.Callback)(smi => smi.master.UpdateAnimAndSoundParams(smi.master.on))).Update((System.Action<MasteryAnalyzerDoor.Controller.Instance, float>)((smi, dt) =>
				{
					if (smi.master.doorOpeningSound == null)
						return;
					smi.master.loopingSounds.UpdateSecondParameter(smi.master.doorOpeningSound, MasteryAnalyzerDoor.SOUND_PROGRESS_PARAMETER, smi.Get<KBatchedAnimController>().GetPositionPercent());
				}), UpdateRate.SIM_33ms).Enter("SetActive", (StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State.Callback)(smi => smi.master.SetActive(true))).Exit("SetActive", (StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State.Callback)(smi => smi.master.SetActive(false))).PlayAnim("opening").OnAnimQueueComplete(this.open);
				this.Sealed.Enter((StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State.Callback)(smi =>
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
				})).Enter("SetWorldStateClosed", (StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State.Callback)(smi => smi.master.SetWorldState())).Exit((StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State.Callback)(smi =>
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
				this.Sealed.awaiting_unlock.ToggleChore((Func<MasteryAnalyzerDoor.Controller.Instance, Chore>)(smi => this.CreateUnsealChore(smi, true)), this.Sealed.chore_pst);
				this.Sealed.chore_pst.Enter((StateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State.Callback)(smi =>
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




			private Chore CreateUnsealChore(MasteryAnalyzerDoor.Controller.Instance smi, bool approach_right) => (Chore)new WorkChore<Unsealable>(Db.Get().ChoreTypes.Toggle, (IStateMachineTarget)smi.master);

			public class SealedStates :
			  GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State
			{
				public GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State closed;
				public MasteryAnalyzerDoor.Controller.SealedStates.AwaitingUnlock awaiting_unlock;
				public GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State chore_pst;

				public class AwaitingUnlock :
				  GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State
				{
					public GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State awaiting_arrival;
					public GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.State unlocking;
				}
			}

			public new class Instance :
			  GameStateMachine<MasteryAnalyzerDoor.Controller, MasteryAnalyzerDoor.Controller.Instance, MasteryAnalyzerDoor, object>.GameInstance
			{
				public Instance(MasteryAnalyzerDoor AttributeAnalyzerDoor)
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

	[Serializable]
	public class MasteryConfig
	{


		[Serialize]
		public bool needsAll;

		[Serialize]

		public List<MasteryGroup> masteries;

		//public List<MasteryCondition> GetMasteries()
		//{
		//	if (masteries == null)
		//	{
		//		masteries = CreateMasteries();
		//	}

		//	return masteries;
		//}

		public List<MasteryCondition> CreateMasteries()
		{
			var list = new List<MasteryCondition>();

			foreach (var skill in Db.Get().Skills.resources)
			{
				list.Add(new MasteryCondition(skill.Id));
			}

			return list;
		}

	}

	[Serializable]
	public class MasteryGroup : IListableOption
	{

		public string id;

		private string name;

		List<MasteryCondition> skills;
		public MasteryGroup(string id)
		{

			this.id = id;
			var res = Db.Get().Skills.resources;
			foreach (var skill in res)
			{
				if (id == skill.skillGroup)
				{
					skills.Add(new MasteryCondition(skill.Id));
				}
			}
		}

		public string GetProperName()
		{
			if (name == null)
			{
				var skill = Db.Get().SkillGroups.Get(id);
				name = skill.Name;
			}

			return name;
		}

	}

	[Serializable]
	public class MasteryCondition : IListableOption
	{

		public string id;

		private string name;

		public MasteryCondition(string attributeId)
		{

			id = attributeId;
		}

		public string GetProperName()
		{
			if (name == null)
			{
				var skill = Db.Get().Skills.Get(id);
				name = skill.Name;
			}

			return name;
		}

		public string GetProperDescription()
		{

			return "";
		}

	}


}