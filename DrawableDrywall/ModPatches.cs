using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using PeterHan.PLib.Options;
using PeterHan.PLib.UI;

namespace DrawableDecoration
{

	// ----------------------------------------------------------------------------



	public class ModPatches
	{

		[HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
		public static class SideScreenCreator
		{
			internal static void Postfix()
			{
				PUIUtils.AddSideScreenContent<DrawableWallSideScreen>();

			}
		}

		[HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
		public static class Db_Initialize_Patch
		{

			public static void Postfix()
			{

				AddStrings(DrawableWallConfig.ID, DrawableWallConfig.NAME, DrawableWallConfig.DESC, DrawableWallConfig.EFFECT);
				ModUtil.AddBuildingToPlanScreen("Utilities", DrawableWallConfig.ID);
				Db.Get().Techs.Get("Suits").unlockedItemIDs.Add(DrawableWallConfig.ID);

			}

			private static void AddStrings(string ID, string Name, string Description, string Effect)
			{
				Strings.Add(new string[]{
					"STRINGS.BUILDINGS.PREFABS." + ID.ToUpperInvariant() + ".NAME", Name
			});

				Strings.Add(new string[]{
					"STRINGS.BUILDINGS.PREFABS." + ID.ToUpperInvariant() + ".DESC",Description
			});

				Strings.Add(new string[]{
					"STRINGS.BUILDINGS.PREFABS." + ID.ToUpperInvariant() + ".EFFECT", Effect
			});

			}


		}





	}

}