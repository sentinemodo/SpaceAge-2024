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

		public static bool IsVisibleTo(Faction faction, string scopeId)
		{
			if (faction == null)
			{
				return false;
			}
			if (string.IsNullOrEmpty(scopeId))
			{
				return true;
			}
			if (Planet.All.ContainsKey(scopeId))
			{
				return FactionHasStacksOnPlanet(faction, Planet.All[scopeId]);
			}
			if (Moon.All.ContainsKey(scopeId))
			{
				return FactionHasStacksOnMoon(faction, Moon.All[scopeId]);
			}
			return false;
		}

		private static bool FactionHasStacksOnPlanet(Faction faction, Planet planet)
		{
			foreach (Region region in planet.Regions.Values)
			{
				if (region.ModuleStacks.Contains(faction))
				{
					return true;
				}
			}
			if (planet.Orbit != null && planet.Orbit.ModuleStacks.Contains(faction))
			{
				return true;
			}
			return false;
		}

		private static bool FactionHasStacksOnMoon(Faction faction, Moon moon)
		{
			foreach (Region region in moon.Regions.Values)
			{
				if (region.ModuleStacks.Contains(faction))
				{
					return true;
				}
			}
			if (moon.Orbit != null && moon.Orbit.ModuleStacks.Contains(faction))
			{
				return true;
			}
			return false;
		}

		public static string ScopeReportName(string scopeId)
		{
			if (string.IsNullOrEmpty(scopeId))
			{
				return string.Empty;
			}
			if (Planet.All.ContainsKey(scopeId))
			{
				return Planet.All[scopeId].ReportName;
			}
			if (Moon.All.ContainsKey(scopeId))
			{
				return Moon.All[scopeId].ReportName;
			}
			return scopeId;
		}

		public static string ResolveScopeId(Region location)
		{
			if (location == null || location.RegionHolder == null)
			{
				return string.Empty;
			}
			Planet planet = location.RegionHolder as Planet;
			if (planet != null)
			{
				return planet.Name;
			}
			Moon moon = location.RegionHolder as Moon;
			if (moon != null)
			{
				return moon.Name;
			}
			return string.Empty;
		}

		public static List<string> ReportPublications(Faction faction)
		{
			List<string> lines = new List<string>();
			lines.AddRange(ReportPress(faction));
			lines.AddRange(ReportRumors(faction));
			return lines;
		}

		public static List<string> ReportPublicationsForScope(Faction faction, string scopeId)
		{
			List<string> lines = new List<string>();
			lines.AddRange(ReportPress(faction, scopeId));
			lines.AddRange(ReportRumors(faction, scopeId));
			return lines;
		}

		public static List<string> ReportPress(Faction faction)
		{
			return ReportPress(faction, null);
		}

		public static List<string> ReportPress(Faction faction, string scopeFilter)
		{
			List<string> lines = new List<string>();
			bool headerAdded = false;
			foreach (PressRelease release in PressRelease.All)
			{
				if (release.Anonymous || release.Issuer == null)
				{
					continue;
				}
				if (!string.IsNullOrEmpty(scopeFilter)
					&& !string.Equals(release.PlanetId, scopeFilter, System.StringComparison.Ordinal))
				{
					continue;
				}
				if (!IsVisibleTo(faction, release.PlanetId))
				{
					continue;
				}
				if (!headerAdded)
				{
					lines.Add("Press releases:");
					headerAdded = true;
				}
				if (!string.IsNullOrEmpty(release.PlanetId))
				{
					lines.Add(string.Format("  {0}: {1} — {2}.",
						ScopeReportName(release.PlanetId),
						release.Issuer.ReportName,
						release.Title));
				}
				else
				{
					lines.Add(string.Format("  {0}: {1}.",
						release.Issuer.ReportName,
						release.Title));
				}
				if (!string.IsNullOrEmpty(release.Flavour))
				{
					lines.Add(string.Format("    {0}", release.Flavour));
				}
			}
			return lines;
		}

		public static List<string> ReportRumors()
		{
			return ReportRumors(null, null);
		}

		public static List<string> ReportRumors(Faction faction)
		{
			return ReportRumors(faction, null);
		}

		public static List<string> ReportRumors(Faction faction, string scopeFilter)
		{
			List<string> lines = new List<string>();
			bool headerAdded = false;
			foreach (PressRelease release in PressRelease.All)
			{
				if (!release.Anonymous)
				{
					continue;
				}
				if (!string.IsNullOrEmpty(scopeFilter)
					&& !string.Equals(release.PlanetId, scopeFilter, System.StringComparison.Ordinal))
				{
					continue;
				}
				if (faction != null && !IsVisibleTo(faction, release.PlanetId))
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
				else if (!string.IsNullOrEmpty(release.PlanetId) && Moon.All.ContainsKey(release.PlanetId))
				{
					lines.Add(string.Format("  {0}: {1}.",
						Moon.All[release.PlanetId].ReportName,
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
					elPress.GetAttribute("planet"));
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
					if (!string.IsNullOrEmpty(release.PlanetId))
					{
						elPress.SetAttribute("planet", release.PlanetId);
					}
					elPress.SetAttribute("title", release.Title ?? string.Empty);
					elPress.SetAttribute("flavour", release.Flavour ?? string.Empty);
					elPublications.AppendChild(elPress);
				}
			}
			return elPublications;
		}
	}
}
