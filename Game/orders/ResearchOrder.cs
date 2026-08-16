using NUnit.Framework.Constraints;
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
            else if (token != string.Empty)
            { 
                if (ModuleType.All.ContainsKey(token))
                {
                    this.ResearchType = EResearchType.Feature;
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

            switch (token)
            {
                case "agricultural":
                    this.ModuleTypesGroup = EModuleTypesGroup.agricultural;
                    break;
                case "command":
                    this.ModuleTypesGroup = EModuleTypesGroup.command;
                    break;
                case "spacecraft":
                    this.ModuleTypesGroup = EModuleTypesGroup.spacecraft;
                    break;
                case "energy":
                    this.ModuleTypesGroup = EModuleTypesGroup.energy;
                    break;
                case "extraction":
                    this.ModuleTypesGroup = EModuleTypesGroup.extraction;
                    break;
                case "habitat":
                    this.ModuleTypesGroup = EModuleTypesGroup.habitat;
                    break;
                case "infantry":
                    this.ModuleTypesGroup = EModuleTypesGroup.infantry;
                    break;
                case "military":
                    this.ModuleTypesGroup = EModuleTypesGroup.military;
                    break;
                case "production":
                    this.ModuleTypesGroup = EModuleTypesGroup.production;
                    break;
                case "propulsion":
                    this.ModuleTypesGroup = EModuleTypesGroup.propulsion;
                    break;
                case "research":
                    this.ModuleTypesGroup = EModuleTypesGroup.research;
                    break;
                case "vehicle":
                    this.ModuleTypesGroup = EModuleTypesGroup.vehicle;
                    break;
                default:
                    this.ResearchType = EResearchType.Any;
                    this.ResearchToken = token;
                    break;
            }
            if (this.ResearchType != EResearchType.Any)
            {
                this.ResearchToken = this.ModuleTypesGroup.ToString();
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
                    line = string.Concat(line, " group ", this.ModuleTypesGroup.ToString());
                    break;
                default:
                    line = string.Concat(line, " ", this.ResearchToken);
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
                    elResearch.SetAttribute("group", this.ModuleTypesGroup.ToString()); 
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

        Technologies targettedTechnologies = new Technologies();
        Technologies availableTechnologies = new Technologies();
        Technology researchedTechnology;

        public override void Execute(int week)
        {
            // check if can research at all
            if (this.CanOperate(week) && this.CanResearch(week))
            {
                // research: roll a breakthrough; if none, accumulate research points
                this.getAvailableTechnologies();
                int output = Research.WeeklyOutput(this.Researcher);

                if (this.availableTechnologies.Count > 0 && Research.RollBreakthrough(this.availableTechnologies, output))
                {
                    if (this.ResearchType == EResearchType.Feature)
                    {
                        // guaranteed getting feature technology
                        this.researchedTechnology = Technology.All[this.ResearchToken];
                    }
                    else if (this.ResearchType != EResearchType.Any)
                    {
                        // check if targetted was sought and hit (50%)
                        this.getTargettedTechnologies();

                        if (Sequence.GenerateRandomInt(0, 100) <= 50 && this.targettedTechnologies.Count > 0)
                        {
                            // TODO: battlelike hitting into random tech
                            this.researchedTechnology = this.getRandomTechnology(this.targettedTechnologies);
                        }
                        else
                        {
                            this.researchedTechnology = this.getRandomTechnology(this.availableTechnologies);
                        }
                    }
                    else
                    {
                        // no targetted techs
                        this.researchedTechnology = this.getRandomTechnology(this.availableTechnologies);
                    }
                    // breakthrough consumes the accumulated research points
                    this.Researcher.EventReports.Add(
                        week,
                        string.Format("Breakthough!!! Researched {0} technology.",
                            this.researchedTechnology.ReportName));
                    this.Researcher.Owner.TechnologiesToShow.Add(this.researchedTechnology);
                    this.Researcher.Technologies.Add(this.researchedTechnology);
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

        public Technology getRandomTechnology(Technologies technologies)
        {
            int totalArea = 0;
            foreach (Technology technology in technologies)
            {
                totalArea += Convert.ToInt32(100 / technology.Level);
            }
            int roll = Sequence.GenerateRandomInt(0, totalArea);
            foreach (Technology technology in technologies)
            {
                roll -= Convert.ToInt32(100 / technology.Level);
                if (roll <= 0)
                {
                    return technology;
                }
            }
            return null;
        }

        private void getTargettedTechnologies()
        {
            this.targettedTechnologies.Clear();
            bool addTechnology;
            foreach (Technology technology in this.availableTechnologies)
            {
                addTechnology = false;
                switch (this.ResearchType)
                {
                    case EResearchType.Technology:
                        // TODO: add technologies that are requiring or required by the technology
                        if (technology.Name == this.ResearchToken)
                        {
                            addTechnology = true;
                        }
                        break;
                    case EResearchType.ItemType:
                        if (technology.UseProduceItems != null)
                        {
                            if (technology.UseProduceItems.ContainsKey(ItemType.All[this.ResearchToken]))
                            {
                                addTechnology = true;
                            }
                        }
                        if (technology.UseConsumeItems != null)
                        {
                            if (technology.UseConsumeItems.ContainsKey(ItemType.All[this.ResearchToken]))
                            {
                                addTechnology = true;
                            }
                        }                        
                        break;
                    case EResearchType.ModuleType:
                        if (technology.UseProduceModules.Name == this.ResearchToken
                            || technology.UseConsumeModules.Name == this.ResearchToken)
                        {
                            addTechnology = true;
                        }
                        break;
                    case EResearchType.Group:
                        if (technology.UseCondition_ModuleTypesGroup.ToString() == this.ResearchToken)
                        {
                            addTechnology = true;
                        }
                        if (technology.UseProduceModules != null)
                        {
                            if (technology.UseProduceModules.Group.ToString() == this.ResearchToken)
                            {
                                addTechnology = true;
                            }
                        }
                        break;
                    default:
                        break;
                }
                if (addTechnology)
                {
                    this.targettedTechnologies.Add(technology);
                }
            }
        }

        private void getAvailableTechnologies()
        {
            this.availableTechnologies.Clear(); 
            foreach (Technology technology in Technology.All)
            {
               // TODO: add validation for technoligies owned
               // eliminate technologies owned by faction
               // eliminate technologies requiring prerequisites
               if (technology.Level >= 1
                    && technology.Level <= this.Researcher.Owner.MaxTechnologyLevel + 1
                    && technology.Level <= this.Researcher.TechnologyCapacity - this.Researcher.TechnologyCapacityUsed
                    && this.Researcher.Technologies[technology.Name] == null)
                {
                    this.availableTechnologies.Add(technology);
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
