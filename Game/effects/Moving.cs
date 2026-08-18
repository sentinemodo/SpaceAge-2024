using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class Moving : Effect
	{
		public ModuleStack Mover
		{
			get
			{
				return (ModuleStack)this.Subject;
			}
		}

        public EMoveMode    MoveMode        { get; set; }
		public IHolder      Destination     { get; set; }
        public int          InitialDuration { get; set; }

		public Moving(IEffectable mover)
			: base(mover, 0)
		{
			this.ExecuteCondition = false;
		}

		public Moving(IEffectable mover, EMoveMode moveMode, IHolder destination, int duration)
			: base(mover, duration)
		{
            this.MoveMode = moveMode;
			this.Destination = destination;
            this.InitialDuration = duration;
			this.ExecuteCondition = false;
		}

		public override void Execute(int week)
		{
			if (this.ExecuteCondition)
			{
				this.Duration--;
				if (this.Duration == 0)
				{
					this.Executed = true;
				}
				this.ExecuteCondition = false;
			}
		}

		public void Use()
		{
			this.ExecuteCondition = true;
		}

        public override string Description
        {
            get
            {
                return string.Format("will move for another {0} weeks before next destination.", this.Duration);
            }
        }

        public override void LoadXml(XmlElement elEffect)
        {
            base.LoadXml(elEffect);
            this.Destination = this.findDestination(elEffect.GetAttribute("destination"));
            switch (elEffect.GetAttribute("move-mode"))
            {
                case "space":
                    this.MoveMode = EMoveMode.space;
                    break;
                default:
                    this.MoveMode = EMoveMode.ground;
                    break;
            }
            this.InitialDuration = this.Duration;
            if (this.Mover.MovingTo == null)
            {
                this.Mover.MovingTo = this.Destination;
            }
        }

        public override XmlElement SaveXml(XmlDocument doc)
        {
            base.SaveXml(doc);
            this.xmlElement.SetAttribute("type", "moving");
            this.xmlElement.SetAttribute("destination", this.Destination.Name);
            switch (this.MoveMode)
            {
                case EMoveMode.ground:
                    this.xmlElement.SetAttribute("move-mode", "ground");
                    break;
                case EMoveMode.space:
                    this.xmlElement.SetAttribute("move-mode", "space");
                    break;
                default:
                    throw new Exception("Uknown move mode");
            }            

            return this.xmlElement;
        }

		private IHolder findDestination(string token)
		{
			if (Region.All.ContainsKey(token))
			{
				return Region.All[token];
			}
			if (Orbit.All.ContainsKey(token))
			{
				return Orbit.All[token];
			}
			if (Star.All.ContainsKey(token))
			{
				return Star.All[token].Orbit;
			}
			if (Planet.All.ContainsKey(token))
			{
				return Planet.All[token].Orbit;
			}
			if (Moon.All.ContainsKey(token))
			{
				return Moon.All[token].Orbit;
			}
			if (Anomaly.All.ContainsKey(token))
			{
				return Anomaly.All[token].Orbit;
			}
			throw new Exception("The target location has not been found: " + token);
		}
	}
}
