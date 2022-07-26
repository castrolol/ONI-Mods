using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ModBuildingStatus
{

	public static StatusItem WarpPortalCharging;

	private static StatusItem CreateStatusItem(
	   string id,
	   string prefix,
	   string icon,
	   StatusItem.IconType icon_type,
	   NotificationType notification_type,
	   bool allow_multiples,
	   HashedString render_overlay,
	   bool showWorldIcon = true,
	   int status_overlays = 129022)
	{
		return Db.Get().BuildingStatusItems.Add(new StatusItem(id, prefix, icon, icon_type, notification_type, allow_multiples, render_overlay, showWorldIcon, status_overlays));
	}


	public static void CreateStatusItems()
	{
		ModBuildingStatus.WarpPortalCharging = ModBuildingStatus.CreateStatusItem("WarpPortal2Charging", "BUILDING", "", StatusItem.IconType.Info, NotificationType.Neutral, false, OverlayModes.None.ID, false);
		ModBuildingStatus.WarpPortalCharging.resolveStringCallback = (Func<string, object, string>)((str, data) =>
		{
			str = str.Replace("{charge}", GameUtil.GetFormattedPercent((float)(100.0 * ((double)((WarpPortal2)data).rechargeProgress / 3000.0))));
			return str;
		});
		ModBuildingStatus.WarpPortalCharging.resolveTooltipCallback = (Func<string, object, string>)((str, data) =>
		{
			str = str.Replace("{cycles}", string.Format("{0:0.0}", (object)(float)((3000.0 - (double)((WarpPortal2)data).rechargeProgress) / 600.0)));
			return str;
		});
	}
}
