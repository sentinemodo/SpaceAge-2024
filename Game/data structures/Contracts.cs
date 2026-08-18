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
			if (created.Count == 0)
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
				if (visible.Count == 0)
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
				writer.WriteLine("A new contract has been published:");
				foreach (Contract contract in visible)
				{
					foreach (string line in contract.Report())
					{
						writer.WriteLine(line);
					}
				}
				writer.Close();
			}
		}
	}
}
