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

		public bool RenamesIssuerStack
		{
			get
			{
				if (this.named == null)
				{
					return true;
				}
				return this.named == (NamedObject)this.Namer;
			}
		}

		public override void Parse(string command)
		{
			// NAME "new name" — rename issuing stack
			// NAME LOCATION <region|planet|moon|orbit-id> "new name" — rename map object (patrol rules)
			// NAME <id> "new name" — legacy location form

			if (string.IsNullOrEmpty(command.Trim()))
			{
				throw new Exception("Bad syntax");
			}
			string commandCopy = command;

			string token = LineParser.GetToken(ref commandCopy);
			if (token.Length > 0 && token[0] == '"')
			{
				this.description = LineParser.GetQuotedToken(ref command);
				this.named = this.Namer;
				return;
			}

			if (string.Equals(token, "location", StringComparison.OrdinalIgnoreCase))
			{
				token = LineParser.GetToken(ref commandCopy);
			}

			this.named = this.resolveMapNameTarget(token);
			if (this.named == null)
			{
				throw new Exception("bad syntax or unknown object to name " + token);
			}
			this.description = LineParser.GetQuotedToken(ref commandCopy);
		}

		private NamedObject resolveMapNameTarget(string token)
		{
			if (Planet.All.ContainsKey(token))
			{
				return Planet.All[token];
			}
			if (Moon.All.ContainsKey(token))
			{
				return Moon.All[token];
			}
			if (Orbit.All.ContainsKey(token))
			{
				return Orbit.All[token];
			}
			if (Region.All.ContainsKey(token))
			{
				return Region.All[token];
			}
			return null;
		}

        public override List<string> Report(Faction owner)
        {
            List<string> lines = new List<string>();
			string line;
			if (this.RenamesIssuerStack)
			{
				line = string.Format("{0}name \"{1}\"", this.Conditions, this.Description);
			}
			else
			{
				line = string.Format(
					"{0}name location {1} \"{2}\"",
					this.Conditions,
					this.named.Name,
					this.Description);
			}
            lines.Add(line);
            return lines;
        }

        public override void LoadXml(XmlElement elOrder)
        {
            XmlElement elName = (XmlElement)elOrder.SelectNodes("name")[0];
            this.Description = elName.GetAttribute("description");
			string target = elName.GetAttribute("target");
			if (string.IsNullOrEmpty(target))
			{
				this.named = this.Namer;
			}
			else
			{
				this.named = this.resolveMapNameTarget(target);
				if (this.named == null)
				{
					this.named = this.Namer;
				}
			}
        }

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
            XmlElement elName = doc.CreateElement("name");
            elName.SetAttribute("description", this.Description);
			if (!this.RenamesIssuerStack && this.named != null)
			{
				elName.SetAttribute("target", this.named.Name);
			}
			this.xmlElement.AppendChild(elName);
			return this.xmlElement;
		}

		public override void Execute(int week)
		{
			try
			{
				this.Executed = false;
				if (this.RenamesIssuerStack)
				{
					this.Namer.FullName = this.Description;
					this.Executed = true;
				}
				else if (this.Namer.Location == null)
				{
					this.Namer.EventReports.Add(week, "NAME failed. You must be in location to name it.");
				}
				else if (this.Namer.Location.Name == this.named.Name)
				{
					ModuleStack patrolBlocker = PatrolGuard.FindRenameBlocker(this.named, this.Namer.Owner);
					if (patrolBlocker != null)
					{
						this.Namer.EventReports.Add(
							week,
							string.Format(
								"NAME failed. {0} is patrolling this location.",
								patrolBlocker.ReportName));
					}
					else
					{
						this.named.FullName = this.Description;
						this.Executed = true;
					}
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
