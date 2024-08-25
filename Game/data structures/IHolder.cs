using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public interface IHolder : ISingular, IOrderable, IReporting
	{
        string          BattleReportName    { get; }

        IHolder         Parent              { get; set; }
        Location        Location            { get; }
        bool            IsLocation          { get; }
		double			Capacity	    	{ get; }
		double			CapacityUsed	    { get; }

        int             TechnologyCapacity      { get; }
        int             TechnologyCapacityUsed  { get; }

        People          People              { get; }
        bool            HasPeople           { get; }
        ModuleStacks    ModuleStacks        { get; }
        bool HasModuleStacks(ModuleType moduleType = null);
	}
}
