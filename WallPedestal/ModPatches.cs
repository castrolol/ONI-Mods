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

		 
		[HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
		public static class Db_Initialize_Patch
		{

			public static void Postfix()
			{ 

				AddStrings(WallItemFrameConfig.ID, WallItemFrameConfig.NAME, WallItemFrameConfig.DESC, WallItemFrameConfig.EFFECT);
				ModUtil.AddBuildingToPlanScreen("Furniture", WallItemFrameConfig.ID);
				Db.Get().Techs.Get("Artistry").unlockedItemIDs.Add(WallItemFrameConfig.ID);

			}

			private static void AddStrings(string iD, object nAME, object dESC, object eFFECT)
			{
				throw new NotImplementedException();
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