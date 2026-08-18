using System;
using System.Xml;

namespace SpaceAge
{
	public class ResearchWreckageTrigger : IContractTrigger
	{
		public string TypeName
		{
			get { return "research"; }
		}

		public ModuleStack Target { get; set; }
		public int RequiredPoints { get; set; }
		public int Progress { get; set; }
		public Faction Winner { get; set; }

		public ResearchWreckageTrigger(ModuleStack target, int requiredPoints)
		{
			if (target == null)
			{
				throw new Exception("Unknown wreckage stack for contract.");
			}
			if (requiredPoints < 1)
			{
				throw new Exception("Contract research points must be a positive amount.");
			}

			this.Target = target;
			this.RequiredPoints = requiredPoints;
			this.Progress = 0;
		}

		public static ResearchWreckageTrigger Load(XmlElement elContract)
		{
			ModuleStack target = ModuleStack.All[elContract.GetAttribute("target")];
			int required = Convert.ToInt32(elContract.GetAttribute("points"));
			ResearchWreckageTrigger trigger = new ResearchWreckageTrigger(target, required);
			if (elContract.HasAttribute("progress"))
			{
				trigger.Progress = Convert.ToInt32(elContract.GetAttribute("progress"));
			}
			if (elContract.HasAttribute("winner") && Faction.All.ContainsKey(elContract.GetAttribute("winner")))
			{
				trigger.Winner = Faction.All[elContract.GetAttribute("winner")];
			}
			return trigger;
		}

		public void NotifyTransfer(Faction giver, ModuleStack giverStack, ModuleStack receiver, ModuleType moduleType, int quantity, Faction issuer)
		{
		}

		public void NotifyResearch(Faction researcher, ModuleStack researcherStack, ModuleStack target, int points)
		{
			if (this.IsComplete())
			{
				return;
			}
			if (target != this.Target || researcher == null || points < 1)
			{
				return;
			}
			if (researcherStack == null || researcherStack.Location != this.Target.Location)
			{
				return;
			}

			this.Progress += points;
			this.Winner = researcher;
		}

		public bool IsComplete()
		{
			return this.Winner != null && this.Progress >= this.RequiredPoints;
		}

		public void SaveAttributes(XmlElement elContract)
		{
			elContract.SetAttribute("target", this.Target.Name);
			elContract.SetAttribute("points", this.RequiredPoints.ToString());
			elContract.SetAttribute("progress", this.Progress.ToString());
			if (this.Winner != null)
			{
				elContract.SetAttribute("winner", this.Winner.Name);
			}
		}
	}
}
