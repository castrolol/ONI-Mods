// Decompiled with JetBrains decompiler
// Type: WarpPortalSideScreen
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1C7E9CCB-4AA8-44E2-BE45-38990AABF98E
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll

using PeterHan.PLib.UI;
using UnityEngine;
using UnityEngine.UI;

public class WarpPortal2SideScreen : SideScreenContent
{
	[SerializeField]
	private LocText label;
	[SerializeField]
	private KButton button;
	[SerializeField]
	private LocText buttonLabel;
	[SerializeField]
	private KButton cancelButton;
	[SerializeField]
	private LocText cancelButtonLabel;
	[SerializeField]
	private WarpPortal2 target;
	[SerializeField]
	private GameObject contents;
	[SerializeField]
	private GameObject progressBar;
	[SerializeField]
	private LocText progressLabel;

	protected override void OnPrefabInit()
	{


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


		PPanel panel1 = new PPanel("WarpPortal2Panel1")
		{
			FlexSize = Vector2.one,
			Alignment = TextAnchor.MiddleCenter,
			Spacing = 10,
			Direction = PanelDirection.Vertical,
			Margin = new RectOffset(4, 4, 4, 4)
		}.AddChild(new PLabel("WarpPortal2Panel1Label")
		{
			SpriteMode = Image.Type.Simple,
			SpritePosition = TextAnchor.MiddleCenter,
			MaintainSpriteAspect = true,
			ToolTip = "",
			Text = "--",
			FlexSize = Vector2.left,
		}.AddOnRealize(obj => label = obj.GetComponentInChildren<LocText>()));

		panel1.AddChild(new PLabel("WarpPortal2Panel1Progress")
		{
			SpriteMode = Image.Type.Simple,
			SpritePosition = TextAnchor.MiddleCenter,
			MaintainSpriteAspect = true,
			ToolTip = "",
			Text = "0%",
			FlexSize = Vector2.left,
		}.AddOnRealize(obj =>
		{
			progressBar = obj;
			progressLabel = obj.GetComponentInChildren<LocText>();
		}));


		panel1.AddChild(new PButton($"WarpPortal2Panel1Button")
		{
			Margin = new RectOffset(4, 4, 4, 4),
			FlexSize = Vector2.one,
			Text = "Teleportar",
			OnClick = source => OnButtonClick(),

		}).AddOnRealize(source =>
		{
			button = source.GetComponentInChildren<KButton>();
			buttonLabel = source.GetComponentInChildren<LocText>();
		});

		panel1.AddChild(new PButton($"WarpPortal2Panel1CancelButton")
		{
			Margin = new RectOffset(4, 4, 4, 4),
			FlexSize = Vector2.one,
			Text = "Cancelar",
			OnClick = source => OnCancelClick(),

		}).AddOnRealize(source =>
		{
			cancelButton = source.GetComponentInChildren<KButton>();
			cancelButtonLabel = source.GetComponentInChildren<LocText>();
		});

		panel1.AddTo(gameObject);
		ContentContainer = gameObject;

		base.OnPrefabInit();
	}


	protected override void OnSpawn()
	{
		base.OnSpawn();
		this.buttonLabel.SetText((string)STRINGS.UI.UISIDESCREENS.WARPPORTALSIDESCREEN.BUTTON);
		this.cancelButtonLabel.SetText((string)STRINGS.UI.UISIDESCREENS.WARPPORTALSIDESCREEN.CANCELBUTTON);
		//this.button.onClick += new System.Action(this.OnButtonClick);
		//this.cancelButton.onClick += new System.Action(this.OnCancelClick);
		this.Refresh();
	}

	public override bool IsValidForTarget(GameObject target) => (UnityEngine.Object)target.GetComponent<WarpPortal2>() != (UnityEngine.Object)null;

	public override void SetTarget(GameObject target)
	{
		WarpPortal2 component = target.GetComponent<WarpPortal2>();
		if ((UnityEngine.Object)component == (UnityEngine.Object)null)
		{
			Debug.LogError((object)"Target doesn't have a WarpPortal associated with it.");
		}
		else
		{
			this.target = component;
			target.GetComponent<Assignable>().OnAssign += new System.Action<IAssignableIdentity>(this.Refresh);
			if(progressBar)
				this.Refresh();
		}
	}

	private void Update()
	{
		if (!this.progressBar.activeSelf)
			return;
		RectTransform rectTransform = this.progressBar.GetComponentsInChildren<Image>()[1].rectTransform;
		float num = this.target.rechargeProgress / 3000f;
		rectTransform.sizeDelta = new Vector2(rectTransform.transform.parent.GetComponent<LayoutElement>().minWidth * num, 24f);
		this.progressLabel.text = GameUtil.GetFormattedPercent(num * 100f);
	}

	private void OnButtonClick()
	{
		if (!this.target.ReadyToWarp)
			return;
		this.target.StartWarpSequence();
		this.Refresh();
	}

	private void OnCancelClick()
	{
		this.target.CancelAssignment();
		this.Refresh();
	}

	private void Refresh(object data = null)
	{
		this.progressBar.SetActive(false);
		this.cancelButton.gameObject.SetActive(false);
		if ((UnityEngine.Object)this.target != (UnityEngine.Object)null)
		{
			if (this.target.ReadyToWarp)
			{
				this.label.text = (string)STRINGS.UI.UISIDESCREENS.WARPPORTALSIDESCREEN.WAITING;
				this.button.gameObject.SetActive(true);
				this.cancelButton.gameObject.SetActive(true);
			}
			else if (this.target.IsConsumed)
			{
				this.button.gameObject.SetActive(false);
				this.progressBar.SetActive(true);
				this.label.text = (string)STRINGS.UI.UISIDESCREENS.WARPPORTALSIDESCREEN.CONSUMED;
			}
			else if (this.target.IsWorking)
			{
				this.label.text = (string)STRINGS.UI.UISIDESCREENS.WARPPORTALSIDESCREEN.UNDERWAY;
				this.button.gameObject.SetActive(false);
				this.cancelButton.gameObject.SetActive(true);
			}
			else
			{
				this.label.text = (string)STRINGS.UI.UISIDESCREENS.WARPPORTALSIDESCREEN.IDLE;
				this.button.gameObject.SetActive(false);
			}
		}
		else
		{
			this.label.text = (string)STRINGS.UI.UISIDESCREENS.WARPPORTALSIDESCREEN.IDLE;
			this.button.gameObject.SetActive(false);
		}
	}
}
