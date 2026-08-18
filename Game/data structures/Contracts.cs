using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class Contracts : List<Contract>
	{
		public Contract this[string name]
		{
			get
			{
				foreach (Contract contract in this)
				{
					if (contract.Name == name)
					{
						return contract;
					}
				}
				return null;
			}
		}

		public Contracts this[Region location]
		{
			get
			{
				Contracts contracts = new Contracts();
				foreach (Contract contract in this)
				{
					if (contract.Location == location)
					{
						contracts.Add(contract);
					}
				}
				return contracts;
			}
		}

		public bool Contains(string name)
		{
			return this[name] != null;
		}

		public new void Clear()
		{
			base.Clear();
		}

		public void NotifyTransfer(Faction giver, ModuleStack receiver, ModuleType moduleType, int quantity)
		{
			this.NotifyTransfer(giver, null, receiver, moduleType, quantity);
		}

		public void NotifyTransfer(Faction giver, ModuleStack giverStack, ModuleStack receiver, ModuleType moduleType, int quantity)
		{
			foreach (Contract contract in this)
			{
				if (contract.Trigger != null)
				{
					contract.Trigger.NotifyTransfer(giver, giverStack, receiver, moduleType, quantity, contract.Issuer);
				}
			}
		}

		public void NotifyResearch(Faction researcher, ModuleStack researcherStack, ModuleStack target, int points)
		{
			foreach (Contract contract in this)
			{
				if (contract.Trigger != null)
				{
					contract.Trigger.NotifyResearch(researcher, researcherStack, target, points);
				}
			}
		}

		public void Evaluate(int week)
		{
			List<Contract> snapshot = new List<Contract>(this);
			foreach (Contract contract in snapshot)
			{
				if (contract.Evaluate(week))
				{
					this.Remove(contract);
				}
			}
		}

		public List<string> Report(Region location)
		{
			List<string> lines = new List<string>();
			Contracts open = this[location];
			if (open.Count == 0)
			{
				return lines;
			}

			lines.Add("Contracts:");
			foreach (Contract contract in open)
			{
				lines.AddRange(contract.Report());
			}
			return lines;
		}

		public XmlElement SaveXml(XmlDocument doc)
		{
			XmlElement elContracts = doc.CreateElement("contracts");
			foreach (Contract contract in this)
			{
				elContracts.AppendChild(contract.SaveXml(doc));
			}
			return elContracts;
		}

		public void LoadXml(XmlElement elContracts)
		{
			if (elContracts == null)
			{
				return;
			}

			foreach (XmlElement elContract in elContracts.SelectNodes("contract"))
			{
				Contract.Load(elContract);
			}
		}

		// announce.{turn}.{faction}.txt for factions with stacks at a newly published location.
		public void WriteAnnouncements(string turnDir, Game game)
		{
			List<Contract> created = new List<Contract>();
			foreach (Contract contract in this)
			{
				if (contract.CreatedThisSession)
				{
					created.Add(contract);
				}
			}

			List<PressRelease> press = new List<PressRelease>();
			foreach (PressRelease release in PressRelease.All)
			{
				if (release.CreatedThisSession)
				{
					press.Add(release);
				}
			}

			if (created.Count == 0 && press.Count == 0)
			{
				return;
			}

			foreach (Faction faction in Faction.All.Values)
			{
				List<Contract> visible = new List<Contract>();
				foreach (Contract contract in created)
				{
					if (contract.Location != null && contract.Location.ModuleStacks.Contains(faction))
					{
						visible.Add(contract);
					}
				}
				if (visible.Count == 0 && press.Count == 0)
				{
					continue;
				}

				string fileName = string.Format("announce.{0}.{1}.txt", game.Turn, faction.Name);
				string path = Path.Combine(turnDir, fileName);
				TextWriter writer = new StreamWriter(path, false, Encoding.GetEncoding(1251));
				writer.WriteLine("To: " + faction.Email);
				writer.WriteLine(string.Format("Subject: [SpaceAge] Report for turn {0}", game.Turn));
				writer.WriteLine("Content-Disposition: attachment");
				writer.WriteLine();
				if (press.Count > 0)
				{
					writer.WriteLine("Press releases:");
					foreach (PressRelease release in press)
					{
						writer.WriteLine(string.Format("  {0}: {1}.",
							release.Issuer.ReportName,
							release.Title));
						if (!string.IsNullOrEmpty(release.Flavour))
						{
							writer.WriteLine(string.Format("    {0}", release.Flavour));
						}
					}
					if (visible.Count > 0)
					{
						writer.WriteLine();
					}
				}
				if (visible.Count > 0)
				{
					writer.WriteLine("A new contract has been published:");
					foreach (Contract contract in visible)
					{
						foreach (string line in contract.Report())
						{
							writer.WriteLine(line);
						}
					}
				}
				writer.Close();
			}
		}
	}
}
