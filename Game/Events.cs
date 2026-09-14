using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Events
	{
		public int Execute()
		{
			FaunaRumors.IssueAll();
			return 0;
		}
	}
}
