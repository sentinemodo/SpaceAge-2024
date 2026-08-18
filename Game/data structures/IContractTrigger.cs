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
		void NotifyResearch(Faction researcher, ModuleStack researcherStack, ModuleStack target, int points);
		bool IsComplete();
		void SaveAttributes(XmlElement elContract);
	}
}
