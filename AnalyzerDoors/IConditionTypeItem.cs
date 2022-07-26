namespace AnalyzerDoors
{
	public interface IConditionTypeItem
	{
		bool Equals(object obj);
		string GetProperName();
	}
}