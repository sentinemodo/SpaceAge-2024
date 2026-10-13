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
				return this.Subject as ModuleStack;
			}
		}

        public EMoveMode    MoveMode        { get; set; }
		public IHolder      Origin          { get; set; }
		public IHolder      Destination     { get; set; }
        public int          InitialDuration { get; set; }
		public int          ReturnLegWeeks  { get; set; }

		public Moving(IEffectable mover)
			: base(mover, 0)
		{
			this.ExecuteCondition = false;
		}

		public Moving(IEffectable mover, EMoveMode moveMode, IHolder origin, IHolder destination, int duration)
			: base(mover, duration)
		{
            this.MoveMode = moveMode;
			this.Origin = origin;
			this.Destination = destination;
            this.InitialDuration = duration;
			this.ReturnLegWeeks = 0;
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
				if (this.Origin != null && this.Destination != null)
				{
					return string.Format(
						"moving from {0} to {1}, {2} weeks remaining of {3} week leg.",
						this.Origin.ReportName,
						this.Destination.ReportName,
						this.Duration,
						this.InitialDuration);
				}
                return string.Format("will move for another {0} weeks before next destination.", this.Duration);
            }
        }

        public override void LoadXml(XmlElement elEffect)
        {
            base.LoadXml(elEffect);
            this.Destination = this.findDestination(elEffect.GetAttribute("destination"));
			if (elEffect.HasAttribute("origin"))
			{
				this.Origin = this.findDestination(elEffect.GetAttribute("origin"));
			}
            if (elEffect.HasAttribute("move-mode"))
            {
                this.MoveMode = MoveModeXml.Parse(elEffect.GetAttribute("move-mode"));
            }
            else
            {
                this.MoveMode = EMoveMode.ground;
            }
			if (elEffect.HasAttribute("initial-duration"))
			{
				this.InitialDuration = this.XMLAssignInteger(elEffect.GetAttribute("initial-duration"), this.Duration);
			}
			else
			{
				this.InitialDuration = this.Duration;
			}
			if (elEffect.HasAttribute("return-leg-weeks"))
			{
				this.ReturnLegWeeks = this.XMLAssignInteger(elEffect.GetAttribute("return-leg-weeks"), 0);
			}
            this.syncTransitDestination();
        }

        public override XmlElement SaveXml(XmlDocument doc)
        {
            base.SaveXml(doc);
            this.xmlElement.SetAttribute("type", "moving");
			if (this.Origin != null)
			{
				this.xmlElement.SetAttribute("origin", this.Origin.Name);
			}
            this.xmlElement.SetAttribute("destination", this.Destination.Name);
            this.xmlElement.SetAttribute("move-mode", MoveModeXml.ToToken(this.MoveMode));
			this.xmlElement.SetAttribute("initial-duration", this.InitialDuration.ToString());
			this.xmlElement.SetAttribute("return-leg-weeks", this.ReturnLegWeeks.ToString());

            return this.xmlElement;
        }

		private void syncTransitDestination()
		{
			ModuleStack stack = this.Subject as ModuleStack;
			if (stack != null)
			{
				if (stack.MovingTo == null)
				{
					stack.MovingTo = this.Destination;
				}
				return;
			}

			Person person = this.Subject as Person;
			if (person != null && person.MovingTo == null)
			{
				person.MovingTo = this.Destination;
			}
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
			if (Belt.All.ContainsKey(token))
			{
				return Belt.All[token];
			}
			if (Alderson.All.ContainsKey(token))
			{
				return Alderson.All[token].Orbit;
			}
			if (Anomaly.All.ContainsKey(token))
			{
				return Anomaly.All[token].Orbit;
			}
			throw new Exception("The target location has not been found: " + token);
		}
	}
}
