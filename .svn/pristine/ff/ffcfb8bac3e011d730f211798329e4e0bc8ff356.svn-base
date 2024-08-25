using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
    public class Orders : List<Order>, IReporting
	{
			public bool Execute(int week)
			{
				bool executedOrder = true;
				bool executedAnyOrder = false;

				while (executedOrder)
				{
					executedOrder = this.ExecuteList(week, this.Immediate);
					if (executedOrder)
					{
						executedAnyOrder = true;
					}
				}

				executedOrder = this.ExecuteList(week, this.Long);
				if (executedOrder)
				{
					executedAnyOrder = true;
				}

				executedOrder = true;
				while (executedOrder)
				{
					executedOrder = this.ExecuteList(week, this.Immediate);
					if (executedOrder)
					{
						executedAnyOrder = true;
					}
				}

				return executedAnyOrder;
			}

		public bool ExecuteList(int week, List<Order> ordersList)
		{
			bool executedOrder = false;
			foreach (Order order in ordersList)
			{
                if (order is ImmediateOrder)
                {
                    if (!order.Executed & order.ConditionalOrders.Count == 0)
                    {
                        order.Execute(week);
                        if (order.Executed)
                        {
                            this.RemoveConditions(order);
                            executedOrder = true;
                        }
                    }
                }
                else
                {
                    if (!order.Subject.ExecutedLongOrder & !order.Executed & order.ConditionalOrders.Count == 0)
                    {
                        order.Execute(week);
                        
                        if (order.Executed)
                        {
                            this.RemoveConditions(order);
                        }

                        if (order.Executed | order.Executing)
                        {
                            executedOrder = true;
                            order.Subject.ExecutedLongOrder = true;
                        }
                    }
                }
			}
			return executedOrder;
		}

		public void RemoveConditions(Order order)
		{
            //Console.WriteLine(string.Concat("removing conditions of ", order.Report(), " conditioned ", order.ConditionedOrders.Count));

			foreach (Order conditionedOrder in order.ConditionedOrders)
			{
                //Console.WriteLine(string.Concat("conditioned ", conditionedOrder.Report()));

				conditionedOrder.ConditionalOrders.Remove(order);
                if (conditionedOrder.Level > order.Level)
                {
                    //// '-' condition - should suffice to cut the most left dash
                    //if (order.Conditions.Length > 1)
                    //{
                    //    order.Conditions = order.Conditions.Substring(1, order.Conditions.Length - 1);
                    //}
                    //else if (order.Conditions.Length == 1)
                    //{
                    //    order.Conditions = string.Empty;
                    //}

                    order.Level--;
                } else {
                    // '+' condition, nothing to do as the order get removed along with it's conditions
                }
			}
			order.ConditionedOrders.Clear();
		}

		public void RemoveExecuted()
		{
			this.RemoveAll(nonRepeatingExecutedPredicate);
		}

        private static bool executedPredicate(Order order)
        {
            return order.Executed;
        }
        
        private static bool notExecutedPredicate(Order order)
		{
			return !order.Executed;
		}

		private static bool nonRepeatingExecutedPredicate(Order order)
		{
			return order.Executed && (order.Repeat == 0);
		}

		private static bool immediatePredicate(Order order)
		{
			return (order is ImmediateOrder);
		}

		private static bool longPredicate(Order order)
		{
			return (order is LongOrder);
		}
	
		private void executeAction(int week, Order order)
		{
			order.Execute(week);
		}

		public List<Order> Executed
		{
			get
			{
				return this.FindAll(executedPredicate);
			}
		}

        public List<Order> NotExecuted
        {
            get
            {
                return this.FindAll(notExecutedPredicate);
            }
        }
        
        public List<Order> Immediate
		{
			get
			{
				return this.FindAll(immediatePredicate);
			}
		}

		public List<Order> Long
		{
			get
			{
				return this.FindAll(longPredicate);
			}
		}

        public List<string> Report(Faction faction)
        {
            return this.Report(faction, 0);
        }


        public List<string> Report(Faction faction, int level)
        {
            ReportLines reportLines = new ReportLines();

            foreach (Order order in this.NotExecuted)
            {
                reportLines.Add(order.Report(faction), level);
            }

            return reportLines.IndentedLines;
        }
    }
	
}
