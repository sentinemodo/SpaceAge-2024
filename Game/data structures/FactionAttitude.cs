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
}
