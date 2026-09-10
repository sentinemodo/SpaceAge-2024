using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class ExitMode
	{
		private EMoveMode mode;
		public EMoveMode Mode
		{
			get { return this.mode; }
			set { this.mode = value; }
		}

		private int duration;
		public int Duration
		{
			get { return this.duration; }
			set { this.duration = value; }
		}

		public string ReportName
		{
			get
			{
				string modeName;
				modeName = MoveModeXml.ToToken(this.mode);
				return string.Format("{0} travel duration {1} {2}", 
					modeName, this.duration, (this.duration == 1) ? "week" : "weeks");
			}
		}
	}
}
