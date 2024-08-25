using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Races : Dictionary<string, Race>
	{

		new public Race this[string index]
		{
			get
			{
				try
				{
					return base[index];
				}
				catch (Exception ex)
				{
					throw new KeyNotFoundException ("Tried to lookup Race by [" + index +"]", ex);
				}
			}
			set { base[index] = value; }
		}
	
	}
}
