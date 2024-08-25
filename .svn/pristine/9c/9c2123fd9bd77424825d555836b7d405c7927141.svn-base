using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class SkillTypes : Dictionary<string, SkillType>
	{
		new public SkillType this[string index]
		{
			get
			{
				try
				{
					return base[index];
				}
				catch (Exception ex)
				{
					throw new KeyNotFoundException("Tried to lookup SkillType [" + index + "]", ex);
				}
			}
			set { base[index] = value; }
		}

	}
}
