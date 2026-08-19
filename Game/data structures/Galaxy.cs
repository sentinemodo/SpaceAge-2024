using System;
using System.Collections.Generic;
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
				dataFile.assignNames(elSystem, system);
                //system.Coordinates.X = Convert.ToDouble(elSystem.GetAttribute("X"));
                //system.Coordinates.Y = Convert.ToDouble(elSystem.GetAttribute("Y"));
                //system.Coordinates.Z = Convert.ToDouble(elSystem.GetAttribute("Z"));

				#region stars
				foreach (XmlElement elStar in elSystem.SelectNodes("star"))
				{
					Star star = new Star(system, elStar.GetAttribute("name"));
					dataFile.assignNames(elStar, star);
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
					Planet planet = new Planet(system, elPlanet.GetAttribute("name"));
					dataFile.assignNames(elPlanet, planet);
					try
					{
						planet.PlanetType = dataFile.Game.PlanetTypes[elPlanet.GetAttribute("type")];
						planet.AU = dataFile.XMLAssignDouble(elPlanet.GetAttribute("AU"), 0);
						planet.SurfaceSizeX = dataFile.XMLAssignInteger(elPlanet.GetAttribute("surface-size-X"), 0);
						planet.SurfaceSizeY = dataFile.XMLAssignInteger(elPlanet.GetAttribute("surface-size-Y"), 0);

						if (elPlanet.HasAttribute("mass"))
							planet.Mass = Convert.ToDouble(elPlanet.GetAttribute("mass"));

						foreach (XmlElement elMoon in elPlanet.SelectNodes("moon"))
						{
							Moon moon = new Moon(system, planet, elPlanet.GetAttribute("name"));
							dataFile.assignNames(elMoon, moon);

							try
							{
								moon.MoonType = dataFile.Game.MoonTypes[elMoon.GetAttribute("type")];
								moon.AU = dataFile.XMLAssignDouble(elMoon.GetAttribute("AU"), 0);
								moon.SurfaceSizeX = dataFile.XMLAssignInteger(elMoon.GetAttribute("surface-size-X"), 0);
								moon.SurfaceSizeY = dataFile.XMLAssignInteger(elMoon.GetAttribute("surface-size-Y"), 0);

								if (elMoon.HasAttribute("mass"))
									moon.Mass = Convert.ToDouble(elMoon.GetAttribute("mass"));

								this.loadOrbit(elMoon, moon, dataFile);
								this.loadRegions(elMoon, moon, dataFile);
							}
							catch (Exception ex)
							{
								throw new Exception("Tried to parse moon " + moon.ReportName, ex);
							}
						}

						this.loadOrbit(elPlanet, planet, dataFile);
						this.loadRegions(elPlanet, planet, dataFile);
						system.Objects.Add(planet.Name, planet);
					}
					catch (Exception ex)
					{
						throw new Exception("Tried to parse planet: " + planet.ReportName + ".", ex);
					}
				}

				// alderson points

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
							try
							{
								// orbits
								// regions
								foreach (XmlElement elRegion in elPlanet.SelectNodes("region"))
								{
									try
									{
										Region region = Region.All[elRegion.GetAttribute("name")];
										this.loadGalaxyExits(elRegion, region, dataFile);
									}
									catch (Exception ex)
									{
										throw new Exception("Tried to parse exits for region " + elRegion.GetAttribute("name"), ex);
									}
								}
							}
							catch (Exception ex)
							{
								throw new Exception("Tried to parse exits for planet " + elPlanet.GetAttribute("name"), ex);
							}
							// moons
						}

						// alderson points

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

		private void loadRegions(XmlElement elRegionsHolder, IRegionHolder regionHolder, DataFile dataFile)
		{
			foreach (XmlElement elRegion in elRegionsHolder.SelectNodes("region"))
			{
				Region region = new Region(regionHolder, elRegion.GetAttribute("name"));
				dataFile.assignNames(elRegion, region);

				try
				{
					region.RegionType = dataFile.Game.RegionTypes[elRegion.GetAttribute("type")];

					region.Coordinates.X = dataFile.XMLAssignInteger(elRegion.GetAttribute("X"), 0);
					region.Coordinates.Y = dataFile.XMLAssignInteger(elRegion.GetAttribute("Y"), 0);

					this.loadCapacities(elRegion, region, dataFile);
					this.loadResources(elRegion, region, dataFile);

                    ModuleStack.All.LoadXml(elRegion, region);
				}
				catch (Exception ex)
				{
					throw new Exception("Tried to parse region" + region.ReportName, ex);
				}
			}
		}

		private void loadCapacities(XmlElement elRegion, Region region, DataFile dataFile)
		{
			foreach (XmlElement elCapacity in elRegion.SelectNodes("capacity"))
			{
				Capacity capacity = new Capacity();
				capacity.Group = ModuleTypeGroupXml.Parse(elCapacity.GetAttribute("group"));
				capacity.Quantity = dataFile.XMLAssignInteger(elCapacity.GetAttribute("quantity"), 1);
				region.Capacities.Add(capacity);
			}
		}

		private void loadOrbit(XmlElement elOrbitHolder, IOrbitHolder orbitHolder, DataFile dataFile)
		{
			XmlElement elOrbit = (XmlElement)elOrbitHolder.SelectSingleNode("orbit");
			Orbit orbit = new Orbit(orbitHolder, elOrbit.GetAttribute("name"));
			try
			{
				foreach (XmlElement elRace in elOrbit.SelectNodes("race"))
				{
					try
					{
						orbit.Races.Add(elRace.GetAttribute("type"), Race.All[elRace.GetAttribute("type")]);
					}
					catch (Exception ex)
					{
						throw new Exception("Tried to parse race ", ex);
					}
				}
				this.loadResources(elOrbit, orbit, dataFile);

                ModuleStack.All.LoadXml(elOrbit, orbit);

			}
			catch (Exception ex)
			{
				throw new Exception("tried to parse orbit " + orbit.ReportName, ex);
			}
		}

		private void loadResources(XmlElement elHolder, IResourcesHolder holder, DataFile dataFile)
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

		private void loadGalaxyExits(XmlElement elHolder, Region holder, DataFile dataFile)
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
					foreach (XmlElement elExitMode in elExit.SelectNodes("exitmode"))
					{
						ExitMode exitMode = new ExitMode();
						switch (elExitMode.GetAttribute("mode"))
						{
							case "space":
								exitMode.Mode = EMoveMode.space;
								break;
							default:
								exitMode.Mode = EMoveMode.ground;
								break;
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

		public XmlElement SaveXml(XmlDocument doc, Faction factionXMLreport = null)
		{
			XmlElement elGalaxy = doc.CreateElement("galaxy");

			// Systems
			foreach (SpaceSystem system in this.SpaceSystems)
			{

				// do nost save is the xml report is prepared for faction and faction is not observing the object
				if (factionXMLreport != null & !system.Visible(factionXMLreport))
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
					if (factionXMLreport != null & !systemObject.Visible(factionXMLreport))
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
						Planet planet;
						planet = (Planet)systemObject;
						elObject = doc.CreateElement("planet");
						elSystem.AppendChild(elObject);
						elObject.SetAttribute("name", planet.Name);
						elObject.SetAttribute("name-en", planet.FullName);
						elObject.SetAttribute("AU", planet.AU.ToString());
						elObject.SetAttribute("surface-size-X", planet.SurfaceSizeX.ToString());
						elObject.SetAttribute("surface-size-Y", planet.SurfaceSizeY.ToString());
						elObject.SetAttribute("type", planet.PlanetType.Name);

						foreach (Moon moon in planet.Moons.Values)
						{
							// do nost save is the xml report is prepared for faction and faction is not observing the object
							if (factionXMLreport != null & !moon.Visible(factionXMLreport))
								continue;

							XmlElement elMoon;
							elMoon = doc.CreateElement("moon");

							elObject.AppendChild(elMoon);
							elMoon.SetAttribute("name", moon.Name);
							elMoon.SetAttribute("name-en", moon.FullName);
							elMoon.SetAttribute("AU", moon.AU.ToString());
							elMoon.SetAttribute("surface-size-X", moon.SurfaceSizeX.ToString());
							elMoon.SetAttribute("surface-size-Y", moon.SurfaceSizeY.ToString());
							elMoon.SetAttribute("type", moon.MoonType.Name);
							this.saveOrbit(doc, elMoon, moon, factionXMLreport);
							this.saveRegions(doc, elMoon, moon, factionXMLreport);
						}

						this.saveOrbit(doc, elObject, (IOrbitHolder)systemObject, factionXMLreport);
						this.saveRegions(doc, elObject, (IRegionHolder)systemObject, factionXMLreport);
					}

				}
            }
			return elGalaxy;
		}

		private void saveOrbit(XmlDocument doc, XmlElement elObject, IOrbitHolder orbitHolder, Faction factionXMLreport = null)
		{
			XmlElement elOrbit = doc.CreateElement("orbit");
			elObject.AppendChild(elOrbit);
			elOrbit.SetAttribute("name", orbitHolder.Orbit.Name);

			// do nost save is the xml report is prepared for faction and faction is not observing the object
			if (factionXMLreport != null & !orbitHolder.Orbit.Visible(factionXMLreport))
			{
			}
			else
			{
				XmlElement elRace;
				foreach (Race race in orbitHolder.Orbit.Races.Values)
				{
					elRace = doc.CreateElement("race");
					elOrbit.AppendChild(elRace);
					elRace.SetAttribute("type", race.Name);
				}
				this.saveResources(doc, elOrbit, orbitHolder.Orbit, factionXMLreport);

                orbitHolder.Orbit.ModuleStacks.SaveXml(doc, elOrbit, factionXMLreport);
			}
		}

		private void saveResources(XmlDocument doc, XmlElement elObject, IResourcesHolder resourcesHolder, Faction factionXMLreport = null)
		{
			XmlElement elResource;
			if (factionXMLreport != null & !resourcesHolder.Visible(factionXMLreport))
			{
			}
			else
			{
				foreach (Resource resource in resourcesHolder.Resources)
				{
					if (factionXMLreport != null & !resource.Visible(factionXMLreport))
						continue;

					elResource = doc.CreateElement("resource");
					elObject.AppendChild(elResource);
					elResource.SetAttribute("type", resource.ItemType.Name);
					elResource.SetAttribute("quantity", resource.Quantity.ToString());
				}
			}
		}

		private void saveRegions(XmlDocument doc, XmlElement elObject, IRegionHolder regionHolder, Faction factionXMLreport = null)
		{
			XmlElement elRegion, elCapacity, elExit, elExitMode;

			foreach (Region region in regionHolder.Regions.Values)
			{
				if (factionXMLreport != null & !region.Visible(factionXMLreport))
					    continue;

				elRegion = doc.CreateElement("region");
				elObject.AppendChild(elRegion);
				elRegion.SetAttribute("name", region.Name);
				elRegion.SetAttribute("name-en", region.FullName);
				elRegion.SetAttribute("X", region.Coordinates.X.ToString());
				elRegion.SetAttribute("Y", region.Coordinates.Y.ToString());
				elRegion.SetAttribute("type", region.RegionType.Name);

				foreach (Capacity capacity in region.Capacities)
				{
					elCapacity = doc.CreateElement("capacity");
					elRegion.AppendChild(elCapacity);
					string groupToken = ModuleTypeGroupXml.ToToken(capacity.Group);
					if (groupToken != null)
					{
						elCapacity.SetAttribute("group", groupToken);
					}
					
					elCapacity.SetAttribute("quantity", capacity.Quantity.ToString());

				}
				foreach (Exit exit in region.Exits)
				{
					elExit = doc.CreateElement("exit");
					elRegion.AppendChild(elExit);
					elExit.SetAttribute("region", exit.To.Name);
					foreach (ExitMode exitMode in exit.ExitModes.Values)
					{
						elExitMode = doc.CreateElement("exitmode");
						elExit.AppendChild(elExitMode);

						switch (exitMode.Mode)
						{
							case EMoveMode.ground:
								elExitMode.SetAttribute("mode", "ground");
								break;
							case EMoveMode.space:
								elExitMode.SetAttribute("mode", "space");
								break;
						}						
						elExitMode.SetAttribute("duration", exitMode.Duration.ToString());
					}
				}
				this.saveResources(doc, elRegion, region, factionXMLreport);

                region.ModuleStacks.SaveXml(doc, elRegion, factionXMLreport);
			}
		}
	}
}
