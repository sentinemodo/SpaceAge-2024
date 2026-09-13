using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	public class PressRelease
	{
		public static readonly List<PressRelease> All = new List<PressRelease>();

		public Faction Issuer { get; set; }
		public string Title { get; set; }
		public string Flavour { get; set; }
		public bool Anonymous { get; set; }
		public string PlanetId { get; set; }
		public bool CreatedThisSession { get; set; }

		public PressRelease(Faction issuer, string title, string flavour)
			: this(issuer, title, flavour, false, string.Empty)
		{
		}

		public PressRelease(Faction issuer, string title, string flavour, bool anonymous, string planetId)
		{
			this.Issuer = issuer;
			this.Title = title ?? string.Empty;
			this.Flavour = flavour ?? string.Empty;
			this.Anonymous = anonymous;
			this.PlanetId = planetId ?? string.Empty;
			this.CreatedThisSession = true;
			PressRelease.All.Add(this);
		}

		private static PressRelease CreateLoaded(Faction issuer, string title, string flavour, bool anonymous, string planetId)
		{
			PressRelease release = new PressRelease();
			release.Issuer = issuer;
			release.Title = title ?? string.Empty;
			release.Flavour = flavour ?? string.Empty;
			release.Anonymous = anonymous;
			release.PlanetId = planetId ?? string.Empty;
			release.CreatedThisSession = false;
			PressRelease.All.Add(release);
			return release;
		}

		private PressRelease()
		{
		}

		public static List<string> ReportRumors()
		{
			List<string> lines = new List<string>();
			bool headerAdded = false;
			foreach (PressRelease release in PressRelease.All)
			{
				if (!release.Anonymous)
				{
					continue;
				}
				if (!headerAdded)
				{
					lines.Add("Rumors:");
					headerAdded = true;
				}
				if (!string.IsNullOrEmpty(release.PlanetId) && Planet.All.ContainsKey(release.PlanetId))
				{
					lines.Add(string.Format("  {0}: {1}.",
						Planet.All[release.PlanetId].ReportName,
						release.Title));
				}
				else
				{
					lines.Add(string.Format("  {0}.", release.Title));
				}
				if (!string.IsNullOrEmpty(release.Flavour))
				{
					lines.Add(string.Format("    {0}", release.Flavour));
				}
			}
			return lines;
		}

		public static void LoadXml(XmlElement elPublications)
		{
			if (elPublications == null)
			{
				return;
			}

			foreach (XmlElement elRumor in elPublications.SelectNodes("rumor"))
			{
				CreateLoaded(
					null,
					elRumor.GetAttribute("title"),
					elRumor.GetAttribute("flavour"),
					true,
					elRumor.GetAttribute("planet"));
			}

			foreach (XmlElement elPress in elPublications.SelectNodes("press"))
			{
				string issuerName = elPress.GetAttribute("issuer");
				Faction issuer = Faction.All.ContainsKey(issuerName) ? Faction.All[issuerName] : null;
				CreateLoaded(
					issuer,
					elPress.GetAttribute("title"),
					elPress.GetAttribute("flavour"),
					false,
					string.Empty);
			}
		}

		public static XmlElement SaveXml(XmlDocument doc)
		{
			if (PressRelease.All.Count == 0)
			{
				return null;
			}

			XmlElement elPublications = doc.CreateElement("publications");
			foreach (PressRelease release in PressRelease.All)
			{
				if (release.Anonymous)
				{
					XmlElement elRumor = doc.CreateElement("rumor");
					elRumor.SetAttribute("planet", release.PlanetId ?? string.Empty);
					elRumor.SetAttribute("title", release.Title ?? string.Empty);
					elRumor.SetAttribute("flavour", release.Flavour ?? string.Empty);
					elPublications.AppendChild(elRumor);
				}
				else if (release.Issuer != null)
				{
					XmlElement elPress = doc.CreateElement("press");
					elPress.SetAttribute("issuer", release.Issuer.Name);
					elPress.SetAttribute("title", release.Title ?? string.Empty);
					elPress.SetAttribute("flavour", release.Flavour ?? string.Empty);
					elPublications.AppendChild(elPress);
				}
			}
			return elPublications;
		}
	}
}
