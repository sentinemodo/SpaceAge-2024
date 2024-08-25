using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public abstract class SpaceSystemObject : NamedObject, IReporting, IBattleReporting
	{
		public SpaceSystemObject(string name)
			: base(name)
		{		
		}

        //protected Point3D coordinates = null;
        //public Point3D Coordinates 
        //{
        //    get { return this.coordinates; }
        //    set { this.coordinates = value; }
        //}

		protected SpaceSystem spaceSystem;
		public SpaceSystem SpaceSystem
		{
			get { return this.spaceSystem; }
			set { this.spaceSystem = value; }
		}

		// AU - Astronomical Unit - a distnace form center of the SpaceSystem to the object
        protected double au;
		public double AU
		{
			get { return this.au; }
            set { this.au = value; }
		}

		#region IReporting Members

		public abstract List<string> Report(Faction faction);

		public abstract string BattleReportName
		{
			get;
		}

		#endregion

		public virtual bool Visible(Faction faction)
		{
			return true;
		}

        public double DistanceTo(SpaceSystemObject spaceSystemObject)
        {
            if (this is Planet && spaceSystemObject is Moon && this == ((Moon)spaceSystemObject).Planet)
            {
                return spaceSystemObject.AU;
            }
            else
            {
                throw new Exception("Not implemented");
            }
        }

        public double DistanceTo(Orbit orbit)
        {
            if (orbit.OrbitHolder is Planet)
            {
                return this.DistanceTo((Planet)orbit.OrbitHolder);
            }
            else if (orbit.OrbitHolder is Moon)
            {
                return this.DistanceTo((Moon)orbit.OrbitHolder);
            }
            return 0.002;
        }
    }
}
