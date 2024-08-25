using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Resources : List<Resource>
	{
		public bool Contains(ItemType itemType)
		{
			foreach (Resource resource in this)
			{
				if (resource.ItemType == itemType)
				{
					return true;
				}
			}
			return false;
		}

		public string Report
		{
			get
			{
				string line = string.Format("Resources: {0}", this[0].ReportName); 
				if (this.Count > 1)				  
				{
					for (int i = 1; i < this.Count; i++)
					{
						line = string.Format("{0}, {1}", line, this[i].ReportName);
					}
				}
				line = string.Concat(line, ".");
				return line;
			}
		}
	}
}
