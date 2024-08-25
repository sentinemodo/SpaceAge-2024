using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class ReportLine
	{
		public static string IndentationStringStep = "  ";
		public static int LineLength = 100;

		public ReportLine(string line)
			: this(line, 0)
		{
		}

        public ReportLine(string line, int level)
        {
            this.Line = line;
            this.Level = level;
        }

        public string   Line { get; set; }

		private int level = 0;
		public int Level
		{
			get { return this.level; }
			set 
			{
				if (value < 0 || value > ReportLine.LineLength / ReportLine.IndentationStringStep.Length)
				{
					throw new ArgumentOutOfRangeException("ReportLine level out of range");
				}
				this.level = value; 
			}
		}

		public string IndentationString
		{
			get
			{
				string indentationString = "";
				if (level > 0)
				{
					for (int i = 0; i < this.level; i++)
					{
						indentationString = string.Concat(indentationString, ReportLine.IndentationStringStep);
					}
				}
				return indentationString;
			}
		}
		public List<string> IndentedLines
		{
			get
			{
				List<string> lines = new List<string>
                {
                    string.Concat(this.IndentationString, this.Line)
                };
				return lines;				
			}
		}
	}
}
