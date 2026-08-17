using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SpaceAge
{
	public class OrdersReader : IDisposable
	{
		private Game game;
		public Game Game
		{
			get { return this.game; }
		}

		public OrdersReader(Game game)
		{
			this.game = game;
		}

		public void Load(string turnDir)
		{
			if (string.IsNullOrEmpty(turnDir))
			{
				throw new ArgumentNullException();
			}

			DirectoryInfo directoryInfo = new DirectoryInfo(turnDir);
			foreach (FileInfo fileInfo in directoryInfo.GetFiles("order.*"))
			{
				this.LoadOrders(fileInfo.FullName, false);
			}
		}

		public static int Check(string turn_dir)
		{
			return 0;
		}

		private List<string> commands = new List<string>();
		public List<string> Commands
		{
			get { return this.commands; }
			set { this.commands = value; }
		}

		private List<string> checkerOutput = new List<string>();
		public List<string> CheckerOutput
		{
			get { return this.checkerOutput; }
			set { this.checkerOutput = value; }
		}

        public TextReader TextReader { get; set; }
        public void Dispose()
        {
            this.TextReader.Dispose();
        }

        public List<string> ReadOrdersFile(string filename)
		{
            if (this.TextReader != null)
            {
                this.Dispose();
                this.TextReader = null;
            }

            this.TextReader = new StreamReader(filename, System.Text.Encoding.GetEncoding(1251));
			List<string> lines = new List<string>();
			string line;
			while ((line = this.TextReader.ReadLine()) != null)
				lines.Add(line);
			return lines;
		}



        public List<string> RemoveCommentsAndEmptyLines(List<string> source)
		{
			List<string> parsed = new List<string>();
			string command;
			foreach (string line in source)
			{
				// ignore empty lines
				if (string.IsNullOrEmpty(line))
					continue;

				// ignore comments (';' or '//', whichever comes first) and trailing, leading spaces
				int commentStart = LineParser.CommentIndex(line);
				if (commentStart >= 0)
					command = line.Substring(0, commentStart).Trim();
				else
					command = line.Trim();

				// ignore emptied lines
				if (string.IsNullOrEmpty(command))
					continue;

				parsed.Add(command);
			}
			return parsed;
		}

		public void AssignOrders(List<string> commands)
		{
			Faction faction = null;
			IOrderable subject = null;
			
			string tokens, token, token2;

			foreach (string command in commands)
			{
				bool finished = false;
				if (finished)
				{
					break;
				}

				try
				{
					tokens = command;
					token = LineParser.GetToken(ref tokens);
					if (token[0] != ';')
					{
						switch (token)
						{
							case "#faction":
								token2 = LineParser.GetToken(ref tokens);
								if (Faction.All.ContainsKey(token2))
								{
									faction = Faction.All[token2];
									subject = faction;

                                    token2 = LineParser.GetQuotedToken(ref tokens);
									if (token2 != faction.Password)
									{
                                        throw new Exception("wrong password: " + token2 + " given");
                                    }
                                }
                                else
								{
									throw new Exception("bad syntax or unknown faction" + token2);
								}
								break;
							case "#modulestack":
								if (faction == null)
								{
									throw new Exception("#faction should precede #modulestack order");
								}
								token2 = LineParser.GetToken(ref tokens);
								subject = ModuleStack.All.GetOrCreateNewModuleStack(faction, token2);

                                if (subject != null) 
								{
									if (subject.Owner != faction)
									{
										subject = null;
										faction.EventReports.Add(
											string.Format("PARSING: #modulestack order failed. Modulestack {0} is not owned by faction.",
											token2));
                                        throw new Exception("Tried to assign order to modulestack for wrong faction");
                                    }
                                }
								else
								{
									throw new Exception("bad syntax #modulestack " + token2);
								}
								break;
							case "#person":
								if (faction == null)
								{
									throw new Exception("#faction should precede #person order");
								}
								token2 = LineParser.GetToken(ref tokens);
								subject = Person.All.GetOrCreateNewPerson(faction, token2);
								if (subject != null) 
								{
									if (subject.Owner != faction)
									{
										subject = null;
										faction.EventReports.Add(
											string.Format("PARSING: #person order failed. Person {0} is not owned by faction.",
											token2));
									}
								}
								else
								{
									throw new Exception("bad syntax #person " + token2);
								}
								break;
							case "#end":
								finished = true;
								break;
							default:
								if (subject == null)
								{
									if (faction != null)
									{
										faction.EventReports.Add(
											string.Format("PARSING: {0} - order ignored, due to invalid #modulestack or #person.",
											command));
									}
									else
									{
										throw new Exception("bad syntax " + command);
									}

								}
								else
								{
									this.AssignOrder(subject, command);
								}
								break;
						}
					}
				}
				catch (Exception ex)
				{
					throw new Exception("bad command: " + command, ex);
				}
			}
		}

		private void ParseReport(string p)
		{
			//throw new Exception("The method or operation is not implemented.");
		}

		private int getOrderLevel(string token)
		{
			//int orderLevel = token.Length - token.TrimStart('@', '+', '-', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9').Length;
			int orderLevel = token.Length - token.TrimStart('+', '-').Length;
			return orderLevel;
		}

		private Order getParentOrder(IOrderable subject, Order order, int orderLevel)
		{
			Order parentOrder = null;
			foreach (Order subjectOrder in subject.Orders)
			{
				if (subjectOrder != order)
				{
					if (parentOrder == null)
					{
						parentOrder = subjectOrder;
					} else if (subjectOrder.Level < orderLevel & subjectOrder.Level >= parentOrder.Level)
					{
						parentOrder = subjectOrder;
					}
				}
			}
			return parentOrder;
		}

		private void assignCondition(Order parentOrder, Order order, char condition)
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

		public Order AssignOrder(IOrderable subject, string command)
		{
			Order order;
			string tokens = command;
			string token = LineParser.GetToken(ref tokens);
			int repeat = 1;
			int orderLevel = this.getOrderLevel(token);
			string conditions = token.Substring(0, orderLevel);
			try
			{
				// this needed to be absolute value, 
				// there is no possibility for negative number of repeats, 
				// and yet when - condition combines with number condition the result is negative number of repeats
				repeat = Math.Abs(System.Convert.ToInt32(token));
				token = LineParser.GetToken(ref tokens);
			}
			catch
			{
				// that is ok, there was just no number of repetition to the order
			}

			if (token[0] == '@')
			{
				repeat = -1;
			}
			switch (token.TrimStart('@','+','-'))
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
                case "has":
					order = new HasOrder(subject);
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
				case "name":
					order = new NameOrder(subject);
					break;
				case "move":
					order = new MoveOrder(subject);
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
                case "set":
					order = new SetOrder(subject);
					break;
                case "see":
                    order = new SeeOrder(subject);
                    break;
                case "sell":
                    order = new SellOrder(subject);
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
				case "use":
					order = new UseOrder(subject);
					break;
				default:
					throw new Exception("Unknown order: " + command);
			}
			order.Parse(tokens);
			order.Repeat = repeat;
			order.Level = orderLevel;            
			while (orderLevel > 0)
			{
				Order parentOrder = this.getParentOrder(subject, order, orderLevel);
				this.assignCondition(parentOrder, order, conditions[orderLevel-1]);
                orderLevel--;
			}
            //order.Conditions = conditions;
			return order;
		}

		public void LoadOrders(string filename, bool checker)
		{
			this.commands = this.ReadOrdersFile(filename);
			this.commands = this.RemoveCommentsAndEmptyLines(this.commands);
			this.AssignOrders(this.commands);
		}

  					// Get first word as command
					//string cmd = LineParser.GetToken(ref copy).ToLower();

					// read line
					// Directives
					//if (cmd == "#orders")
					//{
					//    string t2 = MyStrings.GetToken(ref copy);
					//    if (!MyStrings.IsNumber(t2))
					//        throw new Exception("Bad faction");
					//    int num = Convert.ToInt32(t2);
					//    string password = MyStrings.GetQuotedToken(ref copy);
					//    faction = (Faction)Faction.Get(num);
					//    if (faction == null)
					//        throw new Exception("No such faction");
					//    if (password != faction.Password || faction.IsNPC)
					//        throw new Exception("Wrong password");

					//    Console.WriteLine("..orders for " + faction.Num.ToString());

					//    CheckerOutput.Add("To: " + faction.Email);
					//    CheckerOutput.Add("Subject: [Wasteland] Checker Output");
					//    CheckerOutput.Add("");
					//    CheckerOutput.Add(line);

					//    do_read = true;
					//    continue;
					//}
		//            if (cmd == "#end")
		//                do_read = false;
		//            if (!do_read || cmd == "")
		//                continue;
		//            if (cmd == "person")
		//            {
		//                string t2 = MyStrings.GetToken(ref copy);
		//                if (!MyStrings.IsNumber(t2))
		//                    throw new Exception("Bad person");
		//                int num = Convert.ToInt32(t2);
		//                person = faction.Persons.GetByNumber(num);
		//                if (person == null)
		//                    throw new Exception("This person is not in your faction");
		//                CheckerOutput.Add("\r\n" + line);
		//                continue;
		//            }

		//            CheckerOutput.Add(line);

		//            if (person == null)
		//                throw new Exception("Order given with no person specified");

		//            Order order = OrdersReader.ParseOrder(person, faction, cmd, copy);

		//            // Overwrite monthlong order
		//            if (order.IsMonthlong)
		//            {
		//                int i = 0;
		//                while (i < person.Orders.Count)
		//                    if (((Order)person.Orders[i]).IsMonthlong)
		//                    {
		//                        person.Orders.RemoveAt(i);
		//                        CheckerOutput.Add("; **** Overwriting previous monthlong order ****\r\n");
		//                        errors = true;
		//                    }
		//                    else
		//                        i++;
		//            }

		//            // Overwrite trade order
		//            if (order.GetType() == typeof(TradeOrder))
		//            {
		//                int i = 0;
		//                while (i < person.Orders.Count)
		//                    if (person.Orders[i].GetType() == typeof(TradeOrder))
		//                    {
		//                        person.Orders.RemoveAt(i);
		//                        CheckerOutput.Add("; **** Overwriting previous trade order ****\r\n");
		//                        errors = true;
		//                    }
		//                    else
		//                        i++;
		//            }

		//            person.Orders.Add(order);
			//    }
			//    catch (Exception ex)
			//    {
			//        CheckerOutput.Add("; **** " + ex.Message + " ****\r\n");
			//        errors = true;
			//    }
			//}

		//    textReader.Close();

		//    if (checker)
		//    {
		//        TextWriter tw = new StreamWriter(filename + ".checker", false, System.Text.Encoding.GetEncoding(1251));
		//        if (errors)
		//            foreach (string s in CheckerOutput)
		//                tw.WriteLine(s);
		//        else
		//        {
		//            // Write only message header
		//            foreach (string s in CheckerOutput)
		//            {
		//                tw.WriteLine(s);
		//                if (s == "") break;
		//            }
		//            tw.WriteLine("Your order was accepted without errors.");
		//        }
		//        tw.Close();
	}
}
