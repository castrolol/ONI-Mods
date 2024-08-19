using HarmonyLib;
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
	public class RadBoltSilder : KMonoBehaviour, IIntSliderControl
	{
		public const string Title = "Radbolts Rate";

		public const string Tooltip = "Radbolts Rate";

		public string SliderTitleKey => " Amout";

		public string SliderUnits =>  UI.UNITSUFFIXES.PERCYCLE;

		[MyCmpGet]
		public FreeRadbolt radbolt;

		public string GetSliderTooltip(int index) => Title;

		public string GetSliderTooltipKey(int index)
		{
			return "Amout of radbolts generated per cycle";
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
			return 20000f;
		}

		public float GetSliderValue(int index)
		{
			return radbolt.amountPerCycle;
		}

		public void SetSliderValue(float percent, int index)
		{
			radbolt.amountPerCycle = percent;
		}
	}
}
