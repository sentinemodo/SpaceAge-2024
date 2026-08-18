using System;
using System.Xml;

namespace SpaceAge
{
	// First qualifying module delivery: count on the receiver must reach
	// baseline + quantity, and a non-issuer / non-self giver must be recorded.
	public class GiveModuleTrigger : IContractTrigger
	{
		public string TypeName
		{
			get { return "give-module"; }
		}

		public int Quantity { get; set; }
		public ModuleType ModuleType { get; set; }
		public ModuleStack Receiver { get; set; }
		public int Baseline { get; set; }
		public Faction LastGiver { get; set; }
		public ModuleStack LastGiverStack { get; set; }

		public Faction Winner
		{
			get { return this.LastGiver; }
		}

		public GiveModuleTrigger(int quantity, ModuleType moduleType, ModuleStack receiver)
		{
			if (quantity < 1)
			{
				throw new Exception("Contract quantity must be a positive amount.");
			}
			if (moduleType == null)
			{
				throw new Exception("Unknown module type for contract.");
			}
			if (receiver == null)
			{
				throw new Exception("Unknown receiver stack for contract.");
			}

			this.Quantity = quantity;
			this.ModuleType = moduleType;
			this.Receiver = receiver;
			this.Baseline = receiver.ModuleCountRecursive(moduleType);
		}

		public static GiveModuleTrigger Load(XmlElement elContract)
		{
			int quantity = Convert.ToInt32(elContract.GetAttribute("quantity"));
			ModuleType moduleType = ModuleType.All[elContract.GetAttribute("module")];
			ModuleStack receiver = ModuleStack.All[elContract.GetAttribute("receiver")];
			GiveModuleTrigger trigger = new GiveModuleTrigger(quantity, moduleType, receiver);
			if (elContract.HasAttribute("baseline"))
			{
				trigger.Baseline = Convert.ToInt32(elContract.GetAttribute("baseline"));
			}
			return trigger;
		}

		public void NotifyResearch(Faction researcher, ModuleStack researcherStack, ModuleStack target, int points)
		{
		}

		public void NotifyTransfer(Faction giver, ModuleStack giverStack, ModuleStack receiver, ModuleType moduleType, int quantity, Faction issuer)
		{
			if (receiver != this.Receiver)
			{
				return;
			}
			if (moduleType != this.ModuleType)
			{
				return;
			}
			if (giver == null)
			{
				return;
			}
			if (giver == issuer)
			{
				return;
			}
			if (giver == this.Receiver.Owner)
			{
				return;
			}

			this.LastGiver = giver;
			if (giverStack != null)
			{
				this.LastGiverStack = giverStack;
			}
		}

		public bool IsComplete()
		{
			if (this.LastGiver == null)
			{
				return false;
			}
			return this.Receiver.ModuleCountRecursive(this.ModuleType) >= this.Baseline + this.Quantity;
		}

		public void SaveAttributes(XmlElement elContract)
		{
			elContract.SetAttribute("quantity", this.Quantity.ToString());
			elContract.SetAttribute("module", this.ModuleType.Name);
			elContract.SetAttribute("receiver", this.Receiver.Name);
			elContract.SetAttribute("baseline", this.Baseline.ToString());
		}
	}
}
