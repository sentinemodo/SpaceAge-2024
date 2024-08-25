using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Anomaly : SpaceSystemObject, IOrbitHolder, IBattleReporting
	{
		public Anomaly(string name)
			: base(name)
		{		
			while (string.IsNullOrEmpty(this.name) || Anomaly.All.ContainsKey(this.name))
				this.name = this.GenerateRandomIdentifier("A", Anomaly.AnomalyNameLength);
			Anomaly.All.Add(this.name, this);
		}

		public const int AnomalyNameLength = 5;
		public static Anomalies All = new Anomalies();

		protected Point3D coordinates = new Point3D();
		public Point3D Coordinates
		{
			get { return this.coordinates; }
			set { this.coordinates = value; }
		}

		public Orbit Orbit
		{
			get { return Orbit.All[this]; }
		}

		public override string BattleReportName
		{
			get
			{
				return "Space anomaly [A00001] (0, 0, 0)";
			}
		}

        public override List<string> Report(Faction faction)
        {
            throw new NotImplementedException();
        }
    }
}
