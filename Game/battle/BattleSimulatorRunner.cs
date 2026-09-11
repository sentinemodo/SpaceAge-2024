using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class BattleSimulatorRunner
	{
		private readonly string dataDir;

		public BattleSimulatorRunner(string dataDir)
		{
			this.dataDir = dataDir;
		}

		public string Run(string simulationFilePath, int? seedOverride = null)
		{
			DataFile dataFile = new DataFile(this.dataDir);
			dataFile.LoadConfiguration();
			Game catalogGame = dataFile.Game;

			XmlDocument doc = new XmlDocument();
			doc.Load(simulationFilePath);
			XmlElement root = doc.DocumentElement;
			if (root == null || root.Name != "battle-sim")
			{
				throw new Exception("Expected root element <battle-sim>.");
			}

			int seed = seedOverride ?? parseSeed(root);
			string locationType = root.GetAttribute("location-type");
			if (string.IsNullOrEmpty(locationType))
			{
				locationType = "orbit";
			}

			Sequence.Reset(seed);

			Location location = this.createSimLocation(locationType, catalogGame);
			XmlElement attackersElement = (XmlElement)root.SelectSingleNode("attackers");
			XmlElement defendersElement = (XmlElement)root.SelectSingleNode("defenders");
			if (attackersElement == null || defendersElement == null)
			{
				throw new Exception("Simulation requires <attackers> and <defenders> sections.");
			}

			Faction attackerFaction = this.createSimFaction(attackersElement);
			Faction defenderFaction = this.createSimFaction(defendersElement);
			this.setMutualEnmity(attackerFaction, defenderFaction);

			List<ModuleStack> attackerRoots = this.buildSideStacks(attackersElement, location, attackerFaction);
			List<ModuleStack> defenderRoots = this.buildSideStacks(defendersElement, location, defenderFaction);
			if (attackerRoots.Count == 0 || defenderRoots.Count == 0)
			{
				throw new Exception("Each side requires at least one <stack>.");
			}

			Dictionary<string, bool> intactBefore = this.recordIntactState(attackerRoots, defenderRoots);

			ModuleStack primaryAttacker = attackerRoots[0];
			ModuleStack primaryDefender = defenderRoots[0];
			Battle battle = new Battle(primaryAttacker, primaryDefender);
			battle.Execute(1);

			string output = this.formatOutput(
				seed,
				locationType,
				battle,
				attackerFaction,
				attackerRoots,
				defenderRoots,
				intactBefore);

			new Game().ClearDictionaries();
			return output;
		}

		private static int parseSeed(XmlElement root)
		{
			string seedText = root.GetAttribute("seed");
			if (string.IsNullOrEmpty(seedText))
			{
				return 1;
			}
			return Convert.ToInt32(seedText);
		}

		private Location createSimLocation(string locationType, Game catalogGame)
		{
			SpaceSystem system = new SpaceSystem("SIMSYS1");
			Planet planet = new Planet(system, "SIMP001");
			if (catalogGame.PlanetTypes.ContainsKey("dust"))
			{
				planet.PlanetType = catalogGame.PlanetTypes["dust"];
			}

			if (locationType == "region")
			{
				Region region = new Region(planet, "SIMR001");
				if (catalogGame.RegionTypes.ContainsKey("dust"))
				{
					region.RegionType = catalogGame.RegionTypes["dust"];
				}
				return region;
			}

			return new Orbit(planet, "SIMO001");
		}

		private Faction createSimFaction(XmlElement sideElement)
		{
			string factionId = sideElement.GetAttribute("faction");
			if (string.IsNullOrEmpty(factionId))
			{
				throw new Exception("Side element requires faction attribute.");
			}
			string sideName = sideElement.GetAttribute("name");
			if (string.IsNullOrEmpty(sideName))
			{
				sideName = factionId;
			}
			return new Faction(factionId, sideName);
		}

		private void setMutualEnmity(Faction attackerFaction, Faction defenderFaction)
		{
			attackerFaction.Attitudes[defenderFaction.Name] = FactionAttitude.Enemy;
			defenderFaction.Attitudes[attackerFaction.Name] = FactionAttitude.Enemy;
		}

		private List<ModuleStack> buildSideStacks(XmlElement sideElement, Location location, Faction owner)
		{
			List<ModuleStack> roots = new List<ModuleStack>();
			foreach (XmlElement stackElement in sideElement.SelectNodes("stack"))
			{
				roots.Add(this.buildStack(stackElement, location, owner));
			}
			return roots;
		}

		private ModuleStack buildStack(XmlElement stackElement, IHolder parent, Faction owner)
		{
			string id = stackElement.GetAttribute("id");
			if (string.IsNullOrEmpty(id))
			{
				id = stackElement.GetAttribute("name");
			}
			if (string.IsNullOrEmpty(id))
			{
				throw new Exception("Stack requires id or name attribute.");
			}

			string typeName = stackElement.GetAttribute("type");
			if (string.IsNullOrEmpty(typeName) || !ModuleType.All.ContainsKey(typeName))
			{
				throw new Exception("Unknown or missing stack type: " + typeName);
			}

			ModuleStack stack = new ModuleStack(parent, owner, ModuleType.All[typeName], id);
			if (stackElement.HasAttribute("name"))
			{
				stack.Description = stackElement.GetAttribute("name");
			}

			int quantity = this.parseQuantity(stackElement.GetAttribute("quantity"), 1);
			for (int i = 0; i < quantity; i++)
			{
				stack.AddModule();
			}

			XmlElement itemsElement = stackElement.SelectSingleNode("items") as XmlElement;
			if (itemsElement != null)
			{
				stack.ItemStacks.LoadXml(itemsElement, stack, "item");
			}

			XmlElement nestedElement = stackElement.SelectSingleNode("nested") as XmlElement;
			if (nestedElement != null)
			{
				foreach (XmlElement childElement in nestedElement.SelectNodes("stack"))
				{
					this.buildStack(childElement, stack, owner);
				}
			}

			string tactic = stackElement.GetAttribute("tactic");
			if (!string.IsNullOrEmpty(tactic))
			{
				if (tactic.StartsWith("prioritize "))
				{
					stack.ApplyPrioritizeTactic(tactic);
				}
				else
				{
					stack.ApplyTactic(tactic);
				}
			}

			stack.SetOnline(true);
			return stack;
		}

		private int parseQuantity(string quantityText, int defaultValue)
		{
			if (string.IsNullOrEmpty(quantityText))
			{
				return defaultValue;
			}
			return Convert.ToInt32(quantityText);
		}

		private Dictionary<string, bool> recordIntactState(List<ModuleStack> attackerRoots, List<ModuleStack> defenderRoots)
		{
			Dictionary<string, bool> intact = new Dictionary<string, bool>();
			this.recordStacksRecursive(attackerRoots, intact);
			this.recordStacksRecursive(defenderRoots, intact);
			return intact;
		}

		private void recordStacksRecursive(List<ModuleStack> roots, Dictionary<string, bool> intact)
		{
			foreach (ModuleStack root in roots)
			{
				this.recordStackRecursive(root, intact);
			}
		}

		private void recordStackRecursive(ModuleStack stack, Dictionary<string, bool> intact)
		{
			intact[stack.Name] = stack.HasIntactModules();
			foreach (ModuleStack nested in stack.ModuleStacks.Values)
			{
				this.recordStackRecursive(nested, intact);
			}
		}

		private string formatOutput(
			int seed,
			string locationType,
			Battle battle,
			Faction attackerFaction,
			List<ModuleStack> attackerRoots,
			List<ModuleStack> defenderRoots,
			Dictionary<string, bool> intactBefore)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("SpaceAge Battle Simulator v" + Program.EngineVersion);
			sb.AppendLine("Seed: " + seed);
			sb.AppendLine("Location: " + capitalizeLocationType(locationType));
			sb.AppendLine();

			List<string> report = battle.Report(attackerFaction);
			bool includeLine = false;
			foreach (string line in report)
			{
				string trimmed = line.TrimStart();
				if (trimmed.StartsWith("Round "))
				{
					includeLine = true;
				}
				if (includeLine)
				{
					sb.AppendLine(trimmed);
				}
			}

			sb.AppendLine("============================================================");
			sb.AppendLine("SIMULATION RESULT: " + this.determineResult(battle));
			sb.AppendLine("Rounds: " + battle.Round);
			sb.AppendLine("Casualties Attackers: " + this.describeCasualties(attackerRoots, intactBefore));
			sb.AppendLine("Casualties Defenders: " + this.describeCasualties(defenderRoots, intactBefore));
			sb.AppendLine("Captured Modules: none");
			sb.AppendLine("============================================================");
			return sb.ToString();
		}

		private static string capitalizeLocationType(string locationType)
		{
			if (string.IsNullOrEmpty(locationType))
			{
				return "Orbit";
			}
			return char.ToUpper(locationType[0]) + locationType.Substring(1);
		}

		private string determineResult(Battle battle)
		{
			if (battle.Attackers.Count > 0 && battle.Defenders.Count == 0)
			{
				return "ATTACKERS_WON";
			}
			if (battle.Attackers.Count == 0 && battle.Defenders.Count > 0)
			{
				return "DEFENDERS_WON";
			}
			return "INDECISIVE";
		}

		private string describeCasualties(List<ModuleStack> roots, Dictionary<string, bool> intactBefore)
		{
			List<string> casualties = new List<string>();
			foreach (ModuleStack root in roots)
			{
				this.collectCasualties(root, intactBefore, casualties);
			}
			if (casualties.Count == 0)
			{
				return "none";
			}
			return string.Join(", ", casualties.ToArray());
		}

		private void collectCasualties(ModuleStack stack, Dictionary<string, bool> intactBefore, List<string> casualties)
		{
			bool wasIntact = intactBefore.ContainsKey(stack.Name) && intactBefore[stack.Name];
			if (wasIntact && !stack.HasIntactModules())
			{
				casualties.Add(stack.ReportName + " [" + stack.Name + "] wrecked");
			}
			foreach (ModuleStack nested in stack.ModuleStacks.Values)
			{
				this.collectCasualties(nested, intactBefore, casualties);
			}
		}
	}
}
