using System;
using System.Xml;

namespace SpaceAge
{
	public static class OrderXml
	{
		public static void LoadAll(XmlElement elOrders, XMLProcessing xml)
		{
			Order order = null;
			IOrderable subject = null;
			string subjectType, subjectName = string.Empty;

			foreach (XmlElement elOrder in elOrders.SelectNodes("order"))
			{
				if (elOrder.HasAttribute("subject"))
				{
					subjectType = elOrder.GetAttribute("subject");
					subjectName = elOrder.GetAttribute("name");
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

                switch (elOrder.FirstChild.Name)
				{
					case "active":
                        order = new ActiveOrder(subject);
						break;
					case "alias":
                        order = new AliasOrder(subject);
						break;
					case "attack":
                        order = new AttackOrder(subject);
						break;
					case "capture":
                        order = new CaptureOrder(subject);
						break;
					case "buy":
                        order = new BuyOrder(subject);
						break;
                    case "copy":
                        order = new CopyOrder(subject);
                        break;
					case "contract":
						order = new ContractOrder(subject);
						break;
					case "press":
						order = new PressOrder(subject);
						break;
					case "declare":
						order = new DeclareOrder(subject);
						break;
					case "form":
                        order = new FormOrder(subject);
						break;
					case "get":
                        order = new GetOrder(subject);
						break;
					case "give":
                        order = new GiveOrder(subject);
						break;
                    case "has":
                        order = new HasOrder(subject);
                        break;
                    case "move":
						order = new MoveOrder(subject);						
						break;
					case "name":
                        order = new NameOrder(subject);
						break;
                    case "produce":
                        order = new ProduceOrder(subject);
                        break;
                    case "repair":
                        order = new RepairOrder(subject);
                        break;
                    case "research":
                        order = new ResearchOrder(subject);
                        break;
                    case "see":
                        order = new SeeOrder(subject);
                        break;
                    case "sell":
                        order = new SellOrder(subject);
                        break;
                    case "set":
                        order = new SetOrder(subject);
                        break;
                    case "stack":
                        order = new StackOrder(subject);
                        break;
                    case "tactic":
                        order = new TacticOrder(subject);
                        break;
                    case "train":
                        order = new TrainOrder(subject);
                        break;
                    case "transfer":
                        order = new TransferOrder(subject);
						break;
					case "use":
                        order = new UseOrder(subject);
						break;
					default:
                        throw new Exception("Unknown order. " + elOrder.FirstChild.Name);
				}
				order.LoadXml(elOrder);

				//TODO: validate conditional orders load up in xml

				if (elOrder.GetAttribute("repeat") == "unlimited")
				{ 
					order.Repeat = -1; 
				}
				else 
				{
					order.Repeat = xml.XMLAssignInteger(elOrder.GetAttribute("repeat"), 1);
				}
            }
		}

		public static void SaveAll(XmlDocument doc, Faction factionXMLreport = null)
		{
			XmlElement elOrders = doc.CreateElement("orders");
			doc.DocumentElement.AppendChild(elOrders);

			XmlElement elOrder;
			foreach (Faction faction in Faction.All.Values)
			{
				if (factionXMLreport != null & faction != factionXMLreport)
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
				if (factionXMLreport != null & moduleStack.Owner != factionXMLreport)
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
				if (factionXMLreport != null & person.Owner != factionXMLreport)
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
