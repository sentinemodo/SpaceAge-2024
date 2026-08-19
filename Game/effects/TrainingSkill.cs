using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class TrainingSkill : Training
	{

		#region skill being trained
		// person receiving training (order executing unit)
		public Person Trainee
		{
			get { return (Person)this.Subject; }
		}

		// skill being trained if not training officer
		public SkillType SkillType  { get; set; }
		#endregion

		public override string Description
		{
			get
			{
				return string.Format("training {0} skill, {1} weeks to complete.",
					this.SkillType.ReportName,
					this.Duration);
			}
		}

		public TrainingSkill(Person trainee)
			: base(trainee, 0)
		{
		}

		public TrainingSkill(Person trainee, int duration, SkillType skillType)
			: base(trainee, duration)
		{
			this.SkillType = skillType;
		}

		public TrainingSkill(TrainOrder trainOrder)
			: this(trainOrder.Trainee, trainOrder.DurationLeft, trainOrder.SkillType)
		{
			this.TrainOrder = trainOrder;
		}

		public override void Execute(int week)
		{
            if (this.ExecuteCondition)
			{
				// start of the training
				if (this.Duration == this.SkillType.TrainingDuration)
				{
					this.Trainee.EventReports.Add(
						week,
						string.Format("started training of {0} skill.",
						this.SkillType.ReportName));
				}

				// training
				this.Duration--; 
				this.Trainee.ExecutedLongOrder = true;

				// finished training
				if (this.Duration == 0)
				{
					this.Trainee.Skills.Add(this.SkillType);
					this.Trainee.EventReports.Add(
						week,
						string.Format("trained {0} skill.",
						this.SkillType.ReportName));
				}

				base.Execute(week);
			}
		}

        public override void LoadXml(XmlElement elTrainingSkill)
        {
            base.LoadXml(elTrainingSkill);
            this.SkillType = SkillType.All[elTrainingSkill.GetAttribute("skill")];
        }

        public override XmlElement SaveXml(XmlDocument doc)
        {
            base.SaveXml(doc);
            this.xmlElement.SetAttribute("type", "training-officer");
            this.xmlElement.SetAttribute("skill", this.SkillType.Name);
            return this.xmlElement;
        }

	}
}
