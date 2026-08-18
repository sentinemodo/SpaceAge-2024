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
			this.Title = string.Empty;
			this.Flavour = string.Empty;
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
				throw new Exception("Bad syntax, PRESS title expected.");
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

			new PressRelease(issuer, this.Title, this.Flavour);
			issuer.EventReports.Add(week, string.Format("issued press release {0}.", this.Title));
			this.Executed = true;
			base.Execute(week);
		}

		public override void LoadXml(XmlElement elOrder)
		{
			XmlElement elPress = (XmlElement)elOrder.SelectNodes("press")[0];
			this.Title = elPress.GetAttribute("title");
			this.Flavour = elPress.GetAttribute("flavour");
		}

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
			XmlElement elPress = doc.CreateElement("press");
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
			return new List<string>
			{
				string.Format("{0}press title \"{1}\" flavour \"{2}\"", prefix, this.Title, this.Flavour)
			};
		}
	}
}
