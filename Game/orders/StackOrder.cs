using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class StackOrder : ImmediateOrder
	{
		public StackOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.stack;
		}

		public StackOrder(IOrderable subject, bool top)
			: this(subject)
		{
			this.top = top;
		}

		public StackOrder(IOrderable subject, string parentName)
			: this(subject)
		{
			this.parentName = parentName;
            this.parent = ModuleStack.All.GetOrCreateNewModuleStack(subject.Owner, this.parentName);
		}

        public StackOrder(IOrderable subject, IHolder parent)
            : this(subject)
        {
            this.parentName = parent.Name;
            this.parent = parent;
        }
        
        public IItemStacksHolder Stacker
		{
            get { return (IItemStacksHolder)this.Subject; }
		}

		private bool top = false;
		public bool Top
		{
			get { return this.top; }
			set { this.top = value; }
		}

        private bool location = false;
        public bool Location
        {
            get { return this.location; }
            set { this.location = value; }
        }

		private string parentName = null;
		public string ParentName
		{
			get { return this.parentName; }
			set { this.parentName = value; }
		}

		private IHolder parent = null;
		public IHolder Parent
		{
			get { return this.parent; }
			set { this.parent = value; }
		}

		public override void Parse(string command)
		{
			// stack under modulestack
			// STACK module_name
			// stack under root possible parent (city, hull, etc)
			// STACK TOP			
			// stak under location
			// STACK OUT

			string token;
			token = LineParser.GetQuotedToken(ref command);
			if (token == string.Empty)
			{
				throw new Exception("bad syntax: modulestack name or TOP or OUT expected");
			}
			else
			{
				if (token == "top")
				{
					this.top = true;
					this.location = false;
				}
				if (token == "out")
				{
					this.top = false;
					this.location = true;
				}
				else
				{
					this.parentName = token;
					this.parent = ModuleStack.All.GetOrCreateNewModuleStack(this.Stacker.Owner, this.parentName);
					if (this.parent == null)
					{
						throw new Exception("bad syntax modulestack id expected. Received: " + this.parentName);
					}
				}
			}
		}

        public override List<string> Report(Faction owner)
        {
            List<string> lines = new List<string>();
            string line;
            line = string.Format("{0}{1}stack {2}",
					this.Conditions,
					(this.Repeat > 1) ? string.Concat(this.Repeat.ToString(), " ") : ((this.Repeat < 0) ? "@" : string.Empty),
					(this.top) ? "top" : (this.location) ? "out" : string.Concat(((ModuleStack)this.parent).IsFormed ? string.Empty : "new", this.parent.Name));
            lines.Add(line);
            return lines;
		}

        public override void LoadXml(XmlElement elOrder)
        {
            XmlElement elStack = (XmlElement)elOrder.SelectNodes("stack")[0];
            switch (elStack.GetAttribute("parent"))
            {
                case "top":
                    this.Top = true;
                    break;
                case "out":
                    this.Location = true;
                    break;
                default:
                    this.Parent = ModuleStack.All.GetOrCreateNewModuleStack(this.Stacker.Owner, elStack.GetAttribute("parent"));
                    break;
            }
        }

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
			XmlElement elStack = doc.CreateElement("stack");
			if (this.top)
			{
				elStack.SetAttribute("parent", "top");
			}
			else if (this.location)
			{
                elStack.SetAttribute("parent", "out");
			}
			else
			{
                elStack.SetAttribute("parent", this.parent.Name);
			}
			this.xmlElement.AppendChild(elStack);
			return this.xmlElement;
		}
		
		public override void Execute(int week)
		{
			this.Executed = false;
            if (this.location)
            {
                this.Stacker.Parent = this.Stacker.Location;
                this.Stacker.EventReports.Add(week, string.Concat("stacked out into ", this.Stacker.Parent.ReportName, "."));			
                this.Executed = true;
            }
            else if (this.top)
            {
                IHolder stackerParent = this.Stacker;
                while (!stackerParent.Parent.IsLocation)
                {
                    stackerParent = stackerParent.Parent;
                }
				ModuleStack hangarStacker = this.Stacker as ModuleStack;
				if (hangarStacker != null && hangarStacker.IsHangarCraft
					&& !ModuleStack.CanNestHangarCraft(stackerParent, hangarStacker.ModuleType))
				{
					this.Stacker.EventReports.Add(week, "STACK failed. fighter drones can only nest in a location or a fighter drone bay.");
					base.Execute(week);
					return;
				}
                this.Stacker.Parent = stackerParent;
                this.Stacker.EventReports.Add(week, string.Concat("stacked out under ", this.Stacker.Parent.ReportName, "."));
                this.Executed = true;
            }
            else
            {
                if (this.canStackUnder(week))                    
                {
                    this.Stacker.Parent = this.parent;
                    this.Executed = true;
                    this.Stacker.EventReports.Add(week, string.Concat("stacked under ", this.Stacker.Parent.ReportName, "."));
                }
            }
			base.Execute(week);
		}

		private bool canStackUnder(int week)
		{
            bool canStack = true;
            if (!this.Stacker.IsFormed)
            {
                canStack = false;
                //this.Stacker.EventReports.Add(week, "STACK failed. stacking unit need to be formed first..");
            }
            else
            {
                if (this.Stacker.Location != this.Parent.Location)
                {
                    canStack = false;
                    this.Stacker.EventReports.Add(week, "STACK failed. Parent is in different location.");
                }
								if (this.Stacker == this.Parent)
								{
									canStack = false;
									this.Stacker.EventReports.Add(week, "STACK failed. Parent cannot be the same as stacker.");
								}
							//TODO: add attitude veirifcation for stacking
								//if (this.Stacker.Owner != this.Parent.Owner & this.Parent.Owner.Name != "1")
								if (this.Stacker.Owner != this.Parent.Owner)
								{
                    canStack = false;
                    this.Stacker.EventReports.Add(week, "STACK failed. Parent is not the same faction.");
                }
				ModuleStack hangarStacker = this.Stacker as ModuleStack;
				if (canStack && hangarStacker != null && hangarStacker.IsHangarCraft
					&& !ModuleStack.CanNestHangarCraft(this.Parent, hangarStacker.ModuleType))
				{
					canStack = false;
					this.Stacker.EventReports.Add(week, "STACK failed. fighter drones can only nest in a location or a fighter drone bay.");
				}
            }
            return canStack;
		}
	}
}
