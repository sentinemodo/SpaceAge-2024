using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	public partial class Orbit
	{
		public static void LoadGalaxyXml(XmlElement elOrbitHolder, IOrbitHolder orbitHolder, DataFile dataFile)
		{
			XmlElement elOrbit = (XmlElement)elOrbitHolder.SelectSingleNode("orbit");
			Orbit orbit = new Orbit(orbitHolder, elOrbit.GetAttribute("name"));
			try
			{
				Galaxy.loadRaces(elOrbit, orbit.Races);
				Galaxy.loadResources(elOrbit, orbit, dataFile);

                ModuleStack.All.LoadXml(elOrbit, orbit);

			}
			catch (Exception ex)
			{
				throw new Exception("tried to parse orbit " + orbit.ReportName, ex);
			}
		}

		public static void SaveGalaxyXml(XmlDocument doc, XmlElement elObject, IOrbitHolder orbitHolder, Faction factionXMLreport = null)
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
				Galaxy.saveRaces(doc, elOrbit, orbitHolder.Orbit.Races);
				Galaxy.saveResources(doc, elOrbit, orbitHolder.Orbit, factionXMLreport);

                orbitHolder.Orbit.ModuleStacks.SaveXml(doc, elOrbit, factionXMLreport);
			}
		}
	}
}