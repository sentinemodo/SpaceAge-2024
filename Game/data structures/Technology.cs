using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class Technology : NamedType
	{
		readonly public static Technologies All = new Technologies();

		public Technology(string name) : base(name) 
		{
            if (Technology.All[name] != null)
            { 
                    throw new Exception("Technology with name " + name + " already exists");
            }
            else
            {
                Technology.All.Add(this);
                this.ProductionType = EProductionType.None;
                this.UseCondition_ModuleTypesGroup = EModuleTypesGroup.all;
            }
		}

		public int Level { get; set; }
		public int UseTime { get; set; }

		// Research point cost for a breakthrough. Defaults to a per-level value
		// (see Research.DefaultCostForLevel) unless explicitly overridden in the datafile.
		private int? costOverride = null;
		public int Cost
		{
			get { return this.costOverride ?? Research.DefaultCostForLevel(this.Level); }
			set { this.costOverride = value; }
		}

		// Free-form tags (e.g. "military", "production", "research") used to target research.
		private List<string> tags = new List<string>();
		public List<string> Tags
		{
			get { return this.tags; }
		}

		public bool HasTag(string tag)
		{
			return this.tags.Contains(tag);
		}

		public void LoadTags(string raw)
		{
			this.tags.Clear();
			if (!string.IsNullOrEmpty(raw))
			{
				foreach (string tag in raw.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					this.tags.Add(tag);
				}
			}
		}

		// Single prerequisite technology (this technology is "enabled by" Requires).
		public Technology Requires { get; set; }

        public ItemStacks UseConsumeItems { get; set; }
        public ItemStacks UseProduceItems { get; set; }

		public ModuleType UseConsumeModules { get; set; }
        public ModuleType UseProduceModules { get; set; }

		public EProductionType ProductionType { get; set; }

		#region conditions

		public PlanetTypes          UseCondition_PlanetTypes            { get; set; }
		public LocationTypes        UseCondition_LocationTypes          { get; set; }
        public ItemTypes            UseCondition_AtmosphereResources    { get; set; }        
        public EModuleTypesGroup    UseCondition_ModuleTypesGroup       { get; set; }

        #endregion

		#region battle
		public bool IsBattleTechnology
		{
			get	
			{
				if (this.Attack != 0 | this.Defense != 0 | this.Initiative != 0) 
				{
					return true;
				}
				return false;
			}
		}

		public int Attack { get; set; }
		public int Defense { get; set; }
		public int Initiative { get; set; }
		#endregion

		public string ReportDetails()
		{
			string line = this.ReportName;
			if (this.Level > 1 | this.IsBattleTechnology)
			{
				line = string.Concat(line, " (");
				if (this.Level > 1)
				{
					line = string.Concat(line, "level: ", this.Level);
				}
				if (this.Level > 1 & this.IsBattleTechnology)
				{
					line = string.Concat(line, ", ");
				}
				if (this.IsBattleTechnology)
				{
					bool firstAdded = false;
					if (this.Attack != 0)
					{
						line = string.Format("{0}{1}attack: {2}",
							line,
							(firstAdded) ? ", " : "",
							this.Attack);
						firstAdded = true;
					}
					if (this.Defense != 0)
					{
						line = string.Format("{0}{1}defense: {2}",
							line,
							(firstAdded) ? ", " : "",
							this.Defense);
						firstAdded = true;
					}
					if (this.Initiative != 0)
					{
						line = string.Format("{0}{1}initiative: {2}",
							line,
							(firstAdded) ? ", " : "",
							this.Initiative);
					}
				}
				line = string.Concat(line, ")");
			}
			return line;
		}

        public XmlElement SaveXml(XmlDocument doc, bool instance = true)
        {
            base.SaveXml(doc, "technology");

            if (!instance)
            {
                // full technology description        
            }

            return this.xmlElement;
        }
	}
}
