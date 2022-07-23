using KSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LuxuryDecoration
{
	public class LuxuryTypeSelectorWall : KMonoBehaviour, ILuxuryWallSelector
	{
		public static string lastSelectedType = "chess";
		public static string lastSelectedColor = "white";


		[SerializeField]
		[Serialize]
		private string selectedType;
		[SerializeField]
		[Serialize]
		private string selectedColor;

		string Type => selectedType ?? lastSelectedType ?? "chess";
		string Color => selectedColor ?? lastSelectedColor ?? "white";

		[MyCmpReq]
		KBatchedAnimController anim;

		[MyCmpAdd]
		private CopyBuildingSettings copyBuildingSettings;

		List<String> types = new List<string> { "chess", "bighex", "bricks", "cubes", "diamond", "hex", "mosaic", "liquid", "leather", "fabric", "stones", "old", "braids" };
		List<String> colors = new List<string> { "white", "black", "red", "green", "blue", "purple", "yellow" };

		private static readonly EventSystem.IntraObjectHandler<LuxuryTypeSelectorWall> OnCopySettingsDelegate = new EventSystem.IntraObjectHandler<LuxuryTypeSelectorWall>((System.Action<LuxuryTypeSelectorWall, object>)((component, data) => component.OnCopySettings(data)));
		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			this.Subscribe<LuxuryTypeSelectorWall>(-905833192, LuxuryTypeSelectorWall.OnCopySettingsDelegate);




		}

		protected override void OnSpawn()
		{
			base.OnSpawn();
			if (String.IsNullOrEmpty(selectedColor))
				selectedColor = lastSelectedColor;
			if (String.IsNullOrEmpty(selectedType))
				selectedType = lastSelectedType;
			UpdateAnim();
		}
		void UpdateStatics()
		{
			lastSelectedColor = selectedColor ?? "chess";
			lastSelectedType = selectedType ?? "white";
		}

		private void OnCopySettings(object data)
		{

			LuxuryTypeSelectorWall component = ((GameObject)data).GetComponent<LuxuryTypeSelectorWall>();


			selectedColor = component.Color;
			selectedType = component.Type;
			UpdateStatics();

			UpdateAnim();
		}

		public List<string> GetItems(int index) => index == 0 ? types : colors;
		public string GetValue(int index) => index == 0 ? Type : Color;
		public void SetValue(int index, string value)
		{
			if (index == 0)
			{
				selectedType = value;
			}
			else
			{
				selectedColor = value;
			}

			Debug.Log($"SetValue({index}, {value})");
			UpdateAnim();

		}


		private void UpdateAnim()
		{
			Debug.Log($"{ Type}_{ Color}");
			anim.Play($"{Type}_{Color}");
			UpdateStatics();
		}
	}
}
