using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class LocationType : NamedObject
	{
		public LocationType(string name) : base(name) { }

		private string surfaceType;
		public string SurfaceType
		{
			get { return this.surfaceType; }
			set { this.surfaceType = value; }
		}

        private EMoveMode moveMode = EMoveMode.ground;
        public EMoveMode MoveMode
        {
            get { return this.moveMode; }
            set { this.moveMode = value; }
        }
	}
}
