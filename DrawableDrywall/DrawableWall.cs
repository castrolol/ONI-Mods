using KSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DrawableDecoration
{
	public class DrawableWall : KMonoBehaviour, IDrawableColorSelectable
	{

		[MyCmpAdd]
		private CopyBuildingSettings copyBuildingSettings;

		[MyCmpReq]
		KBatchedAnimController anim;

		[SerializeField]
		[Serialize]
		public Color[] colors = new Color[9]
		{
			Color.white,
			Color.white,
			Color.white,
			Color.white,
			Color.white,
			Color.white,
			Color.white,
			Color.white,
			Color.white
		};
		private static readonly EventSystem.IntraObjectHandler<DrawableWall> OnCopySettingsDelegate = new EventSystem.IntraObjectHandler<DrawableWall>((System.Action<DrawableWall, object>)((component, data) => component.OnCopySettings(data)));
		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			this.Subscribe<DrawableWall>(-905833192, DrawableWall.OnCopySettingsDelegate);
		}

		private void OnCopySettings(object data)
		{

			DrawableWall component = ((GameObject)data).GetComponent<DrawableWall>();

			for(var i = 0; i < component.colors.Length; i++)
			{
				colors[i] = component.colors[i];
			}

			UpdateColor();
		}

		protected override void OnSpawn()
		{
			base.OnSpawn();

			if (colors == null)
			{
				colors = new Color[9];
			}

			UpdateColor();
		}

		public Color GetValue(int index)
		{
			return colors[index];
		}

		public void SetValue(int index, Color color)
		{
			colors[index] = color;
			UpdateColor();
		}


		void UpdateColor()
		{
			anim.SetSymbolTint("wall_left_top", colors[0]);
			anim.SetSymbolTint("wall_top", colors[1]);
			anim.SetSymbolTint("wall_right_top", colors[2]);

			anim.SetSymbolTint("wall_left", colors[3]);
			anim.SetSymbolTint("wall_center", colors[4]);
			anim.SetSymbolTint("wall_right", colors[5]);

			anim.SetSymbolTint("wall_left_bottom", colors[6]);
			anim.SetSymbolTint("wall_bottom", colors[7]);
			anim.SetSymbolTint("wall_right_bottom", colors[8]);

		}
	}
}
