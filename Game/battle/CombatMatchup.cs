using System;

namespace SpaceAge
{
	public static class CombatMatchup
	{
		public static double ChanceMultiplier(string weaponGroup, string resists)
		{
			if (string.IsNullOrEmpty(weaponGroup))
			{
				return 1.0;
			}
			string resist = resists ?? string.Empty;
			switch (weaponGroup)
			{
				case "laser":
					if (resist == "shield") return 0.5;
					return 1.5;
				case "kinetic":
					if (resist == "armour" || resist == "armor") return 0.5;
					return 1.5;
				case "missile":
					if (resist == "pbpd") return 0.5;
					return 1.5;
				case "drone":
					if (resist == "ew") return 0.5;
					return 1.5;
				default:
					return 1.0;
			}
		}

		public static bool IsArmor(ModuleType type)
		{
			if (type == null)
			{
				return false;
			}
			if (type.ArmorModule)
			{
				return true;
			}
			return type.Resists == "armour" || type.Resists == "armor";
		}

		public static bool IsShield(ModuleType type)
		{
			return type != null && type.Resists == "shield";
		}

		public static int ShieldIntercept(int damage)
		{
			if (damage <= 0)
			{
				return 0;
			}
			return (int)Math.Floor(damage * 0.9);
		}
	}
}
