using System;
using System.Collections.Generic;

namespace SpaceAge
{
	public class Belts : Dictionary<string, Belt>
	{
		new public Belt this[string index]
		{
			get
			{
				try
				{
					return base[index];
				}
				catch (Exception ex)
				{
					throw new KeyNotFoundException("Tried to lookup Belt [" + index + "]", ex);
				}
			}
			set { base[index] = value; }
		}

		public Belts this[SpaceSystem system]
		{
			get
			{
				Belts list = new Belts();
				foreach (Belt belt in this.Values)
				{
					if (belt.SpaceSystem == system && belt.Planet == null)
					{
						list.Add(belt.Name, belt);
					}
				}
				return list;
			}
		}

		public Belts this[Planet planet]
		{
			get
			{
				Belts list = new Belts();
				foreach (Belt belt in this.Values)
				{
					if (belt.Planet == planet)
					{
						list.Add(belt.Name, belt);
					}
				}
				return list;
			}
		}
	}
}
