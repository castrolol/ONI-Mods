using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrittersCanPass
{
	public class CritterPassable : KMonoBehaviour 
	{

		[MyCmpReq]
		public Building building;
		 

		protected override void OnSpawn()
		{
			base.OnSpawn();
			 
			foreach (var cell in this.building.PlacementCells)
			{
				Grid.CritterImpassable[cell] = false;// this.controlState != Door.ControlState.Opened;
				Grid.DupeImpassable[cell] = true;
				Grid.FakeFloor.Add(cell);
				Grid.Foundation[cell] = true;
				 
				Pathfinding.Instance.AddDirtyNavGridCell(cell);
			}
		}

	 
	 

	}
}
