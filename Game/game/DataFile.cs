using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace SpaceAge
{
	public class DataFile : XMLProcessing
	{
		private string gameDir;
		private Game game;
		public Game Game
		{
			get { return this.game; }
		}

        public override void LoadXml(XmlElement xmlElement)
        {
            // this never will get called, and eventually all xml releated stuff should be refactored to relevant classes
            throw new NotImplementedException();
        }

        public override XmlElement SaveXml(XmlDocument doc)
        {
            // this never will get called, and eventually all xml releated stuff should be refactored to relevant classes
            throw new NotImplementedException();
        }

		private XmlDocument gameDocument;

		private XmlDocument confDocument;

		private bool configurationLoaded = false;

		public XmlDocument LoadDocument(string dir, string filename)
		{
			return this.loadXmlDocument(Path.Combine(dir, filename));
		}
        
		public Game LoadGame()
		{
			if (string.IsNullOrEmpty(this.gameDir))
			{
				throw new ArgumentNullException();
			}

			if (!this.configurationLoaded)
			{
				throw new InvalidOperationException();
			}

			this.LoadGameDocument();
			this.LoadTurnNumber();
			this.LoadFactions();
			this.LoadGalaxy();
			this.LoadContracts();
			this.LoadOrders();
			return game;
		}

		public void LoadGameDocument()
		{
			this.gameDocument = this.LoadDocument(this.gameDir, "gamein.xml");
		}

		public void LoadGameDocument(string gameDir, string gameFileName)
		{
			this.gameDocument = this.LoadDocument(gameDir, gameFileName);
			XmlElement elGame = (XmlElement)gameDocument.SelectSingleNode("/game");
            Game.Turn = this.XMLAssignInteger(elGame.GetAttribute("turn"), 0);
		}

		public void LoadConfiguration()
		{
			this.LoadConfiguration(this.gameDir);
		}

		public void LoadConfiguration(string confDir, string dataFile = "data.xml")
		{
			this.LoadConfDocument(confDir, dataFile);

			this.LoadConfigurationItems(true);
			this.LoadConfigurationItems(false);
			this.ValidateTypeNameUniqueness();
			this.configurationLoaded = true;
		}

		// A name resolved by orders like `has <qty> <name>` must be unambiguous: it may
		// belong to an item/race (ItemType.All) or a module (ModuleType.All), never both.
		public void ValidateTypeNameUniqueness()
		{
			List<string> collisions = new List<string>();
			foreach (string name in ItemType.All.Keys)
			{
				if (ModuleType.All.ContainsKey(name))
				{
					collisions.Add(name);
				}
			}

			if (collisions.Count > 0)
			{
				collisions.Sort();
				throw new FileLoadException(
					"Ambiguous type name(s) defined as both an item/race and a module: "
					+ string.Join(", ", collisions.ToArray()));
			}
		}

		public void LoadConfDocument(string confDir, string dataFile = "data.xml")
		{
			this.confDocument = this.LoadDocument(confDir, dataFile);
		}

		public void LoadConfigurationItems(bool loadStub)
		{
			new CatalogLoader(this).LoadItems(this.confDocument, this.game, loadStub);
		}

		internal void assignItemStacks(XmlNodeList elements, ItemStacks itemStacks)
		{
			ItemStack itemStack = null;
			foreach (XmlElement element in elements)
			{
				itemStack = new ItemStack(this.game.ItemTypes[element.GetAttribute("type")]);
				itemStack.Quantity = this.XMLAssignInteger(element.GetAttribute("quantity"), 1);
				itemStacks.Add(itemStack);
			}
		}

		internal void assignNames(XmlElement element, NamedObject namedObject)
		{
			namedObject.FullName = element.GetAttribute("name-en");
			if (element.HasAttribute("description"))
			{
				namedObject.Description = element.GetAttribute("description");
			}
		}

		internal void assignNamesMultiple(XmlElement element, IMultiple namedObject)
		{
			if (element.HasAttribute("name-en2"))
			{
				namedObject.FullNameMultiple = element.GetAttribute("name-en2");
			}
			else
			{
				namedObject.FullNameMultiple = namedObject.FullName;
			}
		}

		public DataFile(string gameDir)
		{
			this.gameDir = gameDir;
			this.game = new Game();
		}

		public void LoadFactions()
		{
			foreach (XmlElement elFaction in this.gameDocument.SelectNodes("/game/faction"))
			{
				Faction faction = new Faction(elFaction.GetAttribute("name"), elFaction.GetAttribute("name-en"));
				if (elFaction.HasAttribute("description"))
				{
					faction.Description = elFaction.GetAttribute("description");
				}
				faction.Password = elFaction.GetAttribute("password");
				faction.Email = elFaction.GetAttribute("email");
				if (elFaction.HasAttribute("default-attitude"))
					faction.DefaultAttitude = (FactionAttitude)Convert.ToInt32(elFaction.GetAttribute("default-attitude"));
				if (elFaction.HasAttribute("unknown-attitude"))
					faction.UnknownAttitude = (FactionAttitude)Convert.ToInt32(elFaction.GetAttribute("unknown-attitude"));
				faction.Options.TextReport = this.XMLAssignBoolean(elFaction.GetAttribute("text-report"), true);
				faction.Options.ReportLineLength = this.XMLAssignInteger(elFaction.GetAttribute("text-report-line-length"), ReportLine.LineLength);
				faction.Options.XmlReport = this.XMLAssignBoolean(elFaction.GetAttribute("xml-report"), true);

				faction.Bank.Balance = this.XMLAssignDouble(elFaction.GetAttribute("balance"), 0);
				faction.Bank.CreditLine = this.XMLAssignInteger(elFaction.GetAttribute("credit-line"), 0);
				faction.Bank.CreditRate = this.XMLAssignDouble(elFaction.GetAttribute("credit-rate"), 0);
				faction.Bank.DepositRate = this.XMLAssignDouble(elFaction.GetAttribute("deposit-rate"), 0);

				// known (seen) technologies tracked at faction level
				foreach (XmlElement elTechnology in elFaction.SelectNodes("technology"))
				{
					string technologyName = elTechnology.GetAttribute("name");
					if (Technology.All.Contains(technologyName))
					{
						faction.TechnologiesSeen.Add(Technology.All[technologyName]);
					}
				}

				//foreach (XmlElement el in elFaction.SelectNodes("shown-item"))
				//    f.ShownItems.Add(ItemType.Get(el.GetAttribute("name")));
				//foreach (XmlElement el in elFaction.SelectNodes("shown-skill"))
				//    f.ShownSkills.Add(SkillType.Get(el.GetAttribute("name")));
				//foreach (XmlElement el in elFaction.SelectNodes("shown-building"))
				//    f.ShownBuildings.Add(BuildingType.Get(el.GetAttribute("name")));

				foreach (XmlElement elAttitude in elFaction.SelectNodes("attitude"))
				{
					FactionAttitude attitude = FactionAttitudeParser.Parse(elAttitude.GetAttribute("attitude"));
					if (elAttitude.HasAttribute("faction"))
					{
						faction.Attitudes[elAttitude.GetAttribute("faction")] = attitude;
					}
					else if (elAttitude.HasAttribute("unit"))
					{
						faction.UnitAttitudes[elAttitude.GetAttribute("unit")] = attitude;
					}
				}
			}
		}

		public void LoadTurnNumber()
		{
			XmlElement el = (XmlElement)gameDocument.SelectSingleNode("/game");
			Game.Turn = Convert.ToInt32(el.GetAttribute("turn"));
		}

		public void LoadContracts()
		{
			XmlElement elContracts = (XmlElement)this.gameDocument.SelectSingleNode("/game/contracts");
			Contract.All.LoadXml(elContracts);
		}


		public void LoadOrders()
		{
			XmlElement elOrders = (XmlElement)gameDocument.SelectSingleNode("/game/orders");
			OrderXml.LoadAll(elOrders, this);
		}

		public void LoadGalaxy()
		{
			XmlElement elGalaxy = (XmlElement)gameDocument.SelectSingleNode("/game/galaxy");

			// systems
			foreach (XmlElement elSystem in elGalaxy.SelectNodes("system"))
			{
				SpaceSystem system = new SpaceSystem(elSystem.GetAttribute("name"));
				this.assignNames(elSystem, system);
                //system.Coordinates.X = Convert.ToDouble(elSystem.GetAttribute("X"));
                //system.Coordinates.Y = Convert.ToDouble(elSystem.GetAttribute("Y"));
                //system.Coordinates.Z = Convert.ToDouble(elSystem.GetAttribute("Z"));

				#region stars
				foreach (XmlElement elStar in elSystem.SelectNodes("star"))
				{
					Star star = new Star(system, elStar.GetAttribute("name"));
					this.assignNames(elStar, star);
                    //star.Coordinates.X = this.assignDouble(elStar.GetAttribute("X"), 0);
                    //star.Coordinates.Y = this.assignDouble(elStar.GetAttribute("Y"), 0);
                    //star.Coordinates.Z = this.assignDouble(elStar.GetAttribute("Z"), 0);

					if (elStar.HasAttribute("type"))
						star.StarType = this.game.StarTypes[elStar.GetAttribute("type")];

					if (elStar.HasAttribute("mass"))
						star.Mass = Convert.ToDouble(elStar.GetAttribute("mass"));

					system.Objects.Add(star.Name, star);
				}
				#endregion

				// planets
				foreach (XmlElement elPlanet in elSystem.SelectNodes("planet"))
				{
					Planet planet = new Planet(system, elPlanet.GetAttribute("name"));
					this.assignNames(elPlanet, planet);
					try
					{
						planet.PlanetType = this.game.PlanetTypes[elPlanet.GetAttribute("type")];
						planet.AU = this.XMLAssignDouble(elPlanet.GetAttribute("AU"), 0);
						planet.SurfaceSizeX = this.XMLAssignInteger(elPlanet.GetAttribute("surface-size-X"), 0);
						planet.SurfaceSizeY = this.XMLAssignInteger(elPlanet.GetAttribute("surface-size-Y"), 0);

						if (elPlanet.HasAttribute("mass"))
							planet.Mass = Convert.ToDouble(elPlanet.GetAttribute("mass"));

						foreach (XmlElement elMoon in elPlanet.SelectNodes("moon"))
						{
							Moon moon = new Moon(system, planet, elPlanet.GetAttribute("name"));
							this.assignNames(elMoon, moon);

							try
							{
								moon.MoonType = this.game.MoonTypes[elMoon.GetAttribute("type")];
								moon.AU = this.XMLAssignDouble(elMoon.GetAttribute("AU"), 0);
								moon.SurfaceSizeX = this.XMLAssignInteger(elMoon.GetAttribute("surface-size-X"), 0);
								moon.SurfaceSizeY = this.XMLAssignInteger(elMoon.GetAttribute("surface-size-Y"), 0);

								if (elMoon.HasAttribute("mass"))
									moon.Mass = Convert.ToDouble(elMoon.GetAttribute("mass"));

								this.loadOrbit(elMoon, moon);
								this.loadRegions(elMoon, moon);
							}
							catch (Exception ex)
							{
								throw new Exception("Tried to parse moon " + moon.ReportName, ex);
							}
						}

						this.loadOrbit(elPlanet, planet);
						this.loadRegions(elPlanet, planet);
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

				this.game.Galaxy.SpaceSystems.Add(system);
			}
			this.loadGalaxyExits();
		}

		private void loadRegions(XmlElement elRegionsHolder, IRegionHolder regionHolder)
		{
			foreach (XmlElement elRegion in elRegionsHolder.SelectNodes("region"))
			{
				Region region = new Region(regionHolder, elRegion.GetAttribute("name"));
				this.assignNames(elRegion, region);

				try
				{
					region.RegionType = this.game.RegionTypes[elRegion.GetAttribute("type")];

					region.Coordinates.X = this.XMLAssignInteger(elRegion.GetAttribute("X"), 0);
					region.Coordinates.Y = this.XMLAssignInteger(elRegion.GetAttribute("Y"), 0);

					this.loadCapacities(elRegion, region);
					this.loadResources(elRegion, region);

                    ModuleStack.All.LoadXml(elRegion, region);
				}
				catch (Exception ex)
				{
					throw new Exception("Tried to parse region" + region.ReportName, ex);
				}
			}
		}

		private void loadCapacities(XmlElement elRegion, Region region)
		{
			foreach (XmlElement elCapacity in elRegion.SelectNodes("capacity"))
			{
				Capacity capacity = new Capacity();
				capacity.Group = ModuleTypeGroupXml.Parse(elCapacity.GetAttribute("group"));
				capacity.Quantity = this.XMLAssignInteger(elCapacity.GetAttribute("quantity"), 1);
				region.Capacities.Add(capacity);
			}
		}

		private void loadOrbit(XmlElement elOrbitHolder, IOrbitHolder orbitHolder)
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
				this.loadResources(elOrbit, orbit);

                ModuleStack.All.LoadXml(elOrbit, orbit);

			}
			catch (Exception ex)
			{
				throw new Exception("tried to parse orbit " + orbit.ReportName, ex);
			}
		}

		private void loadResources(XmlElement elHolder, IResourcesHolder holder)
		{
			foreach (XmlElement elResource in elHolder.SelectNodes("resource"))
			{
				try
				{
					Resource resource = new Resource();
					resource.ItemType = this.game.ItemTypes[elResource.GetAttribute("type")];
					resource.Quantity = this.XMLAssignInteger(elResource.GetAttribute("quantity"), 1);
					holder.Resources.Add(resource);
				}
				catch (Exception ex)
				{
					throw new Exception("Tried to parse resources", ex);
				}
			}
		}


		private void loadGalaxyExits()
		{
			try
			{
				XmlElement elGalaxy = (XmlElement)gameDocument.SelectSingleNode("/game/galaxy");

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
										this.loadGalaxyExits(elRegion, region);
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

		private void loadGalaxyExits(XmlElement elHolder, Region holder)
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

						exitMode.Duration = this.XMLAssignInteger(elExitMode.GetAttribute("duration"), 0);
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

		public void LoadItemstacks(XmlElement element, IItemStacksHolder holder)
		{
			foreach (XmlElement elHolder in element.SelectNodes("itemstack"))
			{
				ItemType itemType = this.game.ItemTypes[elHolder.GetAttribute("type")];
				ItemStack itemstack = new ItemStack(itemType);
				itemstack.Quantity = this.XMLAssignInteger(elHolder.GetAttribute("quantity"), 1);
				holder.ItemStacks.Add(itemstack);
			}
		}

        public ELocationType LoadLocationType(XmlElement element)
        {
            string attribute;
            attribute = element.GetAttribute("location-type");
            switch (attribute)
            {
                case "orbit":           return ELocationType.orbit;                  
                case "solid-surface":   return ELocationType.solidSurface;
                case "liquid-surface":  return ELocationType.liquidSurface;
                case "space":           return ELocationType.space;                 
                default:
                    throw new FileLoadException("Tried to load location type item " + attribute);
            }
        }

		public void SaveGame()
		{
			this.SaveGame(this.gameDir, string.Concat("gameout.", this.game.Turn, ".xml"));
		}

		public void SaveGame(string dir, string fileName, Faction factionXMLreport = null)
		{
			//save all xml - equal to game.out when faction is NPC faction
            if (factionXMLreport != null)
			    if (factionXMLreport.Name == "1")
				    factionXMLreport = null;

			XmlDocument doc = new XmlDocument();
			doc.LoadXml("<game/>");
			doc.DocumentElement.SetAttribute("turn", Game.Turn.ToString());
			// Factions
			#region factions
			foreach (Faction faction in Faction.All.Values)
			{
				// do nost save is the xml report is prepared for faction
				if (factionXMLreport != null & faction != factionXMLreport)
					continue;

				// Do not save factions without modulestacks
				if (ModuleStack.All[faction].Count == 0)
					continue;

				XmlElement elFaction = doc.CreateElement("faction");
				doc.DocumentElement.AppendChild(elFaction);
				elFaction.SetAttribute("name", faction.Name);
				elFaction.SetAttribute("name-en", faction.FullName);
				elFaction.SetAttribute("password", faction.Password);
				elFaction.SetAttribute("email", faction.Email);
				elFaction.SetAttribute("default-attitude", ((int)faction.DefaultAttitude).ToString());
				if (faction.UnknownAttitude != FactionAttitude.Hostile)
				{
					elFaction.SetAttribute("unknown-attitude", ((int)faction.UnknownAttitude).ToString());
				}
				elFaction.SetAttribute("text-report", faction.Options.TextReport.ToString());
				elFaction.SetAttribute("text-report-line-length", faction.Options.ReportLineLength.ToString());
				elFaction.SetAttribute("xml-report", faction.Options.XmlReport.ToString());
				elFaction.SetAttribute("balance", faction.Bank.Balance.ToString());
				elFaction.SetAttribute("credit-line", faction.Bank.CreditLine.ToString());
				elFaction.SetAttribute("credit-rate", faction.Bank.CreditRate.ToString());
				elFaction.SetAttribute("deposit-rate", faction.Bank.DepositRate.ToString());

				// persist known (seen) technologies
				foreach (Technology technology in faction.TechnologiesSeen)
				{
					XmlElement elTechnology = doc.CreateElement("technology");
					elTechnology.SetAttribute("name", technology.Name);
					elFaction.AppendChild(elTechnology);
				}

				//foreach (ItemType it in f.ShownItems)
				//    SaveItemType(it, elFaction, "shown-item");
				//foreach (SkillType st in f.ShownSkills)
				//    SaveSkillType(st, elFaction, "shown-skill");
				//foreach (BuildingType bt in f.ShownBuildings)
				//    SaveBuildingType(bt, elFaction, "shown-building");


				foreach (KeyValuePair<string, FactionAttitude> declaration in faction.Attitudes)
				{
					XmlElement elAttitude = doc.CreateElement("attitude");
					elAttitude.SetAttribute("faction", declaration.Key);
					elAttitude.SetAttribute("attitude", FactionAttitudeParser.ToToken(declaration.Value));
					elFaction.AppendChild(elAttitude);
				}
				foreach (KeyValuePair<string, FactionAttitude> declaration in faction.UnitAttitudes)
				{
					XmlElement elAttitude = doc.CreateElement("attitude");
					elAttitude.SetAttribute("unit", declaration.Key);
					elAttitude.SetAttribute("attitude", FactionAttitudeParser.ToToken(declaration.Value));
					elFaction.AppendChild(elAttitude);
				}
			}
			#endregion

			#region contracts
			if (Contract.All.Count > 0)
			{
				doc.DocumentElement.AppendChild(Contract.All.SaveXml(doc));
			}
			#endregion

			#region galaxy
			// Galaxy
			XmlElement elGalaxy = doc.CreateElement("galaxy");
			doc.DocumentElement.AppendChild(elGalaxy);

			// Systems
			foreach (SpaceSystem system in Game.Galaxy.SpaceSystems)
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
            #endregion

            this.SaveOrders(doc);

			XmlTextWriter xmlWriter = new XmlTextWriter(
				Path.Combine(dir, fileName), 
				System.Text.Encoding.GetEncoding(1251));
			xmlWriter.Formatting = Formatting.Indented;
			xmlWriter.IndentChar = '\t';
			xmlWriter.Indentation = 1;
			xmlWriter.WriteStartDocument();
			doc.WriteContentTo(xmlWriter);
			xmlWriter.Close();
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

		public void SaveOrders(XmlDocument doc, Faction factionXMLreport = null)
		{
			OrderXml.SaveAll(doc, factionXMLreport);
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
