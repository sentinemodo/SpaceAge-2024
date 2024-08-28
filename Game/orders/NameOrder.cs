using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class NameOrder : ImmediateOrder
	{
		public NameOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.name;
		}

		public NameOrder(ModuleStack namer, NamedObject named, string name)
			: base(namer)
		{
			this.named = named;
			this.description = name;
		}

		public NameOrder(ModuleStack namer, string name)
			: base(namer)
		{
			this.named = namer;
			this.description = name;
		}

		public ModuleStack Namer
		{
			get { return (ModuleStack)this.Subject; }
		}

		private NamedObject named;
		public NamedObject Named
		{
			get { return this.named; }
			set { this.named = value; }
		}

		private string description = string.Empty;
		public string Description
		{
			get { return this.description; }
			set { this.description = value; }
		}
		
		public override void Parse(string command)
		{
			// change name of the named object
			// NAME "new name"
			// NAME target "new name"

			string token;
			if (string.IsNullOrEmpty(command.Trim()))
			{
				throw new Exception("Bad syntax");
			}
			string commandCopy = command;

			token = LineParser.GetToken(ref commandCopy);			
			if (token[0] == '"')
			{
				this.description = LineParser.GetQuotedToken(ref command);
				this.named = this.Namer;
			} else 
			{
				token = LineParser.GetToken(ref command);			
				//if (Star.All.ContainsKey(token)) 
				//{
				//    this.named = Star.All[token];
				//} else 
				if (Planet.All.ContainsKey(token))
				{
					this.named = Planet.All[token];
				} else if (Moon.All.ContainsKey(token))
				{
					this.named = Moon.All[token];
				} else if (Orbit.All.ContainsKey(token))
				{
					this.named = Orbit.All[token];
				} else if (Region.All.ContainsKey(token))
				{
					this.named = Region.All[token];
				} else {
					throw new Exception("bad syntax or unknown object to name " + token);
				}
				this.description = LineParser.GetQuotedToken(ref command);
			}

		}

        public override List<string> Report(Faction owner)
        {
            List<string> lines = new List<string>();
            string line = string.Format("{0}name \"{1}\"",
                this.Conditions,
                this.Description);
            lines.Add(line);
            return lines;
        }

        public override void LoadXml(XmlElement elOrder)
        {
            XmlElement elName = (XmlElement)elOrder.SelectNodes("name")[0];
            this.Description = elName.GetAttribute("description");
        }

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
            XmlElement elName = doc.CreateElement("name");
            elName.SetAttribute("description", this.Description);
			this.xmlElement.AppendChild(elName);
			return this.xmlElement;
		}

		public override void Execute(int week)
		{
			try
			{
				this.Executed = false;
				if (this.Namer == this.named)
				{
					this.named.FullName = this.Description;
					this.Executed = true;
				} 
				else if (this.Namer.Location.Name == this.named.Name)
				{
					this.named.FullName = this.Description;
					this.Executed = true;
				}
				else 
				{
					this.Namer.EventReports.Add(week, "NAME failed. You must be in location to name it.");
				}
				base.Execute(week);
			}
			catch (Exception ex)
			{
				throw new Exception("tried to execute Name Order by " + this.Namer.ToString(), ex);
			}

		}
	}
}
