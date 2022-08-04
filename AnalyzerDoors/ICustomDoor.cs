using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzerDoors
{
	public interface ICustomDoor
	{
		Door.ControlState RequestedState { get; }
		Door.ControlState CurrentState { get; }
	}
}
