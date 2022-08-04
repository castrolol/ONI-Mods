//// Decompiled with JetBrains decompiler
//// Type: WarpReceiver2
//// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
//// MVID: 1C7E9CCB-4AA8-44E2-BE45-38990AABF98E
//// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll

//using KSerialization;
//using System.Collections.Generic;
//using System.Linq;

//public class WarpReceiver2 : Workable
//{
//    [MyCmpAdd]
//    public Notifier notifier;
//    private WarpReceiver2.WarpReceiver2SM.Instance warpReceiverSMI;
//    private Notification notification;
//    [Serialize]
//    public bool IsConsumed;
//    private Chore chore;
//    [Serialize]
//    public bool Used;

//    protected override void OnPrefabInit() => base.OnPrefabInit();

//    protected override void OnSpawn()
//    {
//        base.OnSpawn();
//        this.warpReceiverSMI = new WarpReceiver2.WarpReceiver2SM.Instance(this);
//        this.warpReceiverSMI.StartSM();
//        // TODO: check this...
//        //Components.WarpReceiver2s.Add(this);
//    }

//    public void ReceiveWarpedDuplicant(Worker dupe)
//    {
//        dupe.transform.SetPosition(Grid.CellToPos(Grid.PosToCell((KMonoBehaviour)this), CellAlignment.Bottom, Grid.SceneLayer.Move));
//        Debug.Assert(this.chore == null);
//        KAnimFile anim1 = Assets.GetAnim((HashedString)"anim_interacts_warp_portal_receiver_kanim");
//        ChoreType migrate = Db.Get().ChoreTypes.Migrate;
//        KAnimFile kanimFile = anim1;
//        ChoreProvider component1 = dupe.GetComponent<ChoreProvider>();
//        System.Action<Chore> on_complete = (System.Action<Chore>)(o => this.CompleteChore());
//        KAnimFile override_anims = kanimFile;
//        this.chore = (Chore)new WorkChore<Workable>(migrate, (IStateMachineTarget)this, component1, on_complete: on_complete, ignore_schedule_block: true, override_anims: override_anims, allow_prioritization: false, priority_class: PriorityScreen.PriorityClass.compulsory);
//        Workable component2 = this.GetComponent<Workable>();
//        component2.workLayer = Grid.SceneLayer.Building;
//        component2.workAnims = new HashedString[2]
//        {
//      (HashedString) "printing_pre",
//      (HashedString) "printing_loop"
//        };
//        component2.workingPstComplete = new HashedString[1]
//        {
//      (HashedString) "printing_pst"
//        };
//        component2.workingPstFailed = new HashedString[1]
//        {
//      (HashedString) "printing_pst"
//        };
//        component2.synchronizeAnims = true;
//        float work_time = 0.0f;
//        KAnimFileData data = anim1.GetData();
//        for (int index = 0; index < data.animCount; ++index)
//        {
//            KAnim.Anim anim2 = data.GetAnim(index);
//            if (((IEnumerable<HashedString>)component2.workAnims).Contains<HashedString>(anim2.hash))
//                work_time += anim2.totalTime;
//        }
//        component2.SetWorkTime(work_time);
//        this.Used = true;
//    }

//    private void CompleteChore()
//    {
//        this.chore.Cleanup();
//        this.chore = (Chore)null;
//        this.warpReceiverSMI.GoTo((StateMachine.BaseState)this.warpReceiverSMI.sm.idle);
//    }

//    protected override void OnCleanUp()
//    {
//        base.OnCleanUp();
//        // check this
//        // Components.WarpReceiver2s.Remove(this);
//    }

//    public class WarpReceiver2SM :
//      GameStateMachine<WarpReceiver2.WarpReceiver2SM, WarpReceiver2.WarpReceiver2SM.Instance, WarpReceiver2>
//    {
//        public GameStateMachine<WarpReceiver2.WarpReceiver2SM, WarpReceiver2.WarpReceiver2SM.Instance, WarpReceiver2, object>.State idle;

//        public override void InitializeStates(out StateMachine.BaseState default_state)
//        {
//            default_state = (StateMachine.BaseState)this.idle;
//            this.idle.PlayAnim("idle");
//        }

//        public new class Instance :
//          GameStateMachine<WarpReceiver2.WarpReceiver2SM, WarpReceiver2.WarpReceiver2SM.Instance, WarpReceiver2, object>.GameInstance
//        {
//            public Instance(WarpReceiver2 master)
//              : base(master)
//            {
//            }
//        }
//    }
//}
