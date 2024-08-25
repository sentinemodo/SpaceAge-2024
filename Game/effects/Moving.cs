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

		public Moving(IEffectable mover, EMoveMode moveMode, IHolder destination, int duration)
			: base(mover, duration)
		{
            this.MoveMode = moveMode;
			this.Destination = destination;
            this.InitialDuration = duration;
			this.ExecuteCondition = true;
		}

		public override void Execute(int week)
		{
			this.ExecuteCondition = false;
            this.Duration--;

			if (this.Duration == 0)
			{
				this.Executed = true;
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
	}
}
