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
					Planet planet = new Planet(system, elPlanet.GetAttribute("name"));
					planet.LoadXml(elPlanet);
					try
					{
						planet.PlanetType = dataFile.Game.PlanetTypes[elPlanet.GetAttribute("type")];
						planet.AU = dataFile.XMLAssignDouble(elPlanet.GetAttribute("AU"), 0);
						planet.SurfaceSizeX = dataFile.XMLAssignInteger(elPlanet.GetAttribute("surface-size-X"), 0);
						planet.SurfaceSizeY = dataFile.XMLAssignInteger(elPlanet.GetAttribute("surface-size-Y"), 0);
						planet.GravityBand = BodyEnvironment.ParseGravity(elPlanet.HasAttribute("gravity") ? elPlanet.GetAttribute("gravity") : "normal");
						planet.TemperatureBand = BodyEnvironment.ParseTemperature(elPlanet.HasAttribute("temperature") ? elPlanet.GetAttribute("temperature") : "habitable");
						planet.AtmosphereBand = BodyEnvironment.ParseAtmosphere(elPlanet.GetAttribute("atmosphere"));
						planet.HasEnvironmentAttrs = elPlanet.HasAttribute("gravity")
							|| elPlanet.HasAttribute("temperature")
							|| elPlanet.HasAttribute("atmosphere");
						if (elPlanet.HasAttribute("pair"))
						{
							planet.PairName = elPlanet.GetAttribute("pair");
						}

						if (elPlanet.HasAttribute("mass"))
							planet.Mass = Convert.ToDouble(elPlanet.GetAttribute("mass"));

						this.loadRaces(elPlanet, planet.Races);

						foreach (XmlElement elMoon in elPlanet.SelectNodes("moon"))
						{
							Moon moon = new Moon(system, planet, elMoon.GetAttribute("name"));
							moon.LoadXml(elMoon);

							try
							{
								moon.MoonType = dataFile.Game.MoonTypes[elMoon.GetAttribute("type")];
								moon.AU = dataFile.XMLAssignDouble(elMoon.GetAttribute("AU"), 0);
								moon.SurfaceSizeX = dataFile.XMLAssignInteger(elMoon.GetAttribute("surface-size-X"), 0);
								moon.SurfaceSizeY = dataFile.XMLAssignInteger(elMoon.GetAttribute("surface-size-Y"), 0);
								moon.GravityBand = BodyEnvironment.ParseGravity(elMoon.HasAttribute("gravity") ? elMoon.GetAttribute("gravity") : "low");
								moon.TemperatureBand = BodyEnvironment.ParseTemperature(elMoon.GetAttribute("temperature"));
								moon.AtmosphereBand = BodyEnvironment.ParseAtmosphere(elMoon.GetAttribute("atmosphere"));
								moon.HasEnvironmentAttrs = elMoon.HasAttribute("gravity")
									|| elMoon.HasAttribute("temperature")
									|| elMoon.HasAttribute("atmosphere");

								if (elMoon.HasAttribute("mass"))
									moon.Mass = Convert.ToDouble(elMoon.GetAttribute("mass"));

								this.loadRaces(elMoon, moon.Races);
								this.loadOrbit(elMoon, moon, dataFile);
								this.loadRegions(elMoon, moon, dataFile);
							}
							catch (Exception ex)
							{
								throw new Exception("Tried to parse moon " + moon.ReportName, ex);
							}
						}

						foreach (XmlElement elBelt in elPlanet.SelectNodes("belt"))
						{
							this.loadBelt(elBelt, system, planet, dataFile);
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

				foreach (XmlElement elBelt in elSystem.SelectNodes("belt"))
				{
					this.loadBelt(elBelt, system, null, dataFile);
				}

				foreach (XmlElement elAlderson in elSystem.SelectNodes("alderson"))
				{
					this.loadAlderson(elAlderson, system, dataFile);
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
							foreach (XmlElement elMoon in elPlanet.SelectNodes("moon"))
							{
								foreach (XmlElement elRegion in elMoon.SelectNodes("region"))
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
							foreach (XmlElement elBelt in elPlanet.SelectNodes("belt"))
							{
								this.loadGalaxyExits(elBelt, Belt.All[elBelt.GetAttribute("name")], dataFile);
							}
						}

						foreach (XmlElement elBelt in elSystem.SelectNodes("belt"))
						{
							this.loadGalaxyExits(elBelt, Belt.All[elBelt.GetAttribute("name")], dataFile);
						}

						foreach (XmlElement elAlderson in elSystem.SelectNodes("alderson"))
						{
							Alderson alderson = Alderson.All[elAlderson.GetAttribute("name")];
							this.loadGalaxyExits(elAlderson, alderson.Orbit, dataFile);
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

		private void loadRegions(XmlElement elRegionsHolder, IRegionHolder regionHolder, DataFile dataFile)
		{
			foreach (XmlElement elRegion in elRegionsHolder.SelectNodes("region"))
			{
				Region region = new Region(regionHolder, elRegion.GetAttribute("name"));
				region.LoadXml(elRegion);

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
				this.loadRaces(elOrbit, orbit.Races);
				this.loadResources(elOrbit, orbit, dataFile);

                ModuleStack.All.LoadXml(elOrbit, orbit);

			}
			catch (Exception ex)
			{
				throw new Exception("tried to parse orbit " + orbit.ReportName, ex);
			}
		}

		private void loadRaces(XmlElement elHolder, Races races)
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

		private void loadGalaxyExits(XmlElement elHolder, Location holder, DataFile dataFile)
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

		private void loadBelt(XmlElement elBelt, SpaceSystem system, Planet planet, DataFile dataFile)
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

		private void loadAlderson(XmlElement elAlderson, SpaceSystem system, DataFile dataFile)
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
				this.loadOrbit(elAlderson, alderson, dataFile);
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
						if (planet.HasEnvironmentAttrs)
						{
							elObject.SetAttribute("gravity", BodyEnvironment.GravityToken(planet.GravityBand));
							elObject.SetAttribute("temperature", BodyEnvironment.TemperatureToken(planet.TemperatureBand));
							elObject.SetAttribute("atmosphere", BodyEnvironment.AtmosphereToken(planet.AtmosphereBand));
						}
						if (!string.IsNullOrEmpty(planet.PairName))
						{
							elObject.SetAttribute("pair", planet.PairName);
						}

						this.saveRaces(doc, elObject, planet.Races);

						foreach (Moon moon in planet.Moons.Values)
						{
							// do nost save is the xml report is prepared for faction and faction is not observing the object
							if (factionXMLreport != null && !moon.Visible(factionXMLreport))
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
							if (moon.HasEnvironmentAttrs)
							{
								elMoon.SetAttribute("gravity", BodyEnvironment.GravityToken(moon.GravityBand));
								elMoon.SetAttribute("temperature", BodyEnvironment.TemperatureToken(moon.TemperatureBand));
								elMoon.SetAttribute("atmosphere", BodyEnvironment.AtmosphereToken(moon.AtmosphereBand));
							}
							this.saveRaces(doc, elMoon, moon.Races);
							this.saveOrbit(doc, elMoon, moon, factionXMLreport);
							this.saveRegions(doc, elMoon, moon, factionXMLreport);
						}

						foreach (Belt ring in Belt.All[planet].Values)
						{
							if (factionXMLreport != null && !ring.Visible(factionXMLreport))
							{
								continue;
							}
							this.saveBelt(doc, elObject, ring, factionXMLreport);
						}

						this.saveOrbit(doc, elObject, (IOrbitHolder)systemObject, factionXMLreport);
						this.saveRegions(doc, elObject, (IRegionHolder)systemObject, factionXMLreport);
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
						this.saveOrbit(doc, elObject, alderson, factionXMLreport);
						if (alderson.Orbit != null)
						{
							this.saveExits(doc, elObject, alderson.Orbit);
						}
					}

				}

				foreach (Belt belt in Belt.All[system].Values)
				{
					if (factionXMLreport != null && !belt.Visible(factionXMLreport))
					{
						continue;
					}
					this.saveBelt(doc, elSystem, belt, factionXMLreport);
				}
            }
			return elGalaxy;
		}

		private void saveRaces(XmlDocument doc, XmlElement elHolder, Races races)
		{
			foreach (Race race in races.Values)
			{
				XmlElement elRace = doc.CreateElement("race");
				elHolder.AppendChild(elRace);
				elRace.SetAttribute("type", race.Name);
			}
		}

		private void saveOrbit(XmlDocument doc, XmlElement elObject, IOrbitHolder orbitHolder, Faction factionXMLreport = null)
		{
			XmlElement elOrbit = doc.CreateElement("orbit");
			elObject.AppendChild(elOrbit);
			elOrbit.SetAttribute("name", orbitHolder.Orbit.Name);

			// do nost save is the xml report is prepared for faction and faction is not observing the object
			if (factionXMLreport != null && !orbitHolder.Orbit.Visible(factionXMLreport))
			{
			}
			else
			{
				this.saveRaces(doc, elOrbit, orbitHolder.Orbit.Races);
				this.saveResources(doc, elOrbit, orbitHolder.Orbit, factionXMLreport);

                orbitHolder.Orbit.ModuleStacks.SaveXml(doc, elOrbit, factionXMLreport);
			}
		}

		private void saveResources(XmlDocument doc, XmlElement elObject, IResourcesHolder resourcesHolder, Faction factionXMLreport = null)
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

		private void saveBelt(XmlDocument doc, XmlElement elParent, Belt belt, Faction factionXMLreport = null)
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

			this.saveExits(doc, elBelt, belt);
			belt.ModuleStacks.SaveXml(doc, elBelt, factionXMLreport);
		}

		private void setExitTarget(XmlElement elExit, IHolder to)
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

		private void saveExits(XmlDocument doc, XmlElement elHolder, Location holder)
		{
			foreach (Exit exit in holder.Exits)
			{
				XmlElement elExit = doc.CreateElement("exit");
				elHolder.AppendChild(elExit);
				this.setExitTarget(elExit, exit.To);
				foreach (ExitMode exitMode in exit.ExitModes.Values)
				{
					XmlElement elExitMode = doc.CreateElement("exitmode");
					elExit.AppendChild(elExitMode);
					elExitMode.SetAttribute("mode", MoveModeXml.ToToken(exitMode.Mode));
					elExitMode.SetAttribute("duration", exitMode.Duration.ToString());
				}
			}
		}

		private void saveRegions(XmlDocument doc, XmlElement elObject, IRegionHolder regionHolder, Faction factionXMLreport = null)
		{
			XmlElement elRegion, elCapacity, elExit, elExitMode;

			foreach (Region region in regionHolder.Regions.Values)
			{
				if (factionXMLreport != null && !region.Visible(factionXMLreport))
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
					this.setExitTarget(elExit, exit.To);
					foreach (ExitMode exitMode in exit.ExitModes.Values)
					{
						elExitMode = doc.CreateElement("exitmode");
						elExit.AppendChild(elExitMode);

						elExitMode.SetAttribute("mode", MoveModeXml.ToToken(exitMode.Mode));
						elExitMode.SetAttribute("duration", exitMode.Duration.ToString());
					}
				}
				this.saveResources(doc, elRegion, region, factionXMLreport);

                region.ModuleStacks.SaveXml(doc, elRegion, factionXMLreport);
			}
		}
	}
}
