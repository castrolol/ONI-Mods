using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxuryDecoration
{
	public interface ILuxuryWallSelector
	{
		List<string> GetItems(int index);

		string GetValue(int index);

		void SetValue(int index, string value);
	}
}
