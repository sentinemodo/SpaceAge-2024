using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace SpaceAge
{
    public class ReportWriter
    {

        private Game game;
        public Game Game
        {
            get { return this.game; }
        }

				private DataFile dataFile;
				public DataFile DataFile
				{
					get { return this.dataFile; }
				}

				public ReportWriter(Game game, DataFile dataFile, string turnDir)
				{
					this.game = game;
					this.dataFile = dataFile;
					this.turnDir = turnDir;
				}

        private int maxLineLength = ReportLine.LineLength;

        public int Generate()
        {
            return 0;
        }

				private string turnDir;
				public string TurnDir
				{
					get { return this.turnDir; }
					set { this.turnDir = value; }
				}

				public void GenerateReports()
				{
					this.GenerateReports(this.turnDir);
				}

				public void GenerateReports(string turnDir)
				{
					if (string.IsNullOrEmpty(turnDir))
					{
						throw new ArgumentNullException();
					}

					DirectoryInfo directoryInfo = new DirectoryInfo(turnDir);

					foreach (Faction faction in this.game.Factions.Values)
					{
						this.GenerateFactionReport(faction, turnDir, String.Format("report.{0}.{1}", this.game.Turn, faction.Name));
						// promote technologies shown this turn to the faction's known set
						faction.AllShown();
					}
				}

        private TextWriter textWriter;

				public void GenerateFactionReport(Faction faction, string turnDir, string reportFileName)
				{
					this.InitiateTextWriter(faction, turnDir, reportFileName);

					// Header
					this.Write("To: " + faction.Email);
					this.Write(String.Format("Subject: [SpaceAge] Report for turn {0}", Game.Turn));
					this.Write("Content-Disposition: attachment");
					this.Write();
					this.Write(String.Format("SpaceAge report for {0}.", faction.ReportName));
					this.Write(String.Format("Turn {0}, {1}.", this.game.Turn, this.game.Date));
					this.Write();

					// Engine
					this.Write(String.Format("SpaceAge Engine Version: {0}.", Program.EngineVersion));
					this.Write();

					// game events reports
					this.Write("Events during turn:");
					this.Write("  none.");
					this.Write();

					// faction events and data (ends with the bank report)
					this.Write(faction);

					// technology reports (between the bank report and the galaxy report)
					if (faction.TechnologiesToShow.Count > 0)
					{
						this.Write();
						this.Write("Technology reports:");
						this.Write(faction.TechnologiesToShow.ReportDescriptions(faction, 0));
					}

					// battles reports
					if (this.game.Battles.Count > 0)
					{
						this.Write();
						this.Write(this.game.Battles, faction);
					}

					// galaxy reports (includes the per-location market section)
					this.Write(this.game.Galaxy, faction);

					// Orders template
					this.WriteOrdersTemplate(faction);

					this.Write();
					this.textWriter.Close();

					if (faction.Options.XmlReport)
					{
						// XML report
						this.dataFile.SaveGame(this.turnDir, string.Concat(reportFileName, ".xml"), faction);
					}
				}

				private void InitiateTextWriter(Faction faction, string turnDir, string reportFileName)
				{
					string reportFileNameWithDir = Path.Combine(turnDir, string.Concat(reportFileName, ".txt"));
					this.textWriter = new StreamWriter(reportFileNameWithDir, false, System.Text.Encoding.GetEncoding(1251));
					this.maxLineLength = faction.Options.ReportLineLength;
				}

		private void WriteOrdersTemplate(Faction faction)
		{
			this.Write("Orders Template:");
			this.Write(String.Format("#faction {0} \"{1}\"", faction.Name, faction.Password));
			this.Write(faction.Orders, faction);

			ModuleStacks moduleStacks = ModuleStack.All[faction];
			List<ModuleStack> moduleStackSorted = new List<ModuleStack>(moduleStacks.Values);
			moduleStackSorted.Sort(ModuleStacks.CompareByNames);

			foreach (ModuleStack moduleStack in moduleStackSorted)
			{
				this.Write(string.Concat("#modulestack ", moduleStack.Name));
				this.Write(moduleStack.ReportOrdersTemplateHeader(faction), false, true);
				this.Write(moduleStack.Orders, faction, false);
				this.Write();
			}

			People people = Person.All[faction];
			List<Person> peopleSorted = new List<Person>(people.Values);
			peopleSorted.Sort(People.CompareByNames);

			foreach (Person person in peopleSorted)
			{
				this.Write(string.Concat("#person ", person.Name));
				this.Write(person.ReportOrdersTemplateHeader(faction), false, true);
				this.Write(person.Orders, faction, false);
				this.Write();
			}
			this.Write("#end");
		}

        public void Write(Faction faction)
        {
            this.Write(faction, faction);
        }

		public void Write(IReporting reportingObject, Faction faction, bool comment = false)
		{
			foreach (string reportLine in reportingObject.Report(faction))
			{
				this.Write(reportLine, comment);
			}
		}

        public void Write()
        {
            this.Write("");
        }

		public void Write(List<string> lines, bool indent = false, bool comment = false)
		{
			foreach (string reportLine in lines)
			{
				this.Write(reportLine, comment);
			}
		}

        public bool ShouldIndent(string line)
        {
            string trimmed = line.Trim();
            bool indent = false;
            if (trimmed.Length > 0)
            {
                if ((trimmed[0] == '+' | trimmed[0] == '-' | trimmed[0] == '*') & trimmed[1] == ' ')
                {
                    indent = true;
                }
            }
            return indent;
        }

        public void Write(string line, bool comment = false)
        {
            // Count indent to wrap
            string indentString = string.Empty;
            int i = 0;

            while (i < line.Length && line[i] == ' ')
            {
                indentString = string.Concat(indentString, " ");
                i++;
            }

            // Write by lines
            while (true)
            {
                int j = this.maxLineLength;
                while (line.Length > j && line[j] != ' ') j--;
                if (line.Length > j)
                {
                    this.textWriter.WriteLine(
                        string.Concat(
                            (comment) ? "; " : "",
                            line.Substring(0, j)));
                    if (this.ShouldIndent(line))
                    {
                        line = string.Concat(
                            ReportLine.IndentationStringStep,
                            indentString,
                            line.Substring(j + 1));
                    }
                    else
                    {
                        line = string.Concat(
                            indentString,
                            line.Substring(j + 1));
                    }
                }
                else
                {
                    this.textWriter.WriteLine(
                        string.Concat(
                            (comment) ? "; " : "",
                            line));
                    break;
                }
            }
            this.textWriter.Flush();
        }

    }
}
