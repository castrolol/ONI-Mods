using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreWarps
{
	public class CustomWarpWorkable : Workable
	{

		[MyCmpReq]
		public Assignable assignable;

		public Chore chore;

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();


			this.assignable.OnAssign += new System.Action<IAssignableIdentity>(this.Assign);


		}

		private void Assign(IAssignableIdentity obj)
		{
			Debug.Log("Assigned with success!");
			if (this.chore == null)
				CancelChore();

			ActivateChore();
		}
		private void ActivateChore()
		{

			Debug.Assert(this.chore == null);
			this.chore = (Chore)new WorkChore<Workable>(Db.Get().ChoreTypes.Idle, (IStateMachineTarget)this, on_complete: ((System.Action<Chore>)(o => this.CompleteChore())),/*override_anims: Assets.GetAnim((HashedString)"anim_interacts_warp_portal_sender_kanim"), */ allow_prioritization: false, priority_class: PriorityScreen.PriorityClass.high);
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

		}


		private void CompleteChore()
		{

			Debug.Log("Chore completed!");

		}

		protected override void OnStartWork(Worker worker)
		{
			//base.OnStartWork(worker);

			Debug.Log("Work started");

		}

	}
}
