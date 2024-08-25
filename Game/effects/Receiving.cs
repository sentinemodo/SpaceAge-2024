using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public abstract class Receiving : Effect
	{

		public ModuleStack Receiver
		{
			get
			{
				return (ModuleStack)this.Subject;
			}
		}

		public Receiving(IEffectable receiver, int duration)
			: base(receiver, duration)
		{
		}

		public override void Execute(int week)
		{
			this.Duration--;
			if (this.Duration == 0)
			{
				this.Executed = true;
			}
		}

	}
}
