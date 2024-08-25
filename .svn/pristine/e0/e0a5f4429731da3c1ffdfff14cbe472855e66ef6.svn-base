using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public abstract class Order : XMLProcessing, IReporting
	{

		public Order(IOrderable subject)
		{
			this.repeat = 1;
			this.executed = false;
			this.executing = false;
			this.subject = subject;
			this.subject.Orders.Add(this);
		}

		private IOrderable subject;
		public IOrderable Subject
		{
			get { return this.subject; }
			set { this.subject = value; }
		}

		protected EOrderType type;
		public EOrderType Type
		{
			get { return this.type; }
		}

		protected int level = 0;
		public int Level
		{
			get { return this.level; }
			set { this.level = value; }
		}

		public string Conditions
		{
			get 
            {
                if (this.level > 0)
                {
                    string conditions = string.Empty.PadRight(this.level, ' '); 

                    foreach (Order order in conditionalOrders)
                    {
                        if (order.Level >= 0 & order.Level < this.Level)
                        {
                            conditions = string.Concat(
                                conditions.Substring(0, order.Level),
                                "-",
                                conditions.Substring(order.Level + 1, this.level - order.Level - 1));
                        }
                    }
                    foreach (Order order in conditionedOrders)
                    {
                        if (order.Level >= 0 & order.Level < this.Level)
                        {
                            conditions = string.Concat(
                                conditions.Substring(0, order.Level), 
                                "+", 
                                conditions.Substring(order.Level + 1, this.level - order.Level - 1));
                        }
                    }
                    //+has
                    //length = 1, char = 0, this.level = 1, order.level = 0
                    //0 0
                    //+
                    //1 0 

                    //x+has
                    //length = 2, char = 1, this.level = 2, order.level = 1
                    //0 1
                    //+
                    //2 0 

                    //+xhas
                    //length = 2, char = 1, this.level = 2, order.level = 0
                    //0 0
                    //+
                    //1 1 
                    return conditions.Replace(" ","");
                }
                return string.Empty;
            }
		}

		private Orders conditionalOrders = new Orders();
		public Orders ConditionalOrders
		{
			get { return this.conditionalOrders; }
		}

		private Orders conditionedOrders = new Orders();
		public Orders ConditionedOrders
		{
			get { return this.conditionedOrders; }
		}

		private bool executed;
		public bool Executed
		{
			get { return this.executed; }
			set { this.executed = value; }
		}

		private bool executing;
		public bool Executing
		{
			get { return this.executing; }
			set { this.executing = value; }
		}

		private int repeat = 1;
		public int Repeat
		{
			get { return this.repeat; }
			set { this.repeat = value; }
		}

		virtual public void Execute(int week)
		{
			foreach (Order order in this.conditionalOrders)
			{				
				order.Execute(week);
			}
		}

        public override XmlElement SaveXml(XmlDocument doc)
        {
            return this.SaveXml(doc, String.Empty);
        }

        public XmlElement SaveXml(XmlDocument doc, string subject)
        {
            this.saveXml_pre(doc, subject);
            this.SaveXml_core(doc, subject);
            this.saveXml_post(doc, subject);
            return this.xmlElement;
        }

        abstract public XmlElement SaveXml_core(XmlDocument doc, string subject);

		protected void saveXml_pre(XmlDocument doc, string subject)
		{
			this.xmlElement = doc.CreateElement("order");
			//this.elOrder.SetAttribute("type", this.Type.ToString());

			if (this.Level == 0)
			{
				this.xmlElement.SetAttribute("subject", subject);
				this.xmlElement.SetAttribute("name", this.subject.Name);
			}
			else
			{
				this.xmlElement.SetAttribute("conditions", this.Conditions);
			}

			if (this.Repeat <= 0)
			{
				this.xmlElement.SetAttribute("repeat", "unlimited");
			}
			else if (this.Repeat > 1)
			{
				this.xmlElement.SetAttribute("repeat", this.Repeat.ToString());
			}

		}

		protected void saveXml_post(XmlDocument doc, string subject)
		{
			foreach (Order order in this.ConditionalOrders)
			{
				if (order.Level > this.level)
				{
					if (order.Conditions[0] == '+')
					{
						this.xmlElement.AppendChild(order.SaveXml(doc, subject));
					}
				}
			}
			foreach (Order order in this.ConditionedOrders)
			{
				if (order.Level > this.level)
				{
					this.xmlElement.AppendChild(order.SaveXml(doc, subject));
				}
			}
		}

		abstract public void Parse(string command);

        virtual public List<string> Report(Faction owner)
        {
            List<string> lines = new List<string>();
            string line = string.Format("{0}{1}",
                this.Conditions,
                this.Type.ToString());
            lines.Add(line);
            return lines;
        }
    }
}
