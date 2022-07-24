using DrawableDecoration.UI;
using HarmonyLib;
using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DrawableDecoration
{


	public class DrawableWallSideScreen : SideScreenContent
	{

		public IDrawableColorSelectable target;

		public GameObject[,] buttons;
		public KButton[,] kbuttons;

		public GameObject labelGo;
		public UnityEngine.Color[,] COLORS;


		public Sprite iconSprite;
		public Sprite IconSprite
		{
			get
			{
				if (!iconSprite)
				{
					var sprite = NotificationScreen.Instance.icon_normal;

					Debug.Log("IconSprite");
					iconSprite = Sprite.Create(sprite.texture, sprite.textureRect, sprite.pivot, sprite.pixelsPerUnit * 1.5f);
				}
				return iconSprite;
			}
		}

		private Color GetContrastColor(Color c)
		{
			return (c.r * 0.299f + c.g * 0.587f + c.b * 0.114f) > 0.5f ? Color.black : Color.white;

		}

		void UpdateContrast(int x, int y)
		{
			Debug.Log("UpdateContrast");
			var index = (x * 3) + y;


			var color = GetContrastColor(target.GetValue(index));
			kbuttons[x, y].fgImage.color = color;
		}

		public UnityEngine.Color GetFromRgb(int r, int g, int b)
		{
			return new UnityEngine.Color(r / 255f, g / 255f, b / 255f);
		}
		private PPanel CreateCell(UnityEngine.Color c, int x, int y)
		{
			var c1 = ScriptableObject.CreateInstance<ColorStyleSetting>();


			c1.Init(c);
			PPanel iconPanel = new PPanel($"LuxuryWallIconPanel{x}x{y}")
			{
				FlexSize = Vector2.one,
				Alignment = TextAnchor.MiddleCenter,
				Spacing = 1,
				Direction = PanelDirection.Vertical,
				Margin = new RectOffset(1, 1, 1, 1)
			};
			iconPanel.AddChild(new PButton($"DrawableWallBoxButton{x}x{y}")
			{
				Color = c1,
				Margin = new RectOffset(1, 1, 1, 1),
				FlexSize = Vector2.one * .3f,
				Text = " ",
				OnClick = source => OnColorClick(source, x, y),
				Sprite = IconSprite


			}.AddOnRealize(go =>
			{
				kbuttons[x, y] = go.GetComponent<KButton>();
				kbuttons[x, y].fgImage.color = Color.black;
				kbuttons[x, y].fgImage.transform.localScale = Vector2.one * 0.2f;
				kbuttons[x, y].fgImage.SetAlpha(0);
				buttons[x, y] = go;
			}));
			return iconPanel;
		}

		private PPanel CreateColor(UnityEngine.Color c, int x, int y)
		{
			Debug.Log("CreateColor");
			var c1 = ScriptableObject.CreateInstance<ColorStyleSetting>();

			c1.Init(c);
			PPanel iconPanel = new PPanel($"LuxuryWallICconPanel{x}x{y}")
			{
				FlexSize = Vector2.one,
				Alignment = TextAnchor.MiddleCenter,
				Spacing = 1,
				Direction = PanelDirection.Vertical,
				Margin = new RectOffset(0, 0, 0, 0)
			};
			iconPanel.AddChild(new PButton($"DrawableWallCBoxButton{x}x{y}")
			{
				Color = c1,
				Margin = new RectOffset(0, 0, 0, 0),
				FlexSize = Vector2.one * .3f,
				Text = " ",
				OnClick = source => OnChangeColorClick(source, x, y),

			});
			return iconPanel;
		}

		private GameObject selected;

		private Vector2Int? selectedPos = null;
		void OnChangeColorClick(GameObject source, int x, int y)
		{
			Debug.Log("OnChangeColorClick");
			if (selected == null) return;
			var color = COLORS[x, y]; 
			UpdateColor(selectedPos.Value.x, selectedPos.Value.y, color, kbuttons[selectedPos.Value.x, selectedPos.Value.y]);
		}

		void UpdateColor(int x, int y, Color color, KButton btn)
		{
			Debug.Log("UpdateColor");
			var bgColorStyle = btn.bgImage.colorStyleSetting;


			bgColorStyle.activeColor = color;
			bgColorStyle.inactiveColor = color;
			bgColorStyle.disabledColor = color;
			bgColorStyle.disabledActiveColor = color;
			bgColorStyle.hoverColor = color;
			bgColorStyle.disabledhoverColor = color;

			UIDetoursClone.COLOR_STYLE_SETTING.Set(btn.bgImage, bgColorStyle);
			UIDetoursClone.APPLY_COLOR_STYLE.Invoke(btn.bgImage);
			Debug.Log("Should I trigger?");
			if (selectedPos.HasValue)
			{
				Debug.Log("yes");
				var index = (selectedPos.Value.x * 3) + selectedPos.Value.y;
				target?.SetValue(index, color);
				UpdateContrast(selectedPos.Value.x, selectedPos.Value.y);
			}
		}
		private void OnColorClick(GameObject source, int x, int y)
		{
			Debug.Log("OnColorClick");
			selectedPos = new Vector2Int(x, y);

			selected = source;


			UpdateSelectionMark();
			UpdateContrast(x, y);

		}

		void UpdateSelectionMark()
		{
			Debug.Log("UpdateSelectionMark");
			if (kbuttons == null) return;
			var x = -1;
			var y = -1;

			if (selectedPos.HasValue)
			{
				x = selectedPos.Value.x;
				y = selectedPos.Value.y;
			}

			for (var m = 0; m < kbuttons.GetLength(0); m++)
			{
				for (var n = 0; n < kbuttons.GetLength(1); n++)
				{
					var btn = kbuttons[m, n];
					if (x == m && y == n)
					{
						btn.fgImage.SetAlpha(1);


					}
					else
					{
						btn.fgImage.SetAlpha(0);
					}
				}
			}
		}

		protected override void OnPrefabInit()
		{
			Debug.Log("OnPrefabInit");
			COLORS = new Color[7, 16] {
				{GetFromRgb(255,255,255), GetFromRgb(252,208,209), GetFromRgb(248,222,203), GetFromRgb(252,234,206), GetFromRgb(251,244,206), GetFromRgb(254,254,207), GetFromRgb(235,253,204), GetFromRgb(209,252,217), GetFromRgb(205,251,222), GetFromRgb(206,254,255), GetFromRgb(221,232,252), GetFromRgb(209,228,243), GetFromRgb(199,208,255), GetFromRgb(228,210,253), GetFromRgb(255,201,255), GetFromRgb(245,211,227)},
				{GetFromRgb(214,214,214), GetFromRgb(252,165,155), GetFromRgb(253,183,152), GetFromRgb(255,209,156), GetFromRgb(255,226,143), GetFromRgb(251,250,160), GetFromRgb(207,255,138), GetFromRgb(162,246,149), GetFromRgb(168,246,207), GetFromRgb(167,249,252), GetFromRgb(170,208,255), GetFromRgb(157,193,237), GetFromRgb(157,163,253), GetFromRgb(206,157,254), GetFromRgb(249,165,248), GetFromRgb(240,165,208)},
				{GetFromRgb(170,170,170), GetFromRgb(252,133,132), GetFromRgb(255,146,110), GetFromRgb(240,183,94), GetFromRgb(251,221,94), GetFromRgb(247,253,48), GetFromRgb(190,253,99), GetFromRgb(104,254,111), GetFromRgb(119,253,190), GetFromRgb(106,252,252), GetFromRgb(136,208,229), GetFromRgb(128,165,255), GetFromRgb(124,128,252), GetFromRgb(187,127,255), GetFromRgb(255,110,255), GetFromRgb(253,126,190)},
				{GetFromRgb(128,128,128), GetFromRgb(252,79,67), GetFromRgb(249,111,61), GetFromRgb(255,151,51), GetFromRgb(254,195,0), GetFromRgb(239,239,0), GetFromRgb(153,255,0), GetFromRgb(68,255,93), GetFromRgb(73,255,160), GetFromRgb(5,251,252), GetFromRgb(94,185,246), GetFromRgb(87,133,253), GetFromRgb(95,95,255), GetFromRgb(161,80,246), GetFromRgb(249,70,255), GetFromRgb(255,79,171)},
				{GetFromRgb(85,85,85), GetFromRgb(241,3,17), GetFromRgb(233,51,4), GetFromRgb(200,109,0), GetFromRgb(190,143,4), GetFromRgb(179,173,10), GetFromRgb(121,187,4), GetFromRgb(9,203,4), GetFromRgb(1,206,105), GetFromRgb(3,189,197), GetFromRgb(7,133,217), GetFromRgb(5,81,255), GetFromRgb(27,30,255), GetFromRgb(128,2,245), GetFromRgb(217,5,219), GetFromRgb(249,2,123)},
				{GetFromRgb(42,42,42), GetFromRgb(193,0,0), GetFromRgb(156,39,4), GetFromRgb(153,85,0), GetFromRgb(138,113,3), GetFromRgb(144,143,1), GetFromRgb(84,145,0), GetFromRgb(0,156,0), GetFromRgb(20,161,91), GetFromRgb(1,140,147), GetFromRgb(8,101,155), GetFromRgb(3,66,182), GetFromRgb(0,0,217), GetFromRgb(94,0,200), GetFromRgb(159,0,164), GetFromRgb(186,3,97)},
				{GetFromRgb(0,0,0), GetFromRgb(73,2,7), GetFromRgb(84,18,0), GetFromRgb(85,39,2), GetFromRgb(79,59,0), GetFromRgb(79,83,0), GetFromRgb(46,79,0), GetFromRgb(0,78,0), GetFromRgb(0,77,39), GetFromRgb(0,81,88), GetFromRgb(0,52,89), GetFromRgb(0,25,77), GetFromRgb(1,0,94), GetFromRgb(40,1,79), GetFromRgb(77,0,83), GetFromRgb(78,0,36)},
			};

			var margin = new RectOffset(4, 4, 4, 8);
			var baseLayout = gameObject.GetComponent<BoxLayoutGroup>();
			if (baseLayout != null)
				baseLayout.Params = new BoxLayoutParams()
				{
					Margin = margin,
					Direction = PanelDirection.Vertical,
					Alignment = TextAnchor.UpperCenter,
					Spacing = 8
				};

			buttons = new GameObject[3, 3];
			kbuttons = new KButton[3, 3];

			PGridPanel iconPanel = new PGridPanel("DrawableGridButtons");

			new PLabel("DrawableGridHeader")
			{
				Text = "Select one block",
				TextAlignment = TextAnchor.MiddleLeft,
				ToolTip = "",
				TextStyle = PUITuning.Fonts.TextDarkStyle,
				FlexSize = Vector2.left,
			}.AddTo(gameObject);

			iconPanel.AddRow(new GridRowSpec(86, 0.33f));
			iconPanel.AddRow(new GridRowSpec(86, 0.33f));
			iconPanel.AddRow(new GridRowSpec(86, 0.33f));


			iconPanel.AddColumn(new GridColumnSpec(86, 0.33f));
			iconPanel.AddColumn(new GridColumnSpec(86, 0.33f));
			iconPanel.AddColumn(new GridColumnSpec(86, 0.33f));


			iconPanel.AddChild(CreateCell(target.GetValue(0), 0, 0), new GridComponentSpec(0, 0));
			iconPanel.AddChild(CreateCell(target.GetValue(1), 0, 1), new GridComponentSpec(0, 1));
			iconPanel.AddChild(CreateCell(target.GetValue(2), 0, 2), new GridComponentSpec(0, 2));

			iconPanel.AddChild(CreateCell(target.GetValue(3), 1, 0), new GridComponentSpec(1, 0));
			iconPanel.AddChild(CreateCell(target.GetValue(4), 1, 1), new GridComponentSpec(1, 1));
			iconPanel.AddChild(CreateCell(target.GetValue(5), 1, 2), new GridComponentSpec(1, 2));

			iconPanel.AddChild(CreateCell(target.GetValue(6), 2, 0), new GridComponentSpec(2, 0));
			iconPanel.AddChild(CreateCell(target.GetValue(7), 2, 1), new GridComponentSpec(2, 1));
			iconPanel.AddChild(CreateCell(target.GetValue(8), 2, 2), new GridComponentSpec(2, 2));


			PPanel pallete = new PPanel("DrwableWallSpacerPalletePanel")
			{
				FlexSize = Vector2.one,
				Alignment = TextAnchor.MiddleCenter,
				Spacing = 10,
				Direction = PanelDirection.Horizontal,
				Margin = new RectOffset(4, 4, 4, 8),

			};

			var colorsPanel = new PGridPanel("DrwableWallSpacerPanelColors")
			{

			};


			var colors = new List<UnityEngine.Color>();

			for (var i = 0; i < 7; i++)
			{
				colorsPanel.AddRow(new GridRowSpec(16, 0.25f));
			}
			for (var i = 0; i < 16; i++)
			{
				colorsPanel.AddColumn(new GridColumnSpec(16, 0.1f));
			}
			for (var x = 0; x < 7; x++)
			{
				for (var y = 0; y < 16; y++)
				{
					var c = CreateColor(COLORS[x, y], x, y);
					colorsPanel.AddChild(c, new GridComponentSpec(x, y));
				}
			}

			pallete.AddChild(colorsPanel);


			iconPanel.AddTo(gameObject);
			new PLabel("DrawableseeGridHeader")
			{
				Text = "Choose the new color",
				TextAlignment = TextAnchor.MiddleLeft,
				ToolTip = "",
				TextStyle = PUITuning.Fonts.TextDarkStyle,
				FlexSize = Vector2.left,
			}.AddTo(gameObject);
			pallete.AddTo(gameObject);
			ContentContainer = gameObject;

			base.OnPrefabInit();
		}

		public override string GetTitle() => "Drawable Wall";

		public override int GetSideScreenSortOrder() => 300;

		public override bool IsValidForTarget(GameObject target)
		{

			return target.GetComponent<IDrawableColorSelectable>() != null;
		}

		public override void SetTarget(GameObject target)
		{
			Debug.Log("SetTarget");
			selected = null;
			selectedPos = null;

			this.target = target.GetComponent<IDrawableColorSelectable>();

			if (target == null) return;

			UpdateColors();
			UpdateSelectionMark();

		}

		private void UpdateColors()
		{
			Debug.Log("UpdateColors");

			if (target == null) return;
			if (kbuttons == null) return;

			for (var x = 0; x < kbuttons.GetLength(0); x++)
			{
				for (var y = 0; y < kbuttons.GetLength(1); y++)
				{
					var index = (x * 3) + y;

					UpdateColor(x, y, target.GetValue(index), kbuttons[x, y]);

				}
			}

		}
	}



}