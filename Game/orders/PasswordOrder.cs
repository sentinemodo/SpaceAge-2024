using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	public class PasswordOrder : ImmediateOrder
	{
		public override bool AllowedBetweenTurns
		{
			get { return true; }
		}

		public string NewPassword { get; set; }

		public PasswordOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.password;
		}

		public override void Parse(string command)
		{
			string token = LineParser.GetQuotedToken(ref command);
			if (string.IsNullOrEmpty(token))
			{
				throw new Exception("Bad syntax, PASSWORD value expected.");
			}
			if (token.IndexOf('"') >= 0)
			{
				throw new Exception("Bad syntax, PASSWORD may not contain double quotes.");
			}
			this.NewPassword = token;
		}

		public override void Execute(int week)
		{
			this.Executed = false;
			Faction issuer = this.Subject as Faction;
			if (issuer == null)
			{
				Faction owner = this.Subject.Owner;
				if (owner != null)
				{
					owner.EventReports.Add(week, "PASSWORD failed. Use under #faction only.");
				}
				base.Execute(week);
				return;
			}

			issuer.PendingPassword = this.NewPassword;
			issuer.EventReports.Add(week, "password change scheduled for after this turn.");
			this.Executed = true;
			base.Execute(week);
		}

		public override void LoadXml(XmlElement elOrder)
		{
			XmlElement elPassword = (XmlElement)elOrder.SelectNodes("password")[0];
			this.NewPassword = elPassword.GetAttribute("value");
		}

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
			XmlElement elPassword = doc.CreateElement("password");
			elPassword.SetAttribute("value", this.NewPassword ?? string.Empty);
			this.xmlElement.AppendChild(elPassword);
			return this.xmlElement;
		}

		public override List<string> Report(Faction owner)
		{
			string prefix = string.Format("{0}{1}",
				this.Conditions,
				(this.Repeat == 1) ? string.Empty : ((this.Repeat < 0) ? "@" : string.Concat(this.Repeat.ToString(), " ")));
			return new List<string>
			{
				string.Format("{0}password \"{1}\"", prefix, this.NewPassword)
			};
		}
	}
}
