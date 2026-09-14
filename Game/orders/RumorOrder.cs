using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	public class RumorOrder : ImmediateOrder
	{
		public override bool AllowedBetweenTurns
		{
			get { return true; }
		}

		public string PlanetId { get; set; }
		public string Title { get; set; }
		public string Flavour { get; set; }

		public RumorOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.rumor;
		}

		public override void Parse(string command)
		{
			this.PlanetId = string.Empty;
			this.Title = string.Empty;
			this.Flavour = string.Empty;

			string planetToken = LineParser.GetToken(ref command);
			if (string.IsNullOrEmpty(planetToken))
			{
				throw new Exception("Bad syntax, RUMOR planet-id expected.");
			}
			this.PlanetId = planetToken;

			while (!string.IsNullOrEmpty(command))
			{
				string token = LineParser.GetToken(ref command);
				if (token.ToLowerInvariant() == "title")
				{
					this.Title = LineParser.GetQuotedToken(ref command);
				}
				else if (token.ToLowerInvariant() == "flavour" || token.ToLowerInvariant() == "flavor")
				{
					this.Flavour = LineParser.GetQuotedToken(ref command);
				}
				else if (string.IsNullOrEmpty(this.Title))
				{
					this.Title = token;
				}
				else
				{
					this.Flavour = token;
				}
			}

			if (string.IsNullOrEmpty(this.Title) && string.IsNullOrEmpty(this.Flavour))
			{
				throw new Exception("Bad syntax, RUMOR title expected.");
			}
		}

		public override void Execute(int week)
		{
			this.Executed = false;
			if (!Planet.All.ContainsKey(this.PlanetId))
			{
				Faction issuer = this.Subject as Faction;
				if (issuer != null)
				{
					issuer.EventReports.Add(week, string.Format("RUMOR failed. Unknown planet {0}.", this.PlanetId));
				}
				base.Execute(week);
				return;
			}

			new PressRelease(null, this.Title, this.Flavour, true, this.PlanetId);
			this.Executed = true;
			base.Execute(week);
		}

		public override void LoadXml(XmlElement elOrder)
		{
			XmlElement elRumor = (XmlElement)elOrder.SelectNodes("rumor")[0];
			this.PlanetId = elRumor.GetAttribute("planet");
			this.Title = elRumor.GetAttribute("title");
			this.Flavour = elRumor.GetAttribute("flavour");
		}

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
			XmlElement elRumor = doc.CreateElement("rumor");
			elRumor.SetAttribute("planet", this.PlanetId);
			elRumor.SetAttribute("title", this.Title);
			elRumor.SetAttribute("flavour", this.Flavour);
			this.xmlElement.AppendChild(elRumor);
			return this.xmlElement;
		}

		public override List<string> Report(Faction owner)
		{
			string prefix = string.Format("{0}{1}",
				this.Conditions,
				(this.Repeat == 1) ? string.Empty : ((this.Repeat < 0) ? "@" : string.Concat(this.Repeat.ToString(), " ")));
			return new List<string>
			{
				string.Format("{0}rumor {1} title \"{2}\" flavour \"{3}\"", prefix, this.PlanetId, this.Title, this.Flavour)
			};
		}
	}
}
