
using Newtonsoft.Json;

using PeterHan.PLib.Options;

namespace FreeResourceBuildings
{
	[RestartRequired]
	[JsonObject(MemberSerialization.OptIn)]
	public class ModConfig
	{

		[JsonProperty]
		[Option("How to get these items", "Defines the cost and time to get that items")]
		public CostOptions HowToGetTheseItems { set; get; }

		[JsonProperty]
		[Option("Discover Items", "If checked will discover all itens usable in each building")]
		public bool DiscoverAllUsableItems { get; set; }


		[JsonProperty]
		[Option("Allows Farmer Shelf", "If checked, you can build Farmer Shelf ", "Farmer Shelf")]
		public bool UseFarmerShelf { set; get; }

		[JsonProperty]
		[Option("Item Limit", "How much items will be generate", "Farmer Shelf")]
		[Limit(1, 10)]
		public int farmerShelfItemsLimit { set; get; }

		[JsonProperty]
		[Option("Elements Limit", "How much kg of element will be generate", "Farmer Shelf", Format = "0 Kg")]
		[Limit(1, 1000)]
		public int farmerShelfElementsLimit { set; get; }




		[JsonProperty]
		[Option("Allows FirstAid Box", "If checked, you can build FirstAid Box ", "Firstaid Box")]
		public bool UseFirstAidBox { set; get; }

		[JsonProperty]
		[Option("Item Limit", "How much items will be generate", "Firstaid Box")]
		[Limit(1, 10)]
		public int farmerFirstAidBoxItemsLimit { set; get; }





		[JsonProperty]
		[Option("Allows Powerbox", "If checked, you can build Free Energy Generator ", "Powerbox")]
		public bool UseFreeEnergyGenerator { set; get; }

		[JsonProperty]
		[Option("Default Powerbox Wattage", "It can changed in the each powerbox", "Powerbox", Format = "G5")]
		[Limit(0, 40000)]
		public int defaultEnergyWattage { set; get; }






		[JsonProperty]
		[Option("Allows Free Gas Source", "If checked, you can build Free Gas Source ", "Free Gas")]
		public bool UseFreeGasSource { set; get; }

		[JsonProperty]
		[Option("Default Flow Rate", "It can changed in the each source", "Free Gas", Format = "0 g/s")]

		[Limit(0, 1000)]
		public int defaultGasFlowRate { set; get; }





		[JsonProperty]
		[Option("Allows Free Liquid Source", "If checked, you can build Free Liquid Source ", "Free Liquid")]
		public bool UseFreeLiquidSource { set; get; }

		[JsonProperty]
		[Option("Default Flow Rate", "It can changed in the each source", "Free Liquid", Format = "0 g/s")]
		[Limit(0, 10000)]
		public int defaultLiquidFlowRate { set; get; }





		[JsonProperty]
		[Option("Allows Improvised Radbolt", "If checked, you can build Free Radbolt Source ", "Free Radbolt")]
		public bool UseFreeRadboltSource { set; get; }

		[JsonProperty]
		[Option("Default Radbolt Threshold", "It can changed in the each source", "Free Radbolt", Format = "0 rads")]
		[Limit(50, 500)]
		public int defaultRadthreshold { set; get; }

		[JsonProperty]
		[Option("Default Radbolt Threshold (per cycle)", "It can changed in the each source", "Free Radbolt", Format = "G5")]
		[Limit(0, 20000)]
		public int defaultRadGeneration { set; get; }




		[JsonProperty]
		[Option("Allows Magic Storage", "If checked, you can build Magic Storage ", "Magic Storage")]
		public bool UseFreeStorage { set; get; }

		[JsonProperty]
		[Option("Allows Trashcan", "If checked, you can build Trashcan ", "Magic Storage")]
		public bool UseTrashcan { set; get; }

		[JsonProperty]
		[Option("Item Limit", "How much items will be generate", "Magic Storage")]
		[Limit(1, 10)]
		public int freeStorageItemsLimit { set; get; }

		[JsonProperty]
		[Option("Elements Limit", "How much kg of element will be generate", "Magic Storage", Format = "0 Kg")]
		[Limit(1, 1000)]
		public int freeStorageElementsLimit { set; get; }




		[JsonProperty]
		[Option("Allows Magic Refrigerator", "If checked, you can build Magic Refrigerator ", "Magic Refrigerator")]
		public bool UseMagicRefrigerator { set; get; }

		[JsonProperty]
		[Option("Item Limit", "How much items will be generate", "Magic Refrigerator")]
		[Limit(1, 10)]
		public int foodStorageItemsLimit { set; get; }




		[JsonProperty]
		[Option("Allows Magic Wardobre", "If checked, you can build Magic Wardobre ", "Magic Wardobre")]
		public bool UseMagicWardobre { set; get; }

		[JsonProperty]
		[Option("Item Limit", "How much items will be generate", "Magic Wardobre")]
		[Limit(1, 10)]
		public int wardobreItemsLimit { set; get; }



		[JsonProperty]
		[Option("Allows Magic Feeder", "If checked, you can build Magic Feeder ", "Magic Feeder")]
		public bool UseMagicFeeder { set; get; }

		[JsonProperty] 
		[Option("Item Limit", "How much items will be generate", "Magic Feeder")]
		[Limit(1, 10)]
		public int magicFeederItemsLimit { set; get; }

		[JsonProperty] 
		[Option("Elements Limit", "How much kg of element will be generate", "Magic Feeder", Format = "0 Kg")]
		[Limit(1, 1000)]
		public int magicFeederElementsLimit { set; get; }



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
			UseMagicFeeder = true;


			DiscoverAllUsableItems = true;
			farmerShelfItemsLimit = 2;
			farmerShelfElementsLimit = 1000;
			farmerFirstAidBoxItemsLimit = 2;
			defaultEnergyWattage = 2000;
			defaultGasFlowRate = 10000;
			defaultLiquidFlowRate = 10000;
			defaultRadthreshold = 50;
			defaultRadGeneration = 600;
			freeStorageItemsLimit = 4;
			freeStorageElementsLimit = 1000;
			foodStorageItemsLimit = 1;
			wardobreItemsLimit = 1;
			magicFeederItemsLimit = 1;
			magicFeederElementsLimit = 1000;


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