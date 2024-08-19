
using Klei;
using KSerialization;
using STRINGS;
using System;
using System.Collections.Generic;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
public class BottleVanisher :
  StateMachineComponent<BottleVanisher.StatesInstance>,
  IGameObjectEffectDescriptor
{
    public float emptyRate = 10f;
    [Serialize]
    public bool allowManualPumpingStationFetching;
    public bool isGasEmptier;
    private static Dictionary<bool, string[]> manualPumpingAffectedBuildings = new Dictionary<bool, string[]>();
    private static readonly EventSystem.IntraObjectHandler<BottleVanisher> OnRefreshUserMenuDelegate = new EventSystem.IntraObjectHandler<BottleVanisher>((Action<BottleVanisher, object>)((component, data) => component.OnRefreshUserMenu(data)));
    private static readonly EventSystem.IntraObjectHandler<BottleVanisher> OnCopySettingsDelegate = new EventSystem.IntraObjectHandler<BottleVanisher>((Action<BottleVanisher, object>)((component, data) => component.OnCopySettings(data)));

    protected override void OnSpawn()
    {
        base.OnSpawn();
        this.smi.StartSM();
        this.DefineManualPumpingAffectedBuildings();
        this.Subscribe<BottleVanisher>(493375141, BottleVanisher.OnRefreshUserMenuDelegate);
        this.Subscribe<BottleVanisher>(-905833192, BottleVanisher.OnCopySettingsDelegate);
    }

    public List<Descriptor> GetDescriptors(GameObject go) => (List<Descriptor>)null;

    private void DefineManualPumpingAffectedBuildings()
    {
        if (BottleVanisher.manualPumpingAffectedBuildings.ContainsKey(this.isGasEmptier))
            return;
        List<string> stringList = new List<string>();
        Tag tag = this.isGasEmptier ? GameTags.GasSource : GameTags.LiquidSource;
        foreach (BuildingDef buildingDef in Assets.BuildingDefs)
        {
            if (buildingDef.BuildingComplete.HasTag(tag))
                stringList.Add(buildingDef.Name);
        }
        BottleVanisher.manualPumpingAffectedBuildings.Add(this.isGasEmptier, stringList.ToArray());
    }

    private void OnChangeAllowManualPumpingStationFetching()
    {
        this.allowManualPumpingStationFetching = !this.allowManualPumpingStationFetching;
        this.smi.RefreshChore();
    }

    private void OnRefreshUserMenu(object data)
    {
        string tooltipText1 = (string)(this.isGasEmptier ? UI.USERMENUACTIONS.MANUAL_PUMP_DELIVERY.ALLOWED_GAS.TOOLTIP : UI.USERMENUACTIONS.MANUAL_PUMP_DELIVERY.ALLOWED.TOOLTIP);
        string tooltipText2 = (string)(this.isGasEmptier ? UI.USERMENUACTIONS.MANUAL_PUMP_DELIVERY.DENIED_GAS.TOOLTIP : UI.USERMENUACTIONS.MANUAL_PUMP_DELIVERY.DENIED.TOOLTIP);
        if (BottleVanisher.manualPumpingAffectedBuildings.ContainsKey(this.isGasEmptier))
        {
            foreach (string str1 in BottleVanisher.manualPumpingAffectedBuildings[this.isGasEmptier])
            {
                string str2 = string.Format((string)UI.USERMENUACTIONS.MANUAL_PUMP_DELIVERY.ALLOWED.ITEM, (object)str1);
                tooltipText1 += str2;
                tooltipText2 += str2;
            }
        }
        if (this.isGasEmptier)
            Game.Instance.userMenu.AddButton(this.gameObject, this.allowManualPumpingStationFetching ? new KIconButtonMenu.ButtonInfo("action_bottler_delivery", (string)UI.USERMENUACTIONS.MANUAL_PUMP_DELIVERY.DENIED_GAS.NAME, new System.Action(this.OnChangeAllowManualPumpingStationFetching), tooltipText: tooltipText2) : new KIconButtonMenu.ButtonInfo("action_bottler_delivery", (string)UI.USERMENUACTIONS.MANUAL_PUMP_DELIVERY.ALLOWED_GAS.NAME, new System.Action(this.OnChangeAllowManualPumpingStationFetching), tooltipText: tooltipText1), 0.4f);
        else
            Game.Instance.userMenu.AddButton(this.gameObject, this.allowManualPumpingStationFetching ? new KIconButtonMenu.ButtonInfo("action_bottler_delivery", (string)UI.USERMENUACTIONS.MANUAL_PUMP_DELIVERY.DENIED.NAME, new System.Action(this.OnChangeAllowManualPumpingStationFetching), tooltipText: tooltipText2) : new KIconButtonMenu.ButtonInfo("action_bottler_delivery", (string)UI.USERMENUACTIONS.MANUAL_PUMP_DELIVERY.ALLOWED.NAME, new System.Action(this.OnChangeAllowManualPumpingStationFetching), tooltipText: tooltipText1), 0.4f);
    }

    private void OnCopySettings(object data)
    {
        this.allowManualPumpingStationFetching = ((GameObject)data).GetComponent<BottleVanisher>().allowManualPumpingStationFetching;
        this.smi.RefreshChore();
    }

    public class StatesInstance :
      GameStateMachine<BottleVanisher.States, BottleVanisher.StatesInstance, BottleVanisher, object>.GameInstance
    {
        private FetchChore chore;

        public MeterController meter { get; private set; }

        public StatesInstance(BottleVanisher smi)
          : base(smi)
        {
            this.master.GetComponent<TreeFilterable>().OnFilterChanged += new Action<HashSet<Tag>>(this.OnFilterChanged);
            this.meter = new MeterController((KAnimControllerBase)this.GetComponent<KBatchedAnimController>(), "meter_target", nameof(meter), Meter.Offset.Infront, Grid.SceneLayer.NoLayer, new string[3]
            {
        "meter_target",
        "meter_arrow",
        "meter_scale"
            });
            this.Subscribe(-1697596308, new Action<object>(this.OnStorageChange));
            this.Subscribe(644822890, new Action<object>(this.OnOnlyFetchMarkedItemsSettingChanged));
        }

        public void CreateChore()
        {
            HashSet<Tag> tags = this.GetComponent<TreeFilterable>().GetTags();
            Tag[] forbidden_tags;
            if (!this.master.allowManualPumpingStationFetching)
                forbidden_tags = new Tag[2]
                {
          GameTags.LiquidSource,
          GameTags.GasSource
                };
            else
                forbidden_tags = new Tag[0];
            Storage component = this.GetComponent<Storage>();
            this.chore = new FetchChore(Db.Get().ChoreTypes.StorageFetch, component, component.Capacity(), tags, FetchChore.MatchCriteria.MatchID, Tag.Invalid, forbidden_tags);
        }

        public void CancelChore()
        {
            if (this.chore == null)
                return;
            this.chore.Cancel("Storage Changed");
            this.chore = (FetchChore)null;
        }

        public void RefreshChore() => this.GoTo((StateMachine.BaseState)this.sm.unoperational);

        private void OnFilterChanged(HashSet<Tag> tags) => this.RefreshChore();

        private void OnStorageChange(object data)
        {
            Storage component = this.GetComponent<Storage>();
            this.meter.SetPositionPercent(Mathf.Clamp01(component.RemainingCapacity() / component.capacityKg));
        }

        private void OnOnlyFetchMarkedItemsSettingChanged(object data) => this.RefreshChore();

        public void StartMeter()
        {
            PrimaryElement firstPrimaryElement = this.GetFirstPrimaryElement();
            if ((UnityEngine.Object)firstPrimaryElement == (UnityEngine.Object)null)
                return;
            this.meter.SetSymbolTint(new KAnimHashedString("meter_fill"), firstPrimaryElement.Element.substance.colour);
            this.meter.SetSymbolTint(new KAnimHashedString("water1"), firstPrimaryElement.Element.substance.colour);
            this.GetComponent<KBatchedAnimController>().SetSymbolTint(new KAnimHashedString("leak_ceiling"), (Color)firstPrimaryElement.Element.substance.colour);
        }

        private PrimaryElement GetFirstPrimaryElement()
        {
            Storage component1 = this.GetComponent<Storage>();
            for (int idx = 0; idx < component1.Count; ++idx)
            {
                GameObject gameObject = component1[idx];
                if (!((UnityEngine.Object)gameObject == (UnityEngine.Object)null))
                {
                    PrimaryElement component2 = gameObject.GetComponent<PrimaryElement>();
                    if (!((UnityEngine.Object)component2 == (UnityEngine.Object)null))
                        return component2;
                }
            }
            return (PrimaryElement)null;
        }

        public void Emit(float dt)
        {
            PrimaryElement firstPrimaryElement = GetFirstPrimaryElement();
            if ((UnityEngine.Object)firstPrimaryElement == (UnityEngine.Object)null)
                return;
            Storage component = GetComponent<Storage>();
            float amount = Mathf.Min(firstPrimaryElement.Mass, master.emptyRate * dt);
            if ((double)amount <= 0.0)
                return;
            Tag prefabTag = firstPrimaryElement.GetComponent<KPrefabID>().PrefabTag;

            component.ConsumeAndGetDisease(prefabTag, amount, out var _, out var _, out var _);
            //Vector3 position = this.transform.GetPosition();
            //position.y += 1.8f;
            //bool flag = this.GetComponent<Rotatable>().GetOrientation() == Orientation.FlipH;
            //position.x += flag ? -0.2f : 0.2f;
            //int num = Grid.PosToCell(position) + (flag ? -1 : 1);
            //if (Grid.Solid[num])
            //    num += flag ? 1 : -1;
            //Element element = firstPrimaryElement.Element;
            //ushort idx = element.idx;
            //if (element.IsLiquid)
            //    FallingWater.instance.AddParticle(num, idx, amount_consumed, aggregate_temperature, disease_info.idx, disease_info.count, true);
            //else
            //    SimMessages.ModifyCell(num, idx, aggregate_temperature, amount_consumed, disease_info.idx, disease_info.count);
        }
    }

    public class States :
      GameStateMachine<BottleVanisher.States, BottleVanisher.StatesInstance, BottleVanisher>
    {
        private StatusItem statusItem;
        public GameStateMachine<BottleVanisher.States, BottleVanisher.StatesInstance, BottleVanisher, object>.State unoperational;
        public GameStateMachine<BottleVanisher.States, BottleVanisher.StatesInstance, BottleVanisher, object>.State waitingfordelivery;
        public GameStateMachine<BottleVanisher.States, BottleVanisher.StatesInstance, BottleVanisher, object>.State emptying;

        public override void InitializeStates(out StateMachine.BaseState default_state)
        {
            default_state = (StateMachine.BaseState)this.waitingfordelivery;
            this.statusItem = new StatusItem(nameof(BottleVanisher), "", "", "", StatusItem.IconType.Info, NotificationType.Neutral, false, OverlayModes.None.ID);
            this.statusItem.resolveStringCallback = (Func<string, object, string>)((str, data) =>
            {
                BottleVanisher bottleEmptier = (BottleVanisher)data;
                if ((UnityEngine.Object)bottleEmptier == (UnityEngine.Object)null)
                    return str;
                return bottleEmptier.allowManualPumpingStationFetching ? (string)(bottleEmptier.isGasEmptier ? BUILDING.STATUSITEMS.CANISTER_EMPTIER.ALLOWED.NAME : BUILDING.STATUSITEMS.BOTTLE_EMPTIER.ALLOWED.NAME) : (string)(bottleEmptier.isGasEmptier ? BUILDING.STATUSITEMS.CANISTER_EMPTIER.DENIED.NAME : BUILDING.STATUSITEMS.BOTTLE_EMPTIER.DENIED.NAME);
            });
            this.statusItem.resolveTooltipCallback = (Func<string, object, string>)((str, data) =>
            {
                BottleVanisher bottleEmptier = (BottleVanisher)data;
                if ((UnityEngine.Object)bottleEmptier == (UnityEngine.Object)null)
                    return str;
                return !bottleEmptier.allowManualPumpingStationFetching ? (!bottleEmptier.isGasEmptier ? (string)BUILDING.STATUSITEMS.BOTTLE_EMPTIER.DENIED.TOOLTIP : (string)BUILDING.STATUSITEMS.CANISTER_EMPTIER.DENIED.TOOLTIP) : (!bottleEmptier.isGasEmptier ? (string)BUILDING.STATUSITEMS.BOTTLE_EMPTIER.ALLOWED.TOOLTIP : (string)BUILDING.STATUSITEMS.CANISTER_EMPTIER.ALLOWED.TOOLTIP);
            });
            this.root.ToggleStatusItem(this.statusItem, (Func<BottleVanisher.StatesInstance, object>)(smi => (object)smi.master));
            this.unoperational.TagTransition(GameTags.Operational, this.waitingfordelivery).PlayAnim("off");
            this.waitingfordelivery.TagTransition(GameTags.Operational, this.unoperational, true).EventTransition(GameHashes.OnStorageChange, this.emptying, (StateMachine<BottleVanisher.States, BottleVanisher.StatesInstance, BottleVanisher, object>.Transition.ConditionCallback)(smi => !smi.GetComponent<Storage>().IsEmpty())).Enter("CreateChore", (StateMachine<BottleVanisher.States, BottleVanisher.StatesInstance, BottleVanisher, object>.State.Callback)(smi => smi.CreateChore())).Exit("CancelChore", (StateMachine<BottleVanisher.States, BottleVanisher.StatesInstance, BottleVanisher, object>.State.Callback)(smi => smi.CancelChore())).PlayAnim("on");
            this.emptying.TagTransition(GameTags.Operational, this.unoperational, true).EventTransition(GameHashes.OnStorageChange, this.waitingfordelivery, (StateMachine<BottleVanisher.States, BottleVanisher.StatesInstance, BottleVanisher, object>.Transition.ConditionCallback)(smi => smi.GetComponent<Storage>().IsEmpty())).Enter("StartMeter", (StateMachine<BottleVanisher.States, BottleVanisher.StatesInstance, BottleVanisher, object>.State.Callback)(smi => smi.StartMeter())).Update("Emit", (Action<BottleVanisher.StatesInstance, float>)((smi, dt) => smi.Emit(dt))).PlayAnim("working_loop", KAnim.PlayMode.Loop);
        }
    }
}
