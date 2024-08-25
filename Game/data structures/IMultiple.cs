using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public interface IMultiple : ISingular
	{
		string FullNameMultiple		{ get; set; }
		string ReportNameMultiple	{ get; }
	}
}
