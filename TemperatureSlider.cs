using KSerialization;
using STRINGS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeResourceBuildings
{
	[SerializationConfig(MemberSerialization.OptIn)]
	public class TemperatureSlider : KMonoBehaviour, ISingleSliderControl
	{
		public const string Title = "Temperature";
		public const string Tooltip = "Temperature";

		public string SliderTitleKey => "STRINGS.UI.UISIDESCREENS.INFINITESOURCE.TEMP.TITLE";

		string ISliderControl.SliderUnits
		{
			get
			{
				LocString locString = (LocString)null;
				switch (GameUtil.temperatureUnit)
				{
					case GameUtil.TemperatureUnit.Celsius:
						locString = UI.UNITSUFFIXES.TEMPERATURE.CELSIUS;
						break;
					case GameUtil.TemperatureUnit.Fahrenheit:
						locString = UI.UNITSUFFIXES.TEMPERATURE.FAHRENHEIT;
						break;
					case GameUtil.TemperatureUnit.Kelvin:
						locString = UI.UNITSUFFIXES.TEMPERATURE.KELVIN;
						break;
				}
				return locString;
			}
		}

		[MyCmpGet]
		public FreeSource source;

		[MyCmpGet]
		public Filterable filterable;

		public string GetSliderTooltip() => Tooltip;

		public string GetSliderTooltipKey(int index) => "STRINGS.UI.UISIDESCREENS.INFINITESOURCE.TEMP.TOOLTIP";

		public int SliderDecimalPlaces(int index) => 1;

		public float GetSliderMin(int index)
		{
			Element element = ElementLoader.GetElement(filterable.SelectedTag);
			return GameUtil.GetConvertedTemperature(element?.lowTemp ?? 273f);
		}



		public float GetSliderMax(int index)
		{
			Element element = ElementLoader.GetElement(filterable.SelectedTag);
			return GameUtil.GetConvertedTemperature(Math.Min(element?.highTemp ?? 300f, 2700f));
		}

		public float GetSliderValue(int index)
		{
			return GameUtil.GetConvertedTemperature(source.Temp);
		}

		public void SetSliderValue(float percent, int index)
		{
			source.Temp = GameUtil.GetTemperatureConvertedToKelvin(percent);
		}
	}
}
