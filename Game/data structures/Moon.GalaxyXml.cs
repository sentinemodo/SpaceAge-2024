using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	public partial class Moon
	{
		public static void LoadGalaxyXml(XmlElement elMoon, SpaceSystem system, Planet planet, DataFile dataFile)
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

								Galaxy.loadRaces(elMoon, moon.Races);
								Orbit.LoadGalaxyXml(elMoon, moon, dataFile);
								Region.LoadGalaxyXml(elMoon, moon, dataFile);
							}
							catch (Exception ex)
							{
								throw new Exception("Tried to parse moon " + moon.ReportName, ex);
							}
		}

		public static void LoadGalaxyExits(XmlElement elMoon, DataFile dataFile)
		{

								foreach (XmlElement elRegion in elMoon.SelectNodes("region"))
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

		public static void SaveGalaxyXml(XmlDocument doc, XmlElement elPlanet, Moon moon, Faction factionXMLreport)
		{

							// do nost save is the xml report is prepared for faction and faction is not observing the object
							if (factionXMLreport != null && !moon.Visible(factionXMLreport))
								return;

							XmlElement elMoon;
							elMoon = doc.CreateElement("moon");

							elPlanet.AppendChild(elMoon);
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
							Galaxy.saveRaces(doc, elMoon, moon.Races);
							Orbit.SaveGalaxyXml(doc, elMoon, moon, factionXMLreport);
							Region.SaveGalaxyXml(doc, elMoon, moon, factionXMLreport);
		}
	}
}