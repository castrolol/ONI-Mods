using PeterHan.PLib.Detours;
using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AnalyzerDoors
{
	public class AttributesAnalyzerSideScreen : SideScreenContent
	{
		public static readonly IDetouredField<LocText, TextStyleSetting> LOCTEXT_STYLE = PDetours.DetourFieldLazy<LocText, TextStyleSetting>(nameof(LocText.textStyleSetting));
		public static readonly IDetouredField<KImage, ColorStyleSetting> COLOR_STYLE_SETTING = PDetours.DetourFieldLazy<KImage, ColorStyleSetting>("colorStyleSetting");

		public KButton andButton;
		public KButton orButton;

		AttributesAnalyzerDoor source;

		List<AttributeConditionRow> rows;

		public override bool IsValidForTarget(GameObject target)
		{
			return target.GetComponent<AttributesAnalyzerDoor>() != null;
		}


		protected override void OnPrefabInit()
		{

			rows = new List<AttributeConditionRow>();

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
			
			var typePanel = CreateConditionModeUI();

			var checkboxPanel = new PPanel("AttributesAnalyzerCheckPanel")
			{
				FlexSize = Vector2.one,
				Margin = new RectOffset(4, 4, 4, 4),
				Direction = PanelDirection.Horizontal,
			};

			checkboxPanel.AddChild(new PCheckBox("AttributesAnalyzerSideCheckbox")
			{
				ToolTip = "Should consider the base attribute or both base and bonus",
				FlexSize = Vector2.one,
				Text = "Use only base value",
				TextStyle = PUITuning.Fonts.UIDarkStyle,
				TextAlignment = TextAnchor.MiddleLeft,
				InitialState = source.config.isBaseValue ? 1 : 0,
				OnChecked = OnBaseCheckboxChange
			});


			var attributesPanel = new PGridPanel("AttributesAnalyzerSideScreen")
			{
				FlexSize = Vector2.one,
				Margin = new RectOffset(4, 4, 4, 4),

			};
			var conditionList = new List<ConditionTypeItem>
			{
				AttributeConditionType.Ignore,
				AttributeConditionType.Greater,
				AttributeConditionType.GreaterOrEqual,
				AttributeConditionType.Equals,
				AttributeConditionType.LessOrEqual,
				AttributeConditionType.Less,
			};

			var list = source.config.GetAttributesList();

			for (var i = 0; i < list.Count; i++)
			{
				attributesPanel.AddRow(new GridRowSpec());
			}

			attributesPanel.AddColumn(new GridColumnSpec(60, 1f));
			attributesPanel.AddColumn(new GridColumnSpec(30, 1f));
			attributesPanel.AddColumn(new GridColumnSpec(30, 1f));

			for (var i = 0; i < list.Count; i++)
			{
				var attr = list[i];

				var row = new AttributeConditionRow(this, attr);
				rows.Add(row);
				row.CreateUI(attributesPanel, i, conditionList);
			}

			typePanel.AddTo(gameObject);
			checkboxPanel.AddTo(gameObject);
			attributesPanel.AddTo(gameObject);

			ContentContainer = gameObject;

			base.OnPrefabInit();

		}

		public void RefreshMode()
		{
			RefreshButtonAND();
			RefreshButtonOR();
		}

		public void RefreshButtonAND()
		{
			var kImage = andButton.GetComponentInChildren<KImage>();
			var style = PUITuning.Colors.ButtonPinkStyle;
			if (!source.config.needsAll)
			{
				style = PUITuning.Colors.ButtonBlueStyle;
			}

			COLOR_STYLE_SETTING.Set(kImage, style);
			kImage.ApplyColorStyleSetting();
		}

		public void RefreshButtonOR()
		{
			var kImage = orButton.GetComponentInChildren<KImage>();
			var style = PUITuning.Colors.ButtonPinkStyle;
			if (source.config.needsAll)
			{
				style = PUITuning.Colors.ButtonBlueStyle;
			}

			COLOR_STYLE_SETTING.Set(kImage, style);
			kImage.ApplyColorStyleSetting();
		}

		private PPanel CreateConditionModeUI()
		{
			var typePanel = new PPanel("AttributesAnalyzerPanel")
			{
				FlexSize = Vector2.one,
				Margin = new RectOffset(4, 4, 4, 4),
				Direction = PanelDirection.Horizontal,
			};

			

			var andPanel = new PPanel("AttributesAnalyzerPanelAND")
			{
				FlexSize = Vector2.one,
				Margin = new RectOffset(4, 4, 4, 4),
				Direction = PanelDirection.Horizontal,
			};

			var orPanel = new PPanel("AttributesAnalyzerPanelOR")
			{
				FlexSize = Vector2.one,
				Margin = new RectOffset(4, 4, 4, 4),
				Direction = PanelDirection.Horizontal,
			};

			


			andPanel.AddChild(new PButton("AttributesAnalyzerSideAND")
			{
				Text = "ALL",
				FlexSize = Vector2.one,
				Margin = new RectOffset(4, 4, 4, 4),
				OnClick = (s) => UpdateMode(true)
			}.AddOnRealize(s =>
			{
				andButton = s.GetComponent<KButton>();
				RefreshButtonAND();
			}));


			orPanel.AddChild(new PButton("AttributesAnalyzerSideOR")
			{
				Text = "SOME",
				FlexSize = Vector2.one,
				Margin = new RectOffset(4, 4, 4, 4),
				OnClick = (s) => UpdateMode(false)

			}.AddOnRealize(s =>
			{
				orButton = s.GetComponent<KButton>();
				RefreshButtonOR();
			}));

			typePanel.AddChild(andPanel);
			typePanel.AddChild(orPanel);
			

			return typePanel;

		}

		private void OnBaseCheckboxChange(GameObject target, int state)
		{

			source.config.isBaseValue = state == 1;
		
			PCheckBox.SetCheckState(target, state == 1 ? 0 : 1);

		}

		public void UpdateMode(bool needsAll)
		{
			source.config.needsAll = needsAll;
			RefreshMode();
		}

		public override void SetTarget(GameObject target)
		{
			source = target.GetComponent<AttributesAnalyzerDoor>();

			if (source == null || rows == null) return;

			var list = source.config.GetAttributesList();
			foreach (var row in rows)
			{
				var item = list.Find(x => x.id == row.condition.id);
				row.Update(item);
			}
			RefreshMode();
		}

		public class ConditionTypeItem : IListableOption
		{
			public AttributeConditionType condition;

			public ConditionTypeItem(AttributeConditionType value)
			{
				condition = value;
			}

			public static implicit operator ConditionTypeItem(AttributeConditionType item)
			{
				return new ConditionTypeItem(item);
			}

			public string GetProperName()
			{
				switch (condition)
				{
					case AttributeConditionType.Ignore:
						return "Ignore";
					case AttributeConditionType.Greater:
						return ">";
					case AttributeConditionType.GreaterOrEqual:
						return ">=";
					case AttributeConditionType.Less:
						return "<";
					case AttributeConditionType.LessOrEqual:
						return "<=";
					case AttributeConditionType.Equals:
						return "=";
					default:
						return "-";
				}
			}

			public string GetOpositeName()
			{
				switch (condition)
				{
					case AttributeConditionType.Ignore:
						return "Ignore";
					case AttributeConditionType.Greater:
						return "<=";
					case AttributeConditionType.GreaterOrEqual:
						return "<";
					case AttributeConditionType.Less:
						return ">=";
					case AttributeConditionType.LessOrEqual:
						return ">";
					case AttributeConditionType.Equals:
						return "≠";
					default:
						return "-";
				}
			}
			public override int GetHashCode()
			{
				return condition.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				if (obj is ConditionTypeItem)
					return ((ConditionTypeItem)obj).condition == this.condition;
				return base.Equals(obj);
			}
		}
		public void UpdateAttributesAccess()
		{
			if (source != null)
			{
				source.UpdateAttributesAccess(true);
			}
		}

		public class AttributeConditionRow
		{
			public AttributeCondition condition;

			public GameObject source;

			public GameObject dropdown;

			public GameObject field;

			public LocText label;

			public TextStyleSetting labelSelectedText;

			public AttributesAnalyzerSideScreen target;

			public AttributeConditionRow(AttributesAnalyzerSideScreen target, AttributeCondition condition)
			{
				this.condition = condition;
				labelSelectedText = PUITuning.Fonts.TextDarkStyle.DeriveStyle(newColor: new Color(0.7019608f, 0.3647059f, 0.5333334f, 1f), style: TMPro.FontStyles.Bold);

				this.target = target;


			}



			public void CreateUI(PGridPanel panel, int index, IEnumerable<ConditionTypeItem> conditions)
			{


				panel.AddChild(new PLabel
				{

					FlexSize = new Vector2(1f, 0),
					Text = condition.GetProperName(),
					ToolTip = condition.GetProperDescription(),
					TextAlignment = TextAnchor.MiddleLeft,
					TextStyle = PUITuning.Fonts.TextDarkStyle,

				}.AddOnRealize(source =>
				{
					label = source.GetComponentInChildren<LocText>();
					RefreshLabel();
				}), new GridComponentSpec(index, 0));

				var dropdownPanel = new PPanel
				{
					FlexSize = Vector2.one,
					Margin = new RectOffset(4, 4, 4, 4),
				}.AddChild(new PComboBox<ConditionTypeItem>
				{
					FlexSize = Vector2.one,
					ToolTip = condition.GetProperDescription(),
					TextAlignment = TextAnchor.MiddleLeft,
					Content = conditions,
					TextStyle = PUITuning.Fonts.UILightStyle,
					InitialItem = new ConditionTypeItem(AttributeConditionType.Ignore),
					OnOptionSelected = OnTypeSelected
				}.AddOnRealize(s =>
				{
					dropdown = s;
					RefreshDropdown();
				}));
				panel.AddChild(dropdownPanel, new GridComponentSpec(index, 1));

				var fieldPanel = new PPanel
				{
					FlexSize = Vector2.one,
					Margin = new RectOffset(4, 4, 4, 4),
				}.AddChild(new PTextField
				{

					FlexSize = new Vector2(1f, 1f),
					PlaceholderText = "0",
					Text = "0",
					Type = PTextField.FieldType.Integer,
					OnTextChanged = OnTextChanged
				}.AddOnRealize(s =>
				{
					field = s;
					RefreshField();
				}));

				panel.AddChild(fieldPanel, new GridComponentSpec(index, 2));


			}
			public void OnTextChanged(GameObject source, string text)
			{
				var value = 0;
				Int32.TryParse(text, out value);
				condition.amount = value;
				target.UpdateAttributesAccess();
				Refresh();
			}

			public void OnTypeSelected(GameObject source, ConditionTypeItem selected)
			{
				condition.condition = selected.condition;

				target.UpdateAttributesAccess();
				Refresh();

			}

			public void Update(AttributeCondition condition)
			{
				this.condition = condition;
				Refresh();
			}

			public void Refresh()
			{
				RefreshLabel();
				RefreshField();
				RefreshDropdown();
			}

			public void RefreshLabel()
			{
				if (condition.condition == AttributeConditionType.Ignore)
				{
					LOCTEXT_STYLE.Set(label, PUITuning.Fonts.TextDarkStyle);
				}
				else
				{
					LOCTEXT_STYLE.Set(label, labelSelectedText);
				}
				label.ApplySettings();
			}

			public void RefreshDropdown()
			{
				var style = PUITuning.Colors.ButtonPinkStyle;
				var kImage = dropdown.GetComponentInChildren<KImage>();
				if (condition.condition == AttributeConditionType.Ignore)
				{
					style = PUITuning.Colors.ButtonBlueStyle;

				}

				PComboBox<ConditionTypeItem>.SetSelectedItem(dropdown, (ConditionTypeItem)condition.condition);

				COLOR_STYLE_SETTING.Set(kImage, style);
				kImage.ApplyColorStyleSetting();
			}

			public void RefreshField()
			{
				if (condition.condition == AttributeConditionType.Ignore)
				{
					field.SetActive(false);

					return;
				}

				field.SetActive(true);
				field.GetComponent<TMPro.TMP_InputField>().text = condition.amount.ToString();

			}


		}


	}
}
