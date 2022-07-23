using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using PeterHan.PLib.Options;
using PeterHan.PLib.UI;

namespace LuxuryDecoration
{

	// ----------------------------------------------------------------------------



	public class ModPatches
	{

		[HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
		public static class SideScreenCreator
		{
			internal static void Postfix()
			{
				PUIUtils.AddSideScreenContent<LuxuryWallSideScreen>();

			}
		}
		[HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
		public static class Db_Initialize_Patch
		{

			public static void Postfix()
			{

				AddStrings(LuxuryWallConfig.ID, LuxuryWallConfig.NAME, LuxuryWallConfig.DESC, LuxuryWallConfig.EFFECT);
				ModUtil.AddBuildingToPlanScreen("Utilities", LuxuryWallConfig.ID);
				  Db.Get().Techs.Get("Suits").unlockedItemIDs.Add(LuxuryWallConfig.ID);

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