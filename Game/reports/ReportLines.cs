using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class ReportLines : List<ReportLine>
	{
		public void Add(string line)
		{
			this.Add(new ReportLine(line));
		}

		public void Add(string line, int level)
		{
            this.Add(new ReportLine(line, level));
		}

		public void Add(List<string> lines)
		{
			foreach (string line in lines)
			{
				this.Add(line);
			}
		}
		
		public void Add(List<string> lines, int level)
		{
			foreach (string line in lines)
			{
				this.Add(line, level);
			}
		}

		public List<string> IndentedLines
		{
			get 
			{
				List<string> lines = new List<string>();
				foreach (ReportLine line in this)
				{
					lines.AddRange(line.IndentedLines);
				}
				return lines;
			}
		}
	}
}
