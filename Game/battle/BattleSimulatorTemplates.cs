using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public static class BattleSimulatorTemplates
	{
		private static readonly Dictionary<string, BattleSimTemplate> catalog = buildCatalog();

		public static IEnumerable<BattleSimTemplate> All
		{
			get { return catalog.Values; }
		}

		public static BattleSimTemplate Get(string id)
		{
			if (!catalog.ContainsKey(id))
			{
				throw new System.Exception("Unknown battle sim template: " + id);
			}
			return catalog[id];
		}

		public static string ToSimInputXml(BattleSimTemplate template, int seed, string locationType = "orbit")
		{
			return ToSimInputXml(
				template.Stack,
				"sim_a",
				"Attacker " + template.DisplayName,
				template.Stack,
				"sim_d",
				"Defender " + template.DisplayName,
				seed,
				locationType);
		}

		public static string ToSimInputXml(
			BattleSimStackTemplate attackerStack,
			string attackerFaction,
			string attackerName,
			BattleSimStackTemplate defenderStack,
			string defenderFaction,
			string defenderName,
			int seed,
			string locationType = "orbit")
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("<?xml version=\"1.0\" encoding=\"windows-1251\"?>");
			sb.AppendLine(string.Format("<battle-sim seed=\"{0}\" location-type=\"{1}\">", seed, locationType));
			appendSide(sb, "attackers", attackerFaction, attackerName, cloneStack(attackerStack, "a"), "a");
			appendSide(sb, "defenders", defenderFaction, defenderName, cloneStack(defenderStack, "d"), "d");
			sb.AppendLine("</battle-sim>");
			return sb.ToString();
		}

		private static void appendSide(StringBuilder sb, string sideTag, string faction, string sideName, BattleSimStackTemplate stack, string idPrefix)
		{
			sb.AppendLine(string.Format("  <{0} faction=\"{1}\" name=\"{2}\">", sideTag, faction, sideName));
			appendStack(sb, stack, "    ");
			sb.AppendLine(string.Format("  </{0}>", sideTag));
		}

		private static BattleSimStackTemplate cloneStack(BattleSimStackTemplate source, string idPrefix)
		{
			BattleSimStackTemplate clone = new BattleSimStackTemplate
			{
				Id = "sim_" + idPrefix + "1",
				Type = source.Type,
				Name = source.Name,
				Quantity = source.Quantity,
				Tactic = source.Tactic
			};
			foreach (BattleSimItemTemplate item in source.Items)
			{
				clone.Items.Add(new BattleSimItemTemplate { Type = item.Type, Quantity = item.Quantity });
			}
			int nestedIndex = 1;
			foreach (BattleSimStackTemplate nested in source.Nested)
			{
				BattleSimStackTemplate nestedClone = cloneStackNested(nested, idPrefix, nestedIndex);
				clone.Nested.Add(nestedClone);
				nestedIndex++;
			}
			return clone;
		}

		private static BattleSimStackTemplate cloneStackNested(BattleSimStackTemplate source, string idPrefix, int nestedIndex)
		{
			BattleSimStackTemplate clone = new BattleSimStackTemplate
			{
				Id = "sim_" + idPrefix + "1_n" + nestedIndex,
				Type = source.Type,
				Name = source.Name,
				Quantity = source.Quantity,
				Tactic = source.Tactic
			};
			foreach (BattleSimItemTemplate item in source.Items)
			{
				clone.Items.Add(new BattleSimItemTemplate { Type = item.Type, Quantity = item.Quantity });
			}
			foreach (BattleSimStackTemplate nested in source.Nested)
			{
				clone.Nested.Add(cloneStackNested(nested, idPrefix, nestedIndex));
			}
			return clone;
		}

		private static void appendStack(StringBuilder sb, BattleSimStackTemplate stack, string indent)
		{
			sb.Append(string.Format("{0}<stack id=\"{1}\" type=\"{2}\"", indent, stack.Id, stack.Type));
			if (!string.IsNullOrEmpty(stack.Name))
			{
				sb.Append(string.Format(" name=\"{0}\"", stack.Name));
			}
			if (stack.Quantity != 1)
			{
				sb.Append(string.Format(" quantity=\"{0}\"", stack.Quantity));
			}
			if (!string.IsNullOrEmpty(stack.Tactic))
			{
				sb.Append(string.Format(" tactic=\"{0}\"", stack.Tactic));
			}
			if (stack.Items.Count == 0 && stack.Nested.Count == 0)
			{
				sb.AppendLine("/>");
				return;
			}
			sb.AppendLine(">");
			if (stack.Items.Count > 0)
			{
				sb.AppendLine(indent + "  <items>");
				foreach (BattleSimItemTemplate item in stack.Items)
				{
					sb.AppendLine(string.Format("{0}    <item type=\"{1}\" quantity=\"{2}\" />", indent, item.Type, item.Quantity));
				}
				sb.AppendLine(indent + "  </items>");
			}
			if (stack.Nested.Count > 0)
			{
				sb.AppendLine(indent + "  <nested>");
				foreach (BattleSimStackTemplate nested in stack.Nested)
				{
					appendStack(sb, nested, indent + "    ");
				}
				sb.AppendLine(indent + "  </nested>");
			}
			sb.AppendLine(indent + "</stack>");
		}

		private static List<BattleSimStackTemplate> standardOrbitSupport()
		{
			return new List<BattleSimStackTemplate>
			{
				new BattleSimStackTemplate
				{
					Type = "fisrec",
					Name = "Reactor",
					Quantity = 1,
					Items = new List<BattleSimItemTemplate>
					{
						new BattleSimItemTemplate { Type = "uraniu", Quantity = 10 },
						new BattleSimItemTemplate { Type = "terran", Quantity = 3 }
					}
				},
				new BattleSimStackTemplate
				{
					Type = "crwqrt",
					Name = "Crew Quarters",
					Quantity = 1,
					Items = new List<BattleSimItemTemplate>
					{
						new BattleSimItemTemplate { Type = "terran", Quantity = 12 },
						new BattleSimItemTemplate { Type = "food", Quantity = 100 }
					}
				}
			};
		}

		private static Dictionary<string, BattleSimTemplate> buildCatalog()
		{
			Dictionary<string, BattleSimTemplate> templates = new Dictionary<string, BattleSimTemplate>();

			List<BattleSimStackTemplate> corvetteNested = new List<BattleSimStackTemplate>
			{
				new BattleSimStackTemplate { Type = "pdltur", Name = "Point-Defense Laser", Quantity = 2 },
				new BattleSimStackTemplate { Type = "cermpl", Name = "Ceramic Armour", Quantity = 2 }
			};
			corvetteNested.AddRange(standardOrbitSupport());

			templates.Add("system-patrol-corvette", new BattleSimTemplate
			{
				Id = "system-patrol-corvette",
				DisplayName = "System Patrol Corvette",
				Role = "Light Patrol",
				Stack = new BattleSimStackTemplate
				{
					Type = "corhul",
					Name = "Patrol Corvette",
					Tactic = "destroy",
					Nested = corvetteNested
				}
			});

			List<BattleSimStackTemplate> frigateNested = new List<BattleSimStackTemplate>
			{
				new BattleSimStackTemplate { Type = "railgn", Name = "Railgun", Quantity = 2 },
				new BattleSimStackTemplate { Type = "shplas", Name = "Plasma Shield", Quantity = 1 },
				new BattleSimStackTemplate { Type = "cargob", Name = "Cargo Bay", Quantity = 1 }
			};
			frigateNested.AddRange(standardOrbitSupport());

			templates.Add("escort-frigate", new BattleSimTemplate
			{
				Id = "escort-frigate",
				DisplayName = "Escort Frigate",
				Role = "Defense / Anti-Craft",
				Stack = new BattleSimStackTemplate
				{
					Type = "alnhul",
					Name = "Escort Frigate",
					Tactic = "destroy",
					Nested = frigateNested
				}
			});

			List<BattleSimStackTemplate> droneNested = new List<BattleSimStackTemplate>
			{
				new BattleSimStackTemplate { Type = "drnbay", Name = "Fighter Drone Bay", Quantity = 1 },
				new BattleSimStackTemplate
				{
					Type = "alndrn",
					Name = "Fighter Drone",
					Quantity = 6,
					Tactic = "evade",
					Items = new List<BattleSimItemTemplate>
					{
						new BattleSimItemTemplate { Type = "heliu3", Quantity = 4 }
					}
				},
				new BattleSimStackTemplate { Type = "shplas", Name = "Plasma Shield", Quantity = 1 }
			};
			droneNested.AddRange(standardOrbitSupport());

			templates.Add("drone-carrier-frigate", new BattleSimTemplate
			{
				Id = "drone-carrier-frigate",
				DisplayName = "Drone Carrier Frigate",
				Role = "Drone Swarm",
				Stack = new BattleSimStackTemplate
				{
					Type = "alnhul",
					Name = "Drone Carrier",
					Tactic = "evade",
					Nested = droneNested
				}
			});

			List<BattleSimStackTemplate> destroyerNested = new List<BattleSimStackTemplate>
			{
				new BattleSimStackTemplate { Type = "coilgn", Name = "Coilgun", Quantity = 1 },
				new BattleSimStackTemplate { Type = "crumis", Name = "Cruise Missiles", Quantity = 1 },
				new BattleSimStackTemplate { Type = "ciwst", Name = "CIWS", Quantity = 1 },
				new BattleSimStackTemplate { Type = "armplt", Name = "Spaced Armour", Quantity = 2 }
			};
			destroyerNested.AddRange(standardOrbitSupport());

			templates.Add("line-destroyer", new BattleSimTemplate
			{
				Id = "line-destroyer",
				DisplayName = "Line Destroyer",
				Role = "Fleet Combat",
				Stack = new BattleSimStackTemplate
				{
					Type = "deshul",
					Name = "Line Destroyer",
					Tactic = "destroy",
					Nested = destroyerNested
				}
			});

			templates.Add("planetary-defense-battery", new BattleSimTemplate
			{
				Id = "planetary-defense-battery",
				DisplayName = "Planetary Defense Battery",
				Role = "Static Ground",
				Stack = new BattleSimStackTemplate
				{
					Type = "gunplc",
					Name = "Defense Battery",
					Tactic = "destroy",
					Quantity = 4
				}
			});

			templates.Add("laser-emplacement", new BattleSimTemplate
			{
				Id = "laser-emplacement",
				DisplayName = "Laser Emplacement",
				Role = "Static Laser",
				Stack = new BattleSimStackTemplate
				{
					Type = "laztrt",
					Name = "Laser Emplacement",
					Tactic = "destroy"
				}
			});

			templates.Add("armored-tank-platoon", new BattleSimTemplate
			{
				Id = "armored-tank-platoon",
				DisplayName = "Armored Tank Platoon",
				Role = "Ground Armor",
				Stack = new BattleSimStackTemplate
				{
					Type = "tanks",
					Name = "Tank Platoon",
					Tactic = "destroy",
					Items = new List<BattleSimItemTemplate>
					{
						new BattleSimItemTemplate { Type = "oil", Quantity = 16 },
						new BattleSimItemTemplate { Type = "food", Quantity = 16 }
					}
				}
			});

			templates.Add("infantry-battalion", new BattleSimTemplate
			{
				Id = "infantry-battalion",
				DisplayName = "Infantry Battalion",
				Role = "Garrison / Capture",
				Stack = new BattleSimStackTemplate
				{
					Type = "inftry",
					Name = "Infantry Battalion",
					Tactic = "capture",
					Items = new List<BattleSimItemTemplate>
					{
						new BattleSimItemTemplate { Type = "food", Quantity = 100 },
						new BattleSimItemTemplate { Type = "terair", Quantity = 100 }
					}
				}
			});

			return templates;
		}
	}
}
