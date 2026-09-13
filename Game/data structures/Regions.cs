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

		public bool TryGroundDistance(Region region1, Region region2, out int distance)
		{
			distance = 0;
			if (region1 == region2)
			{
				return true;
			}
			if (region1.RegionHolder != region2.RegionHolder)
			{
				return false;
			}

			Dictionary<Region, int> best = new Dictionary<Region, int>();
			Queue<Region> queue = new Queue<Region>();
			best[region1] = 0;
			queue.Enqueue(region1);

			while (queue.Count > 0)
			{
				Region current = queue.Dequeue();
				int currentDistance = best[current];
				foreach (Exit exit in current.Exits)
				{
					Region neighbour = exit.To as Region;
					if (neighbour == null || !exit.ExitModes.ContainsKey(EMoveMode.ground))
					{
						continue;
					}
					int nextDistance = currentDistance + exit.ExitModes[EMoveMode.ground].Duration;
					if (best.ContainsKey(neighbour) && best[neighbour] <= nextDistance)
					{
						continue;
					}
					best[neighbour] = nextDistance;
					if (neighbour == region2)
					{
						distance = nextDistance;
						return true;
					}
					queue.Enqueue(neighbour);
				}
			}

			return false;
		}

		public int DistanceBetween(Region region1, Region region2)
		{
			int distance;
			if (this.TryGroundDistance(region1, region2, out distance))
			{
				return distance;
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
