using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public interface IOrderable : ISingular, IEventReporting
	{
        bool    HasOrders           { get; }
		Orders  Orders	            { get; }
		Faction Owner	            { get; }

		bool    ExecutedLongOrder   { get; set; }
        bool    Execute(int week);
	}
}
