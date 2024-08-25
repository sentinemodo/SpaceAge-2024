using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public interface ISingular
	{
        string Name         { get; }
        string Alias        { get; }
        string FullName		{ get; set; }
		string ReportName	{ get; }
	}
}
