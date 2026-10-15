using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace SpaceAge
{
	public partial class CatalogLoader
	{
		private void loadItemStubs(XmlDocument confDocument, Game game)
		{
			string name;

			// Stars
			foreach (XmlElement el in confDocument.SelectNodes("/data/star/entry"))
			{
				
					name = el.GetAttribute("name");
					game.StarTypes.Add(name, new StarType(name));
				
			}

			// Planets
			foreach (XmlElement el in confDocument.SelectNodes("/data/planet/entry"))
			{
				
					name = el.GetAttribute("name");
					game.PlanetTypes.Add(name, new PlanetType(name));
				
			}

			// Moons
			foreach (XmlElement el in confDocument.SelectNodes("/data/moon/entry"))
			{
				
					name = el.GetAttribute("name");
					game.MoonTypes.Add(name, new MoonType(name));
				
			}

			// Regions 
			foreach (XmlElement el in confDocument.SelectNodes("/data/region/entry"))
			{
				
					name = el.GetAttribute("name");
                    game.RegionTypes.Add(name, new RegionType(name));
				
			}

			#region itemTypes
			foreach (XmlElement el in confDocument.SelectNodes("/data/item/entry"))
			{
				ItemType itemType;
				
                    itemType = new ItemType(el.GetAttribute("name"));
				
			}
			#endregion

			#region races
			// races 
			foreach (XmlElement el in confDocument.SelectNodes("/data/race/entry"))
			{
				// as crew items
				ItemType itemType;
				
					itemType = new ItemType(el.GetAttribute("name"));
				

				// as crew officers
				Race race;
				
					race = new Race(el.GetAttribute("name"));
				

			}
			#endregion

			#region skills
			// skills 
			foreach (XmlElement el in confDocument.SelectNodes("/data/skill/entry"))
			{
				SkillType skillType;
				
					skillType = new SkillType(el.GetAttribute("name"));
				
			}
			#endregion

			#region technologies
			Technology technology;
			foreach (XmlElement el in confDocument.SelectNodes("/data/technology/entry"))
			{
				
					name = el.GetAttribute("name");
					technology = new Technology(name);
				
			}
			#endregion

			#region moduletypes
			foreach (XmlElement el in confDocument.SelectNodes("/data/module/entry"))
			{
				ModuleType moduleType;
				
					moduleType = new ModuleType(el.GetAttribute("name"));
				
			}
			#endregion
		}
	}
}
