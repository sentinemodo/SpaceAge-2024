using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Alter : Effect
	{
		private string reason;
		public string Reason
		{
			get { return this.reason; }
			set { this.reason = value; }
		}

		private string change;
		public string Change
		{
			get { return this.change; }
			set { this.change = value; }
		}


		public Alter(IEffectable altered, int duration, string reason, string change)
			: base(altered, duration)
		{
			this.reason = reason;
			this.change = change;
		}

		#region IReporting Members

		new public  List<string> Report(Faction faction)
		{
			List<string> lines = new List<string>();
			return lines;
		}

		#endregion

		public override void Execute(int week)
		{
		}
	}
}
