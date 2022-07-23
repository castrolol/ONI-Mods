using FreeResourceBuildingsPatches;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUNING;

namespace FreeResourceBuildings
{
	public class ModBuildingDefs
	{
		private static ModBuildingDefs instance;
		public static ModBuildingDefs Instance => instance ?? (instance = new ModBuildingDefs());
		public string[] NEUTRONIUM = { SimHashes.Unobtanium.ToString() };
		public float[] GetWeight(float[] weight)
		{
			if (Mod.Options.HowToGetTheseItems == CostOptions.EasyAndFree)
			{
				return new float[] { 1f };
			}
			return weight;
		}

		public string[] GetResources(params string[] res)
		{
			if (Mod.Options.HowToGetTheseItems == CostOptions.EasyAndFree)
			{
				return MATERIALS.ALL_MINERALS;
			}
			else if (Mod.Options.HowToGetTheseItems == CostOptions.JustSandbox)
			{
				return NEUTRONIUM;
			}
			return res;
		}

		public EffectorValues GetNoise(EffectorValues noise)
		{
			if (Mod.Options.HowToGetTheseItems == CostOptions.EasyAndFree)
			{
				return NOISE_POLLUTION.NONE;
			}

			return noise;
		}
		public EffectorValues GetDecor(EffectorValues decor)
		{
			if (Mod.Options.HowToGetTheseItems == CostOptions.EasyAndFree)
			{
				return BUILDINGS.DECOR.BONUS.TIER5;
			}

			return decor;
		}

		public float GetConstructionTime()
		{
			if (Mod.Options.HowToGetTheseItems == CostOptions.EasyAndFree)
			{
				return 1f;
			}

			return 120f;
		}

		public BuildingDefOptions GetDefaultOptions(float[] mass, EffectorValues noise, EffectorValues decor, string audio, params string[] res)
		{
			return new BuildingDefOptions
			{
				Mass = GetWeight(mass),
				Resources = GetResources(res),
				Noise = GetNoise(noise),
				Decor = GetDecor(decor),
				AudioCategory = audio,
				ConstructionTime = GetConstructionTime(),
			};
		}
		 
	}



	public class BuildingDefOptions
	{
		public float[] Mass { get; set; }

		public string[] Resources { get; set; }

		public EffectorValues Noise { get; set; }

		public EffectorValues Decor { get; set; }

		public string AudioCategory { get; set; }

		public float ConstructionTime { get; set; }
	}

}
