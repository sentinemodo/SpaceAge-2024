using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Moons : Dictionary<string, Moon>
	{
		new public Moon this[string index]
		{
			get
			{
				try
				{
					return base[index];
				}
				catch (Exception ex)
				{
					throw new KeyNotFoundException("Tried to lookup Moon [" + index + "]", ex);
				}
			}
			set { base[index] = value; }
		}

		public Moons this[Planet planet]
		{
			get
			{
				Moons list = new Moons();
				foreach (Moon moon in this.Values)
				{
					if (moon.Planet == planet)
					{
						list.Add(moon.Name, moon);
					}
				}
				return list;
			}
		}

	}
}
