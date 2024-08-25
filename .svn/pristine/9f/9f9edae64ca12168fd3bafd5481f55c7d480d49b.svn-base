using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Stars : Dictionary<string, Star>
	{
		new public Star this[string index]
		{
			get
			{
				try
				{
					return base[index];
				}
				catch (Exception ex)
				{
					throw new KeyNotFoundException("Tried to lookup Star [" + index + "]", ex);
				}
			}
			set { base[index] = value; }
		}
	}
}
