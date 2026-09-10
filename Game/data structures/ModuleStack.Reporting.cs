using System;
using System.Collections.Generic;

namespace SpaceAge
{
	public partial class ModuleStack
	{
		#region report

		#region IReporting Members

		public List<string> Report(Faction faction)
		{
			return this.Report(faction, 0);
		}

		#endregion

		protected string alias = string.Empty;
		public override string Alias
		{
			get 
            {
                if (this.Owner != null)
                {
                    return string.Format("{0}_{1}",
                        this.owner.Name,
                        this.alias == string.Empty ? this.name : this.alias);
                } else
                {
                    return string.Format("{1}",
                        this.alias == string.Empty ? this.name : this.alias);
                }
            }
			set { this.alias = value; }
		}
        
		public override string ReportName
		{
			get 
			{
				if (!this.IsFormed)
				{
					return string.Format("{0} [{1}]",
						"empty stack",
						this.name);
				}
				return string.Format("{0} [{1}]",
					string.IsNullOrEmpty(this.FullName) ? this.moduleType.FullName : this.FullName,
					this.name);
			}
		}

		public string BattleReportName
		{
			get { return this.ReportName; }
		}

		public string ReportHeader(Faction faction)
		{
			string line;
			if (this.Quantity > 1)
			{
				line = string.Format("{0} {1}, {2} {3}", 
					(this.owner == faction) ? "+" : "-", 
					this.ReportName, 
					this.Quantity, 
					this.moduleType.ReportNameMultiple);
			}
			else
			{
                if (this.IsFormed)
                {
                    line = string.Format("{0} {1}, {2}",
                        (this.owner == faction) ? "+" : "-",
                        this.ReportName,
                        this.moduleType.ReportName);
                }
                else
                {
                    line = string.Format("{0} {1}, {2}",
                         (this.owner == faction) ? "+" : "-",
						"empty stack",
						this.name);
                }
			}
			if (!this.IsActive)
			{
				line = this.reportActive(line);
			}
			line = string.Format("{0}{1}", line, this.IsImmobile ? ", immobile" : "");
			// IsArmed is used for combat logic but not shown in reports

            if (this.Owner != null & this.Owner != faction)
            {
                line = string.Concat(line, ", owned by ", this.Owner.ReportName);
            }

			return string.Concat(line, ".");
		}

		private string reportDetails(Faction faction)
		{
			string line = "";

			line = this.reportSize(line);
			line = this.reportMass(faction, line);
			line = this.reportCapacity(faction, line);
            line = this.reportResearchPoints(faction, line);
            line = this.reportEnergyUsage(faction, line);
			line = this.reportCrew(faction, line);
			line = this.reportUpkeep(faction, line);
			line = this.reportConsume(faction, line);

			return string.Concat(line, ".");
		}

		private string reportConsume(Faction faction, string line)
		{
			if (this.owner == faction && this.Consume.Count > 0)
			{
				line = string.Format("{0}consume:",
					(line == string.Empty) ? string.Empty : string.Concat(line, ", "));

				int netto;
				bool firstAdded = false;
				foreach (ItemStack itemstack in this.Consume.Values)
				{
					netto = this.ConsumeNetto.Quantity(itemstack.ItemType);
					line = string.Format("{0} {1}{2}{3}",
						(firstAdded == true) ? string.Concat(line, ",") : line,
						(itemstack.Quantity > 1) ? string.Concat(itemstack.Quantity, " ") : "",
						(netto != itemstack.Quantity) ? string.Concat("(", netto, ") ") : "",
						(netto > 1 || itemstack.Quantity > 1) ? itemstack.ItemType.ReportNameMultiple : itemstack.ItemType.ReportName);

					firstAdded = true;
				}
			}
			return line;
		}

        private string reportProduceEnergyConsume(string line)
        {
            line = string.Empty;
            if (this.ProduceEnergyConsume.Count > 0)
            {
                line = string.Format(" (consume: {0} for {1} weeks)",
                    this.ProduceEnergyConsume.ReportList,
                    this.ModuleType.ProduceDuration);
            }
            return line;
        }
        
        private string reportUpkeep(Faction faction, string line)
		{
			if (this.owner == faction && this.modules.Count > 0 && this.Upkeep.Count > 0)
			{
				line = string.Format("{0}upkeep:",
					(line == string.Empty) ? string.Empty : string.Concat(line, ", "));

				int netto;
				bool firstAdded = false;
				foreach (ItemStack itemstack in this.Upkeep.Values)
				{
					netto = this.UpkeepNetto.Quantity(itemstack.ItemType);
					line = string.Format("{0} {1}{2}{3}",
						(firstAdded == true) ? string.Concat(line, ",") : line,
						(itemstack.Quantity > 1) ? string.Concat(itemstack.Quantity, " ") : "",
						(netto != itemstack.Quantity) ? string.Concat("(", netto, ") ") : "",
						(netto > 1 || itemstack.Quantity > 1) ? itemstack.ItemType.ReportNameMultiple : itemstack.ItemType.ReportName);

					firstAdded = true;
				}
			}
			return line;
		}

		private string reportCapacity(Faction faction, string line)
		{
			if (this.Capacity > 0)
			{
				if (this.owner == faction | this.ModuleStacks.Contains(faction))
				{
					line = string.Format("{0}, capacity: {1}/{2}", line, this.Capacity, this.CapacityUsed);
				}
			}
			return line;
		}

		public string reportMoveModes()
		{
			string line = "movement speed:";			
			foreach (MoveMode moveMode in this.moduleType.MoveModes.Values) 
			{
				string fragment;
				if (moveMode.Mode == EMoveMode.ground)
				{
					fragment = string.Concat(moveMode.Speed.ToString("F1"), " on ground");
				}
				else if (moveMode.Mode == EMoveMode.naval)
				{
					fragment = string.Concat(moveMode.Speed.ToString("F1"), " naval");
				}
				else
				{
					fragment = string.Concat((moveMode.MassCapacity / this.Mass).ToString("F1"), " in space");
				}
                line = string.Format("{0} {1}", line, fragment);
			}
			line = string.Concat(line, ".");
			return line;
		}

        public string reportFuel()
        {
            string line = "fuel requirements:";
            bool firstAdded = false;

            foreach (ItemStack fuel in this.Fuel.Values)
            {
                line = string.Format("{0} {1}", line, (firstAdded) ? ", " : "");
                firstAdded = true;

                if (fuel.Quantity > 1)
                {
                    line = string.Format("{0}{1} {2}", line, fuel.Quantity, fuel.ItemType.ReportNameMultiple);
                }
                else
                {
                    line = string.Concat(line, " ", fuel.ItemType.ReportName);
                }
            }
            
            if (this.ModuleType.FuelDuration > 1)
            {
                line = string.Format("{0} per {1} weeks", line, this.ModuleType.FuelDuration);
            }
            else
            {
                line = string.Concat(line, " per week");
            }

            line = string.Concat(line, ".");
            return line;
        }

        private string reportResearchPoints(Faction faction, string line)
        {
            if (this.ResearchPoints > 0)
            {
                if (this.owner == faction)
                {
                    line = string.Format("{0}, research points: {1}", line, this.ResearchPoints);
                }
            }
            return line;
        }

        public List<string> Report(Faction faction, int level)
		{
			ReportLines reportLines = new ReportLines
            {
                { this.ReportHeader(faction), level },
                { this.reportDetails(faction), level + 1 }
            };

            if (this.owner == faction && this.modules.HasPersistedState)
            {
                string hitPoints = string.Empty;
                hitPoints = this.reportOwnHitPoints(hitPoints);
                reportLines.Add(string.Concat(hitPoints, "."), level + 1);
                reportLines.Add(this.modules.Report(faction), level + 2);
            }

            if (this.IsFormed)
            {
                if (this.owner == faction && this.moduleType.MoveModes.Count > 0)
                {
                    reportLines.Add(this.reportMoveModes(), level + 1);
                }

                if (this.owner == faction && this.moduleType.Fuel.Count > 0)
                {
                    reportLines.Add(this.reportFuel(), level + 1);
                }
            }

            if (this.owner == faction && this.technologies.Count > 0)
			{
				reportLines.Add(this.technologies.Report(faction, level + 1));
			}

			if (this.owner == faction && this.itemStacks.Count > 0)
			{
				reportLines.Add(this.itemStacks.Report(faction, level + 1));
			}

			if (this.owner == faction && this.effects.Count > 0)
			{
				reportLines.Add(this.effects.Report(faction, level + 1));
			}

			if (this.owner == faction && this.eventReports.Count > 0)
			{
				reportLines.Add(this.eventReports.Report(faction, level + 1));
			}
	
			foreach (Person person in this.People.Values) 
			{
				if (person.Visible(faction))
				{
					reportLines.Add(person.Report(faction, level + 1));
				}
			}	
			
			// submodulestacks
			foreach (ModuleStack moduleStack in this.ModuleStacks.Values) 
			{
				reportLines.Add(moduleStack.Report(faction, level + 1));
			}
			return reportLines.IndentedLines;
		}

		public bool Visible(Faction faction)
		{
            if (faction == null)
            {
                return true;
            }
            if (faction.FullName == "NPC")
            {
                return true;
            }
			if (this.Owner == faction)
			{
				return true;
			}
			if (this.Size > 0)
			{
				return this.Location.HasPresence(faction);
			}
			return false;
		}

		#endregion
	}
}
