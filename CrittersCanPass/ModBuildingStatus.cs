using AnalyzerDoors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ModBuildingStatus
{

	public static StatusItem ChangeDoorControlState;
	public static StatusItem CurrentDoorControlState;
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


		ModBuildingStatus.ChangeDoorControlState = CreateStatusItem("ChangeDoorControlState", "BUILDING", "status_item_pending_switch_toggle", StatusItem.IconType.Custom, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		ModBuildingStatus.ChangeDoorControlState.resolveStringCallback = delegate (string str, object data)
		{
			ICustomDoor door2 = (ICustomDoor)data;
			return str.Replace("{ControlState}", door2.RequestedState.ToString());
		};
		ModBuildingStatus.CurrentDoorControlState = CreateStatusItem("CurrentDoorControlState", "BUILDING", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		ModBuildingStatus.CurrentDoorControlState.resolveStringCallback = delegate (string str, object data)
		{
			ICustomDoor door = (ICustomDoor)data;
			string newValue13 = Strings.Get("STRINGS.BUILDING.STATUSITEMS.CURRENTDOORCONTROLSTATE." + door.CurrentState.ToString().ToUpper());
			return str.Replace("{ControlState}", newValue13);
		};
	}
}
