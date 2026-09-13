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

		public void AddOrIncrease(ItemType itemType, int quantity)
		{
			if (itemType == null || quantity < 1)
			{
				return;
			}

			foreach (Resource resource in this)
			{
				if (resource.ItemType == itemType)
				{
					resource.Quantity += quantity;
					return;
				}
			}

			Resource added = new Resource();
			added.ItemType = itemType;
			added.Quantity = quantity;
			this.Add(added);
		}

		public string Report
		{
			get
			{
				return this.FormatReport("Resources");
			}
		}

		public string FormatReport(string label)
		{
			if (this.Count < 1)
			{
				return null;
			}

			string line = string.Format("{0}: {1}", label, this[0].ReportName);
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
