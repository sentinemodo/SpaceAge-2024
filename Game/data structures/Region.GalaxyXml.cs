using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	public partial class Region
	{
		public static void LoadGalaxyXml(XmlElement elRegionsHolder, IRegionHolder regionHolder, DataFile dataFile)
		{
			List<KeyValuePair<XmlElement, Region>> regionStackLoads = new List<KeyValuePair<XmlElement, Region>>();
			foreach (XmlElement elRegion in elRegionsHolder.SelectNodes("region"))
			{
				Region region = new Region(regionHolder, elRegion.GetAttribute("name"));
				region.LoadXml(elRegion);

				try
				{
					region.RegionType = dataFile.Game.RegionTypes[elRegion.GetAttribute("type")];

					region.Coordinates.X = dataFile.XMLAssignInteger(elRegion.GetAttribute("X"), 0);
					region.Coordinates.Y = dataFile.XMLAssignInteger(elRegion.GetAttribute("Y"), 0);

					loadCapacities(elRegion, region, dataFile);
					Galaxy.loadResources(elRegion, region, dataFile);
					loadDeepPocket(elRegion, region, dataFile);
					loadAnomaly(elRegion, region, dataFile);

					regionStackLoads.Add(new KeyValuePair<XmlElement, Region>(elRegion, region));
				}
				catch (Exception ex)
				{
					throw new Exception("Tried to parse region" + region.ReportName, ex);
				}
			}

			foreach (KeyValuePair<XmlElement, Region> pending in regionStackLoads)
			{
				try
				{
					ModuleStack.All.LoadXml(pending.Key, pending.Value);
				}
				catch (Exception ex)
				{
					throw new Exception("Tried to parse region" + pending.Value.ReportName, ex);
				}
			}
		}

		private static void loadCapacities(XmlElement elRegion, Region region, DataFile dataFile)
		{
			foreach (XmlElement elCapacity in elRegion.SelectNodes("capacity"))
			{
				Capacity capacity = new Capacity();
				capacity.Group = ModuleTypeGroupXml.Parse(elCapacity.GetAttribute("group"));
				capacity.Quantity = dataFile.XMLAssignInteger(elCapacity.GetAttribute("quantity"), 1);
				region.Capacities.Add(capacity);
			}
		}

		private static void loadDeepPocket(XmlElement elRegion, Region region, DataFile dataFile)
		{
			XmlElement elDeepPocket = (XmlElement)elRegion.SelectSingleNode("deep-pocket");
			if (elDeepPocket == null)
			{
				return;
			}

			foreach (XmlElement elResource in elDeepPocket.SelectNodes("resource"))
			{
				try
				{
					Resource resource = new Resource();
					resource.ItemType = dataFile.Game.ItemTypes[elResource.GetAttribute("type")];
					resource.Quantity = dataFile.XMLAssignInteger(elResource.GetAttribute("quantity"), 1);
					resource.IsDeep = true;
					region.DeepPocketResources.Add(resource);
				}
				catch (Exception ex)
				{
					throw new Exception("Tried to parse deep-pocket resources", ex);
				}
			}
		}

		private static void loadAnomaly(XmlElement elRegion, Region region, DataFile dataFile)
		{
			XmlElement elAnomaly = (XmlElement)elRegion.SelectSingleNode("anomaly");
			if (elAnomaly == null)
			{
				elAnomaly = (XmlElement)elRegion.SelectSingleNode("poi");
			}
			if (elAnomaly == null)
			{
				return;
			}

			region.Anomaly = new RegionAnomaly();
			region.Anomaly.LoadXml(elAnomaly, dataFile);
		}

		private static void saveExitTarget(XmlDocument doc, XmlElement elExit, Exit exit, Region fromRegion, Faction factionXMLreport)
		{
			Region targetRegion = exit.To as Region;
			if (targetRegion == null || factionXMLreport == null)
			{
				return;
			}

			if (!fromRegion.Visible(factionXMLreport))
			{
				return;
			}

			if (fromRegion.RegionHolder != targetRegion.RegionHolder)
			{
				return;
			}

			XmlElement elTarget = doc.CreateElement("target");
			elExit.AppendChild(elTarget);
			elTarget.SetAttribute("name", targetRegion.Name);

			bool targetVisible = targetRegion.Visible(factionXMLreport);
			elTarget.SetAttribute("discovered", targetVisible ? "yes" : "partial");
			elTarget.SetAttribute("name-en", targetRegion.FullName);
			elTarget.SetAttribute("X", targetRegion.Coordinates.X.ToString());
			elTarget.SetAttribute("Y", targetRegion.Coordinates.Y.ToString());
			elTarget.SetAttribute("type", targetRegion.RegionType.Name);

			if (targetRegion.HasSettlement)
			{
				foreach (Capacity capacity in targetRegion.Capacities)
				{
					if (capacity.Group != EModuleTypesGroup.settlement)
					{
						continue;
					}

					XmlElement elCapacity = doc.CreateElement("capacity");
					elTarget.AppendChild(elCapacity);
					string groupToken = ModuleTypeGroupXml.ToToken(capacity.Group);
					if (groupToken != null)
					{
						elCapacity.SetAttribute("group", groupToken);
					}

					elCapacity.SetAttribute("quantity", capacity.Quantity.ToString());
				}
			}

			if (targetVisible)
			{
				foreach (Resource resource in targetRegion.Resources)
				{
					if (!resource.Visible(factionXMLreport))
					{
						continue;
					}

					XmlElement elResource = doc.CreateElement("resource");
					elTarget.AppendChild(elResource);
					elResource.SetAttribute("type", resource.ItemType.Name);
					elResource.SetAttribute("quantity", resource.Quantity.ToString());
				}
			}

			if (targetRegion.HasDeepPocket && fromRegion.HasCdrillTechnologyFor(factionXMLreport))
			{
				XmlElement elDeepPocket = doc.CreateElement("deep-pocket");
				elTarget.AppendChild(elDeepPocket);
				if (targetRegion.HasCdrillTechnologyFor(factionXMLreport))
				{
					foreach (Resource resource in targetRegion.DeepPocketResources)
					{
						XmlElement elResource = doc.CreateElement("resource");
						elDeepPocket.AppendChild(elResource);
						elResource.SetAttribute("type", resource.ItemType.Name);
						elResource.SetAttribute("quantity", resource.Quantity.ToString());
					}
				}
			}

			if (targetRegion.HasAnomaly && !targetRegion.Anomaly.IsResolved(factionXMLreport))
			{
				XmlElement elAnomaly = doc.CreateElement("anomaly");
				elTarget.AppendChild(elAnomaly);
				elAnomaly.SetAttribute("type", targetRegion.Anomaly.Type);
				elAnomaly.SetAttribute("points", targetRegion.Anomaly.Points.ToString());
			}
		}

		public static void SaveGalaxyXml(XmlDocument doc, XmlElement elObject, IRegionHolder regionHolder, Faction factionXMLreport = null)
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
					Galaxy.setExitTarget(elExit, exit.To);
					foreach (ExitMode exitMode in exit.ExitModes.Values)
					{
						elExitMode = doc.CreateElement("exitmode");
						elExit.AppendChild(elExitMode);

						elExitMode.SetAttribute("mode", MoveModeXml.ToToken(exitMode.Mode));
						elExitMode.SetAttribute("duration", exitMode.Duration.ToString());
					}
					if (factionXMLreport != null)
					{
						saveExitTarget(doc, elExit, exit, region, factionXMLreport);
					}
				}
				Galaxy.saveResources(doc, elRegion, region, factionXMLreport);
				bool occupiedByReporter = factionXMLreport == null
					|| region.ModuleStacks.Contains(factionXMLreport);
				if (region.HasDeepPocket && occupiedByReporter)
				{
					XmlElement elDeepPocket = doc.CreateElement("deep-pocket");
					elRegion.AppendChild(elDeepPocket);
					foreach (Resource resource in region.DeepPocketResources)
					{
						XmlElement elResource = doc.CreateElement("resource");
						elDeepPocket.AppendChild(elResource);
						elResource.SetAttribute("type", resource.ItemType.Name);
						elResource.SetAttribute("quantity", resource.Quantity.ToString());
					}
				}
				if (region.HasAnomaly && occupiedByReporter)
				{
					XmlElement elAnomaly = doc.CreateElement("anomaly");
					elRegion.AppendChild(elAnomaly);
					region.Anomaly.SaveXml(doc, elAnomaly);
				}

                region.ModuleStacks.SaveXml(doc, elRegion, factionXMLreport);
			}
		}
	}
}