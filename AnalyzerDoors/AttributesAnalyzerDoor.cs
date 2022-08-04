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
	public class AttributesAnalyzerDoor : Workable, ICustomDoor, ISaveLoadable, ISim200ms, ISim1000ms, INavDoor, IGameObjectEffectDescriptor
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
		private AttributesAnalyzerDoor.Controller.Instance controller;
		private LoggerFSS log;
		private const float REFRESH_HACK_DELAY = 1f;
		private bool doorOpenLiquidRefreshHack;
		private float doorOpenLiquidRefreshTime;
		private static readonly EventSystem.IntraObjectHandler<AttributesAnalyzerDoor> OnCopySettingsDelegate = new EventSystem.IntraObjectHandler<AttributesAnalyzerDoor>((System.Action<AttributesAnalyzerDoor, object>)((component, data) => component.OnCopySettings(data)));
		public static readonly HashedString OPEN_CLOSE_PORT_ID = new HashedString("DoorOpenClose");
		private static readonly KAnimFile[] OVERRIDE_ANIMS = new KAnimFile[1]
		{
			Assets.GetAnim((HashedString) "anim_use_remote_kanim")
		};
		private static readonly EventSystem.IntraObjectHandler<AttributesAnalyzerDoor> OnOperationalChangedDelegate = new EventSystem.IntraObjectHandler<AttributesAnalyzerDoor>((System.Action<AttributesAnalyzerDoor, object>)((component, data) => component.OnOperationalChanged(data)));
		private static readonly EventSystem.IntraObjectHandler<AttributesAnalyzerDoor> OnLogicValueChangedDelegate = new EventSystem.IntraObjectHandler<AttributesAnalyzerDoor>((System.Action<AttributesAnalyzerDoor, object>)((component, data) => component.OnLogicValueChanged(data)));
		private bool applyLogicChange;
		[Serialize]
		public AttributesConfig config;

		private void OnCopySettings(object data)
		{
			AttributesAnalyzerDoor component = ((GameObject)data).GetComponent<AttributesAnalyzerDoor>();
			if (!((UnityEngine.Object)component != (UnityEngine.Object)null))
				return;
			this.QueueStateChange(component.requestedState);
		}


		public AttributesAnalyzerDoor() => this.SetOffsetTable(OffsetGroups.InvertedStandardTable);

		public Door.ControlState CurrentState => this.controlState;

		public Door.ControlState RequestedState => this.requestedState;

		public bool ShouldBlockFallingSand => this.rotatable.GetOrientation() != this.verticalOrientation;

		public bool isSealed => this.controller.sm.isSealed.Get(this.controller);

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			this.overrideAnims = AttributesAnalyzerDoor.OVERRIDE_ANIMS;
			this.synchronizeAnims = false;
			this.SetWorkTime(3f);

			if (config == null)
				config = new AttributesConfig();

			if (!string.IsNullOrEmpty(this.doorClosingSoundEventName))
				this.doorClosingSound = GlobalAssets.GetSound(this.doorClosingSoundEventName);
			if (!string.IsNullOrEmpty(this.doorOpeningSoundEventName))
				this.doorOpeningSound = GlobalAssets.GetSound(this.doorOpeningSoundEventName);
			this.Subscribe<AttributesAnalyzerDoor>(-905833192, AttributesAnalyzerDoor.OnCopySettingsDelegate);
		}

		private Door.ControlState GetNextState(Door.ControlState wantedState) => (Door.ControlState)((int)(wantedState + 1) % 3);

		private static bool DisplacesGas(Door.DoorType type) => type != Door.DoorType.Internal;

		protected override void OnSpawn()
		{
			base.OnSpawn();
			if ((UnityEngine.Object)this.GetComponent<KPrefabID>() != (UnityEngine.Object)null)
				this.log = new LoggerFSS(nameof(AttributesAnalyzerDoor));
			if (!this.allowAutoControl && this.controlState == Door.ControlState.Auto)
				this.controlState = Door.ControlState.Locked;
			StructureTemperatureComponents structureTemperatures = GameComps.StructureTemperatures;
			HandleVector<int>.Handle handle = structureTemperatures.GetHandle(this.gameObject);
			if (AttributesAnalyzerDoor.DisplacesGas(this.doorType))
				structureTemperatures.Bypass(handle);
			this.controller = new AttributesAnalyzerDoor.Controller.Instance(this);
			this.controller.StartSM();
			if (this.doorType == Door.DoorType.Sealed && !this.hasBeenUnsealed)
				this.Seal();
			this.UpdateDoorSpeed(this.operational.IsOperational);
			this.Subscribe<AttributesAnalyzerDoor>(-592767678, AttributesAnalyzerDoor.OnOperationalChangedDelegate);
			this.Subscribe<AttributesAnalyzerDoor>(824508782, AttributesAnalyzerDoor.OnOperationalChangedDelegate);
			this.Subscribe<AttributesAnalyzerDoor>(-801688580, AttributesAnalyzerDoor.OnLogicValueChangedDelegate);
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
				if (AttributesAnalyzerDoor.DisplacesGas(this.doorType))
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
					this.loopingSounds.UpdateFirstParameter(this.doorClosingSound, AttributesAnalyzerDoor.SOUND_POWERED_PARAMETER, 1f);
				if (this.doorOpeningSound == null)
					return;
				this.loopingSounds.UpdateFirstParameter(this.doorOpeningSound, AttributesAnalyzerDoor.SOUND_POWERED_PARAMETER, 1f);
			}
			else
			{
				this.animController.PlaySpeedMultiplier = this.unpoweredAnimSpeed;
				if (this.doorClosingSound != null)
					this.loopingSounds.UpdateFirstParameter(this.doorClosingSound, AttributesAnalyzerDoor.SOUND_POWERED_PARAMETER, 0.0f);
				if (this.doorOpeningSound == null)
					return;
				this.loopingSounds.UpdateFirstParameter(this.doorOpeningSound, AttributesAnalyzerDoor.SOUND_POWERED_PARAMETER, 0.0f);
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
				this.changeStateChore = (Chore)new WorkChore<AttributesAnalyzerDoor>(Db.Get().ChoreTypes.Toggle, (IStateMachineTarget)this, only_when_operational: false);
			}
		}

		private void OnSimDoorOpened()
		{
			if ((UnityEngine.Object)this == (UnityEngine.Object)null || !AttributesAnalyzerDoor.DisplacesGas(this.doorType))
				return;
			StructureTemperatureComponents structureTemperatures = GameComps.StructureTemperatures;
			structureTemperatures.UnBypass(structureTemperatures.GetHandle(this.gameObject));
			this.do_melt_check = false;
		}

		private void OnSimDoorClosed()
		{
			if ((UnityEngine.Object)this == (UnityEngine.Object)null || !AttributesAnalyzerDoor.DisplacesGas(this.doorType))
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
			if (this.openCount == 0 && AttributesAnalyzerDoor.DisplacesGas(this.doorType))
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
			if (this.openCount == 0 && AttributesAnalyzerDoor.DisplacesGas(this.doorType))
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
			if (logicValueChanged.portID != AttributesAnalyzerDoor.OPEN_CLOSE_PORT_ID)
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
			UpdateAttributesAccess();
		}

		public bool IsConditionReached(float value, AttributeConditionType type, float target)
		{
			switch (type)
			{
				case AttributeConditionType.Ignore:
					return true;
				case AttributeConditionType.Greater:
					return value > target;
				case AttributeConditionType.GreaterOrEqual:
					return value >= target;
				case AttributeConditionType.Less:
					return value < target;
				case AttributeConditionType.LessOrEqual:
					return value <= target;
				case AttributeConditionType.Equals:
					return value == target;
			}

			return true;
		}


		public List<MinionIdentity> allowedMinions = new List<MinionIdentity>();
		public List<KeyValuePair<MinionIdentity, List<ConditionReason>>> disallowedMinions = new List<KeyValuePair<MinionIdentity, List<ConditionReason>>>();

		public void UpdateAttributesAccess(bool forceUpdate = true)
		{
			var identities = Components.MinionAssignablesProxy;
			allowedMinions.Clear();
			disallowedMinions.Clear();


			foreach (var obj in identities)
			{

				var assignablesIdentity = (MinionAssignablesProxy)obj;

				var minionObject = assignablesIdentity.GetTargetGameObject();


				if (minionObject.GetMyWorldId() != ClusterManager.Instance.activeWorldId)
					continue;

				var resume = minionObject.GetComponent<MinionResume>();
				var identity = minionObject.GetComponent<MinionIdentity>();
				 
				var attributes = resume.GetAttributes();

				var conditions = config.GetAttributesList();
				var permission = accessControl.DefaultPermission;

				var allowed = true;
				var orConditions = new List<bool>();
				List<ConditionReason> reason = new List<ConditionReason>();

				foreach (var condition in conditions)
				{

					if (condition.condition == AttributeConditionType.Ignore) continue;

					var value = attributes.GetValue(condition.id);

					if (!IsConditionReached(value, condition.condition, condition.amount))
					{
						orConditions.Add(false);
						allowed = false;
						reason.Add(new ConditionReason(
							identity.GetProperName(),
							condition.GetProperName(),
							value,
							((ConditionTypeItem)condition.condition).GetOpositeName(),
							condition.amount
						));
						if(config.needsAll)
							break;
					}
					else
					{
						orConditions.Add(true);
					}

				}

				if (!config.needsAll)
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
					disallowedMinions.Add(new KeyValuePair<MinionIdentity, List<ConditionReason>>(identity, reason));
					accessControl.SetPermission(assignablesIdentity, AccessControl.Permission.Neither);
				}


			}

			if (forceUpdate)
			{
				var detail = Traverse.Create(DetailsScreen.Instance);
				var tabs = detail.Field("tabs").GetValue<List<KScreen>>();
				foreach (var tab in tabs)
				{
					if (tab is SimpleInfoScreen)
					{
						((SimpleInfoScreen)tab).Refresh(true);
					}
				}

			}

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
				foreach (var reason in disallowedMinions)
				{
					if (config.needsAll)
					{

						var identitDescriptor = new Descriptor(
							"• " + reason.Value[0].GetString(false),
							"",
							Descriptor.DescriptorType.Effect
						);
						identitDescriptor.IncreaseIndent();
						descriptors.Add(identitDescriptor);
					}
					else
					{

						var identitDescriptor = new Descriptor(
						"• " + reason.Key.GetProperName(),
						"",
						Descriptor.DescriptorType.Effect
					);
						identitDescriptor.IncreaseIndent();
						descriptors.Add(identitDescriptor);

						foreach (var r in reason.Value)
						{
							var rDescriptor = new Descriptor(
							" - <color=#F44A47>" + r.GetString(true) + "</color>",
							"",
							Descriptor.DescriptorType.Effect
						);
							rDescriptor.IncreaseIndent().IncreaseIndent().IncreaseIndent();
							descriptors.Add(rDescriptor);
						}
					}
				}

			}
			return descriptors;
		}



		[SpecialName]
		bool INavDoor.isSpawned => this.isSpawned;

		public class ConditionReason
		{
			private string minionName;
			private string attributeName;
			private float value;
			private string condition;
			private int amount;

			public ConditionReason(string minionName, string attributeName, float value, string condition, int amount)
			{
				this.minionName = minionName;
				this.attributeName = attributeName;
				this.value = value;
				this.condition = condition;
				this.amount = amount;
			}

			internal string GetString(bool supressMinion)
			{
				if (supressMinion) return $"{attributeName}: ({value}) {condition} {amount}";
				return $"{minionName} - <color=#F44A47>{attributeName} ({value}) {condition} {amount}</color>";
			}
		}
		public class Controller : GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor>
		{
			public GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State open;
			public GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State opening;
			public GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State closed;
			public GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State closing;
			public GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State closedelay;
			public GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State closeblocked;
			public GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State locking;
			public GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State locked;
			public GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State unlocking;
			public AttributesAnalyzerDoor.Controller.SealedStates Sealed;
			public StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.BoolParameter isOpen;
			public StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.BoolParameter isLocked;
			public StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.BoolParameter isBlocked;
			public StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.BoolParameter isSealed;
			public StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.BoolParameter sealDirectionRight;

			public override void InitializeStates(out StateMachine.BaseState default_state)
			{
				this.serializable = StateMachine.SerializeType.Both_DEPRECATED;
				default_state = (StateMachine.BaseState)this.closed;
				this.root.Update("RefreshIsBlocked", (System.Action<AttributesAnalyzerDoor.Controller.Instance, float>)((smi, dt) => smi.RefreshIsBlocked())).ParamTransition<bool>((StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.Parameter<bool>)this.isSealed, this.Sealed.closed, GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.IsTrue);
				this.closeblocked.PlayAnim("open").ParamTransition<bool>((StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.Parameter<bool>)this.isOpen, this.open, GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.IsTrue).ParamTransition<bool>((StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.Parameter<bool>)this.isBlocked, this.closedelay, GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.IsFalse);
				this.closedelay.PlayAnim("open").ScheduleGoTo(0.5f, (StateMachine.BaseState)this.closing).ParamTransition<bool>((StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.Parameter<bool>)this.isOpen, this.open, GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.IsTrue).ParamTransition<bool>((StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.Parameter<bool>)this.isBlocked, this.closeblocked, GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.IsTrue);
				this.closing.ParamTransition<bool>((StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.Parameter<bool>)this.isBlocked, this.closeblocked, GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.IsTrue).ToggleTag(GameTags.Transition).ToggleLoopingSound("Closing loop", (Func<AttributesAnalyzerDoor.Controller.Instance, string>)(smi => smi.master.doorClosingSound), (Func<AttributesAnalyzerDoor.Controller.Instance, bool>)(smi => !string.IsNullOrEmpty(smi.master.doorClosingSound))).Enter("SetParams", (StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State.Callback)(smi => smi.master.UpdateAnimAndSoundParams(smi.master.on))).Update((System.Action<AttributesAnalyzerDoor.Controller.Instance, float>)((smi, dt) =>
				{
					if (smi.master.doorClosingSound == null)
						return;
					smi.master.loopingSounds.UpdateSecondParameter(smi.master.doorClosingSound, AttributesAnalyzerDoor.SOUND_PROGRESS_PARAMETER, smi.Get<KBatchedAnimController>().GetPositionPercent());
				}), UpdateRate.SIM_33ms).Enter("SetActive", (StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State.Callback)(smi => smi.master.SetActive(true))).Exit("SetActive", (StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State.Callback)(smi => smi.master.SetActive(false))).PlayAnim("closing").OnAnimQueueComplete(this.closed);
				this.open.PlayAnim("open").ParamTransition<bool>((StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.Parameter<bool>)this.isOpen, this.closeblocked, GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.IsFalse).Enter("SetWorldStateOpen", (StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State.Callback)(smi => smi.master.SetWorldState()));
				this.closed.PlayAnim("closed").ParamTransition<bool>((StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.Parameter<bool>)this.isOpen, this.opening, GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.IsTrue).ParamTransition<bool>((StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.Parameter<bool>)this.isLocked, this.locking, GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.IsTrue).Enter("SetWorldStateClosed", (StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State.Callback)(smi => smi.master.SetWorldState()));
				this.locking.PlayAnim("locked_pre").OnAnimQueueComplete(this.locked).Enter("SetWorldStateClosed", (StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State.Callback)(smi => smi.master.SetWorldState()));
				this.locked.PlayAnim("locked").ParamTransition<bool>((StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.Parameter<bool>)this.isLocked, this.unlocking, GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.IsFalse);
				this.unlocking.PlayAnim("locked_pst").OnAnimQueueComplete(this.closed);
				this.opening.ToggleTag(GameTags.Transition).ToggleLoopingSound("Opening loop", (Func<AttributesAnalyzerDoor.Controller.Instance, string>)(smi => smi.master.doorOpeningSound), (Func<AttributesAnalyzerDoor.Controller.Instance, bool>)(smi => !string.IsNullOrEmpty(smi.master.doorOpeningSound))).Enter("SetParams", (StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State.Callback)(smi => smi.master.UpdateAnimAndSoundParams(smi.master.on))).Update((System.Action<AttributesAnalyzerDoor.Controller.Instance, float>)((smi, dt) =>
				{
					if (smi.master.doorOpeningSound == null)
						return;
					smi.master.loopingSounds.UpdateSecondParameter(smi.master.doorOpeningSound, AttributesAnalyzerDoor.SOUND_PROGRESS_PARAMETER, smi.Get<KBatchedAnimController>().GetPositionPercent());
				}), UpdateRate.SIM_33ms).Enter("SetActive", (StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State.Callback)(smi => smi.master.SetActive(true))).Exit("SetActive", (StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State.Callback)(smi => smi.master.SetActive(false))).PlayAnim("opening").OnAnimQueueComplete(this.open);
				this.Sealed.Enter((StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State.Callback)(smi =>
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
				})).Enter("SetWorldStateClosed", (StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State.Callback)(smi => smi.master.SetWorldState())).Exit((StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State.Callback)(smi =>
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
				this.Sealed.awaiting_unlock.ToggleChore((Func<AttributesAnalyzerDoor.Controller.Instance, Chore>)(smi => this.CreateUnsealChore(smi, true)), this.Sealed.chore_pst);
				this.Sealed.chore_pst.Enter((StateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State.Callback)(smi =>
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




			private Chore CreateUnsealChore(AttributesAnalyzerDoor.Controller.Instance smi, bool approach_right) => (Chore)new WorkChore<Unsealable>(Db.Get().ChoreTypes.Toggle, (IStateMachineTarget)smi.master);

			public class SealedStates :
			  GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State
			{
				public GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State closed;
				public AttributesAnalyzerDoor.Controller.SealedStates.AwaitingUnlock awaiting_unlock;
				public GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State chore_pst;

				public class AwaitingUnlock :
				  GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State
				{
					public GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State awaiting_arrival;
					public GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.State unlocking;
				}
			}

			public new class Instance :
			  GameStateMachine<AttributesAnalyzerDoor.Controller, AttributesAnalyzerDoor.Controller.Instance, AttributesAnalyzerDoor, object>.GameInstance
			{
				public Instance(AttributesAnalyzerDoor AttributeAnalyzerDoor)
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
	public class AttributesConfig
	{
		public AttributeCondition Strength = new AttributeCondition("Strength");
		[Serialize]
		public AttributeCondition Caring = new AttributeCondition("Caring");
		[Serialize]
		public AttributeCondition Construction = new AttributeCondition("Construction");
		[Serialize]
		public AttributeCondition Digging = new AttributeCondition("Digging");
		[Serialize]
		public AttributeCondition Machinery = new AttributeCondition("Machinery");
		[Serialize]
		public AttributeCondition Learning = new AttributeCondition("Learning");
		[Serialize]
		public AttributeCondition Cooking = new AttributeCondition("Cooking");
		[Serialize]
		public AttributeCondition Botanist = new AttributeCondition("Botanist");
		[Serialize]
		public AttributeCondition Art = new AttributeCondition("Art");
		[Serialize]
		public AttributeCondition Ranching = new AttributeCondition("Ranching");
		[Serialize]
		public AttributeCondition Athletics = new AttributeCondition("Athletics");
		[Serialize]
		public AttributeCondition SpaceNavigation = new AttributeCondition("SpaceNavigation");

		[Serialize]
		public bool needsAll;

		public List<AttributeCondition> GetAttributesList()
		{
			return new List<AttributeCondition>
					{
						Strength,
						Caring,
						Construction,
						Digging,
						Machinery,
						Learning,
						Cooking,
						Botanist,
						Art,
						Ranching,
						Athletics,
						SpaceNavigation,

					};
		}

	}

	[Serializable]
	public class AttributeCondition : IListableOption
	{
		[Serialize]
		public AttributeConditionType condition;

		[Serialize]
		public int amount;

		[Serialize]
		public string id;

		public AttributeCondition(string attributeId)
		{
			amount = 0;
			condition = AttributeConditionType.Ignore;
			id = attributeId;
		}

		public string GetProperName()
		{
			string str = "STRINGS.DUPLICANTS.ATTRIBUTES." + id.ToUpper();
			return (string)Strings.Get(new StringKey(str + ".NAME"));
		}

		public string GetProperDescription()
		{
			string str = "STRINGS.DUPLICANTS.ATTRIBUTES." + id.ToUpper();
			return (string)Strings.Get(new StringKey(str + ".DESC"));
		}

	}

	public enum AttributeConditionType
	{
		Ignore,
		Greater,
		Less,
		Equals,
		LessOrEqual,
		GreaterOrEqual
	}

}