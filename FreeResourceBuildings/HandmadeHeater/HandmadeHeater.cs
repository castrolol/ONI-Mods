using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class HandmadeHeater : GameStateMachine<HandmadeHeater, HandmadeHeater.Instance, IStateMachineTarget, HandmadeHeater.Def>
{
    public const string LIT_ANIM_NAME = "on";
    public const string UNLIT_ANIM_NAME = "off";
    public GameStateMachine<HandmadeHeater, HandmadeHeater.Instance, IStateMachineTarget, HandmadeHeater.Def>.State noOperational;
    public HandmadeHeater.OperationalStates operational;
    public StateMachine<HandmadeHeater, HandmadeHeater.Instance, IStateMachineTarget, HandmadeHeater.Def>.BoolParameter WarmAuraEnabled;

    public override void InitializeStates(out StateMachine.BaseState default_state)
    {
        this.serializable = StateMachine.SerializeType.ParamsOnly;
        default_state = (StateMachine.BaseState)this.noOperational;
        this.noOperational.Enter(new StateMachine<HandmadeHeater, HandmadeHeater.Instance, IStateMachineTarget, HandmadeHeater.Def>.State.Callback(HandmadeHeater.DisableHeatEmission)).TagTransition(GameTags.Operational, (GameStateMachine<HandmadeHeater, HandmadeHeater.Instance, IStateMachineTarget, HandmadeHeater.Def>.State)this.operational).PlayAnim("off", KAnim.PlayMode.Once);
        this.operational.TagTransition(GameTags.Operational, this.noOperational, true).DefaultState(this.operational.needsFuel);
        this.operational.needsFuel.Enter(new StateMachine<HandmadeHeater, HandmadeHeater.Instance, IStateMachineTarget, HandmadeHeater.Def>.State.Callback(HandmadeHeater.DisableHeatEmission)).EventTransition(GameHashes.OnStorageChange, this.operational.working, new StateMachine<HandmadeHeater, HandmadeHeater.Instance, IStateMachineTarget, HandmadeHeater.Def>.Transition.ConditionCallback(HandmadeHeater.HasFuel)).PlayAnim("off", KAnim.PlayMode.Once);
        this.operational.working.Enter(new StateMachine<HandmadeHeater, HandmadeHeater.Instance, IStateMachineTarget, HandmadeHeater.Def>.State.Callback(HandmadeHeater.EnableHeatEmission)).EventTransition(GameHashes.OnStorageChange, this.operational.needsFuel, GameStateMachine<HandmadeHeater, HandmadeHeater.Instance, IStateMachineTarget, HandmadeHeater.Def>.Not(new StateMachine<HandmadeHeater, HandmadeHeater.Instance, IStateMachineTarget, HandmadeHeater.Def>.Transition.ConditionCallback(HandmadeHeater.HasFuel))).PlayAnim("on", KAnim.PlayMode.Loop).Exit(new StateMachine<HandmadeHeater, HandmadeHeater.Instance, IStateMachineTarget, HandmadeHeater.Def>.State.Callback(HandmadeHeater.DisableHeatEmission));
    }

    public static bool HasFuel(HandmadeHeater.Instance smi) => smi.HasFuel;

    public static void EnableHeatEmission(HandmadeHeater.Instance smi) => smi.EnableHeatEmission();

    public static void DisableHeatEmission(HandmadeHeater.Instance smi) => smi.DisableHeatEmission();

    public class Def : StateMachine.BaseDef
    {
        // public Tag fuelTag;
        // public float initialFuelMass;
    }

    public class OperationalStates :
      GameStateMachine<HandmadeHeater, HandmadeHeater.Instance, IStateMachineTarget, HandmadeHeater.Def>.State
    {
        public GameStateMachine<HandmadeHeater, HandmadeHeater.Instance, IStateMachineTarget, HandmadeHeater.Def>.State needsFuel;
        public GameStateMachine<HandmadeHeater, HandmadeHeater.Instance, IStateMachineTarget, HandmadeHeater.Def>.State working;
    }

    public new class Instance :
      GameStateMachine<HandmadeHeater, HandmadeHeater.Instance, IStateMachineTarget, HandmadeHeater.Def>.GameInstance
    {
        [MyCmpGet]
        public Operational operational;
        [MyCmpGet]
        public Storage storage;
        [MyCmpGet]
        public RangeVisualizer rangeVisualizer;
        [MyCmpGet]
        public Light2D light;
        [MyCmpGet]
        public DirectVolumeHeater heater;
        [MyCmpGet]
        public DecorProvider decorProvider;

        public bool HasFuel => true;

        public bool IsAuraEnabled => this.sm.WarmAuraEnabled.Get(this);

        public Instance(IStateMachineTarget master, HandmadeHeater.Def def)
          : base(master, def)
        {
        }

        public void EnableHeatEmission()
        {
            this.operational.SetActive(true);
            this.light.enabled = true;
            this.heater.EnableEmission = true;
            this.decorProvider.SetValues(TUNING.BUILDINGS.DECOR.BONUS.TIER0);
            this.decorProvider.Refresh();
        }

        public void DisableHeatEmission()
        {
            this.operational.SetActive(false);
            this.light.enabled = false;
            this.heater.EnableEmission = false;
            this.decorProvider.SetValues(TUNING.BUILDINGS.DECOR.PENALTY.TIER0);
            this.decorProvider.Refresh();
        }
    }
}