using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public enum FactionAttitude
	{
		// Enemy: fire on identifiable enemy units (also covers hostile interdictions).
		Enemy = 0,
		// Hostile: interdict resource use/construction, but do not fire.
		Hostile = 1,
		Neutral = 2,
		Friendly = 3,
		Ally = 4
	}

	public static class FactionAttitudeParser
	{
		public static FactionAttitude Parse(string token)
		{
			if (string.IsNullOrEmpty(token))
			{
				throw new Exception("Bad syntax, attitude expected.");
			}
			switch (token.ToLowerInvariant())
			{
				case "enemy":
					return FactionAttitude.Enemy;
				case "hostile":
					return FactionAttitude.Hostile;
				case "neutral":
					return FactionAttitude.Neutral;
				case "friendly":
					return FactionAttitude.Friendly;
				case "ally":
					return FactionAttitude.Ally;
				default:
					throw new Exception("Unknown attitude: " + token);
			}
		}

		public static string ToToken(FactionAttitude attitude)
		{
			return attitude.ToString().ToLowerInvariant();
		}
	}
}
