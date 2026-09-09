using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class UseOrder : LongOrder
	{
		public UseOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.use;
		}

		public Technology Technology { get; set; }

		public ModuleStack Producer
		{
			get { return (ModuleStack)this.Subject; }
		}

		public IHolder Receiver         { get; set; }
		public IHolder ReceiverParent   { get; set; }

		public override void Parse(string command)
		{
            // USE technology
            // USE technology AS "alias"
            // USE technology AS "alias" FOR id

			string token;
			if (string.IsNullOrEmpty(command.Trim()))
			{
				throw new Exception("Bad syntax or no technology");
			}

			token = LineParser.GetToken(ref command);
			try
			{
				this.Technology = Technology.All[token];
			}
			catch (Exception ex)
			{
				throw new Exception("Bad syntax or unknown technology", ex);
			}
			if (this.Technology.ProductionType == EProductionType.Modules)
			{
				// grammar: USE tech [AS alias] [FOR id]. Both AS and FOR are optional and
				// independent, so "use tech for id" (no alias) is valid.
				token = LineParser.GetToken(ref command);

				if (token == "as")
				{
					string alias = LineParser.GetQuotedToken(ref command);
					if (alias == string.Empty)
					{
						throw new Exception("Bad syntax receiver modulestack id expected or alias. Received: " + alias);
					}
					this.Receiver = ModuleStack.All.GetOrCreateNewModuleStack(this.Producer.Owner, alias);
					token = LineParser.GetToken(ref command);
				}
				else
				{
					// no explicit alias: generate a receiver name
					string randomName = this.Producer.GenerateUniqueModuleStackIdentifier();
					this.Receiver = ModuleStack.All.GetOrCreateNewModuleStack(this.Producer.Owner, randomName);
				}

				if (token == "for")
				{
					string parent = LineParser.GetQuotedToken(ref command);
					if (parent == string.Empty)
					{
						throw new Exception("Bad syntax receiver parent modulestack id expected or alias. Received: " + parent);
					}
					// TODO: possible error here - if we give specific modulestack AS target, put FOR moduleStack that isn't it's current parent
					this.ReceiverParent = ModuleStack.All.GetOrCreateNewModuleStack(this.Producer.Owner, parent);
				}
				else if (token == string.Empty)
				{
					this.ReceiverParent = this.Producer;
				}
				else
				{
					throw new Exception("Bad syntax, AS or FOR expected. Received: " + token);
				}
			}
		}

        public override void LoadXml(XmlElement elOrder)
        {
            XmlElement elUse = (XmlElement)elOrder.SelectNodes("use")[0];

            this.Technology = Technology.All[elUse.GetAttribute("technology")];
            if (this.Technology.ProductionType == EProductionType.Modules)
            {
                this.Receiver = ModuleStack.All.GetOrCreateNewModuleStack(this.Producer.Owner, elUse.GetAttribute("receiver"));
                this.ReceiverParent = ModuleStack.All.GetOrCreateNewModuleStack(this.Producer.Owner, elUse.GetAttribute("receiver-parent"));
            }
        }

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
			XmlElement elUse = doc.CreateElement("use");            

            elUse.SetAttribute("technology", this.Technology.Name);
            if (this.Technology.ProductionType == EProductionType.Modules)
            {
                elUse.SetAttribute("receiver", this.Receiver.Name);
                elUse.SetAttribute("receiver-parent", this.ReceiverParent.Name);
            }

            this.xmlElement.AppendChild(elUse);
			return this.xmlElement;
		}

        public override List<string> Report(Faction owner)
        {
            List<string> lines = new List<string>();
            string line;
            line = string.Format("{0}{1}use {2}",
                    this.Conditions,
                    (this.Repeat > 1) ? string.Concat(this.Repeat.ToString(), " ") : ((this.Repeat < 0) ? "@" : string.Empty),
                    this.Technology.Name);
            if (this.Technology.ProductionType == EProductionType.Modules)
            {
                line = string.Concat(
                    line, 
                    string.Concat(((ModuleStack)this.Receiver).IsFormed ? " as " : " as new", this.Receiver.Name),
					(this.ReceiverParent == this.Producer) ? string.Empty : string.Concat(((ModuleStack)this.ReceiverParent).IsFormed ? " for " : " for new", this.ReceiverParent.Name));
            }
            lines.Add(line);
            return lines;
		}

		public bool HasTechnology
		{
			get
			{
				if (this.Technology.Level == 0)
				{
					return true;
				}
				else
				{
					return Producer.Technologies.Contains(this.Technology);
				}
			}
		}

		public bool Usable(int week)
		{
			if (this.Producer.ModuleType == null || this.Producer.Location == null)
			{
				return false;
			}

			// verify if moduletype is valid
			// verify if resoruces are present on the planet
			// verify if the atmosphere is valid
			// verify if regionType is valid
			if (this.Producer.ModuleType.UseCondition_LocationTypes.Count > 0
                & !this.Producer.ModuleType.UseCondition_LocationTypes.Contains(BodyEnvironment.EffectiveLocationType(this.Producer.Location)))
			{
				this.Producer.EventReports.Add(
						week,
						string.Format("USE failed: {0} cannot operate in {1}.",
								this.Producer.ModuleType.ReportName,
								this.Producer.Location.ReportName));

				return false;
			}

			if (this.Technology.UseCondition_LocationTypes != null
				&& this.Technology.UseCondition_LocationTypes.Count > 0
				&& !this.Technology.UseCondition_LocationTypes.Contains(BodyEnvironment.EffectiveLocationType(this.Producer.Location)))
			{
				this.Producer.EventReports.Add(
						week,
						string.Format("USE failed: {0} cannot operate in {1}.",
								this.Technology.ReportName,
								this.Producer.Location.ReportName));

				return false;
			}

			if (this.Technology.UseCondition_AtmosphereResources != null
				&& this.Technology.UseCondition_AtmosphereResources.Count > 0
				&& !BodyEnvironment.HasAtmosphereResources(this.Producer.Location, this.Technology.UseCondition_AtmosphereResources))
			{
				this.Producer.EventReports.Add(
						week,
						string.Format("USE failed: {0} cannot operate in {1}.",
								this.Technology.ReportName,
								this.Producer.Location.ReportName));

				return false;
			}

			if (this.Technology.UseCondition_PlanetTypes != null
				&& this.Technology.UseCondition_PlanetTypes.Count > 0
				&& !BodyEnvironment.MatchesPlanetType(this.Producer.Location, this.Technology.UseCondition_PlanetTypes))
			{
				this.Producer.EventReports.Add(
						week,
						string.Format("USE failed: {0} cannot operate in {1}.",
								this.Technology.ReportName,
								this.Producer.Location.ReportName));

				return false;
			}

			if ((this.Technology.UseCondition_ModuleTypesGroup != EModuleTypesGroup.all)
                & (this.Producer.ModuleType.Group != this.Technology.UseCondition_ModuleTypesGroup))
			{
				this.Producer.EventReports.Add(
						week,
						string.Format("USE failed: {0} cannot be used in {1}, this technology is only usable in {2} type of modules.",
								this.Technology.ReportName,
								this.Producer.ReportName,
								this.Technology.UseCondition_ModuleTypesGroup.ToString()));

				return false;
			}

			if (!string.IsNullOrEmpty(this.Technology.UseCondition_ModuleType)
				&& this.Producer.ModuleType.Name != this.Technology.UseCondition_ModuleType)
			{
				this.Producer.EventReports.Add(
						week,
						string.Format("USE failed: {0} cannot be used in {1}, this technology is only usable in {2}.",
								this.Technology.ReportName,
								this.Producer.ReportName,
								this.Technology.UseCondition_ModuleType));
				return false;
			}

			if (this.Technology.UseProduceModules != null
				&& this.Technology.UseProduceModules.Group == EModuleTypesGroup.settlement)
			{
				ETemperatureBand temperature = BodyEnvironment.TemperatureAt(this.Producer.Location);
				if (!BodyEnvironment.AllowsSettlement(this.Technology.UseProduceModules.Name, temperature))
				{
					this.Producer.EventReports.Add(
						week,
						string.Format("USE failed: {0} cannot settle a {1} world.",
							this.Technology.UseProduceModules.ReportName,
							temperature));
					return false;
				}
			}

			if (this.Producer.ModuleType.UseCondition_RequireFuel)
			{
				if (!this.Producer.Effects.IsFuelled)
				{
					if (this.Producer.Fuel.Count > 0)
					{
						if (this.Producer.ItemStacks.Has(this.Producer.Fuel))
						{
							this.Producer.ItemStacks.Minus(this.Producer.Fuel);
							this.Producer.EventReports.Add(
									week,
									string.Format("consumed {0} as fuel.",
									this.Producer.Fuel.ReportList));

							Fuelled fuelled = new Fuelled(this.Producer, this.Producer.FuelDuration);
                            fuelled.Use();
						}
						else
						{
							this.Producer.EventReports.Add(
									week,
									string.Format("USE failed: {0} require fuel.",
											this.Producer.ModuleType.ReportName));
							return false;
						}
					}

				}
				else
				{
					this.Producer.Effects.Fuelled.Use();
				}
			}

			return true;
		}

		public bool HasResources
		{
			get
			{
                if (this.Technology.UseConsumeItems != null)
                {
                    return this.Producer.ItemStacks.Has(this.Technology.UseConsumeItems);
                } else
                {
                    return true;
                }
			}
		}

		public Producing Producing { get; set; }

		private Producing FindMatchingProducing()
		{
			foreach (Effect effect in this.Producer.Effects)
			{
				Producing producing = effect as Producing;
				if (producing != null && !producing.Executed && producing.Technology == this.Technology)
				{
					return producing;
				}
			}
			return null;
		}

		private void TryReconnectProducing()
		{
			if (this.Producing != null)
			{
				return;
			}

			Producing existing = this.FindMatchingProducing();
			if (existing == null)
			{
				return;
			}

			this.Producing = existing;
			this.durationLeft = existing.Duration;
		}

		private void SyncProducingTarget()
		{
			ProducingModule producingModule = this.Producing as ProducingModule;
			if (producingModule == null || this.Technology.ProductionType != EProductionType.Modules)
			{
				return;
			}

			producingModule.UseOrder = this;
			if (this.Receiver != producingModule.Receiver || this.ReceiverParent != producingModule.ReceiverParent)
			{
				producingModule.Retarget(this.Receiver, this.ReceiverParent);
			}
		}

		private bool CanStartEffectProduction(int week)
		{
			if (this.Technology.ProductionType != EProductionType.Effects)
			{
				return true;
			}

			if (this.Technology.UseProduceEffectName == "repair"
				&& this.Technology.UseProduceTarget == "module-damage")
			{
				ModuleStack scope = RepairScope.For(this.Producer);
				if (RepairScope.TotalDamage(scope) < 1)
				{
					this.Producer.EventReports.Add(week, "USE failed: no damage to repair.");
					return false;
				}
			}

			return true;
		}

		public override void Execute(int week)
		{
			//TODO: refactor order into effect based -> move methods such as hastechnology or has resources to the producing effect
			if (this.Usable(week) && this.CanOperate(week) && this.HasTechnology)
			{
				this.TryReconnectProducing();
				this.SyncProducingTarget();

				// assign production if not producing
				if (this.Producing != null && !this.Producing.Executed)
				{
					this.durationLeft = this.Producing.Duration;
					this.durationLeft--;
					this.Producing.Use();
					this.Executing = true;
				}
				else if (this.Producing == null && this.HasResources)
				{
					if (!this.CanStartEffectProduction(week))
					{
					}
					else
					{
					// start production, consume resources
					this.Producer.ItemStacks.Minus(this.Technology.UseConsumeItems);

					switch (this.Technology.ProductionType)
					{
						case EProductionType.Items:
							if (!this.Executing)
							{
								this.durationLeft = this.DurationInitial;
							}

							this.Producing = new ProducingItems(this.Producer, this.Technology, this.durationLeft);
							if (this.Technology.UseConsumeItems != null)
							{
								this.Producer.EventReports.Add(week, string.Format("consumed {0} to produce {1}.",
									this.Technology.UseConsumeItems.ReportList,
									((ProducingItems)this.Producing).ProducedItemStacks.ReportList));
							}
							break;
						case EProductionType.Modules:
							if (!this.Executing)
							{
								this.durationLeft = this.DurationInitial;
							}
                            this.Producing = new ProducingModule(this);

                            break;
						case EProductionType.Effects:
							if (!this.Executing)
							{
								this.durationLeft = this.DurationInitial;
							}
							this.Producing = new ProducingEffect(this.Producer, this.Technology, this.durationLeft);
							if (this.Technology.UseConsumeItems != null)
							{
								this.Producer.EventReports.Add(week, string.Format("consumed {0} for repair.",
									this.Technology.UseConsumeItems.ReportList));
							}
							break;
					}
					this.durationLeft--;
					this.Producing.Execute(week);
					this.Executing = true;
					}
                }
				else if (this.Producing == null && !this.HasResources)
				{
					switch (this.Technology.ProductionType)
					{
						case EProductionType.Items:
							this.Producer.EventReports.Add(week, string.Format("USE failed, tried to produce {0}, but needed to consume {1}.",
									this.Technology.UseProduceItems.ReportList,
									this.Technology.UseConsumeItems.ReportList));
							break;
						case EProductionType.Modules:
							this.Producer.EventReports.Add(week, string.Format("USE failed, tried to produce {0}, but needed to consume {1}.",
									this.Technology.UseProduceModules.ReportName,
									this.Technology.UseConsumeItems.ReportList));
							break;
						case EProductionType.Effects:
							this.Producer.EventReports.Add(week, string.Format("USE failed, tried to run {0}, but needed to consume {1}.",
									this.Technology.ReportName,
									this.Technology.UseConsumeItems.ReportList));
							break;
					}
				}
			}
			else
			{
				//if (!this.Usable(week))
				//{
				//    this.Producer.EventReports.Add(week, "USE failed: unusable.");
				//}
				//if (!this.CanOperate(week))
				//{
				//    this.Producer.EventReports.Add(week, "USE failed: inactive.");
				//}
				if (!this.HasTechnology)
				{
					this.Producer.EventReports.Add(week,
						string.Format("USE failed: lack of technology necessary to operate: {0}.",
						this.Technology.ToString()));
				}
			}
			// finish order execution
			if (this.DurationLeft == 0)
			{
				if (this.Producing is ProducingModule)
				{
					this.Receiver = ((ProducingModule)this.Producing).Receiver;
                }

                this.Producing = null;
			}
			base.Execute(week);
		}

		public override int DurationInitial
		{
			get
			{
				if (this.durationInitial == 0)
				{
					int copies = this.Producer.QuantityOperational;
					if (copies < 1)
					{
						copies = 1;
					}
					int weeks = (int)(Math.Ceiling((double)(this.Technology.UseTime * this.Producer.ModuleType.UseCondition_EfficiencyMultiplier) / copies));
					this.durationInitial = SkillEffects.ApplyDurationPercent(
						weeks,
						SkillEffects.ProductionDurationPercent(this.Producer));
				}
				return this.durationInitial;
			}
			set
			{
				base.DurationInitial = value;
			}
		}
	}
}
