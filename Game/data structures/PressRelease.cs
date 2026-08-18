using System.Collections.Generic;

namespace SpaceAge
{
	public class PressRelease
	{
		public static readonly List<PressRelease> All = new List<PressRelease>();

		public Faction Issuer { get; set; }
		public string Title { get; set; }
		public string Flavour { get; set; }
		public bool CreatedThisSession { get; set; }

		public PressRelease(Faction issuer, string title, string flavour)
		{
			this.Issuer = issuer;
			this.Title = title ?? string.Empty;
			this.Flavour = flavour ?? string.Empty;
			this.CreatedThisSession = true;
			PressRelease.All.Add(this);
		}
	}
}
