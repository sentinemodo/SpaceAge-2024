using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	public class PressOrder : ImmediateOrder
	{
		public override bool AllowedBetweenTurns
		{
			get { return true; }
		}

		public string PlanetId { get; set; }
		public string Title { get; set; }
		public string Flavour { get; set; }

		public Faction Issuer
		{
			get { return this.Subject as Faction; }
		}

		public PressOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.press;
		}

		public override void Parse(string command)
		{
			this.PlanetId = string.Empty;
			this.Title = string.Empty;
			this.Flavour = string.Empty;

			string scopeToken = LineParser.GetToken(ref command);
			if (string.IsNullOrEmpty(scopeToken))
			{
				throw new Exception("Bad syntax, PRESS title expected.");
			}
			if (PressOrder.IsScopeToken(scopeToken))
			{
				this.PlanetId = scopeToken;
				scopeToken = LineParser.GetToken(ref command);
				if (string.IsNullOrEmpty(scopeToken))
				{
					throw new Exception("Bad syntax, PRESS title expected.");
				}
			}

			string title = string.Empty;
			string flavour = string.Empty;
			PressOrder.ParseTitleAndFlavour(ref command, scopeToken, ref title, ref flavour);
			this.Title = title;
			this.Flavour = flavour;
			if (string.IsNullOrEmpty(this.Title) && string.IsNullOrEmpty(this.Flavour))
			{
				throw new Exception("Bad syntax, PRESS title expected.");
			}
		}

		private static bool IsScopeToken(string token)
		{
			if (string.IsNullOrEmpty(token))
			{
				return false;
			}
			return Planet.All.ContainsKey(token) || Moon.All.ContainsKey(token);
		}

		private static void ParseTitleAndFlavour(ref string command, string firstToken, ref string title, ref string flavour)
		{
			string token = firstToken;
			while (!string.IsNullOrEmpty(token) || !string.IsNullOrEmpty(command))
			{
				if (string.IsNullOrEmpty(token))
				{
					token = LineParser.GetToken(ref command);
					if (string.IsNullOrEmpty(token))
					{
						break;
					}
				}
				if (token.ToLowerInvariant() == "title")
				{
					title = LineParser.GetQuotedToken(ref command);
				}
				else if (token.ToLowerInvariant() == "flavour" || token.ToLowerInvariant() == "flavor")
				{
					flavour = LineParser.GetQuotedToken(ref command);
				}
				else if (string.IsNullOrEmpty(title))
				{
					title = token;
				}
				else
				{
					flavour = token;
				}
				token = null;
			}
		}

		public override void Execute(int week)
		{
			this.Executed = false;
			Faction issuer = this.Issuer;
			if (issuer == null)
			{
				base.Execute(week);
				return;
			}

			if (!string.IsNullOrEmpty(this.PlanetId)
				&& !Planet.All.ContainsKey(this.PlanetId)
				&& !Moon.All.ContainsKey(this.PlanetId))
			{
				issuer.EventReports.Add(week, string.Format("PRESS failed. Unknown scope {0}.", this.PlanetId));
				base.Execute(week);
				return;
			}

			new PressRelease(issuer, this.Title, this.Flavour, false, this.PlanetId);
			issuer.EventReports.Add(week, string.Format("issued press release {0}.", this.Title));
			this.Executed = true;
			base.Execute(week);
		}

		public override void LoadXml(XmlElement elOrder)
		{
			XmlElement elPress = (XmlElement)elOrder.SelectNodes("press")[0];
			this.PlanetId = elPress.GetAttribute("planet");
			this.Title = elPress.GetAttribute("title");
			this.Flavour = elPress.GetAttribute("flavour");
		}

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
			XmlElement elPress = doc.CreateElement("press");
			if (!string.IsNullOrEmpty(this.PlanetId))
			{
				elPress.SetAttribute("planet", this.PlanetId);
			}
			elPress.SetAttribute("title", this.Title);
			elPress.SetAttribute("flavour", this.Flavour);
			this.xmlElement.AppendChild(elPress);
			return this.xmlElement;
		}

		public override List<string> Report(Faction owner)
		{
			string prefix = string.Format("{0}{1}",
				this.Conditions,
				(this.Repeat == 1) ? string.Empty : ((this.Repeat < 0) ? "@" : string.Concat(this.Repeat.ToString(), " ")));
			if (!string.IsNullOrEmpty(this.PlanetId))
			{
				return new List<string>
				{
					string.Format("{0}press {1} title \"{2}\" flavour \"{3}\"", prefix, this.PlanetId, this.Title, this.Flavour)
				};
			}
			return new List<string>
			{
				string.Format("{0}press title \"{1}\" flavour \"{2}\"", prefix, this.Title, this.Flavour)
			};
		}
	}
}
