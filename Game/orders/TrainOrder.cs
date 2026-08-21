using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class TrainOrder : LongOrder
	{
		public TrainOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.train;
		}

		private bool trainingOfficer = false;
		public bool TrainingOfficer
		{
			get { return this.trainingOfficer; }
			set { this.trainingOfficer = value; }
		}

		#region skill being trained
		// person receiving training (order executing unit)
		public Person Trainee
		{
			get 
			{
				if (this.trainingOfficer)
				{
					return null;
				}
				else
				{
					return (Person)this.Subject;
				}
			}
		}

		// skill being trained if not training officer
		private SkillType skillType = null;
		public SkillType SkillType 
		{
			get { return this.skillType; }
			set { this.skillType = value; }
		}
		#endregion

		#region officer being trained
		// modulestack training officer (order executing unit)
		public ModuleStack Trainer
		{
			get 
			{
				if (this.trainingOfficer)
				{
					return (ModuleStack)this.Subject;
				}
				else 
				{
					return null;
				}
			}
		}

		// if trained new officer, race of that officer
		private Race race = null;
		public Race Race
		{
			get { return this.race; }
			set { this.race = value; }
		}


		// if trained new officer, target person
		private Person officer = null;
		public Person Officer
		{
			get { return this.officer; }
			set { this.officer = value; }
		}

		// if trained new officer, target parent of the person
		private IHolder officerParent = null;
		public IHolder OfficerParent
		{
			get { return this.officerParent; }
			set { this.officerParent = value; }
		}
		#endregion

		public override void Parse(string command)
		{
			// TRAIN race OFFICER
			// TRAIN race OFFICER AS "alias"
			// TRAIN race OFFICER AS "alias" FOR "alias"
			// TRAIN SKILL skill

			string token;
			if (string.IsNullOrEmpty(command.Trim()))
			{
				throw new Exception("Bad syntax race or SKILL expected.");
			}

			token = LineParser.GetToken(ref command);
			if (token == "skill")
			{
				this.trainingOfficer = false;
				token = LineParser.GetToken(ref command);
				try
				{
					this.skillType = SkillType.All[token];
				}
				catch (Exception ex)
				{
					throw new Exception("Bad syntax or unknown skill", ex);
				}
			}
			else
			{
				this.trainingOfficer = true;
				try
				{
					this.race = Race.All[token];
				}
				catch (Exception ex)
				{
					throw new Exception("Bad syntax or unknown race. " + token + " received", ex);
				}

				token = LineParser.GetToken(ref command);
				if (token != string.Empty && token != "officer")
				{
					throw new Exception("Bad syntax, OFFICER expected. Received: " + token);
				}

				token = LineParser.GetToken(ref command);
				if (token != string.Empty && token != "as")
				{
					throw new Exception("Bad syntax, AS expected. Received: " + token);
				}

				token = LineParser.GetQuotedToken(ref command);
				if (token != string.Empty)
				{
					this.officer = Person.All.GetOrCreateNewPerson(this.Trainer.Owner, token);
				}
				else
				{
					throw new Exception("Bad syntax receiver person id expected or alias. Received: " + token);
				}

				token = LineParser.GetQuotedToken(ref command);
				if (token != string.Empty && token != "for")
				{
					throw new Exception("Bad syntax FOR expected. Received: " + token);
				}

				if (token == "for")
				{
					token = LineParser.GetQuotedToken(ref command);
					if (token != string.Empty)
					{
						this.officerParent = ModuleStack.All.GetOrCreateNewModuleStack(this.Trainer.Owner, token);
					}
					else
					{
						throw new Exception("Bad syntax receiver parent modulestack id expected or alias. Received: " + token);
					}
				}
			}
		}

		public bool MatchesTraining(TrainOrder other)
		{
			if (other == null || this.TrainingOfficer != other.TrainingOfficer)
			{
				return false;
			}
			if (this.TrainingOfficer)
			{
				return this.Race == other.Race && this.Officer == other.Officer;
			}
			return this.SkillType == other.SkillType;
		}

        public override void LoadXml(XmlElement elOrder)
        {
            XmlElement elTrain = (XmlElement)elOrder.SelectNodes("train")[0];
            XmlElement elTrainee = (XmlElement)elTrain.SelectSingleNode("officer");
            if (elTrainee != null)
            {
                this.trainingOfficer = true;
                this.Officer = Person.All.GetOrCreateNewPerson(this.Trainer.Owner, elTrainee.GetAttribute("name"));
                this.Race = Race.All[elTrainee.GetAttribute("race")];
                if (elTrainee.HasAttribute("officer-parent"))
                {
                    this.OfficerParent = ModuleStack.All.GetOrCreateNewModuleStack(this.Trainer.Owner, elTrainee.GetAttribute("officer-parent"));
                }
            }
            else
            {
                XmlElement elSkill = (XmlElement)elTrain.SelectNodes("skill")[0];
                this.SkillType = SkillType.All[elSkill.GetAttribute("skill")];
            }
        }
        
        public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
            XmlElement elTrain = doc.CreateElement("train");

			if (this.trainingOfficer)
            {
					XmlElement elTrainee;
					elTrainee = doc.CreateElement("officer");
					elTrainee.SetAttribute("name", this.officer.Name);
					elTrainee.SetAttribute("race", this.race.Name);
					if (this.officerParent != null)
					{
						elTrainee.SetAttribute("officer-parent", this.officerParent.Name);
					}
					elTrain.AppendChild(elTrainee);
			} else 
            {
					XmlElement elSkill;
					elSkill = doc.CreateElement("skill");
					elSkill.SetAttribute("skill", this.skillType.Name);
                    elTrain.AppendChild(elSkill);
			}

            this.xmlElement.AppendChild(elTrain);
			return this.xmlElement;
		}

        public override List<string> Report(Faction owner)
        {
            List<string> lines = new List<string>();
            string line;
            line = string.Format("{0}{1}train {2}",
				this.Conditions,
				(this.Repeat > 1) ? string.Concat(this.Repeat.ToString(), " ") : ((this.Repeat < 0) ? "@" : string.Empty),
				(this.trainingOfficer)
					? string.Concat(
						this.race.Name,
						(this.officer.IsFormed) ? " officer as " : " officer as new", 
						this.officer.Name, 
						((this.officerParent != null) ? string.Concat(" for ", this.officerParent.Name) : string.Empty)) 
					: string.Concat(
						"skill ",
						this.skillType.Name));
            lines.Add(line);
            return lines;
		}

		private Training training = null;
		public Training Training
		{
			get { return this.training; }
			set { this.training = value;}
		}

		public bool hasRace
		{
			get
			{
				// TODO: verify that officers do not count
				return this.Trainer.ItemStacks.Has(this.race.ItemType);
			}
		}

		private Training FindMatchingTraining()
		{
			IEffectable holder = this.Subject as IEffectable;
			if (holder == null)
			{
				return null;
			}
			foreach (Effect effect in holder.Effects)
			{
				if (this.trainingOfficer)
				{
					TrainingOfficer officer = effect as TrainingOfficer;
					if (officer != null && !officer.Executed && officer.Race == this.Race)
					{
						return officer;
					}
				}
				else
				{
					TrainingSkill skill = effect as TrainingSkill;
					if (skill != null && !skill.Executed && skill.SkillType == this.SkillType)
					{
						return skill;
					}
				}
			}
			return null;
		}

		private void TryReconnectTraining()
		{
			if (this.Training != null)
			{
				return;
			}

			Training existing = this.FindMatchingTraining();
			if (existing == null)
			{
				return;
			}

			this.Training = existing;
			this.durationLeft = existing.Duration;
			existing.TrainOrder = this;
		}

		public override void Execute(int week)
		{
			if (this.CanOperate(week))
			{
                if (this.Training == null)
                {
                    this.TryReconnectTraining();
                }
                if (this.Training == null)
                {
                    this.startTraining(week);
                }

                // train 
                if (this.Training != null && !this.Training.Executed)
                {
                    this.durationLeft--;
                    this.Training.Train();
                    this.Training.Execute(week);
                    this.Executing = true;
                }

				// finish order execution
				if (this.DurationLeft <= 0)
				{
					this.Training = null;
				}
			}
			base.Execute(week);
		}

        private void startTraining(int week)
        {
            if (this.trainingOfficer && this.hasRace)
            {
                if (!this.hasRace)
                {
                    this.Trainer.EventReports.Add(
                        week, 
                        string.Format("TRAIN failed, tried to train {0}, but no available crew found.",
                        this.race.ReportName));
                }
                else
                {
                    this.durationLeft = this.race.OfficerTrainingDuration;
                    this.training = new TrainingOfficer(this.Trainer, this.durationLeft, this.race, this.officer, this.officerParent);
                }
            }
            else
            {
                this.durationLeft = this.skillType.TrainingDuration;
                this.training = new TrainingSkill(this.Trainee, this.durationLeft, this.skillType);
            }
        }
    }
}
