using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public abstract class Unit
	{
		private ModuleStack moduleStack;
		public ModuleStack ModuleStack
		{
			get { return this.moduleStack; }
			set { this.moduleStack = value; }
		}

		public Unit(ModuleStack moduleStack)
		{
			this.moduleStack = moduleStack; 
		}

		private bool moved;
		public bool Moved
		{
			get { return this.moved; }
			set { this.moved = value; }
		}

		private bool attacked;
		public bool Attacked
		{
			get { return this.attacked; }
			set { this.attacked = value; }
		}
	}
}
