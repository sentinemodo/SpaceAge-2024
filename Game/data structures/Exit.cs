using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Exit
	{
		private IHolder to;
        public IHolder To
		{
			get { return this.to; }
			set { this.to = value; }
		}

		private ExitModes exitModes = new ExitModes();
		public ExitModes ExitModes
		{
			get { return this.exitModes; }
		}
		
		//TODO: duration calculation
	}
}
