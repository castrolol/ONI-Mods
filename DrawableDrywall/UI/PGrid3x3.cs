
using DrawableDecoration.UI;
using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using UnityEngine;



public class PGrid3x3 : PContainer, IDynamicSizable
{
	/// <summary>
	/// The number of columns currently defined.
	/// </summary>
	public int Columns => columns.Count;

	public bool DynamicSize { get; set; }

	/// <summary>
	/// The number of rows currently defined.
	/// </summary>
	public int Rows => rows.Count;

	/// <summary>
	/// The children of this panel.
	/// </summary>
	private readonly ICollection<Grid3x3Component<IUIComponent>> children;

	/// <summary>
	/// The columns in this panel.
	/// </summary>
	private readonly IList<GridColumnSpec> columns;

	/// <summary>
	/// The rows in this panel.
	/// </summary>
	private readonly IList<GridRowSpec> rows;

	public PGrid3x3() : this(null) { }

	public PGrid3x3(string name) : base(name ?? "GridPanel")
	{
		children = new List<Grid3x3Component<IUIComponent>>(9);
		columns = new List<GridColumnSpec>(9);
		rows = new List<GridRowSpec>(9);
		DynamicSize = true;
		Margin = null;
	}

	/// <summary>
	/// Adds a child to this panel.
	/// </summary>
	/// <param name="child">The child to add.</param>
	/// <param name="spec">The location where the child will be placed.</param>
	/// <returns>This panel for call chaining.</returns>
	public PGrid3x3 AddChild(IUIComponent child, GridComponentSpec spec)
	{
		if (child == null)
			throw new ArgumentNullException(nameof(child));
		if (spec == null)
			throw new ArgumentNullException(nameof(spec));
		children.Add(new Grid3x3Component<IUIComponent>(spec, child));
		return this;
	}

	/// <summary>
	/// Adds a column to this panel.
	/// </summary>
	/// <param name="column">The specification for that column.</param>
	/// <returns>This panel for call chaining.</returns>
	public PGrid3x3 AddColumn(GridColumnSpec column)
	{
		if (column == null)
			throw new ArgumentNullException(nameof(column));
		columns.Add(column);
		return this;
	}

	/// <summary>
	/// Adds a handler when this panel is realized.
	/// </summary>
	/// <param name="onRealize">The handler to invoke on realization.</param>
	/// <returns>This panel for call chaining.</returns>
	public PGrid3x3 AddOnRealize(PUIDelegates.OnRealize onRealize)
	{
		OnRealize += onRealize;
		return this;
	}

	/// <summary>
	/// Adds a row to this panel.
	/// </summary>
	/// <param name="row">The specification for that row.</param>
	/// <returns>This panel for call chaining.</returns>
	public PGrid3x3 AddRow(GridRowSpec row)
	{
		if (row == null)
			throw new ArgumentNullException(nameof(row));
		rows.Add(row);
		return this;
	}

	public override GameObject Build()
	{
		if (Columns < 1)
			throw new InvalidOperationException("At least one column must be defined");
		if (Rows < 1)
			throw new InvalidOperationException("At least one row must be defined");
		var panel = PUIElements.CreateUI(null, Name);
		SetImage(panel);
		// Add layout component
		var layout = panel.AddComponent<PGridLayoutGroup>();
		layout.Margin = Margin;
		foreach (var column in columns)
			layout.AddColumn(column);
		foreach (var row in rows)
			layout.AddRow(row);
		// Add children
		foreach (var child in children)
			layout.AddComponent(child.Item.Build(), child);
		if (!DynamicSize)
			layout.LockLayout();
		layout.flexibleWidth = FlexSize.x;
		layout.flexibleHeight = FlexSize.y;
		InvokeRealize(panel);
		return panel;
	}

	public override string ToString()
	{
		return string.Format("PGridPanel[Name={0},Rows={1:D},Columns={2:D}]", Name, Rows,
			Columns);
	}
}
