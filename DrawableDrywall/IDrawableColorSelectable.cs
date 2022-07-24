using UnityEngine;

namespace DrawableDecoration
{
	public interface IDrawableColorSelectable
	{

		Color GetValue(int index);
		void SetValue(int index, Color color);

	}
}
