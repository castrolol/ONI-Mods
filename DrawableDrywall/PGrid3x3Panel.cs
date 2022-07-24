using DrawableDecoration.UI;
using PeterHan.PLib.UI;
using System.Collections.Generic;

namespace DrawableDecoration
{
	internal class PGrid3x3Panel : PGridPanel
	{
		public PGrid3x3Panel(string name) : base(name)
		{
		}

		public List<Grid3x3Component<IUIComponent>> children { get; set; }
		public List<GridColumnSpec> columns { get; set; }
	}
}