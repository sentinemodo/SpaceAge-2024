using System.Xml;

namespace SpaceAge
{
	// Location-scoped contract completion rule. New trigger kinds (e.g. research)
	// implement this without changing Contract persistence or weekly evaluation.
	public interface IContractTrigger
	{
		string TypeName { get; }
		Faction Winner { get; }

		void NotifyTransfer(Faction giver, ModuleStack giverStack, ModuleStack receiver, ModuleType moduleType, int quantity, Faction issuer);
		void NotifyFactionTransfer(Faction giver, ModuleStack giverStack, Faction receiverFaction, ModuleType moduleType, int quantity, Region location, Faction issuer);
		void NotifyResearch(Faction researcher, ModuleStack researcherStack, ModuleStack target, int points);
		void NotifyStackDestroyed(Faction killer, ModuleStack stack);
		bool IsComplete();
		void SaveAttributes(XmlElement elContract);
	}
}
