// Decompiled with JetBrains decompiler
// Type: SolidConduitInbox
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 93E6D51E-7C50-4F44-83A7-82AAAF7248C3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll

using KSerialization;

[SerializationConfig(MemberSerialization.OptIn)]
public class FreeConveyour : StateMachineComponent<FreeConveyour.SMInstance>, ISim1000ms
{
    [MyCmpReq]
    private Operational operational;
    [MyCmpReq]
    private SolidConduitDispenser dispenser;
    [MyCmpAdd]
    private Storage storage;
    private FilteredStorage filteredStorage;

    protected override void OnPrefabInit()
    {
        base.OnPrefabInit();
        this.filteredStorage = new FilteredStorage((KMonoBehaviour)this, (Tag[])null, (IUserControlledCapacity)null, false, Db.Get().ChoreTypes.StorageFetch);
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();
        this.filteredStorage.FilterChanged();
        this.smi.StartSM();
    }

    protected override void OnCleanUp() => base.OnCleanUp();

    public void Sim1000ms(float dt)
    {
        if (this.operational.IsOperational && this.dispenser.IsDispensing)
            this.operational.SetActive(true);
        else
            this.operational.SetActive(false);
    }

    public class SMInstance :
      GameStateMachine<FreeConveyour.States, FreeConveyour.SMInstance, FreeConveyour, object>.GameInstance
    {
        public SMInstance(FreeConveyour master)
          : base(master)
        {
        }
    }

    public class States :
      GameStateMachine<FreeConveyour.States, FreeConveyour.SMInstance, FreeConveyour>
    {
        public GameStateMachine<FreeConveyour.States, FreeConveyour.SMInstance, FreeConveyour, object>.State off;
        public FreeConveyour.States.ReadyStates on;

        public override void InitializeStates(out StateMachine.BaseState default_state)
        {
            default_state = (StateMachine.BaseState)this.off;
            this.root.DoNothing();
            this.off.PlayAnim("off").EventTransition(GameHashes.OperationalChanged, (GameStateMachine<FreeConveyour.States, FreeConveyour.SMInstance, FreeConveyour, object>.State)this.on, (StateMachine<FreeConveyour.States, FreeConveyour.SMInstance, FreeConveyour, object>.Transition.ConditionCallback)(smi => smi.GetComponent<Operational>().IsOperational));
            this.on.DefaultState(this.on.idle).EventTransition(GameHashes.OperationalChanged, this.off, (StateMachine<FreeConveyour.States, FreeConveyour.SMInstance, FreeConveyour, object>.Transition.ConditionCallback)(smi => !smi.GetComponent<Operational>().IsOperational));
            this.on.idle.PlayAnim("on").EventTransition(GameHashes.ActiveChanged, this.on.working, (StateMachine<FreeConveyour.States, FreeConveyour.SMInstance, FreeConveyour, object>.Transition.ConditionCallback)(smi => smi.GetComponent<Operational>().IsActive));
            this.on.working.PlayAnim("working_pre").QueueAnim("working_loop", true).EventTransition(GameHashes.ActiveChanged, this.on.post, (StateMachine<FreeConveyour.States, FreeConveyour.SMInstance, FreeConveyour, object>.Transition.ConditionCallback)(smi => !smi.GetComponent<Operational>().IsActive));
            this.on.post.PlayAnim("working_pst").OnAnimQueueComplete((GameStateMachine<FreeConveyour.States, FreeConveyour.SMInstance, FreeConveyour, object>.State)this.on);
        }

        public class ReadyStates :
          GameStateMachine<FreeConveyour.States, FreeConveyour.SMInstance, FreeConveyour, object>.State
        {
            public GameStateMachine<FreeConveyour.States, FreeConveyour.SMInstance, FreeConveyour, object>.State idle;
            public GameStateMachine<FreeConveyour.States, FreeConveyour.SMInstance, FreeConveyour, object>.State working;
            public GameStateMachine<FreeConveyour.States, FreeConveyour.SMInstance, FreeConveyour, object>.State post;
        }
    }
}
