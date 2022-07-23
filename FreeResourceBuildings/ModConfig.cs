using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace FreeResourceBuildings
{
	[RestartRequired]
	[JsonObject(MemberSerialization.OptIn)]
	public class ModConfig
	{
		[Option("How to get these items", "Defines the cost and time to get that items")]
		[JsonProperty]
		public CostOptions HowToGetTheseItems { set; get; }

		
		

		[Option("Allows Farmer Shelf", "If checked, you can build Farmer Shelf ")]
		public bool UseFarmerShelf { set; get; }

		[Option("Allows FirstAid Box", "If checked, you can build FirstAid Box ")]
		public bool UseFirstAidBox { set; get; }
	
		[Option("Allows FreeEnergyGenerator", "If checked, you can build Free Energy Generator ")]
		public bool UseFreeEnergyGenerator { set; get; }
		
		[Option("Allows FreeGasSource", "If checked, you can build Free Gas Source ")]
		public bool UseFreeGasSource { set; get; }
		
		[Option("Allows FreeLiquidSource", "If checked, you can build Free Liquid Source ")]
		public bool UseFreeLiquidSource { set; get; }
		
		[Option("Allows FreeRadboltSource", "If checked, you can build Free Radbolt Source ")]
		public bool UseFreeRadboltSource { set; get; }

		[Option("Allows Magic Storage", "If checked, you can build Magic Storage ")]
		public bool UseFreeStorage { set; get; }

		[Option("Allows MagicRefrigerator", "If checked, you can build Magic Refrigerator ")]
		public bool UseMagicRefrigerator { set; get; }

		[Option("Allows MagicWardobre", "If checked, you can build Magic Wardobre ")]
		public bool UseMagicWardobre { set; get; }

		[Option("Allows Trashcan", "If checked, you can build Trashcan ")]
		public bool UseTrashcan { set; get; }


		/// </summary>
		public ModConfig()
		{
			HowToGetTheseItems = CostOptions.EarlyGame;
			UseFarmerShelf = true;
			UseFirstAidBox = true;
			UseFreeEnergyGenerator = true;
			UseFreeGasSource = true;
			UseFreeLiquidSource = true;
			UseFreeRadboltSource = true;
			UseFreeStorage = true;
			UseMagicRefrigerator = true;
			UseMagicWardobre = true;
			UseTrashcan = true;

		}
	}
}

public enum CostOptions
{
	[Option("Easy and Free!", "The items will be available at the beginning of the game, for free!")]
	EasyAndFree,
	[Option("Easy", "The items will be available at the beginning of the game")]
	Easy,
	[Option("Early Game", "The items will be available after a little research")]
	EarlyGame,
	[Option("Mid Game", "The items will be available after advanced research")]
	MidGAme,
	[Option("Late Game", "The items will be available after late game research")]
	LateGame,
	[Option("Just in sandbox", "The items just will be available in sandbox (impossible materials)")]
	JustSandbox
}