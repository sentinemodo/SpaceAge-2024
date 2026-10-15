using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace SpaceAge
{
	public partial class CatalogLoader
	{
		private readonly DataFile dataFile;

		public CatalogLoader(DataFile dataFile)
		{
			this.dataFile = dataFile;
		}

		public void LoadItems(XmlDocument confDocument, Game game, bool loadStub)
		{
			if (loadStub)
			{
				this.loadItemStubs(confDocument, game);
			}
			else
			{
				this.loadItemFill(confDocument, game);
			}
		}

		private static bool isPercentProduceValue(string value)
		{
			if (string.IsNullOrEmpty(value) || !value.EndsWith("%"))
			{
				return false;
			}
			string digits = value.Substring(0, value.Length - 1);
			int percent;
			return int.TryParse(digits, out percent);
		}
	}
}
