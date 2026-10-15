using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	public partial class Planet
	{
		public static void LoadGalaxyXml(XmlElement elPlanet, SpaceSystem system, DataFile dataFile)
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

						Galaxy.loadRaces(elPlanet, planet.Races);

						foreach (XmlElement elMoon in elPlanet.SelectNodes("moon"))
						{
							Moon.LoadGalaxyXml(elMoon, system, planet, dataFile);
						}

						foreach (XmlElement elBelt in elPlanet.SelectNodes("belt"))
						{
							Galaxy.loadBelt(elBelt, system, planet, dataFile);
						}

						Orbit.LoadGalaxyXml(elPlanet, planet, dataFile);
						Region.LoadGalaxyXml(elPlanet, planet, dataFile);
						system.Objects.Add(planet.Name, planet);
					}
					catch (Exception ex)
					{
						throw new Exception("Tried to parse planet: " + planet.ReportName + ".", ex);
					}
		}

		public static void LoadGalaxyExits(XmlElement elPlanet, DataFile dataFile)
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
										Galaxy.loadGalaxyExits(elRegion, region, dataFile);
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
								Moon.LoadGalaxyExits(elMoon, dataFile);
							}
							foreach (XmlElement elBelt in elPlanet.SelectNodes("belt"))
							{
								Galaxy.loadGalaxyExits(elBelt, Belt.All[elBelt.GetAttribute("name")], dataFile);
							}
		}

		public static void SaveGalaxyXml(XmlDocument doc, XmlElement elSystem, Planet planet, Faction factionXMLreport)
		{

						XmlElement elObject = doc.CreateElement("planet");
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

						Galaxy.saveRaces(doc, elObject, planet.Races);

						foreach (Moon moon in planet.Moons.Values)
						{
							Moon.SaveGalaxyXml(doc, elObject, moon, factionXMLreport);
						}

						foreach (Belt ring in Belt.All[planet].Values)
						{
							if (factionXMLreport != null && !ring.Visible(factionXMLreport))
							{
								continue;
							}
							Galaxy.saveBelt(doc, elObject, ring, factionXMLreport);
						}

						Orbit.SaveGalaxyXml(doc, elObject, planet, factionXMLreport);
						Region.SaveGalaxyXml(doc, elObject, planet, factionXMLreport);
		}
	}
}