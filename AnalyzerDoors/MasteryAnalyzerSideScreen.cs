using PeterHan.PLib.Detours;
using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace AnalyzerDoors
{
	public class MasteryAnalyzerSideScreen : SideScreenContent
	{
		public static readonly IDetouredField<LocText, TextStyleSetting> LOCTEXT_STYLE = PDetours.DetourFieldLazy<LocText, TextStyleSetting>(nameof(LocText.textStyleSetting));
		public static readonly IDetouredField<KImage, ColorStyleSetting> COLOR_STYLE_SETTING = PDetours.DetourFieldLazy<KImage, ColorStyleSetting>("colorStyleSetting");

		public KButton andButton;
		public KButton orButton;

		public List<string> selectedItems => source.allowedSkills ?? new List<String>();
		public List<MasteryGroup> groups = new List<MasteryGroup>();
		MasteryAnalyzerDoor source;


		public override bool IsValidForTarget(GameObject target)
		{
			return target.GetComponent<MasteryAnalyzerDoor>() != null;
		}


		protected override void OnPrefabInit()
		{


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

			var root = new PPanel("GridFilterableSideScreen")
			{
				// White background for scroll bar
				Direction = PanelDirection.Vertical,
				Margin = new RectOffset(4, 4, 4, 4),
				Alignment = TextAnchor.MiddleCenter,
				Spacing = 0,
				BackColor = PUITuning.Colors.BackgroundLight,
				FlexSize = Vector2.one
			};

			var masteryPanel = new PPanel("MasteryAnalyzerSideScreen")
			{
				FlexSize = new Vector2(1.0f, 0.0f),
				Margin = new RectOffset(4, 4, 4, 4),
				Direction = PanelDirection.Vertical,

			};
			var masteryScroll = new PScrollPane("MasteryAnalyzerSideScreen")
			{
				FlexSize = new Vector2(1f, 1f),
				Child = masteryPanel,
				ScrollHorizontal = false,
				ScrollVertical = true,
				AlwaysShowVertical = true,
				TrackSize = 8.0f,
				BackColor = PUITuning.Colors.BackgroundLight,
			}.AddOnRealize((source) =>
			{

				var layoutE = source.AddOrGet<LayoutElement>();
				layoutE.preferredHeight = 425f;
				layoutE.layoutPriority = 100;
			});



			var groups = Db.Get().SkillGroups.resources;
			foreach (var group in groups)
				masteryPanel.AddChild(CreateGroup(group));

			root.AddChild(masteryScroll);
			typePanel.AddTo(gameObject);
			root.AddTo(gameObject);

			ContentContainer = gameObject;

			base.OnPrefabInit();

		}

		private IUIComponent CreateGroup(Database.SkillGroup group)
		{
			var groupItem = new MasteryGroup(this, group);
			groups.Add(groupItem);
			return groupItem.CreateUI();
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
			if (!source.needsAll)
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
			if (source.needsAll)
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

		public void UpdateMode(bool needsAll)
		{
			source.needsAll = needsAll;
			RefreshMode();
		}

		public override void SetTarget(GameObject target)
		{
			source = target.GetComponent<MasteryAnalyzerDoor>();

			if (source == null || groups == null || orButton == null) return;

			RefreshCheckboxes();
			 RefreshMode();
		}

		public void UpdateAttributesAccess()
		{
			if (source != null)
			{
				source.UpdateMasteryAccess(true);
			}
		}
		private void RefreshCheckboxes()
		{ 
			foreach (var item in groups)
				item.Refresh();
		}
		public class MasteryGroup
		{
			public Database.SkillGroup group;

			public GameObject container;
			public GameObject checkboxGo;

			public List<MasteryItem> items;


			MasteryAnalyzerSideScreen screen;

			public MasteryGroup(MasteryAnalyzerSideScreen screen, Database.SkillGroup group)
			{
				this.screen = screen;
				this.group = group;
			}
			public PPanel CreateUI()
			{
				items = new List<MasteryItem>();
				var groupPanel = new PPanel()
				{
					FlexSize = Vector2.one,
					Direction = PanelDirection.Vertical,
					Alignment = TextAnchor.LowerLeft,
					Margin = new RectOffset(4, 4, 4, 4)
				};

				var line = new PGridPanel();

				line.AddColumn(new GridColumnSpec(flex: 1f));
				line.AddColumn(new GridColumnSpec(width: 20));
				line.AddRow(new GridRowSpec());



				var expandButton = new PButton
				{
					Text = "+",
					OnClick = (s) =>
					{
						s.GetComponentInChildren<LocText>().text = container.activeSelf ? "+" : "-";
						container.SetActive(!container.activeSelf);
					}
				};

				var skillsPanel = new PPanel()
				{
					FlexSize = Vector2.one,
					Direction = PanelDirection.Vertical,
					Alignment = TextAnchor.LowerLeft,
					Margin = new RectOffset(4, 4, 4, 4)
				}.AddOnRealize(s =>
				{
					s.SetActive(false);
					container = s;
				});


				groupPanel.AddChild(line);
				var skills = Db.Get().Skills.resources;

				foreach (var skill in skills)
				{
					if (skill.skillGroup == group.Id)
					{
						var item = new MasteryItem(screen, skill);
						items.Add(item);
						skillsPanel.AddChild(item.CreateUI());
					}
				}

				var checkbox = new PCheckBox
				{
					FlexSize = Vector2.one,
					Text = group.Name,
					TextStyle = PUITuning.Fonts.UIDarkStyle,
					TextAlignment = TextAnchor.MiddleLeft,
					InitialState = CalculateCheckboxState(),
					OnChecked = OnCheckboxChange
				}.AddOnRealize(s => checkboxGo = s);


				line.AddChild(checkbox, new GridComponentSpec(0, 0));
				line.AddChild(expandButton, new GridComponentSpec(0, 1));


				groupPanel.AddChild(skillsPanel);

				return groupPanel;

			}

			private void OnCheckboxChange(GameObject source, int state)
			{
				if (state != 0)
				{
					foreach (var item in items)
						screen.selectedItems.Remove(item.skill.Id);
				}
				else
				{

					foreach (var item in items)
					{
						if (!screen.selectedItems.Contains(item.skill.Id))
							screen.selectedItems.Add(item.skill.Id);
					}
				}

				screen.RefreshCheckboxes();
			}

			public void Refresh()
			{
				PCheckBox.SetCheckState(checkboxGo, CalculateCheckboxState());
				foreach (var item in items)
					item.Refresh();
			}

			private int CalculateCheckboxState()
			{
				var checkedItems = 0;
				foreach (var item in items)
				{
					if (screen.selectedItems.Contains(item.skill.Id))
						checkedItems++;
				}

				if (checkedItems == 0) return 0;
				return checkedItems == items.Count ? 1 : 2;

			}


		}



		public class MasteryItem
		{
			public Database.Skill skill;
			MasteryAnalyzerSideScreen screen;
			GameObject checkboxGo;
			public MasteryItem(MasteryAnalyzerSideScreen screen, Database.Skill skill)
			{
				this.screen = screen;
				this.skill = skill;
			}
			public PPanel CreateUI()
			{
				var skillPanel = new PPanel()
				{
					FlexSize = Vector2.one,
					Direction = PanelDirection.Vertical,
					Margin = new RectOffset(12, 4, 4, 4),
					Alignment = TextAnchor.LowerLeft,
				};


				var checkbox = new PCheckBox
				{
					FlexSize = Vector2.one,
					Text = skill.Name,
					TextStyle = PUITuning.Fonts.UIDarkStyle,
					TextAlignment = TextAnchor.LowerLeft,
					InitialState = screen.selectedItems.Contains(skill.Id) ? 1 : 0,
					OnChecked = OnCheckboxChange
				}.AddOnRealize(s => checkboxGo = s);

				skillPanel.AddChild(checkbox);


				return skillPanel;

			}
			private void OnCheckboxChange(GameObject source, int state)
			{
				if (state != 0)
				{
					screen.selectedItems.Remove(skill.Id);
				}
				else
				{
					if (!screen.selectedItems.Contains(skill.Id))
						screen.selectedItems.Add(skill.Id);
				}

				screen.RefreshCheckboxes();
			}


			internal void Refresh()
			{
				PCheckBox.SetCheckState(checkboxGo, screen.selectedItems.Contains(skill.Id) ? 1 : 0);
			}
		}

	}
}
