using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class RegionType	: NamedObject
	{
		public RegionType(string name) : base(name) { }

		private ELocationType locationType = ELocationType.solidSurface;
        public ELocationType LocationType
		{
			get { return this.locationType; }
            set { this.locationType = value; }
		}
	}
}
