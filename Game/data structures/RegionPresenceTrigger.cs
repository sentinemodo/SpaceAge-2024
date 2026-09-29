using System.Xml;

namespace SpaceAge
{
	// Completes when a player faction (not the issuer) has a root stack in the
	// contract anchor region or in orbit of the same planet/moon body.
	public class RegionPresenceTrigger : IContractTrigger
	{
		public string TypeName
		{
			get { return "region-presence"; }
		}

		public Region AnchorRegion { get; set; }
		public Faction Issuer { get; set; }
		public Faction Winner { get; set; }

		public RegionPresenceTrigger(Region anchorRegion, Faction issuer)
		{
			this.AnchorRegion = anchorRegion;
			this.Issuer = issuer;
		}

		public static RegionPresenceTrigger Load(XmlElement elContract, Region location, Faction issuer)
		{
			RegionPresenceTrigger trigger = new RegionPresenceTrigger(location, issuer);
			if (elContract.HasAttribute("winner") && Faction.All.ContainsKey(elContract.GetAttribute("winner")))
			{
				trigger.Winner = Faction.All[elContract.GetAttribute("winner")];
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
		}

		public bool IsComplete()
		{
			if (this.Winner != null)
			{
				return true;
			}

			if (this.AnchorRegion == null)
			{
				return false;
			}

			Faction best = null;
			foreach (ModuleStack stack in ModuleStack.All.Values)
			{
				if (!ContractScope.IsPlayerFaction(stack.Owner))
				{
					continue;
				}
				if (this.Issuer != null && stack.Owner == this.Issuer)
				{
					continue;
				}
				if (!ContractScope.StackCountsAsPresence(stack, this.AnchorRegion))
				{
					continue;
				}

				if (best == null || string.Compare(stack.Owner.Name, best.Name, System.StringComparison.Ordinal) < 0)
				{
					best = stack.Owner;
				}
			}

			if (best != null)
			{
				this.Winner = best;
				return true;
			}

			return false;
		}

		public void SaveAttributes(XmlElement elContract)
		{
			if (this.Winner != null)
			{
				elContract.SetAttribute("winner", this.Winner.Name);
			}
		}
	}
}
