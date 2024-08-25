using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
    public class SpacePoint : Location, IHolder, IBattleReporting
    {
        public SpacePoint(string name)
            : base(name)
        {
            while (string.IsNullOrEmpty(this.name) || SpacePoint.All.ContainsKey(this.name))
                this.name = this.GenerateRandomIdentifier("S", SpacePoint.OrbitNameLength);
            SpacePoint.All.Add(this.name, this);
        }

        public const int OrbitNameLength = 5;
        public static SpacePoints All = new SpacePoints();

        public override ELocationType LocationType
        {
            get { return ELocationType.space; }
        }

        #region IReporting Members

        public override string ReportName
        {
            get
            {
                return string.Format("space point[{0}]", this.name);
            }
        }

        public override string BattleReportName
        {
            get
            {
                return this.ReportName;
            }
        }

        public override List<string> Report(Faction faction)
        {
            return this.Report(faction, 0);
        }

        public List<string> Report(Faction faction, int level)
        {
            ReportLines reportLines = new ReportLines
            {
                { this.reportHeader(), level }
            };

            // region modulestacks
            foreach (ModuleStack moduleStack in this.ModuleStacks.Values)
            {
                if (!moduleStack.Visible(faction))
                    continue;

                reportLines.Add(moduleStack.Report(faction), level);
            }
            return reportLines.IndentedLines;
        }

        private string reportHeader()
        {
            return string.Concat(this.ReportName, ".");
        }

        #endregion
    }
}
