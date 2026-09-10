using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class ProduceOrder : LongOrder
	{
		public ProduceOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.produce;
            this.ProduceType = EProduceType.Energy;
		}

		public EProduceType ProduceType { get; set; } 

		public ModuleStack Producer
		{
			get { return (ModuleStack)this.Subject; }
		}

		public Technology Technology { get; set; }

		public ItemType ItemType { get; set; }

		public ItemStacks ItemStacks
		{
			get { return this.Producer.ModuleType.ItemsProduction; }
		}
		
		public override void Parse(string command)
		{
			// PRODUCE ENERGY
			string token;
			token = LineParser.GetToken(ref command);
			if (ItemType.All.ContainsKey(token))
			{
                this.ProduceType = EProduceType.Items;
                this.ItemType = ItemType.All[token];
            } else if (token == "energy")
            {
			    this.ProduceType = EProduceType.Energy;
            }
            else
            {
                throw new Exception("Bad syntax an ENERGY/itemtype expected. Received: " + token);
            }
		}

        public override List<string> Report(Faction owner)
		{
            List<string> lines = new List<string>();
			string line;
			line = string.Format("{0}{1}produce",
				this.Conditions,
				(this.Repeat > 1) ? string.Concat(this.Repeat.ToString(), " ") : ((this.Repeat < 0) ? "@" : string.Empty));
            switch (this.ProduceType)
            {
                case EProduceType.Items:
                    line = string.Concat(line, " ", this.ItemType.Name);
                    break;
                case EProduceType.Energy:
                    line = string.Concat(line, " energy");
                    break;
                default:
                    throw new Exception("Unknown type for PRODUCE");
            }
            lines.Add(line);
			return lines;
		}

        public override void LoadXml(XmlElement elOrder)
        {
            XmlElement elProduce = (XmlElement)elOrder.SelectNodes("produce")[0];

            switch (elProduce.GetAttribute("produce-type"))
            {
                case "item":
                    this.ProduceType = EProduceType.Items;
                    this.ItemType = ItemType.All[elProduce.GetAttribute("item")];
                    break;
                case "energy":
                    this.ProduceType = EProduceType.Energy;
                    break;
                default:
                    throw new Exception("Unknown type for PRODUCE");
            }
        }

        public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
            XmlElement elProduce = doc.CreateElement("produce");
            
			switch (this.ProduceType)
			{
				case EProduceType.Items:
                    elProduce.SetAttribute("produce-type", "item");
                    elProduce.SetAttribute("item", this.ItemType.Name);
					break;
				case EProduceType.Energy:
                    elProduce.SetAttribute("produce-type", "energy");
                    break;
                default:
                    throw new Exception("Unknown type for PRODUCE");
			}

            this.xmlElement.AppendChild(elProduce);
			return this.xmlElement;
		}

		public bool NeedFuel(int week)
		{
            if (this.Producer.ProduceEnergyConsume.Count > 0)
            {
                return this.consumeFuel(week);
            }
            else
            {
                return false;
            }
		}

		private bool consumeFuel(int week)
		{
			if (!this.Producer.Effects.IsFuelled)
			{
				this.autoGetMissingFuel(week);

				if (this.Producer.ItemStacksSumRecursive.Has(this.Producer.ProduceEnergyConsume))
				{
					IItemStacksHolder holder = this.findFuelHolder(this.Producer, this.Producer.ProduceEnergyConsume);

					if (holder != null)
					{
						holder.ItemStacks.Minus(this.Producer.ProduceEnergyConsume);
						holder.EventReports.Add(
							week,
							string.Format("consumed {0} as fuel{1}.",
							this.Producer.ProduceEnergyConsume.ReportList,
							(holder == this.Producer) ? string.Empty : string.Concat(" for ", this.Producer.ReportName)));

						Fuelled fuelled = new Fuelled(this.Producer, this.Producer.ModuleType.ProduceDuration);
						fuelled.Use();
					}
				}
				else
				{
					this.Producer.EventReports.Add(week, "out of fuel.");
					return true;
				}
			}
			else
			{
				this.Producer.Effects.Fuelled.Use();
			}
			return false;
		}

		private void autoGetMissingFuel(int week)
		{
			if (this.Producer.Location == null)
			{
				return;
			}

			foreach (ItemStack needed in this.Producer.ProduceEnergyConsume.Values)
			{
				int have = this.Producer.ItemStackSumRecursive(needed.ItemType).Quantity;
				int missing = needed.Quantity - have;
				if (missing <= 0)
				{
					continue;
				}

				ItemStack transfer = new ItemStack(needed.ItemType, missing);
				if (this.Producer.Capacity - this.Producer.CapacityUsed < transfer.Size)
				{
					continue;
				}

				IItemStacksHolder source = this.findOwnedFuelInLocation(transfer);
				if (source == null)
				{
					continue;
				}

				source.ItemStacks.Remove(transfer);
				this.Producer.ItemStacks.Add(transfer);
				source.EventReports.Add(
					week,
					string.Format("given {0} to {1}.",
						transfer.ReportName,
						this.Producer.ReportName));
				this.Producer.EventReports.Add(
					week,
					string.Format("got {0} from {1}.",
						transfer.ReportName,
						source.ReportName));
			}
		}

		private IItemStacksHolder findOwnedFuelInLocation(ItemStack need)
		{
			foreach (ModuleStack root in this.Producer.Location.ModuleStacks.Values)
			{
				IItemStacksHolder found = this.findOwnedFuelHolder(root, need);
				if (found != null)
				{
					return found;
				}
			}
			return null;
		}

		private IItemStacksHolder findOwnedFuelHolder(ModuleStack stack, ItemStack need)
		{
			if (stack.Owner == this.Producer.Owner
				&& stack != this.Producer
				&& stack.ItemStacks.Has(need))
			{
				return stack;
			}

			foreach (ModuleStack child in stack.ModuleStacks.Values)
			{
				IItemStacksHolder found = this.findOwnedFuelHolder(child, need);
				if (found != null)
				{
					return found;
				}
			}

			foreach (Person person in stack.People.Values)
			{
				if (person.Owner == this.Producer.Owner && person.ItemStacks.Has(need))
				{
					return person;
				}
			}

			return null;
		}

		private IItemStacksHolder findFuelHolder(IItemStacksHolder holder, ItemStacks fuelItemStacks)
		{
            IItemStacksHolder subHolder;

			if (holder.ItemStacks.Has(fuelItemStacks))
			{
				return holder;
			}

            foreach (ModuleStack moduleStack in holder.ModuleStacks.Values)
            {
                subHolder = this.findFuelHolder(moduleStack, fuelItemStacks);
                if (subHolder != null)
                {
                    return subHolder;
                }
            }

            foreach (Person person in holder.People.Values)
            {
                subHolder = this.findFuelHolder(person, fuelItemStacks);
                if (subHolder != null)
                {
                    return subHolder;
                }
            }

			return null;
		}

		public Producing Producing { get; set; } 

		public override void Execute(int week)
		{
            if (this.CanOperate(week) && !this.NeedFuel(week))
			{                
                // assign production if not producing
				if (this.Producing == null)
				{
                    this.startProduction(week);
                }

				// produce
                if (this.Producing != null && !this.Producing.Executed)
				{
                    this.durationLeft--;
                    // I need to call Use to enable executing of effect, otherwise, the effect would executed by other means
                    // like in execute effects.all during turn run
                    // what is probably needed is to remove calling of execute here, and leave it generic call
                    this.Producing.Use(); 
					this.Producing.Execute(week);
					this.Executing = true;
				}
			}

			// finish order execution
			if (this.durationLeft <= 0)
			{
				this.Producing = null;
			}
			base.Execute(week);
		}

        private void startProduction(int week)
        {
            this.durationLeft = SkillEffects.ApplyDurationPercent(
                this.Producer.ModuleType.ProduceDuration,
                SkillEffects.ProductionDurationPercent(this.Producer));
            switch (this.ProduceType)
            {
                case EProduceType.Items:
                    if (this.durationLeft > 1)
                    {
                        this.Producer.EventReports.Add(
                            week,
                            string.Format(
                                "started {0}, {1} weeks to complete.",
                                (this.ItemType.Name == "terran")
                                    ? "recruitment of terran"
                                    : string.Concat("production of ", this.ItemType.Name),
                                this.DurationLeft));
                    }
                    this.Producing = new ProducingItems(this);
                    break;
                case EProduceType.Energy:
                    this.Producer.EventReports.Add(
                        week,
                        string.Format(
                            "started energy production for the next {0} weeks.",
                            this.DurationLeft));
                    this.Producing = new ProducingEnergy(this);
                    break;
                default:
                    throw new Exception("Unknown type for PRODUCE");
            }
        }
	}
}
