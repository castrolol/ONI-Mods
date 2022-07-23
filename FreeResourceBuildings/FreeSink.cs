using KSerialization;
using STRINGS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FreeResourceBuildings
{
	[SerializationConfig(MemberSerialization.OptIn)]
	public class FreeSink : KMonoBehaviour
	{
		[SerializeField]
		public ConduitType Type;

		[MyCmpGet]
		public KBatchedAnimController anim;

		private int inputCell;
		private Operational.Flag incomingFlag = new Operational.Flag("incoming", Operational.Flag.Type.Requirement);

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			anim.SetSymbolVisiblity("effect", false);
		}

		protected override void OnSpawn()
		{
			base.OnSpawn();

			var building = GetComponent<Building>();
			inputCell = building.GetUtilityInputCell();

			Conduit.GetFlowManager(Type).AddConduitUpdater(ConduitUpdate);
		}

		protected override void OnCleanUp()
		{
			Conduit.GetFlowManager(Type).RemoveConduitUpdater(ConduitUpdate);
			base.OnCleanUp();
		}

		SimHashes lastElement = SimHashes.Vacuum;

		private void ConduitUpdate(float dt)
		{
			var flowManager = Conduit.GetFlowManager(Type);
			if (flowManager == null || !flowManager.HasConduit(inputCell))
			{
				anim.Play("off");
				GetComponent<Operational>().SetFlag(incomingFlag, false);
				return;

			}

			var contents = flowManager.GetContents(inputCell);

			Element element = ElementLoader.FindElementByHash(contents.element);

			if (element != null)
			{
				if (lastElement != contents.element)
				{
					lastElement = contents.element;
					anim.Play("on");

					anim.SetSymbolTint("place_color", element.substance.uiColour);
				}
			}
			else
			{
				anim.Play("off");
			}



			GetComponent<Operational>().SetFlag(incomingFlag, contents.mass > 0.0f);
			if (GetComponent<Operational>().IsOperational)
			{
				flowManager.RemoveElement(inputCell, contents.mass);
			}
		}
	}
}
