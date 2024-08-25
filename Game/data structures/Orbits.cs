using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Orbits : Dictionary<string, Orbit>
	{
		new public Orbit this[string index]
		{
			get
			{
				try
				{
					return base[index];
				}
				catch (Exception ex)
				{
					throw new KeyNotFoundException("Tried to lookup Orbit [" + index + "]", ex);
				}
			}
			set { base[index] = value; }
		}

		public Orbit this[IOrbitHolder holder]
		{
			get
			{
				foreach (Orbit orbit in this.Values)
				{
					if (orbit.OrbitHolder == holder)
					{
						return orbit;
					}
				}
				return null;
			}
		}

	}
}
