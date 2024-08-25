using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class ModuleTypes : Dictionary<string, ModuleType>
	{

		new public ModuleType this[string index]
		{
			get
			{
				try
				{
					return base[index];
				}
				catch (Exception ex)
				{
					throw new KeyNotFoundException ("Tried to lookup ModuleType [" + index +"]", ex);
				}
			}
			set { base[index] = value; }
		}
	
	}
}
