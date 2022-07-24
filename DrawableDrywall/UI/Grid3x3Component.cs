using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawableDecoration.UI
{
	public sealed class Grid3x3Component<T> : GridComponentSpec where T : class
	{
		/// <summary>
		/// The object to place here.
		/// </summary>
		public T Item { get; }

		public Grid3x3Component(GridComponentSpec spec, T item) : base(3, 3)
		{
			Alignment = spec.Alignment;
			Item = item;
			Column = spec.Column;
			ColumnSpan = spec.ColumnSpan;
			Margin = spec.Margin;
			Row = spec.Row;
			RowSpan = spec.RowSpan;
		}
	}
}