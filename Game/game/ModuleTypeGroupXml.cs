using System.Collections.Generic;

namespace SpaceAge
{
	public static class ModuleTypeGroupXml
	{
		public static EModuleTypesGroup Parse(string groupName)
		{
			switch (groupName)
			{
				case "agricultural":
					return EModuleTypesGroup.agricultural;
				case "command":
					return EModuleTypesGroup.command;
				case "energy":
					return EModuleTypesGroup.energy;
				case "extraction":
					return EModuleTypesGroup.extraction;
				case "frigate":
					return EModuleTypesGroup.frigate;
				case "habitat":
					return EModuleTypesGroup.habitat;
				case "infantry":
					return EModuleTypesGroup.infantry;
				case "military":
					return EModuleTypesGroup.military;
				case "production":
					return EModuleTypesGroup.production;
				case "propulsion":
					return EModuleTypesGroup.propulsion;
				case "research":
					return EModuleTypesGroup.research;
				case "settlement":
					return EModuleTypesGroup.settlement;
				case "spacecraft":
					return EModuleTypesGroup.spacecraft;
				case "space station":
					return EModuleTypesGroup.spaceStation;
				case "storage":
					return EModuleTypesGroup.storage;
				case "vehicle":
					return EModuleTypesGroup.vehicle;
				default:
					throw new KeyNotFoundException("Unknown moduletype group " + groupName);
			}
		}

		public static string ToToken(EModuleTypesGroup group)
		{
			switch (group)
			{
				case EModuleTypesGroup.agricultural:
					return "agricultural";
				case EModuleTypesGroup.command:
					return "command";
				case EModuleTypesGroup.energy:
					return "energy";
				case EModuleTypesGroup.extraction:
					return "extraction";
				case EModuleTypesGroup.frigate:
					return "frigate";
				case EModuleTypesGroup.habitat:
					return "habitat";
				case EModuleTypesGroup.infantry:
					return "infantry";
				case EModuleTypesGroup.military:
					return "military";
				case EModuleTypesGroup.production:
					return "production";
				case EModuleTypesGroup.propulsion:
					return "propulsion";
				case EModuleTypesGroup.research:
					return "research";
				case EModuleTypesGroup.settlement:
					return "settlement";
				case EModuleTypesGroup.spacecraft:
					return "spacecraft";
				case EModuleTypesGroup.spaceStation:
					return "space station";
				case EModuleTypesGroup.storage:
					return "storage";
				case EModuleTypesGroup.vehicle:
					return "vehicle";
				default:
					return null;
			}
		}
	}
}
