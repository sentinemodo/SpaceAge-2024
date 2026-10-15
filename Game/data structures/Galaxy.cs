using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class Galaxy : IReporting
	{
		private SpaceSystems spaceSystems = new SpaceSystems();
		public SpaceSystems SpaceSystems
		{
			get { return this.spaceSystems; }
			set { this.spaceSystems = value; }
		}

		#region IReporting Members

		public List<string> Report(Faction faction)
		{
			List<string> reportLines = new List<string>();

			reportLines.AddRange(this.reportHeader(faction));
			// deepSpace moduleStacks
			reportLines.AddRange(this.reportSpaceSystems(faction));
			reportLines.Add("");

			return reportLines;
		}

		private List<string> reportHeader(Faction faction)
		{
			List<string> lines = new List<string>
            {
                "Galaxy report:",
                "------------------------------------------------------------"
            };
			return lines;
		}

		private List<string> reportSpaceSystems(Faction faction)
		{
			List<string> lines = new List<string>();
			foreach (SpaceSystem spaceSystem in this.SpaceSystems)
			{
				if (spaceSystem.Visible(faction))
				{
					lines.AddRange(spaceSystem.Report(faction));
				}
			}

			return lines;
		}

		#endregion

		public void LoadXml(XmlElement elGalaxy, DataFile dataFile)
		{
			foreach (XmlElement elSystem in elGalaxy.SelectNodes("system"))
			{
				SpaceSystem system = new SpaceSystem(elSystem.GetAttribute("name"));
				system.LoadXml(elSystem);
				system.Coordinates.X = dataFile.XMLAssignDouble(elSystem.GetAttribute("X"), 0);
				system.Coordinates.Y = dataFile.XMLAssignDouble(elSystem.GetAttribute("Y"), 0);
				system.Coordinates.Z = dataFile.XMLAssignDouble(elSystem.GetAttribute("Z"), 0);

				#region stars
				foreach (XmlElement elStar in elSystem.SelectNodes("star"))
				{
					Star star = new Star(system, elStar.GetAttribute("name"));
					star.LoadXml(elStar);
                    //star.Coordinates.X = this.assignDouble(elStar.GetAttribute("X"), 0);
                    //star.Coordinates.Y = this.assignDouble(elStar.GetAttribute("Y"), 0);
                    //star.Coordinates.Z = this.assignDouble(elStar.GetAttribute("Z"), 0);

					if (elStar.HasAttribute("type"))
						star.StarType = dataFile.Game.StarTypes[elStar.GetAttribute("type")];

					if (elStar.HasAttribute("mass"))
						star.Mass = Convert.ToDouble(elStar.GetAttribute("mass"));

					system.Objects.Add(star.Name, star);
				}
				#endregion

				// planets
				foreach (XmlElement elPlanet in elSystem.SelectNodes("planet"))
				{
					Planet.LoadGalaxyXml(elPlanet, system, dataFile);
				}

				foreach (XmlElement elBelt in elSystem.SelectNodes("belt"))
				{
					Galaxy.loadBelt(elBelt, system, null, dataFile);
				}

				foreach (XmlElement elAlderson in elSystem.SelectNodes("alderson"))
				{
					loadAlderson(elAlderson, system, dataFile);
				}

				// asteroid belts

				// kuiper belts

				// comets

				this.SpaceSystems.Add(system);
			}
		}

		public void LoadExits(XmlElement elGalaxy, DataFile dataFile)
		{
			try
			{
				
				// systems
				foreach (XmlElement elSystem in elGalaxy.SelectNodes("system"))
				{
					try
					{
						// planets
						foreach (XmlElement elPlanet in elSystem.SelectNodes("planet"))
						{
							Planet.LoadGalaxyExits(elPlanet, dataFile);
						}

						foreach (XmlElement elBelt in elSystem.SelectNodes("belt"))
						{
							Galaxy.loadGalaxyExits(elBelt, Belt.All[elBelt.GetAttribute("name")], dataFile);
						}

						foreach (XmlElement elAlderson in elSystem.SelectNodes("alderson"))
						{
							Alderson alderson = Alderson.All[elAlderson.GetAttribute("name")];
							Galaxy.loadGalaxyExits(elAlderson, alderson.Orbit, dataFile);
						}

						// asteroid belts

						// kuiper belts

						// comets
					}
					catch (Exception ex)
					{
						throw new Exception("Tried to parse exits for system " + elSystem.GetAttribute("name"), ex);
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Tried to parse galaxy for exits", ex);
			}
		}







		internal static void loadRaces(XmlElement elHolder, Races races)
		{
			foreach (XmlElement elRace in elHolder.SelectNodes("race"))
			{
				try
				{
					races.Add(elRace.GetAttribute("type"), Race.All[elRace.GetAttribute("type")]);
				}
				catch (Exception ex)
				{
					throw new Exception("Tried to parse race ", ex);
				}
			}
		}

		internal static void loadResources(XmlElement elHolder, IResourcesHolder holder, DataFile dataFile)
		{
			foreach (XmlElement elResource in elHolder.SelectNodes("resource"))
			{
				try
				{
					Resource resource = new Resource();
					resource.ItemType = dataFile.Game.ItemTypes[elResource.GetAttribute("type")];
					resource.Quantity = dataFile.XMLAssignInteger(elResource.GetAttribute("quantity"), 1);
					holder.Resources.Add(resource);
				}
				catch (Exception ex)
				{
					throw new Exception("Tried to parse resources", ex);
				}
			}
		}





		internal static void loadGalaxyExits(XmlElement elHolder, Location holder, DataFile dataFile)
		{
			foreach (XmlElement elExit in elHolder.SelectNodes("exit"))
			{
				try
				{
					Exit exit = new Exit();
					if (elExit.HasAttribute("region"))
					{
						exit.To = Region.All[elExit.GetAttribute("region")];
					}
					else if (elExit.HasAttribute("orbit"))
					{
						exit.To = Orbit.All[elExit.GetAttribute("orbit")];
					}
					else if (elExit.HasAttribute("belt"))
					{
						exit.To = Belt.All[elExit.GetAttribute("belt")];
					}
					else if (elExit.HasAttribute("alderson"))
					{
						exit.To = Alderson.All[elExit.GetAttribute("alderson")].Orbit;
					}
					foreach (XmlElement elExitMode in elExit.SelectNodes("exitmode"))
					{
						ExitMode exitMode = new ExitMode();
						if (elExitMode.HasAttribute("mode"))
						{
							exitMode.Mode = MoveModeXml.Parse(elExitMode.GetAttribute("mode"));
						}
						else
						{
							exitMode.Mode = EMoveMode.ground;
						}

						exitMode.Duration = dataFile.XMLAssignInteger(elExitMode.GetAttribute("duration"), 0);
						exit.ExitModes.Add(exitMode.Mode, exitMode);
					}
					holder.Exits.Add(exit);
				}
				catch (Exception ex)
				{
					throw new Exception("Tried to parse exits", ex);
				}
			}
		}

		internal static void loadBelt(XmlElement elBelt, SpaceSystem system, Planet planet, DataFile dataFile)
		{
			Belt belt = new Belt(system, planet, elBelt.GetAttribute("name"));
			belt.LoadXml(elBelt);
			try
			{
				belt.AU = dataFile.XMLAssignDouble(elBelt.GetAttribute("AU"), 0);
				XmlElement elComposition = (XmlElement)elBelt.SelectSingleNode("composition");
				if (elComposition != null)
				{
					foreach (XmlElement elResource in elComposition.SelectNodes("resource"))
					{
						CompositionEntry entry = new CompositionEntry();
						entry.ItemType = dataFile.Game.ItemTypes[elResource.GetAttribute("type")];
						entry.Quantity = dataFile.XMLAssignInteger(elResource.GetAttribute("quantity"), 1);
						entry.Probability = dataFile.XMLAssignDouble(elResource.GetAttribute("probability"), 1);
						belt.Composition.Add(entry);
					}
				}
				ModuleStack.All.LoadXml(elBelt, belt);
			}
			catch (Exception ex)
			{
				throw new Exception("Tried to parse belt " + belt.ReportName, ex);
			}
		}

		private static void loadAlderson(XmlElement elAlderson, SpaceSystem system, DataFile dataFile)
		{
			Alderson alderson = new Alderson(system, elAlderson.GetAttribute("name"));
			alderson.LoadXml(elAlderson);
			try
			{
				alderson.AU = dataFile.XMLAssignDouble(elAlderson.GetAttribute("AU"), 0);
				alderson.TemperatureBand = BodyEnvironment.ParseTemperature(elAlderson.HasAttribute("temperature") ? elAlderson.GetAttribute("temperature") : "cold");
				alderson.AtmosphereBand = BodyEnvironment.ParseAtmosphere(elAlderson.GetAttribute("atmosphere"));
				if (elAlderson.HasAttribute("pair"))
				{
					alderson.PairName = elAlderson.GetAttribute("pair");
				}
				Orbit.LoadGalaxyXml(elAlderson, alderson, dataFile);
				system.Objects.Add(alderson.Name, alderson);
			}
			catch (Exception ex)
			{
				throw new Exception("Tried to parse alderson " + alderson.ReportName, ex);
			}
		}

		public XmlElement SaveXml(XmlDocument doc, Faction factionXMLreport = null)
		{
			XmlElement elGalaxy = doc.CreateElement("galaxy");

			// Systems
			foreach (SpaceSystem system in this.SpaceSystems)
			{

				// do nost save is the xml report is prepared for faction and faction is not observing the object
				if (factionXMLreport != null && !system.Visible(factionXMLreport))
					continue;

				XmlElement elSystem = doc.CreateElement("system");
				elGalaxy.AppendChild(elSystem);
				elSystem.SetAttribute("name", system.Name);
				elSystem.SetAttribute("name-en", system.FullName);
				elSystem.SetAttribute("X", system.Coordinates.X.ToString());
				elSystem.SetAttribute("Y", system.Coordinates.Y.ToString());
				elSystem.SetAttribute("Z", system.Coordinates.Z.ToString());

				// System Objects
				foreach (SpaceSystemObject systemObject in system.Objects.Values)
				{
					// do nost save is the xml report is prepared for faction and faction is not observing the object
					if (factionXMLreport != null && !systemObject.Visible(factionXMLreport))
						continue;

					XmlElement elObject;
					if (systemObject is Star)
					{
						elObject = doc.CreateElement("star");
						elSystem.AppendChild(elObject);
						elObject.SetAttribute("name", systemObject.Name);
						elObject.SetAttribute("name-en", systemObject.FullName);
					}
					else if (systemObject is Planet)
					{
						Planet.SaveGalaxyXml(doc, elSystem, (Planet)systemObject, factionXMLreport);
					}
					else if (systemObject is Alderson)
					{
						Alderson alderson = (Alderson)systemObject;
						elObject = doc.CreateElement("alderson");
						elSystem.AppendChild(elObject);
						elObject.SetAttribute("name", alderson.Name);
						elObject.SetAttribute("name-en", alderson.FullName);
						elObject.SetAttribute("AU", alderson.AU.ToString(CultureInfo.InvariantCulture));
						elObject.SetAttribute("temperature", BodyEnvironment.TemperatureToken(alderson.TemperatureBand));
						elObject.SetAttribute("atmosphere", BodyEnvironment.AtmosphereToken(alderson.AtmosphereBand));
						if (!string.IsNullOrEmpty(alderson.PairName))
						{
							elObject.SetAttribute("pair", alderson.PairName);
						}
						Orbit.SaveGalaxyXml(doc, elObject, alderson, factionXMLreport);
						if (alderson.Orbit != null)
						{
							saveExits(doc, elObject, alderson.Orbit);
						}
					}

				}

				foreach (Belt belt in Belt.All[system].Values)
				{
					if (factionXMLreport != null && !belt.Visible(factionXMLreport))
					{
						continue;
					}
					Galaxy.saveBelt(doc, elSystem, belt, factionXMLreport);
				}
			}
			return elGalaxy;
		}

		internal static void saveRaces(XmlDocument doc, XmlElement elHolder, Races races)
		{
			foreach (Race race in races.Values)
			{
				XmlElement elRace = doc.CreateElement("race");
				elHolder.AppendChild(elRace);
				elRace.SetAttribute("type", race.Name);
			}
		}



		internal static void saveResources(XmlDocument doc, XmlElement elObject, IResourcesHolder resourcesHolder, Faction factionXMLreport = null)
		{
			XmlElement elResource;
			if (factionXMLreport != null && !resourcesHolder.Visible(factionXMLreport))
			{
			}
			else
			{
				foreach (Resource resource in resourcesHolder.Resources)
				{
					if (factionXMLreport != null && !resource.Visible(factionXMLreport))
						continue;

					elResource = doc.CreateElement("resource");
					elObject.AppendChild(elResource);
					elResource.SetAttribute("type", resource.ItemType.Name);
					elResource.SetAttribute("quantity", resource.Quantity.ToString());
				}
			}
		}

		internal static void saveBelt(XmlDocument doc, XmlElement elParent, Belt belt, Faction factionXMLreport = null)
		{
			XmlElement elBelt = doc.CreateElement("belt");
			elParent.AppendChild(elBelt);
			elBelt.SetAttribute("name", belt.Name);
			elBelt.SetAttribute("name-en", belt.FullName);
			elBelt.SetAttribute("AU", belt.AU.ToString(CultureInfo.InvariantCulture));

			if (factionXMLreport != null && !belt.Visible(factionXMLreport))
			{
				return;
			}

			if (belt.Composition.Count > 0)
			{
				XmlElement elComposition = doc.CreateElement("composition");
				elBelt.AppendChild(elComposition);
				foreach (CompositionEntry entry in belt.Composition)
				{
					XmlElement elResource = doc.CreateElement("resource");
					elComposition.AppendChild(elResource);
					elResource.SetAttribute("type", entry.ItemType.Name);
					elResource.SetAttribute("quantity", entry.Quantity.ToString(CultureInfo.InvariantCulture));
					elResource.SetAttribute("probability", entry.Probability.ToString(CultureInfo.InvariantCulture));
				}
			}

			saveExits(doc, elBelt, belt);
			belt.ModuleStacks.SaveXml(doc, elBelt, factionXMLreport);
		}

		internal static void setExitTarget(XmlElement elExit, IHolder to)
		{
			Orbit orbit = to as Orbit;
			if (orbit != null)
			{
				Alderson alderson = orbit.OrbitHolder as Alderson;
				if (alderson != null)
				{
					elExit.SetAttribute("alderson", alderson.Name);
					return;
				}
				elExit.SetAttribute("orbit", to.Name);
				return;
			}
			if (to is Belt)
			{
				elExit.SetAttribute("belt", to.Name);
				return;
			}
			elExit.SetAttribute("region", to.Name);
		}

		internal static void saveExits(XmlDocument doc, XmlElement elHolder, Location holder)
		{
			foreach (Exit exit in holder.Exits)
			{
				XmlElement elExit = doc.CreateElement("exit");
				elHolder.AppendChild(elExit);
				setExitTarget(elExit, exit.To);
				foreach (ExitMode exitMode in exit.ExitModes.Values)
				{
					XmlElement elExitMode = doc.CreateElement("exitmode");
					elExit.AppendChild(elExitMode);
					elExitMode.SetAttribute("mode", MoveModeXml.ToToken(exitMode.Mode));
					elExitMode.SetAttribute("duration", exitMode.Duration.ToString());
				}
			}
		}




	}
}
