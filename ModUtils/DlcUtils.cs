using System;

namespace ModUtils
{
	public static class DlcUtils
	{

		public static bool IsSpaceOutActive()
		{
			return DlcManager.IsExpansion1Active();
		}

		public static bool IsFrostyPlanetActive()
		{
			return DlcManager.IsContentSubscribed(DlcManager.DLC2_ID);
		}

		public static bool IsBionicBoosterActive()
		{
			
			return DlcManager.GetActiveDLCIds().Contains(DlcManager.DLC3_ID);
		}

		public static bool IsPrehistoricPlanetActive()
		{

			return DlcManager.GetActiveDLCIds().Contains(DlcManager.DLC4_ID);
		}

	}
}
