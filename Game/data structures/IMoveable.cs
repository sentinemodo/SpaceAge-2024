using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public interface IMoveable : ISingular, IEffectable
	{
        MoveModes       MoveModes       { get; }
        bool HasModuleStacks(ModuleType moduleType = null);
        ModuleStacks    ModuleStacks    { get; }
        ItemStacks      Fuel            { get; }
        int             FuelDuration    { get; }
	}
}
