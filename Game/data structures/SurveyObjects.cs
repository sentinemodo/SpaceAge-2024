using System.Collections.Generic;

namespace SpaceAge
{
	public class SurveyObjects : List<NamedObject>
	{
		public bool Contains(string name)
		{
			foreach (NamedObject spaceObject in this)
			{
				if (spaceObject.Name == name)
				{
					return true;
				}
			}
			return false;
		}

		public List<string> ReportDescriptions(int level)
		{
			ReportLines reportLines = new ReportLines();
			foreach (NamedObject spaceObject in this)
			{
				reportLines.Add(
					string.Format("+ {0}: {1}", spaceObject.ReportName, spaceObject.Description),
					level);
			}
			return reportLines.IndentedLines;
		}
	}
}
