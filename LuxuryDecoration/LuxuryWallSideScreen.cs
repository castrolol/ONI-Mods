
using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LuxuryDecoration
{

	public class LuxuryWallSideScreen : SideScreenContent
	{

		public GameObject typesDropDownGo;
		public GameObject colorsDropDownGo;
		public ILuxuryWallSelector source;
		public Image icon;
		public KAnimFile anim;
		public PComboBox<DropdownItem> dropdownColor;
		public PComboBox<DropdownItem> dropdownType;


		public string Type => source?.GetValue(0) ?? "chess";
		public string Color => source?.GetValue(1) ?? "white";

		public string AnimString => $"{Type}_{Color}";
		public string AnimSymbolString => $"wall_{AnimString}";

		protected override void OnPrefabInit()
		{

			anim = Assets.GetAnim((HashedString)"luxury_walls_kanim");


			Debug.Log("AnimString = " + AnimString);

			var margin = new RectOffset(4, 4, 4, 4);
			var baseLayout = gameObject.GetComponent<BoxLayoutGroup>();
			if (baseLayout != null)
				baseLayout.Params = new BoxLayoutParams()
				{
					Margin = margin,
					Direction = PanelDirection.Vertical,
					Alignment = TextAnchor.UpperCenter,
					Spacing = 8
				};
			var rowBG = PUITuning.Images.GetSpriteByName("overview_highlight_outline_sharp");

			var types = StringsToListables(source.GetItems(0));
			var colors = StringsToListables(source.GetItems(1));

			dropdownType = new PComboBox<DropdownItem>("LuxuryWallType")
			{
				Content = types,
				InitialItem = (DropdownItem)Type,
				ToolTip = "",
				TextStyle = PUITuning.Fonts.TextLightStyle,
				TextAlignment = TextAnchor.MiddleRight,
				OnOptionSelected = OnTypeSelect,
				FlexSize = Vector2.right,
			}.AddOnRealize((obj) => typesDropDownGo = obj);



			dropdownColor = new PComboBox<DropdownItem>("LuxuryWallColor")
			{
				Content = colors,
				InitialItem = (DropdownItem)Color,
				ToolTip = "",
				TextStyle = PUITuning.Fonts.TextLightStyle,
				TextAlignment = TextAnchor.MiddleRight,
				OnOptionSelected = OnColorSelect,
				FlexSize = Vector2.right,
			}.AddOnRealize((obj) => colorsDropDownGo = obj);



			PPanel iconPanel = new PPanel("LuxuryWallIconPanel")
			{
				FlexSize = Vector2.one,
				Alignment = TextAnchor.MiddleCenter,
				Spacing = 10,
				Direction = PanelDirection.Horizontal,
				Margin = new RectOffset(4, 4, 4, 4)
			}.AddChild(new PLabel("LuxuryWallIcon")
			{
				SpriteMode = Image.Type.Simple,
				SpritePosition = TextAnchor.MiddleCenter,
				MaintainSpriteAspect = true,
				Sprite = Def.GetUISpriteFromMultiObjectAnim(anim, AnimString, true, AnimSymbolString),
				ToolTip = "",
				FlexSize = Vector2.left,
			}.AddOnRealize(obj => icon = obj.GetComponentInChildren<Image>()));

			PPanel rowType = new PPanel("LuxuryWallTypePanel")
			{
				FlexSize = Vector2.one,
				Alignment = TextAnchor.MiddleCenter,
				Spacing = 10,
				Direction = PanelDirection.Horizontal,
				Margin = new RectOffset(4, 4, 4, 4)
			}.AddChild(new PLabel("LuxuryWallTypeLabel")
			{
				TextAlignment = TextAnchor.MiddleLeft,
				ToolTip = "",
				Text = "Type",
				TextStyle = PUITuning.Fonts.TextDarkStyle,
				FlexSize = Vector2.left,
			}).AddChild(dropdownType);

			PPanel rowColor = new PPanel("LuxuryWallColorPanel")
			{
				FlexSize = Vector2.one,
				Alignment = TextAnchor.MiddleCenter,
				Spacing = 10,
				Direction = PanelDirection.Horizontal,
				Margin = new RectOffset(4, 4, 4, 4)
			}.AddChild(new PLabel("LuxuryWallColorLabel")
			{
				TextAlignment = TextAnchor.MiddleLeft,
				ToolTip = "",
				Text = "Color",
				TextStyle = PUITuning.Fonts.TextDarkStyle,
				FlexSize = Vector2.left,
			}).AddChild(dropdownColor);

			iconPanel.AddTo(gameObject);
			rowType.AddTo(gameObject);
			rowColor.AddTo(gameObject);

			ContentContainer = gameObject;

			base.OnPrefabInit();
		}



		public override string GetTitle() => "Luxury Wall";

		public override int GetSideScreenSortOrder() => 300;

		public override bool IsValidForTarget(GameObject target)
		{
			Debug.Log("LuxuryWallSideScreen.IsValidForTarget(" + target + ") = " + (target.GetComponent<ILuxuryWallSelector>() != null));

			return target.GetComponent<ILuxuryWallSelector>() != null;
		}

		public override void SetTarget(GameObject target)
		{
			source = target.GetComponent<ILuxuryWallSelector>();

			if (target == null) return;
			if (typesDropDownGo == null) return;
			if (colorsDropDownGo == null) return;


			PComboBox<DropdownItem>.SetSelectedItem(typesDropDownGo, (DropdownItem)Type);

			PComboBox<DropdownItem>.SetSelectedItem(colorsDropDownGo, (DropdownItem)Color);

			UpdateIcon();

		}

		public List<DropdownItem> StringsToListables(List<String> items)
		{
			var options = new List<DropdownItem>();

			foreach (var item in items)
				options.Add(StringToListable(item));

			return options;
		}
		public DropdownItem StringToListable(String item) => new DropdownItem
		{
			Id = item
		};

		private void OnTypeSelect(GameObject data, DropdownItem option)
		{
			source.SetValue(0, option.Id);
			UpdateIcon();

		}
		private void OnColorSelect(GameObject data, DropdownItem option)
		{
			source.SetValue(1, option.Id);
			UpdateIcon();
		}


		void UpdateIcon()
		{
			try
			{
				icon.sprite = Def.GetUISpriteFromMultiObjectAnim(anim, AnimString, true, AnimSymbolString);
			}
			catch (Exception ex)
			{
				Debug.Log(icon);
				Debug.Log(Def.GetUISpriteFromMultiObjectAnim(anim, AnimString, true, AnimSymbolString));
			}

		}

		public class DropdownItem : IListableOption
		{
			public string Id; 

			public string GetName()
			{
				switch (Id)
				{
					case "chess":
						return "Chess";
					case "bighex":
						return "Big Hexes";
					case "bricks":
						return "Bricks";
					case "cubes":
						return "Cubes";
					case "diamond":
						return "Diamond";
					case "hex":
						return "Small Hexes";
					case "mosaic":
						return "Mosaic";
					case "liquid":
						return "Liquid";
					case "leather":
						return "Leather";
					case "fabric":
						return "Fabric";
					case "stones":
						return "Stones";
					case "old":
						return "Old Style";
					case "braids":
						return "Lines";
					case "white":
						return "White";
					case "black":
						return "Black";
					case "blue":
						return "Blue";
					case "red":
						return "Red";
					case "yellow":
						return "Yellow";
					case "purple":
						return "Purple";
					case "green":
						return "Green";

					default:
						return Id;

				}
			}




			public string GetProperName() => GetName();

			public override bool Equals(object obj)
			{
				if (obj != null || obj.GetType() != typeof(DropdownItem)) return base.Equals(obj);
				return Id == ((DropdownItem)obj).Id;
			}

			public static explicit operator DropdownItem(string value)
			{
				return new DropdownItem
				{
					Id = value
				};
			}
		}
	}



}