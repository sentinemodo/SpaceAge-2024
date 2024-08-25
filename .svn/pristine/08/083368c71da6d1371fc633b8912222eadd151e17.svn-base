using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Regions : Dictionary<string, Region>
	{
		public bool AreNeighbouring(Region region1, Region region2)
		{
			if (region1.Exits.Contains(region2))
			{
				return true;
			}
			return false;
		}

		public int DistanceBetween(Region region1, Region region2)
		{
			if (region1 == region2) 
			{
				return 0;
			} else if (this.AreNeighbouring(region1, region2)) 
			{
				Exit exit = region1.Exits[region2];
				return exit.ExitModes[EMoveMode.ground].Duration;
			} 
			throw new Exception("Not implemented");
		}

		new public Region this[string index]
		{
			get
			{
				try
				{
					return base[index];
				}
				catch (Exception ex)
				{
					throw new KeyNotFoundException("Tried to lookup Region [" + index + "]", ex);
				}
			}
			set { base[index] = value; }
		}

		public Regions this[IRegionHolder holder]
		{
			get
			{
				Regions list = new Regions();
				foreach (Region region in this.Values)
				{
					if (region.RegionHolder == holder)
					{
						list.Add(region.Name, region);
					}
				}
				return list;
			}
		}

	}
}
