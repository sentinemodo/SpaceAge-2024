using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public abstract class Tactic : XMLProcessing
	{
		public Tactic(ModuleStack subject)
		{            
			this.Subject = subject;
		}

		public ModuleStack Subject { get; set; }

		protected string name;
		public string ReportName
		{
			get
			{
				return this.name;
			}
		}

		public virtual Module ResolveHitLocation(ModuleStack target)
		{
			return null;
		}

		public virtual int Attack
		{
			get { return this.Subject.Attack; }
		}

        public override void LoadXml(XmlElement xmlElement)
        {
            throw new NotImplementedException();
        }
	}
}
