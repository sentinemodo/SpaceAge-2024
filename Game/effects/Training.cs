using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public abstract class Training : Effect
	{
        public TrainOrder TrainOrder { get; set; }

		protected Training(IEffectable subject, int duration)
			: base(subject, duration)
		{
			this.ExecuteCondition = true;
		}

		public override void Execute(int week)
		{
			this.ExecuteCondition = false;
			if (this.Duration == 0)
			{
				this.Executed = true;
			}
		}

		public void Train()
		{
			this.ExecuteCondition = true;
		}

        public override void LoadXml(XmlElement elTraining)
        {
            base.LoadXml(elTraining);
            this.ExecuteCondition = false;
        }
	}
}
