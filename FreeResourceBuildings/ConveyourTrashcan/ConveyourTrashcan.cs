// Decompiled with JetBrains decompiler
// Type: SolidConduitOutbox
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 93E6D51E-7C50-4F44-83A7-82AAAF7248C3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll

using KSerialization;
using System;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
public class ConveyourTrashcan : StateMachineComponent<ConveyourTrashcan.SMInstance>
{
    [MyCmpReq]
    private Operational operational;
    [MyCmpReq]
    private SolidConduitConsumer consumer;
    [MyCmpAdd]
    private Storage storage;
    private static readonly EventSystem.IntraObjectHandler<ConveyourTrashcan> OnStorageChangedDelegate = new EventSystem.IntraObjectHandler<ConveyourTrashcan>((Action<ConveyourTrashcan, object>)((component, data) => component.OnStorageChanged(data)));

    protected override void OnPrefabInit() => base.OnPrefabInit();

    protected override void OnSpawn()
    {
        base.OnSpawn();

        this.Subscribe<ConveyourTrashcan>((int)GameHashes.OnStorageChange, ConveyourTrashcan.OnStorageChangedDelegate);
        this.smi.StartSM();
    }

    protected override void OnCleanUp() => base.OnCleanUp();

    private void OnStorageChanged(object data)
    {

        foreach (GameObject current in storage.items)
        {
            current.DeleteObject();
        }
    }


    private void UpdateConsuming() => this.smi.sm.consuming.Set(this.consumer.IsConsuming, this.smi);

    public class SMInstance :
      GameStateMachine<ConveyourTrashcan.States, ConveyourTrashcan.SMInstance, ConveyourTrashcan, object>.GameInstance
    {
        public SMInstance(ConveyourTrashcan master)
          : base(master)
        {
        }
    }

    public class States :
      GameStateMachine<ConveyourTrashcan.States, ConveyourTrashcan.SMInstance, ConveyourTrashcan>
    {
        public StateMachine<ConveyourTrashcan.States, ConveyourTrashcan.SMInstance, ConveyourTrashcan, object>.BoolParameter consuming;
        public GameStateMachine<ConveyourTrashcan.States, ConveyourTrashcan.SMInstance, ConveyourTrashcan, object>.State idle;
        public GameStateMachine<ConveyourTrashcan.States, ConveyourTrashcan.SMInstance, ConveyourTrashcan, object>.State working;
        public GameStateMachine<ConveyourTrashcan.States, ConveyourTrashcan.SMInstance, ConveyourTrashcan, object>.State post;

        public override void InitializeStates(out StateMachine.BaseState default_state)
        {
            default_state = (StateMachine.BaseState)this.idle;
            this.root.Update("RefreshConsuming", (Action<ConveyourTrashcan.SMInstance, float>)((smi, dt) => smi.master.UpdateConsuming()), UpdateRate.SIM_1000ms);
            this.idle.PlayAnim("on").ParamTransition<bool>((StateMachine<ConveyourTrashcan.States, ConveyourTrashcan.SMInstance, ConveyourTrashcan, object>.Parameter<bool>)this.consuming, this.working, GameStateMachine<ConveyourTrashcan.States, ConveyourTrashcan.SMInstance, ConveyourTrashcan, object>.IsTrue);
            this.working.PlayAnim("working_pre").QueueAnim("working_loop", true).ParamTransition<bool>((StateMachine<ConveyourTrashcan.States, ConveyourTrashcan.SMInstance, ConveyourTrashcan, object>.Parameter<bool>)this.consuming, this.post, GameStateMachine<ConveyourTrashcan.States, ConveyourTrashcan.SMInstance, ConveyourTrashcan, object>.IsFalse);
            this.post.PlayAnim("working_pst").OnAnimQueueComplete(this.idle);
        }
    }
}
