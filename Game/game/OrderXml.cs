using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	public static class OrderXml
	{
		public static void LoadAll(XmlElement elOrders, XMLProcessing xml)
		{
			IOrderable subject = null;
			List<string> loadedKeys = new List<string>();

			foreach (XmlElement elOrder in elOrders.SelectNodes("order"))
			{
				LoadOrderElement(elOrder, xml, ref subject, loadedKeys);
			}
		}

		private static void LoadOrderElement(XmlElement elOrder, XMLProcessing xml, ref IOrderable subject, List<string> loadedKeys)
		{
			if (elOrder.HasAttribute("subject"))
			{
				string subjectType = elOrder.GetAttribute("subject");
				string subjectName = elOrder.GetAttribute("name");
				switch (subjectType)
				{
					//case "region":
					//    subject = Region.All[subjectName];
					//    break;
					case "faction":
						subject = Faction.All[subjectName];
						break;
					case "modulestack":
						subject = ModuleStack.All[subjectName];
						break;
					case "person":
						subject = Person.All[subjectName];
						break;
					default:
						throw new Exception("Unknown subject type for order.");
				}
				if (subject == null)
					throw new Exception("could not find the order subject: " + subjectType + " named: " + subjectName);
			}

			XmlElement elVerb = FirstVerbElement(elOrder);
			if (elVerb == null)
			{
				throw new Exception("Unknown order. ");
			}

			string conditions = elOrder.HasAttribute("conditions") ? elOrder.GetAttribute("conditions") : string.Empty;
			string subjectKey = subject != null ? subject.Name : string.Empty;
			string loadedKey = subjectKey + "\n" + conditions + "\n" + elVerb.OuterXml;
			if (loadedKeys.Contains(loadedKey))
			{
				return;
			}
			loadedKeys.Add(loadedKey);

			Order order = CreateOrder(elVerb.Name, subject);
			order.LoadXml(elOrder);

			if (elOrder.GetAttribute("repeat") == "unlimited")
			{
				order.Repeat = -1;
			}
			else
			{
				order.Repeat = xml.XMLAssignInteger(elOrder.GetAttribute("repeat"), 1);
			}

			if (conditions.Length > 0)
			{
				order.Level = conditions.Length;
				int orderLevel = order.Level;
				while (orderLevel > 0)
				{
					Order parentOrder = GetParentOrder(subject, order, orderLevel);
					AssignCondition(parentOrder, order, conditions[orderLevel - 1]);
					orderLevel--;
				}
			}

			foreach (XmlElement elNested in elOrder.SelectNodes("order"))
			{
				LoadOrderElement(elNested, xml, ref subject, loadedKeys);
			}
		}

		private static XmlElement FirstVerbElement(XmlElement elOrder)
		{
			foreach (XmlNode child in elOrder.ChildNodes)
			{
				XmlElement elChild = child as XmlElement;
				if (elChild != null && elChild.Name != "order")
				{
					return elChild;
				}
			}
			return null;
		}

		private static Order CreateOrder(string verb, IOrderable subject)
		{
			switch (verb)
			{
				case "active":
					return new ActiveOrder(subject);
				case "alias":
					return new AliasOrder(subject);
				case "attack":
					return new AttackOrder(subject);
				case "capture":
					return new CaptureOrder(subject);
				case "buy":
					return new BuyOrder(subject);
				case "copy":
					return new CopyOrder(subject);
				case "contract":
					return new ContractOrder(subject);
				case "press":
					return new PressOrder(subject);
				case "declare":
					return new DeclareOrder(subject);
				case "form":
					return new FormOrder(subject);
				case "get":
					return new GetOrder(subject);
				case "give":
					return new GiveOrder(subject);
				case "has":
					return new HasOrder(subject);
				case "jump":
					return new JumpOrder(subject);
				case "move":
					return new MoveOrder(subject);
				case "name":
					return new NameOrder(subject);
				case "produce":
					return new ProduceOrder(subject);
				case "repair":
					return new RepairOrder(subject);
				case "research":
					return new ResearchOrder(subject);
				case "see":
					return new SeeOrder(subject);
				case "sell":
					return new SellOrder(subject);
				case "set":
					return new SetOrder(subject);
				case "stack":
					return new StackOrder(subject);
				case "tactic":
					return new TacticOrder(subject);
				case "train":
					return new TrainOrder(subject);
				case "transfer":
					return new TransferOrder(subject);
				case "use":
					return new UseOrder(subject);
				default:
					throw new Exception("Unknown order. " + verb);
			}
		}

		private static Order GetParentOrder(IOrderable subject, Order order, int orderLevel)
		{
			Order parentOrder = null;
			foreach (Order subjectOrder in subject.Orders)
			{
				if (subjectOrder != order)
				{
					if (parentOrder == null)
					{
						parentOrder = subjectOrder;
					}
					else if (subjectOrder.Level < orderLevel & subjectOrder.Level >= parentOrder.Level)
					{
						parentOrder = subjectOrder;
					}
				}
			}
			return parentOrder;
		}

		private static void AssignCondition(Order parentOrder, Order order, char condition)
		{
			if (condition == '-')
			{
				order.ConditionalOrders.Add(parentOrder);
				parentOrder.ConditionedOrders.Add(order);
			}
			else if (condition == '+')
			{
				parentOrder.ConditionalOrders.Add(order);
				order.ConditionedOrders.Add(parentOrder);
			}
		}

		public static void SaveAll(XmlDocument doc, Faction factionXMLreport = null)
		{
			XmlElement elOrders = doc.CreateElement("orders");
			doc.DocumentElement.AppendChild(elOrders);

			XmlElement elOrder;
			foreach (Faction faction in Faction.All.Values)
			{
				if (factionXMLreport != null && faction != factionXMLreport)
					continue;

				foreach (Order order in faction.Orders)
				{
					if (order.Level == 0)
					{
						elOrder = order.SaveXml(doc, "faction");
						elOrders.AppendChild(elOrder);
					}
				}
			}
			
			foreach (ModuleStack moduleStack in ModuleStack.All.Values)
			{
				if (factionXMLreport != null && moduleStack.Owner != factionXMLreport)
					continue;

				foreach (Order order in moduleStack.Orders)
				{
					if (order.Level == 0)
					{
						elOrder = order.SaveXml(doc, "modulestack");
						elOrders.AppendChild(elOrder);
					}
				}
			}

			foreach (Person person in Person.All.Values)
			{
				if (factionXMLreport != null && person.Owner != factionXMLreport)
					continue;

				foreach (Order order in person.Orders)
				{
					if (order.Level == 0)
					{
						elOrder = order.SaveXml(doc, "person");
						elOrders.AppendChild(elOrder);
					}
				}
			}
		}
	}
}
