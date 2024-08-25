using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class SpaceSystemObjects : Dictionary<string, SpaceSystemObject>
	{
		new public SpaceSystemObject this[string index]
		{
			get
			{
				try
				{
					return base[index];
				}
				catch (Exception ex)
				{
					throw new KeyNotFoundException("Tried to lookup SpaceSystemObject [" + index + "]", ex);
				}
			}
			set { base[index] = value; }
		}
	}
}
