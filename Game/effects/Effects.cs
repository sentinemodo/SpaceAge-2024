using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class Effects : List<Effect>, IReporting
	{
		public void Execute(int week)
		{
			foreach (Effect effect in this)
			{
				if (!effect.Executed)
				{
					effect.Execute(week);
				}
			}
		}

		public void RemoveExecuted()
		{
			this.RemoveAll(nonPermanentPredicate);
		}

		private static bool executedPredicate(Effect effect)
		{
			return effect.Executed;
		}

		private static bool nonPermanentPredicate(Effect effect)
		{
			return ((effect.Executed) && (!effect.Permanent) && (effect.Duration == 0));
		}

		private void executeAction(int week, Effect effect)
		{
			effect.Execute(week);
		}

		public List<Effect> Executed
		{
			get
			{
				return this.FindAll(executedPredicate);
			}
		}

		public bool IsReceiving
		{
			get
			{
				foreach (Effect effect in this)
				{
					if (effect is Receiving)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool IsFuelled
		{
			get
			{
				foreach (Effect effect in this)
				{
					if (effect is Fuelled)
					{
						return true;
					}
				}
				return false;
			}
		}

        public Fuelled Fuelled
        {
            get
            {
                foreach (Effect effect in this)
                {
                    if (effect is Fuelled)
                    {
                        return (Fuelled)effect;
                    }
                }
                return null;
            }
        }

        public bool IsMoving
        {
            get
            {
                foreach (Effect effect in this)
                {
                    if (effect is Moving)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public Moving Moving
        {
            get
            {
                foreach (Effect effect in this)
                {
                    if (effect is Moving)
                    {
                        return (Moving)effect;
                    }
                }
                return null;
            }
        }
        
        public bool IsProducing
		{
			get
			{
				foreach (Effect effect in this)
				{
					if (effect is Producing)
					{
						return true;
					}
				}
				return false;
			}
		}

        public Producing Producing
        {
            get
            {
                foreach (Effect effect in this)
                {
                    if (effect is Producing)
                    {
                        return (Producing)effect;
                    }
                }
                return null;
            }
        }

				public bool IsTraining
				{
					get
					{
						foreach (Effect effect in this)
						{
							if (effect is Training)
							{
								return true;
							}
						}
						return false;
					}
				}

				public Training Training
				{
					get
					{
						foreach (Effect effect in this)
						{
							if (effect is Training)
							{
								return (Training)effect;
							}
						}
						return null;
					}
				}

		#region IReporting Members

		private bool anyVisible(Faction faction)
		{
			foreach (Effect effect in this)
			{
				if (effect.Visible(faction))
					return true;
			}
			return false;
		}

		public List<string> Report(Faction faction)
		{
			return this.Report(faction, 0);
		}

		public List<string> Report(Faction faction, int level)
		{
			ReportLines reportLines = new ReportLines();
			if (this.anyVisible(faction))
			{
				reportLines.Add("effects:", level);
				
				foreach (Effect effect in this)
				{
					if (effect.Visible(faction))
					{
						reportLines.Add(effect.Report(faction, level + 1));
					}
				}
			}
			return reportLines.IndentedLines;
		}

		#endregion

        public void RecalculateDuration(double ratio)
        {
            foreach (Effect effect in this)
            {
                if (!effect.Permanent) 
                {
                    effect.Duration = (int)(Math.Ceiling((double)effect.Duration * ratio));
                }
            }
        }

        public void LoadXml(XmlElement elHolder, IEffectable holder)
        {
            Effect effect;

            foreach (XmlElement elEffect in elHolder.SelectNodes("effect"))
            {
                switch (elEffect.GetAttribute("type"))
                {
                    case "fuelled": 
                        effect = new Fuelled(holder);
                        break;
                    case "moving":
                        effect = new Moving(holder);
                        break;
                    case "producing-modules":
                        effect = new ProducingModule(holder);
                        break;
                    case "producing-items":
                        effect = new ProducingItems(holder);
                        break;
                    case "producing-energy":
                        effect = new ProducingEnergy(holder);
                        break;
                    case "receiving-items":
                        effect = new ReceivingItems(holder);
                        break;
                    case "receiving-modules":
                        effect = new ReceivingModules(holder);
                        break;
                    case "receiving-technology":
                        effect = new ReceivingTechnology(holder);
                        break;
                    case "lightly-damaged":
                        effect = new LightlyDamaged(holder);
                        break;
                    case "training-officer":
                        if (elEffect.HasAttribute("skill"))
                        {
                            effect = new TrainingSkill((Person)holder);
                        }
                        else
                        {
                            effect = new TrainingOfficer((ModuleStack)holder);
                        }
                        break;
                    default:
                        throw new Exception("Unknown effect type " + elEffect.GetAttribute("type"));
                }
                effect.LoadXml(elEffect);
            }
        }

        public XmlElement SaveXml(XmlDocument doc, XmlElement elHolder, Faction faction = null)
        {
            XmlElement elEffect;
            foreach (Effect effect in this)
            {
                if (!effect.Visible(faction))
                    continue;

                elEffect = effect.SaveXml(doc);
                elHolder.AppendChild(elEffect);
            }

            return elHolder;
        }
    }
}
