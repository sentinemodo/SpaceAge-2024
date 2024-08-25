using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Anomalies : Dictionary<string, Anomaly>
	{
		new public Anomaly this[string index]
		{
			get
			{
				try
				{
					return base[index];
				}
				catch (Exception ex)
				{
					throw new KeyNotFoundException("Tried to lookup Anomaly [" + index + "]", ex);
				}
			}
			set { base[index] = value; }
		}
	}
}
