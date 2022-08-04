// Decompiled with JetBrains decompiler
// Type: WarpPortal2
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1C7E9CCB-4AA8-44E2-BE45-38990AABF98E
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll

using Klei.AI;
using KSerialization;
using STRINGS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpPortal2 : Workable
{
	[MyCmpReq]
	public Assignable assignable;
	[MyCmpAdd]
	public Notifier notifier;
	private Chore chore;
	private WarpPortal2.WarpPortal2SM.Instance warpPortalSMI;
	public const float RECHARGE_TIME = 3000f;
	[Serialize]
	public bool IsConsumed;
	[Serialize]
	public float rechargeProgress;
	private Coroutine delayWarpRoutine;
	private static readonly HashedString[] printing_anim = new HashedString[3]
	{
	(HashedString) "printing_pre",
	(HashedString) "printing_loop",
	(HashedString) "printing_pst"
	};

	public bool ReadyToWarp => this.warpPortalSMI.IsInsideState((StateMachine.BaseState)this.warpPortalSMI.sm.occupied.waiting);

	public bool IsWorking => this.warpPortalSMI.IsInsideState((StateMachine.BaseState)this.warpPortalSMI.sm.occupied);

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		gameObject.GetComponent<WarpPortal2>().workLayer = Grid.SceneLayer.Building;
		gameObject.GetComponent<Ownable>().slotID = Db.Get().AssignableSlots.WarpPortal.Id;
		gameObject.GetComponent<OccupyArea>().objectLayers = new ObjectLayer[1]
		{
		  ObjectLayer.Building
		};
		gameObject.GetComponent<Deconstructable>();

		this.assignable.OnAssign += new System.Action<IAssignableIdentity>(this.Assign);
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		this.warpPortalSMI = new WarpPortal2.WarpPortal2SM.Instance(this);
		this.warpPortalSMI.sm.isCharged.Set(!this.IsConsumed, this.warpPortalSMI);
		this.warpPortalSMI.StartSM();
	}



	protected override void OnCleanUp()
	{
		base.OnCleanUp();
	}


	public void StartWarpSequence() => this.warpPortalSMI.GoTo((StateMachine.BaseState)this.warpPortalSMI.sm.occupied.warping);

	public void CancelAssignment()
	{
		this.CancelChore();
		this.assignable.Unassign();
		this.warpPortalSMI.GoTo((StateMachine.BaseState)this.warpPortalSMI.sm.idle);
	}

	private void Warp()
	{
		//if ((UnityEngine.Object)this.worker == (UnityEngine.Object)null || this.worker.HasTag(GameTags.Dying) || this.worker.HasTag(GameTags.Dead))
		//	return;
		//WarpReceiver2 receiver = (WarpReceiver2)null;
		//foreach (WarpReceiver2 component in UnityEngine.Object.FindObjectsOfType<WarpReceiver2>())
		//{
		//	if (component.GetMyWorldId() != this.GetMyWorldId())
		//	{
		//		receiver = component;
		//		break;
		//	}
		//}
		//if ((UnityEngine.Object)receiver == (UnityEngine.Object)null)
		//{
		//	SaveGame.Instance.GetComponent<WorldGenSpawner>().SpawnTag(WarpReceiver2Config.ID);
		//	receiver = UnityEngine.Object.FindObjectOfType<WarpReceiver2>();
		//}
		//if ((UnityEngine.Object)receiver != (UnityEngine.Object)null)
		//	this.delayWarpRoutine = this.StartCoroutine(this.DelayedWarp(receiver));
		//else
		//	Debug.LogWarning((object)"No warp receiver found - maybe POI stomping or failure to spawn?");
		//if (!((UnityEngine.Object)SelectTool.Instance.selected == (UnityEngine.Object)this.GetComponent<KSelectable>()))
		//	return;
		//SelectTool.Instance.Select((KSelectable)null, true);
	}

	//public IEnumerator DelayedWarp(WarpReceiver2 receiver)
	//{
	//	// ISSUE: reference to a compiler-generated field
	//	//WarpPortal2 WarpPortal2 = this;
	//	//Debug.Log("TENTOU");
	//	//yield return null;

	//	//int myWorldId = receiver.GetMyWorldId();
	//	//CameraController.Instance.ActiveWorldStarWipe(myWorldId, Grid.CellToPos(Grid.PosToCell((KMonoBehaviour)receiver)));
	//	//Worker worker = WarpPortal2.worker;
	//	//worker.StopWork();
	//	//receiver.ReceiveWarpedDuplicant(worker);
	//	//ClusterManager.Instance.MigrateMinion(worker.GetComponent<MinionIdentity>(), myWorldId);
	//	//WarpPortal2.delayWarpRoutine = (Coroutine)null;

	//}

	public void SetAssignable(bool set_it)
	{
		this.assignable.SetCanBeAssigned(set_it);
		this.RefreshSideScreen();
	}

	private void Assign(IAssignableIdentity new_assignee)
	{
		this.CancelChore();
		if (new_assignee == null)
			return;
		this.ActivateChore();
	}

	private void ActivateChore()
	{

		Debug.Assert(this.chore == null);
		this.chore = (Chore)new WorkChore<Workable>(Db.Get().ChoreTypes.Migrate, (IStateMachineTarget)this, on_complete: ((System.Action<Chore>)(o => this.CompleteChore())), override_anims: Assets.GetAnim((HashedString)"anim_interacts_warp_portal_sender_kanim"), allow_prioritization: false, priority_class: PriorityScreen.PriorityClass.high);
		Debug.Log("this.chore.id" + this.chore.id);
		this.SetWorkTime(float.PositiveInfinity);
		this.workLayer = Grid.SceneLayer.Building;
		this.workAnims = new HashedString[2]
		{
	  (HashedString) "sending_pre",
	  (HashedString) "sending_loop"
		};
		this.workingPstComplete = new HashedString[1]
		{
	  (HashedString) "sending_pst"
		};
		this.workingPstFailed = new HashedString[1]
		{
	  (HashedString) "idle_loop"
		};
		this.showProgressBar = false;
	}

	private void CancelChore()
	{
		Debug.Log("CANCEL?");

		if (this.chore == null)
			return;
		this.chore.Cancel("User cancelled");
		this.chore = (Chore)null;
		if (this.delayWarpRoutine == null)
			return;
		this.StopCoroutine(this.delayWarpRoutine);
		this.delayWarpRoutine = (Coroutine)null;
	}

	private void CompleteChore()
	{
		this.IsConsumed = true;
		this.chore.Cleanup();
		this.chore = (Chore)null;
	}

	public void RefreshSideScreen()
	{
		if (!this.GetComponent<KSelectable>().IsSelected)
			return;
		DetailsScreen.Instance.Refresh(this.gameObject);
	}

	public class WarpPortal2SM :
	  GameStateMachine<WarpPortal2.WarpPortal2SM, WarpPortal2.WarpPortal2SM.Instance, WarpPortal2>
	{
		public GameStateMachine<WarpPortal2.WarpPortal2SM, WarpPortal2.WarpPortal2SM.Instance, WarpPortal2, object>.State idle;
		public GameStateMachine<WarpPortal2.WarpPortal2SM, WarpPortal2.WarpPortal2SM.Instance, WarpPortal2, object>.State become_occupied;
		public WarpPortal2.WarpPortal2SM.OccupiedStates occupied;
		public GameStateMachine<WarpPortal2.WarpPortal2SM, WarpPortal2.WarpPortal2SM.Instance, WarpPortal2, object>.State do_warp;
		public GameStateMachine<WarpPortal2.WarpPortal2SM, WarpPortal2.WarpPortal2SM.Instance, WarpPortal2, object>.State recharging;
		public StateMachine<WarpPortal2.WarpPortal2SM, WarpPortal2.WarpPortal2SM.Instance, WarpPortal2, object>.BoolParameter isCharged;
		private StateMachine<WarpPortal2.WarpPortal2SM, WarpPortal2.WarpPortal2SM.Instance, WarpPortal2, object>.TargetParameter worker;

		public override void InitializeStates(out StateMachine.BaseState default_state)
		{
			ModBuildingStatus.CreateStatusItems();
			default_state = (StateMachine.BaseState)this.root;
			this.root.Enter((StateMachine<WarpPortal2.WarpPortal2SM, WarpPortal2.WarpPortal2SM.Instance, WarpPortal2, object>.State.Callback)(smi =>
			{
				if ((double)smi.master.rechargeProgress == 0.0)
					return;
				smi.GoTo((StateMachine.BaseState)this.recharging);
			})).DefaultState(this.idle);
			this.idle.PlayAnim("idle", KAnim.PlayMode.Loop).Enter((StateMachine<WarpPortal2.WarpPortal2SM, WarpPortal2.WarpPortal2SM.Instance, WarpPortal2, object>.State.Callback)(smi =>
			{
				smi.master.IsConsumed = false;
				smi.sm.isCharged.Set(true, smi);
				smi.master.SetAssignable(true);
			})).Exit((StateMachine<WarpPortal2.WarpPortal2SM, WarpPortal2.WarpPortal2SM.Instance, WarpPortal2, object>.State.Callback)(smi => smi.master.SetAssignable(false))).WorkableStartTransition((Func<WarpPortal2.WarpPortal2SM.Instance, Workable>)(smi => (Workable)smi.master), this.become_occupied).ParamTransition<bool>((StateMachine<WarpPortal2.WarpPortal2SM, WarpPortal2.WarpPortal2SM.Instance, WarpPortal2, object>.Parameter<bool>)this.isCharged, this.recharging, GameStateMachine<WarpPortal2.WarpPortal2SM, WarpPortal2.WarpPortal2SM.Instance, WarpPortal2, object>.IsFalse);
			this.become_occupied.Enter((StateMachine<WarpPortal2.WarpPortal2SM, WarpPortal2.WarpPortal2SM.Instance, WarpPortal2, object>.State.Callback)(smi =>
			{
				Debug.Log("WORKER?");

				this.worker.Set((KMonoBehaviour)smi.master.worker, smi);
				smi.GoTo((StateMachine.BaseState)this.occupied.get_on);
				Debug.Log("WORKER?2");

			}));
			this.occupied.OnTargetLost(this.worker, this.idle).Target(this.worker).TagTransition(GameTags.Dying, this.idle).Target(this.masterTarget).Exit((StateMachine<WarpPortal2.WarpPortal2SM, WarpPortal2.WarpPortal2SM.Instance, WarpPortal2, object>.State.Callback)(smi => this.worker.Set((KMonoBehaviour)null, smi)));
			this.occupied.get_on.PlayAnim("sending_pre").OnAnimQueueComplete(this.occupied.waiting);
			this.occupied.waiting.PlayAnim("sending_loop", KAnim.PlayMode.Loop).ToggleNotification((Func<WarpPortal2.WarpPortal2SM.Instance, Notification>)(smi => smi.CreateDupeWaitingNotification())).Enter((StateMachine<WarpPortal2.WarpPortal2SM, WarpPortal2.WarpPortal2SM.Instance, WarpPortal2, object>.State.Callback)(smi => smi.master.RefreshSideScreen())).Exit((StateMachine<WarpPortal2.WarpPortal2SM, WarpPortal2.WarpPortal2SM.Instance, WarpPortal2, object>.State.Callback)(smi => smi.master.RefreshSideScreen()));
			this.occupied.warping.PlayAnim("sending_pst").OnAnimQueueComplete(this.do_warp);
			this.do_warp.Enter((StateMachine<WarpPortal2.WarpPortal2SM, WarpPortal2.WarpPortal2SM.Instance, WarpPortal2, object>.State.Callback)(smi => smi.master.Warp())).GoTo(this.recharging);
			this.recharging.Enter((StateMachine<WarpPortal2.WarpPortal2SM, WarpPortal2.WarpPortal2SM.Instance, WarpPortal2, object>.State.Callback)(smi =>
			{
				smi.master.SetAssignable(false);
				smi.master.IsConsumed = true;
				this.isCharged.Set(false, smi);
			})).PlayAnim("recharge", KAnim.PlayMode.Loop).ToggleStatusItem(ModBuildingStatus.WarpPortalCharging, (Func<WarpPortal2.WarpPortal2SM.Instance, object>)(smi => (object)smi.master)).Update((System.Action<WarpPortal2.WarpPortal2SM.Instance, float>)((smi, dt) =>
			{
				Debug.Log("DEU?");

				smi.master.rechargeProgress += dt;
				if ((double)smi.master.rechargeProgress <= 3000.0)
				{
					Debug.Log("DEU4?");

					return;
				}
				this.isCharged.Set(true, smi);
				smi.master.rechargeProgress = 0.0f;
				smi.GoTo((StateMachine.BaseState)this.idle);
				Debug.Log("DEU?2");

			}));
		}

		public class OccupiedStates :
		  GameStateMachine<WarpPortal2.WarpPortal2SM, WarpPortal2.WarpPortal2SM.Instance, WarpPortal2, object>.State
		{
			public GameStateMachine<WarpPortal2.WarpPortal2SM, WarpPortal2.WarpPortal2SM.Instance, WarpPortal2, object>.State get_on;
			public GameStateMachine<WarpPortal2.WarpPortal2SM, WarpPortal2.WarpPortal2SM.Instance, WarpPortal2, object>.State waiting;
			public GameStateMachine<WarpPortal2.WarpPortal2SM, WarpPortal2.WarpPortal2SM.Instance, WarpPortal2, object>.State warping;
		}

		public new class Instance :
		  GameStateMachine<WarpPortal2.WarpPortal2SM, WarpPortal2.WarpPortal2SM.Instance, WarpPortal2, object>.GameInstance
		{
			public Instance(WarpPortal2 master)
			  : base(master)
			{
			}

			public Notification CreateDupeWaitingNotification() => (UnityEngine.Object)this.master.worker != (UnityEngine.Object)null ? new Notification(MISC.NOTIFICATIONS.WARP_PORTAL_DUPE_READY.NAME.Replace("{dupe}", this.master.worker.name), NotificationType.Neutral, (Func<List<Notification>, object, string>)((notificationList, data) => MISC.NOTIFICATIONS.WARP_PORTAL_DUPE_READY.TOOLTIP.Replace("{dupe}", this.master.worker.name)), expires: false, click_focus: this.master.transform) : (Notification)null;
		}
	}
}
