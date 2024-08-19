using KSerialization;
using STRINGS;

namespace FreeResourceBuildings
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class HeaterTempSlider : KMonoBehaviour, ISingleSliderControl
    {
        public const string Title = "Heat";
        public const string Tooltip = "Heat";

        public string SliderTitleKey => "STRINGS.UI.UISIDESCREENS.HEATERSLIDER.TEMP.TITLE";

        string ISliderControl.SliderUnits
        {
            get
            {
                LocString locString = UI.UNITSUFFIXES.HEAT.DTU_S;

                return locString;
            }
        }

        [MyCmpGet]
        public DirectVolumeHeater source;


        public string GetSliderTooltip(int index) => Tooltip;

        public string GetSliderTooltipKey(int index) => "STRINGS.UI.UISIDESCREENS.HEATERSLIDER.TEMP.TOOLTIP";

        public int SliderDecimalPlaces(int index) => 1;

        public float GetSliderMin(int index)
        {
            return 4_000f;
        }



        public float GetSliderMax(int index)
        {
            return 40_000f;
        }

        public float GetSliderValue(int index)
        {
            return source.DTUs;
        }

        public void SetSliderValue(float dtus, int index)
        {
            source.DTUs = dtus;
        }
    }
}
