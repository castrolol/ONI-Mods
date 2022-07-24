using PeterHan.PLib.Detours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

internal static class UIDetoursClone
{
 

	#region KButton
	public static readonly IDetouredField<KButton, KImage[]> ADDITIONAL_K_IMAGES = PDetours.DetourFieldLazy<KButton, KImage[]>(nameof(KButton.additionalKImages));
	public static readonly IDetouredField<KButton, KImage> BG_IMAGE = PDetours.DetourFieldLazy<KButton, KImage>(nameof(KButton.bgImage));
	public static readonly IDetouredField<KButton, Image> FG_IMAGE = PDetours.DetourFieldLazy<KButton, Image>(nameof(KButton.fgImage));
	public static readonly IDetouredField<KButton, bool> IS_INTERACTABLE = PDetours.DetourFieldLazy<KButton, bool>(nameof(KButton.isInteractable));
	public static readonly IDetouredField<KButton, ButtonSoundPlayer> SOUND_PLAYER_BUTTON = PDetours.DetourFieldLazy<KButton, ButtonSoundPlayer>(nameof(KButton.soundPlayer));
	#endregion

	#region KImage
	public static readonly IDetouredField<KImage, ColorStyleSetting> COLOR_STYLE_SETTING = PDetours.DetourFieldLazy<KImage, ColorStyleSetting>(nameof(KImage.colorStyleSetting));

	public static readonly DetouredMethod<Action<KImage>> APPLY_COLOR_STYLE = typeof(KImage).DetourLazy<Action<KImage>>(nameof(KImage.ApplyColorStyleSetting));
	#endregion
	 
}