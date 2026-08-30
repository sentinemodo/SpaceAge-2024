using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class ResearchOrder : LongOrder
	{
		public ResearchOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.research;
            this.ResearchType = EResearchType.Technology;
		}

		public EResearchType ResearchType { get; set; } 

		public ModuleStack Researcher
		{
			get { return (ModuleStack)this.Subject; }
		}

		public Technology Technology { get; set; }
        public ModuleType ModuleType { get; set; }
        public ItemType ItemType { get; set; }
        public EModuleTypesGroup ModuleTypesGroup { get; set; }
        public string ResearchToken { get; set; }

		public int ResearchPoints
		{
			get { return this.Researcher.ResearchPoints; }
		}
		
		public override void Parse(string command)
		{
            // RESEARCH TECHNOLOGY technology
            // RESEARCH ITEM item
            // RESEARCH MODULE moduletype
            // RESEARCH GROUP [agricultural|command|spacecraft|energy|extraction|frigate|habitat|infantry|
            // military|production|propulsion|research|settlement|spacestation|storage|vehicle]
            // RESEARCH TAG tag
            // RESEARCH id
            // RESEARCH

            // research technology 1 is automatic when 100 points is gathered
            // below there is a chance of breakthrough at the end of the turn equal to researchpoints rp / tech level * 100
            // max researchable technology is 1 + current max technology and is checked from the top (if there is anything researchable)
            // eg if there are technologies level 3 2 1 in the database check if can research. with 5 RP faction has 5/300 then 5/200 and then 5/100 chance for breakthrough
            // if breakthough (researched technology) each of the techs in the list is getting a chance to be selected
            // the targetted technologie are having 50% of being seelected (so if there are 6 researchable technologies and one of them is targetted the chances are
            //      5/10, 1/10,1/10,1/10,1/10 and 1/10) 
            // the chance for breakthrough is double if not targetted anythign specific
            // when targetting object or feature the technology limit is not applicable 
            // researching yield is dependant on the output of the lab - the general formula is RP = (level of the reasearch tech used build research module + 1)x number of modules
            //   so a basic computer library (tech 0) gives 1 RP / module / week (13 RP / quarter) - with 4 modules that lab should research a level 1 tech every other turn
            // lab can have an increased yield if placed special effect region, if occupeid by research skilled officer or if affected by certain technologies or quests
            // the general rule for technology level is that they more or less double effects and costs on every level compared to the previous level

            string token;
			token = LineParser.GetToken(ref command);
            if (token == "technology")
            {
                token = LineParser.GetQuotedToken(ref command);
                if (Technology.All.Contains(token))
                {
                    this.ResearchType = EResearchType.Technology;
                    this.Technology = Technology.All[token];
                } else
                {
                    this.ResearchType = EResearchType.Any;
                    this.ResearchToken = token;
                }
            } else if (token == "item")
            {
                token = LineParser.GetQuotedToken(ref command);
                if (ItemType.All.ContainsKey(token))
                {
                    this.ResearchType = EResearchType.ItemType;
                    this.ItemType = ItemType.All[token];
                }
                else
                {
                    this.ResearchType = EResearchType.Any;
                    this.ResearchToken = token;
                }
            }
            else if (token == "module")
            {
                token = LineParser.GetQuotedToken(ref command);
                if (ModuleType.All.ContainsKey(token))
                {
                    this.ResearchType = EResearchType.ModuleType;
                    this.ModuleType = ModuleType.All[token];
                }
                else
                {
                    this.ResearchType = EResearchType.Any;
                    this.ResearchToken = token;
                }
            }
            else if (token == "group")
            {
                token = LineParser.GetQuotedToken(ref command);
                this.parseModuleTypeGroup(token);
            }
            else if (token == "tag")
            {
                token = LineParser.GetQuotedToken(ref command);
                this.ResearchType = EResearchType.Tag;
                this.ResearchToken = token;
            }
            else if (token != string.Empty)
            {
                // bare parameter: resolve to the most specific preference it matches
                if (ModuleStack.All.ContainsKey(token))
                {
                    this.ResearchType = EResearchType.ModuleStack;
                    this.ResearchToken = token;
                }
                else if (Technology.All.Contains(token))
                {
                    // a known technology -> prefer technologies it enables
                    this.ResearchType = EResearchType.Technology;
                    this.Technology = Technology.All[token];
                    this.ResearchToken = token;
                }
                else if (Research.IsTag(token))
                {
                    // a tag (e.g. military) -> prefer technologies carrying that tag
                    this.ResearchType = EResearchType.Tag;
                    this.ResearchToken = token;
                }
                else if (ItemType.All.ContainsKey(token))
                {
                    this.ResearchType = EResearchType.ItemType;
                    this.ItemType = ItemType.All[token];
                    this.ResearchToken = token;
                }
                else if (ModuleType.All.ContainsKey(token))
                {
                    this.ResearchType = EResearchType.ModuleType;
                    this.ModuleType = ModuleType.All[token];
                    this.ResearchToken = token;
                }
                else if (Research.IsSpaceObject(token))
                {
                    // a moon/planet/region/orbit -> prefer technologies tied to its resources
                    this.ResearchType = EResearchType.SpaceObject;
                    this.ResearchToken = token;
                }
                else
                {
                    this.ResearchType = EResearchType.Any;
                    this.ResearchToken = token;
                }
            } else
            {
                this.ResearchType = EResearchType.Any;
                this.ResearchToken = string.Empty;
            }
		}

        private void parseModuleTypeGroup(string token)
        {
            this.ResearchType = EResearchType.Group;
            try
            {
                this.ModuleTypesGroup = ModuleTypeGroupXml.Parse(token);
                this.ResearchToken = ModuleTypeGroupXml.ToToken(this.ModuleTypesGroup);
            }
            catch (KeyNotFoundException)
            {
                this.ResearchType = EResearchType.Any;
                this.ResearchToken = token;
            }
        }

        public override List<string> Report(Faction owner)
		{
            List<string> lines = new List<string>();
			string line;
			line = string.Format("{0}{1}research",
				this.Conditions,
				(this.Repeat > 1) ? string.Concat(this.Repeat.ToString(), " ") : ((this.Repeat < 0) ? "@" : string.Empty));
            switch (this.ResearchType)
            {
                case EResearchType.Technology:
                    line = string.Concat(line, " technology ", this.Technology.Name);
                    break;
                case EResearchType.ItemType:
                    line = string.Concat(line, " item ", this.ItemType.Name);
                    break;
                case EResearchType.ModuleType:
                    line = string.Concat(line, " module ", this.ModuleType.Name);
                    break;
                case EResearchType.Group:
                    line = string.Concat(line, " group ", ModuleTypeGroupXml.ToToken(this.ModuleTypesGroup));
                    break;
                default:
                    if (this.ResearchType == EResearchType.ModuleStack)
                    {
                        line = string.Concat(line, " ", this.ResearchToken);
                    }
                    else
                    {
                        line = string.Concat(line, " ", this.ResearchToken);
                    }
                    break;
            }
            lines.Add(line);
			return lines;
		}

        public override void LoadXml(XmlElement elOrder)
        {
            XmlElement elResearch = (XmlElement)elOrder.SelectNodes("research")[0];

            switch (elResearch.GetAttribute("research-type"))
            {
                case "technology":
                    this.ResearchType = EResearchType.Technology;
                    this.Technology = Technology.All[elResearch.GetAttribute("technology")];
                    break;
                case "item":
                    this.ResearchType = EResearchType.ItemType;
                    this.ItemType = ItemType.All[elResearch.GetAttribute("item")];
                    break;
                case "module":
                    this.ResearchType = EResearchType.ModuleType;
                    this.ModuleType = ModuleType.All[elResearch.GetAttribute("module")];
                    break;
                case "group":
                    this.ResearchType = EResearchType.Group;
                    this.parseModuleTypeGroup(elResearch.GetAttribute("group"));
                    break;
                case "feature":
                    this.ResearchType = EResearchType.Feature;
                    this.ResearchToken = elResearch.GetAttribute("feature");
                    break;
                case "tag":
                    this.ResearchType = EResearchType.Tag;
                    this.ResearchToken = elResearch.GetAttribute("tag");
                    break;
                case "object":
                    this.ResearchType = EResearchType.SpaceObject;
                    this.ResearchToken = elResearch.GetAttribute("object");
                    break;
                case "stack":
                    this.ResearchType = EResearchType.ModuleStack;
                    this.ResearchToken = elResearch.GetAttribute("stack");
                    break;
                default:
                    this.ResearchType = EResearchType.Any;
                    break;
            }
        }

        public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
            XmlElement elResearch = doc.CreateElement("research");

            switch (this.ResearchType)
            {
                case EResearchType.Technology:
                    elResearch.SetAttribute("research-type", "technology");
                    elResearch.SetAttribute("technology", this.Technology.Name);                    
                    break;
                case EResearchType.ItemType:
                    elResearch.SetAttribute("research-type", "item");
                    elResearch.SetAttribute("item", this.ItemType.Name);
                    break;
                case EResearchType.ModuleType:
                    elResearch.SetAttribute("research-type", "module");
                    elResearch.SetAttribute("module", this.ModuleType.Name);                  
                    break;
                case EResearchType.Group:
                    elResearch.SetAttribute("research-type", "group");
                    elResearch.SetAttribute("group", ModuleTypeGroupXml.ToToken(this.ModuleTypesGroup)); 
                    break;
                case EResearchType.Tag:
                    elResearch.SetAttribute("research-type", "tag");
                    elResearch.SetAttribute("tag", this.ResearchToken);
                    break;
                case EResearchType.SpaceObject:
                    elResearch.SetAttribute("research-type", "object");
                    elResearch.SetAttribute("object", this.ResearchToken);
                    break;
                case EResearchType.ModuleStack:
                    elResearch.SetAttribute("research-type", "stack");
                    elResearch.SetAttribute("stack", this.ResearchToken);
                    break;
                default:
                    if (this.ResearchToken != string.Empty)
                    {
                        elResearch.SetAttribute("research-type", "feature");
                        elResearch.SetAttribute("feature", this.ResearchToken);
                    }
                    break;
            }            

            this.xmlElement.AppendChild(elResearch);
			return this.xmlElement;
		}

        Technology researchedTechnology;

        public override void Execute(int week)
        {
            // check if can research at all
            if (this.CanOperate(week) && this.CanResearch(week))
            {
                if (this.ResearchType == EResearchType.ModuleStack)
                {
                    this.researchWreckage(week);
                    this.Executing = true;
                    base.Execute(week);
                    return;
                }

                if (this.ResearchType == EResearchType.SpaceObject
                    && !Research.TryRevealSpaceObject(this.Researcher, this.ResearchToken, week))
                {
                    return;
                }

                // research: roll a breakthrough; if none, accumulate research points
                Technologies available = Research.AvailableTechnologies(this.Researcher);
                int output = Research.WeeklyOutput(this.Researcher);

                if (available.Count > 0 && Research.RollBreakthrough(available, output))
                {
                    this.researchedTechnology = Research.SelectResearchedTechnology(this, available);

                    // breakthrough consumes the accumulated research points
                    this.Researcher.EventReports.Add(
                        week,
                        string.Format("Breakthough!!! Researched {0} technology.",
                            this.researchedTechnology.ReportName));
                    this.Researcher.ReceiveTechnologyCopy(this.researchedTechnology, week, null);
                    this.Researcher.ResearchPoints = 0;
                }
                else
                {
                    // TODO: add effect / race / officer impact to weekly output
                    this.Researcher.ResearchPoints += output;
                }

                this.Executing = true;

                // finish order execution
                base.Execute(week);
            }
        }

        private void researchWreckage(int week)
        {
            if (!ModuleStack.All.ContainsKey(this.ResearchToken))
            {
                this.Researcher.EventReports.Add(
                    week,
                    string.Format("RESEARCH failed: unknown stack {0}.", this.ResearchToken));
                return;
            }

            ModuleStack target = ModuleStack.All[this.ResearchToken];
            if (this.Researcher.Location != target.Location)
            {
                this.Researcher.EventReports.Add(
                    week,
                    string.Format("RESEARCH failed: {0} is not at {1}.",
                        this.Researcher.ReportName,
                        target.ReportName));
                return;
            }

            int output = Research.WeeklyOutput(this.Researcher);
            if (output < 1)
            {
                return;
            }

            if (!this.hasOpenWreckageContract(target))
            {
                return;
            }

            this.Researcher.ResearchPoints += output;
            Contract.All.NotifyResearch(this.Researcher.Owner, this.Researcher, target, output);
            this.Researcher.EventReports.Add(
                week,
                string.Format("researched {0}.", target.ReportName));

            if (!this.hasOpenWreckageContract(target))
            {
                this.Researcher.ResearchPoints = 0;
                this.awardCompletedWreckageContracts(week, target);
            }
        }

        private bool hasOpenWreckageContract(ModuleStack target)
        {
            foreach (Contract contract in Contract.All)
            {
                ResearchWreckageTrigger trigger = contract.Trigger as ResearchWreckageTrigger;
                if (trigger != null && trigger.Target == target && !trigger.IsComplete())
                {
                    return true;
                }
            }
            return false;
        }

        private void awardCompletedWreckageContracts(int week, ModuleStack target)
        {
            List<Contract> snapshot = new List<Contract>(Contract.All);
            foreach (Contract contract in snapshot)
            {
                ResearchWreckageTrigger trigger = contract.Trigger as ResearchWreckageTrigger;
                if (trigger != null && trigger.Target == target && contract.Evaluate(week))
                {
                    Contract.All.Remove(contract);
                }
            }
        }

        private bool CanResearch(int week)
        {
            if (this.Researcher.ModuleType.Group == EModuleTypesGroup.research)
            {
                return true;
            }
            else
            {
                this.Researcher.EventReports.Add(
                    week,
                    string.Format("RESEARCH failed: {0} is not a research module.",
                        this.Researcher.ReportName));
                return false;
            }
        }
    }
}
