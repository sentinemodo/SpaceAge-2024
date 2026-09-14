using System;
using System.Xml;

namespace SpaceAge
{
	// Target stack destroyed in combat; killer faction recorded via NotifyStackDestroyed.
	public class DestroyStackTrigger : IContractTrigger
	{
		public string TypeName
		{
			get { return "destroy-stack"; }
		}

		public ModuleStack Target { get; set; }
		public Faction Killer { get; set; }

		public Faction Winner
		{
			get { return this.Killer; }
		}

		public DestroyStackTrigger(ModuleStack target)
		{
			if (target == null)
			{
				throw new Exception("Unknown target stack for destroy-stack contract.");
			}

			this.Target = target;
		}

		public static DestroyStackTrigger Load(XmlElement elContract)
		{
			ModuleStack target = ModuleStack.All[elContract.GetAttribute("target")];
			DestroyStackTrigger trigger = new DestroyStackTrigger(target);
			if (elContract.HasAttribute("killer") && Faction.All.ContainsKey(elContract.GetAttribute("killer")))
			{
				trigger.Killer = Faction.All[elContract.GetAttribute("killer")];
			}
			return trigger;
		}

		public void NotifyTransfer(Faction giver, ModuleStack giverStack, ModuleStack receiver, ModuleType moduleType, int quantity, Faction issuer)
		{
		}

		public void NotifyFactionTransfer(Faction giver, ModuleStack giverStack, Faction receiverFaction, ModuleType moduleType, int quantity, Region location, Faction issuer)
		{
		}

		public void NotifyResearch(Faction researcher, ModuleStack researcherStack, ModuleStack target, int points)
		{
		}

		public void NotifyStackDestroyed(Faction killer, ModuleStack stack)
		{
			if (stack == null || this.Target == null || stack.Name != this.Target.Name)
			{
				return;
			}
			if (killer == null || killer == stack.Owner)
			{
				return;
			}

			this.Killer = killer;
		}

		public bool IsComplete()
		{
			if (this.Killer == null || this.Target == null)
			{
				return false;
			}

			if (!ModuleStack.All.ContainsKey(this.Target.Name))
			{
				return true;
			}

			return !this.Target.HasIntactModules();
		}

		public void SaveAttributes(XmlElement elContract)
		{
			elContract.SetAttribute("target", this.Target.Name);
			if (this.Killer != null)
			{
				elContract.SetAttribute("killer", this.Killer.Name);
			}
		}
	}
}
