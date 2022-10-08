using FreeResourceBuildingsPatches;
using KSerialization;
using STRINGS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FreeResourceBuildings
{
	[SerializationConfig(MemberSerialization.OptIn)]
	public class FreeSourceLiquid : FreeSource
	{
		[SerializeField]
		[Serialize]
		public override float Flow
		{ get; set; } = Mod.Options.defaultLiquidFlowRate;
		 
	}
}
