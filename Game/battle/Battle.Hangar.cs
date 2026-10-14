using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public partial class Battle
	{
		private void launchHangarCraft()
		{
			Location location = null;
			if (this.attacker != null)
			{
				location = this.attacker.Location;
			}
			if (location == null && this.battleDefender != null)
			{
				location = this.battleDefender.Location;
			}
			if (location == null)
			{
				return;
			}

			List<ModuleStack> toLaunch = new List<ModuleStack>();
			foreach (ModuleStack stack in stacksAtLocation(location))
			{
				if (!stack.IsHangarCraft)
				{
					continue;
				}
				ModuleStack parent = stack.Parent as ModuleStack;
				if (parent == null || parent.ModuleType == null || !parent.ModuleType.IsDroneBay)
				{
					continue;
				}
				toLaunch.Add(stack);
			}

			foreach (ModuleStack craft in toLaunch)
			{
				this.addLaunchedHangarCraft(craft);
			}
		}

		private void addLaunchedHangarCraft(ModuleStack craft)
		{
			ModuleStack root = craft.RootModuleStack;
			bool onAttack = Battle.sideContains(this.attackers, craft) || Battle.sideContains(this.attackers, root);
			bool onDefense = Battle.sideContains(this.defenders, craft) || Battle.sideContains(this.defenders, root);

			ModuleStack bay = craft.Parent as ModuleStack;
			ModuleStack carrier = craft.RootModuleStack;
			string bayName = bay != null && bay.ModuleType != null
				? bay.ModuleType.ReportName
				: "hangar";
			string carrierName = carrier != null ? carrier.ReportName : craft.ReportName;

			craft.Parent = craft.Location;
			if (!this.launchedHangarCraft.Contains(craft))
			{
				this.launchedHangarCraft.Add(craft);
			}
			if (carrier != null && !this.hangarLaunchCarriers.Contains(carrier))
			{
				this.hangarLaunchCarriers.Add(carrier);
			}
			this.report(string.Format("{0} launches {1} from {2}.", carrierName, craft.ReportName, bayName));

			if (Battle.sideContains(this.attackers, craft) || Battle.sideContains(this.defenders, craft))
			{
				return;
			}
			if (onAttack && !onDefense)
			{
				this.attackers.Add(craft);
			}
			else if (onDefense)
			{
				this.defenders.Add(craft);
			}
		}

		private static bool sideContains(ModuleStacks side, ModuleStack stack)
		{
			return side != null && stack != null && side.Contains(stack.Name);
		}
	}
}
