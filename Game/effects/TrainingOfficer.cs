using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class TrainingOfficer : Training
	{
		#region officer being trained
		// modulestack training officer (order executing unit)
		public ModuleStack Trainer
		{
			get { return (ModuleStack)this.Subject; }
		}

		// if trained new officer, race of that officer
		public Race Race { get; set; }


		// if trained new officer, target person
		public Person Officer { get; set; }

		// if trained new officer, target parent of the person
		public IHolder OfficerParent { get; set; }
		#endregion

		public override string Description
		{
			get
			{
				return string.Format("training {0} officer{1}, {2} weeks to complete.",
					this.Race.ReportName,
					(this.OfficerParent == null) ? string.Empty : string.Concat(" for ", this.OfficerParent.ReportName),
					this.Duration);
			}
		}

		public TrainingOfficer(ModuleStack trainer)
			: base(trainer, 0)
		{
		}

		public TrainingOfficer(ModuleStack trainer, int duration, Race race, Person officer, IHolder officerParent)
			: base(trainer, duration)
		{
			this.Race = race;
			this.Officer = officer;
			if (OfficerParent == null)
			{
				this.OfficerParent = trainer;
			}
			else
			{
				this.OfficerParent = officerParent;
			}
		}

		public TrainingOfficer(TrainOrder trainOrder)
			: this(trainOrder.Trainer, trainOrder.DurationLeft, trainOrder.Race, trainOrder.Officer, trainOrder.OfficerParent)
		{
			this.TrainOrder = trainOrder;
		}

		public override void Execute(int week)
		{
			if (this.ExecuteCondition)
			{
				// start of the training
				if (this.Duration == this.Race.OfficerTrainingDuration)
				{
					this.Officer.EventReports.Add(
						week,
						string.Format("started training of {0} officer.",
						this.Race.ReportName));
				}

				// training
				this.Duration--; 
				this.Trainer.ExecutedLongOrder = true;

				// finished training
				if (this.Duration == 0)
				{
                    this.Trainer.ItemStacks.Minus(this.Race.ItemType);
                    this.Officer.Race = this.Race;
					this.Officer.Parent = this.Trainer;
					this.Trainer.EventReports.Add(
						week,
						string.Format("trained {0} into {1}.",
						this.Race.ReportName,
						this.Officer.ReportName));
					this.Officer.EventReports.Add(
						week,
						string.Format("trained by {0}.",
						this.Trainer.ReportName));
				}
                
                // stacking if training was for other holder
                if (this.Trainer != this.OfficerParent)
				{
					StackOrder stackOrder = new StackOrder(this.Officer, this.OfficerParent);
					stackOrder.Execute(week);
				}

				base.Execute(week);
			}
		}

        public override void LoadXml(XmlElement elTrainingOfficer)
        {
            base.LoadXml(elTrainingOfficer);
            this.Race = Race.All[elTrainingOfficer.GetAttribute("race")];
            this.Officer = Person.All.GetOrCreateNewPerson(this.Trainer.Owner, elTrainingOfficer.GetAttribute("officer"));
            this.OfficerParent = ModuleStack.All.GetOrCreateNewModuleStack(this.Trainer.Owner, elTrainingOfficer.GetAttribute("officer-parent"));
        }

        public override XmlElement SaveXml(XmlDocument doc)
        {
            base.SaveXml(doc);
            this.xmlElement.SetAttribute("type", "training-officer");
            this.xmlElement.SetAttribute("race", this.Race.Name);
            this.xmlElement.SetAttribute("officer", this.Officer.Name);
            this.xmlElement.SetAttribute("officer-parent", this.OfficerParent.Name);
            return this.xmlElement;
        }
	}
}
