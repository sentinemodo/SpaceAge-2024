using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class SpaceSystem : NamedObject, IReporting
	{
		public SpaceSystem(string name)
			: base(name)
		{		
		}

		private Point3D coordinates = new Point3D();
		public Point3D Coordinates 
		{
			get { return this.coordinates; }
			set { this.coordinates = value; }
		}

		private SpaceSystemObjects objects = new SpaceSystemObjects();
		public SpaceSystemObjects Objects
		{
			get { return this.objects; }
			set { this.objects = value; }
		}

		#region IReporting Members

		public override string ReportName
		{
			get
			{
				return string.Format("system {0} [{1}] ({2}, {3}, {4})",
					this.FullName,
					this.Name,
					this.Coordinates.X,
					this.Coordinates.Y,
					this.Coordinates.Z);
			}
		}

		public List<string> Report(Faction faction)
		{
			List<string> reportLines = new List<string>();

			reportLines.AddRange(this.reportHeader(faction));
			// inside spacesystems reports (quests)
			// out of spacesystem visible events and objects - stars, detected objects, planet explosion

			reportLines.AddRange(this.reportSpaceSystemObjects(faction));
			reportLines.Add("");

			return reportLines;
		}

		private List<string> reportHeader(Faction faction)
		{
			List<string> lines = new List<string>();
			lines.Add("* Sol [SS0001] (0, 0, 0), star system.");
			lines.Add("------------------------------------------------------------");
			return lines;
		}

		private List<string> reportSpaceSystemObjects(Faction faction)
		{
			List<string> lines = new List<string>();
			foreach (SpaceSystemObject spaceSystemObject in this.Objects.Values)
			{
				if (spaceSystemObject.Visible(faction))
				{
					lines.AddRange(spaceSystemObject.Report(faction));
				}
			}

			return lines;
		}		

		#endregion

		public bool Visible(Faction faction) 
		{
			return true;
		}
	}
}
