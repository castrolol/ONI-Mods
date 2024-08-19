using HarmonyLib;
using KSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeResourceBuildings
{
	[SerializationConfig(MemberSerialization.OptIn)]
	public class FlowSlider : KMonoBehaviour, IIntSliderControl
	{
		public const string Title = "Flow Rate";

		public const string Tooltip = "Flow Rate";

		public string SliderTitleKey => "Amout";

		public string SliderUnits => "g/s"  ;

		[MyCmpGet]
		public FreeSource source;

		[MyCmpGet]
		public Filterable filterable;

		public string GetSliderTooltip(int index) => Title;

		public string GetSliderTooltipKey(int index)
		{
			return "Amout of selected liquid per second";
		}

		public int SliderDecimalPlaces(int index)
		{
			return 0;
		}

		public float GetSliderMin(int index)
		{
			return 0;
		}

		public float GetSliderMax(int index)
		{
			var flowManager = Conduit.GetFlowManager(source.conduitType);
			return Traverse.Create(flowManager).Field("MaxMass").GetValue<float>() * 1000f;
		}

		public float GetSliderValue(int index)
		{
			return source.Flow;
		}

		public void SetSliderValue(float percent, int index)
		{
			source.Flow = percent;
		}
	}
}
