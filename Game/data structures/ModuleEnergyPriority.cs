using System;

namespace SpaceAge
{
	public static class ModuleEnergyPriority
	{
		public static int DefaultForGroup(EModuleTypesGroup group)
		{
			switch (group)
			{
				case EModuleTypesGroup.energy:
					return 0;
				case EModuleTypesGroup.habitat:
					return 1;
				case EModuleTypesGroup.command:
					return 2;
				case EModuleTypesGroup.military:
				case EModuleTypesGroup.infantry:
					return 3;
				case EModuleTypesGroup.propulsion:
					return 4;
				case EModuleTypesGroup.extraction:
					return 5;
				case EModuleTypesGroup.production:
					return 6;
				case EModuleTypesGroup.storage:
					return 10;
				case EModuleTypesGroup.research:
					return 10;
				default:
					return 8;
			}
		}
	}
}
