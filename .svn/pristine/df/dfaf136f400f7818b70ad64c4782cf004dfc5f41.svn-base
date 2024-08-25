using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public abstract class ImmediateOrder : Order
	{
		public ImmediateOrder(IOrderable subject)
			: base(subject)
		{
            this.FailedToExecute = false;
		}

        public bool FailedToExecute { get; set; }

		public override void Parse(string command)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override void Execute(int week)
		{
            if (this.Executed)
            {
                if (this.Repeat > 0)
                {
                    this.Repeat--;
                }
                this.FailedToExecute = false;
            }
		}

	}
}
