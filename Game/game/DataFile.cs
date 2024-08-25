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

		public void LoadConfiguration(string confDir)
		{
			this.LoadConfDocument(confDir);

			this.LoadConfigurationItems(true);
			this.LoadConfigurationItems(false);
			this.configurationLoaded = true;
		}

		public void LoadConfDocument(string confDir, string dataFile = "data.xml")
		{
			this.confDocument = this.LoadDocument(confDir, dataFile);
		}

		public void LoadConfigurationItems(bool loadStub)
		{
			string name;

			// Stars
			foreach (XmlElement el in this.confDocument.SelectNodes("/data/star/entry"))
			{
				if (loadStub)
				{
					name = el.GetAttribute("name");
					this.game.StarTypes.Add(name, new StarType(name));
				}
				else
				{
					StarType star = this.game.StarTypes[el.GetAttribute("name")];
					this.assignNames(el, star);
				}
			}

			// Planets
			foreach (XmlElement el in this.confDocument.SelectNodes("/data/planet/entry"))
			{
				if (loadStub)
				{
					name = el.GetAttribute("name");
					this.game.PlanetTypes.Add(name, new PlanetType(name));
				}
				else
				{
					PlanetType planet = this.game.PlanetTypes[el.GetAttribute("name")];
					this.assignNames(el, planet);
				}
			}

			// Moons
			foreach (XmlElement el in this.confDocument.SelectNodes("/data/moon/entry"))
			{
				if (loadStub)
				{
					name = el.GetAttribute("name");
					this.game.MoonTypes.Add(name, new MoonType(name));
				}
				else
				{
					MoonType moon = this.game.MoonTypes[el.GetAttribute("name")];
					this.assignNames(el, moon);
				}
			}

			// Regions 
			foreach (XmlElement el in this.confDocument.SelectNodes("/data/region/entry"))
			{
				if (loadStub)
				{
					name = el.GetAttribute("name");
                    this.game.RegionTypes.Add(name, new RegionType(name));
				}
				else
				{
					RegionType region = this.game.RegionTypes[el.GetAttribute("name")];
					this.assignNames(el, region);
                    if (el.HasAttribute("location-type"))
                    {
                        region.LocationType = this.LoadLocationType(el);
                    }

					// new Region(el.GetAttribute("name"));
					//      t.MP = LoadInteger(el, "mp");
					//      if (el.GetAttribute("walking") != "")
					//          t.Walking = Convert.ToBoolean(el.GetAttribute("walking"));
					//      if (el.GetAttribute("vehicles") != "")
					//          t.Vehicles = Convert.ToBoolean(el.GetAttribute("vehicles"));
					//      if (el.GetAttribute("ships") != "")
					//          t.Ships = Convert.ToBoolean(el.GetAttribute("ships"));
					//      foreach (XmlElement elMonster in el.SelectNodes("monster"))
					//          t.Monsters.Add(ItemType.Get(elMonster.GetAttribute("type")),
					//              Convert.ToInt32(elMonster.GetAttribute("chance")));
				}
			}

			#region itemTypes
			foreach (XmlElement el in this.confDocument.SelectNodes("/data/item/entry"))
			{
				ItemType itemType;
				if (loadStub)
				{
                    itemType = new ItemType(el.GetAttribute("name"));
				}
				else
				{
					itemType = ItemType.All[el.GetAttribute("name")];
					this.assignNames(el, itemType);
					this.assignNamesMultiple(el, itemType);

					itemType.Mass = this.XMLAssignDouble(el.GetAttribute("mass"), 0);
					itemType.Size = this.XMLAssignDouble(el.GetAttribute("size"), 0);

					#region upkeep
					ItemStack item = null;
					foreach (XmlElement elUpkeep in el.SelectNodes("upkeep"))
					{
						item = new ItemStack(this.game.ItemTypes[elUpkeep.GetAttribute("type")]);
						item.Quantity = this.XMLAssignInteger(elUpkeep.GetAttribute("quantity"), 1);
						itemType.Upkeep.Add(item);
					}
					#endregion
					#region consume
					foreach (XmlElement elConsume in el.SelectNodes("consume"))
					{
						item = new ItemStack(this.game.ItemTypes[elConsume.GetAttribute("type")]);
						item.Quantity = this.XMLAssignInteger(elConsume.GetAttribute("quantity"), 1);
						itemType.Consume.Add(item);
					}
					#endregion
				}
			}
			#endregion

			#region races
			// races 
			foreach (XmlElement el in this.confDocument.SelectNodes("/data/race/entry"))
			{
				// as crew items
				ItemType itemType;
				if (loadStub)
				{
					itemType = new ItemType(el.GetAttribute("name"));
				}
				else
				{
					itemType = ItemType.All[el.GetAttribute("name")];
					this.assignNames(el, itemType);
					this.assignNamesMultiple(el, itemType);
					itemType.Group = EItemTypesGroup.crew;
					itemType.Mass = this.XMLAssignDouble(el.GetAttribute("mass"), 0);
					itemType.Size = this.XMLAssignDouble(el.GetAttribute("size"), 0);


					#region upkeep
					ItemStack item = null;
					foreach (XmlElement elUpkeep in el.SelectNodes("upkeep"))
					{
						if (elUpkeep.GetAttribute("crew-type") == "crew")
						{
							item = new ItemStack(this.game.ItemTypes[elUpkeep.GetAttribute("type")]);
							item.Quantity = this.XMLAssignInteger(elUpkeep.GetAttribute("quantity"), 1);
							itemType.Upkeep.Add(item);
						}
					}
					#endregion
					#region consume
					foreach (XmlElement elConsume in el.SelectNodes("consume"))
					{
						item = new ItemStack(this.game.ItemTypes[elConsume.GetAttribute("type")]);
						item.Quantity = this.XMLAssignInteger(elConsume.GetAttribute("quantity"), 1);
						itemType.Consume.Add(item);
					}
					#endregion

				}

				// as crew officers
				Race race;
				if (loadStub)
				{
					race = new Race(el.GetAttribute("name"));
				}
				else
				{
					race = Race.All[el.GetAttribute("name")];
					this.assignNames(el, race);

					race.Mass = this.XMLAssignDouble(el.GetAttribute("mass"), 0);
					race.Size = this.XMLAssignDouble(el.GetAttribute("size"), 0);
					race.Capacity = this.XMLAssignDouble(el.GetAttribute("capacity"), 0);
					race.OfficerTrainingDuration = this.XMLAssignInteger(el.GetAttribute("officer-training-duration"), 0);

					#region upkeep
					ItemStack item = null;
					foreach (XmlElement elUpkeep in el.SelectNodes("upkeep"))
					{
						if (elUpkeep.GetAttribute("crew-type") == "officer")
						{
							item = new ItemStack(this.game.ItemTypes[elUpkeep.GetAttribute("type")]);
							item.Quantity = this.XMLAssignInteger(elUpkeep.GetAttribute("quantity"), 1);
							race.Upkeep.Add(item);
						}
					}
					#endregion

					#region consume
					foreach (XmlElement elConsume in el.SelectNodes("consume"))
					{
						item = new ItemStack(this.game.ItemTypes[elConsume.GetAttribute("type")]);
						item.Quantity = this.XMLAssignInteger(elConsume.GetAttribute("quantity"), 1);
						race.Consume.Add(item);
					}
					#endregion
				}

			}
			#endregion

			#region skills
			// skills 
			foreach (XmlElement el in this.confDocument.SelectNodes("/data/skill/entry"))
			{
				SkillType skillType;
				if (loadStub)
				{
					skillType = new SkillType(el.GetAttribute("name"));
				}
				else
				{
					skillType = SkillType.All[el.GetAttribute("name")];
					this.assignNames(el, skillType);

					skillType.TrainingDuration = this.XMLAssignInteger(el.GetAttribute("training-duration"), 1);
					skillType.Attack = this.XMLAssignInteger(el.GetAttribute("attack"), 0);
					skillType.Defense = this.XMLAssignInteger(el.GetAttribute("defense"), 0);
					skillType.Initiative = this.XMLAssignInteger(el.GetAttribute("initiative"), 0);
				}
			}
			#endregion

			#region technologies
			Technology technology;
			foreach (XmlElement el in this.confDocument.SelectNodes("/data/technology/entry"))
			{
				if (loadStub)
				{
					name = el.GetAttribute("name");
					technology = new Technology(name);
				}
				else
				{
					technology = this.game.Technologies[el.GetAttribute("name")];
					this.assignNames(el, technology);
					technology.Level = this.XMLAssignInteger(el.GetAttribute("level"), 0);
					technology.UseTime = this.XMLAssignInteger(el.GetAttribute("use-time"), 1);

					technology.Attack = this.XMLAssignInteger(el.GetAttribute("attack"), 0);
					technology.Defense = this.XMLAssignInteger(el.GetAttribute("defense"), 0);
					technology.Initiative = this.XMLAssignInteger(el.GetAttribute("initiative"), 0);

					// material and output
					// Effect effect;
					ModuleType module;
					ItemStack item;
                    ItemStacks items;

					try
					{
						foreach (XmlElement elConsume in el.SelectNodes("use-consume"))
						{
							if (elConsume.HasAttribute("item"))
							{
                                items = new ItemStacks();
								item = new ItemStack(ItemType.All[elConsume.GetAttribute("item")]);
								item.Quantity = this.XMLAssignInteger(elConsume.GetAttribute("quantity"), 1);
                                items.Add(item);
                                technology.UseConsumeItems = items;
							}
							else if (el.HasAttribute("module"))
							{
								module = ModuleType.All[elConsume.GetAttribute("module")];
								technology.UseConsumeModules = module;
							}
						}

					    foreach (XmlElement elProduce in el.SelectNodes("use-produce"))
					    {
						    if (elProduce.HasAttribute("item"))
						    {
                                items = new ItemStacks();
							    item = new ItemStack(this.game.ItemTypes[elProduce.GetAttribute("item")]);
							    item.Quantity = this.XMLAssignInteger(elProduce.GetAttribute("quantity"), 1);
                                items.Add(item);
                                technology.UseProduceItems = items;
							    technology.ProductionType = EProductionType.Items;
						    }
						    else if (elProduce.HasAttribute("module"))
						    {
							    module = this.game.ModuleTypes[elProduce.GetAttribute("module")];
							    technology.UseProduceModules = module;
							    technology.ProductionType = EProductionType.Modules;
						    }
						    else if (elProduce.HasAttribute("effect"))
						    {
							    int duration = this.XMLAssignInteger(elProduce.GetAttribute("duration"), -1);
							    string reason = this.XMLAssignString(elProduce.GetAttribute("effect"), "effect");
							    if (elProduce.HasAttribute("target") == false || elProduce.HasAttribute("change") == false)
							    {
								    throw new FileLoadException("tried to parse effect " + reason);
							    }
							    string target = elProduce.GetAttribute("target");
							    string change = elProduce.GetAttribute("change");
							    technology.ProductionType = EProductionType.Effects;
						    }
					    }


					    // use conditions
					    foreach (XmlElement elAllowed in el.SelectNodes("use-allowed-in"))
					    {
						    string condition;
						    if (elAllowed.HasAttribute("planet-type"))
						    {
							    condition = elAllowed.GetAttribute("planet-type");
                                technology.UseCondition_PlanetTypes = new PlanetTypes();
                                technology.UseCondition_PlanetTypes.Add(condition, this.game.PlanetTypes[condition]);
						    }

						    if (elAllowed.HasAttribute("planet-atmosphere"))
						    {
							    condition = elAllowed.GetAttribute("planet-atmosphere");
                                technology.UseCondition_AtmosphereResources = new ItemTypes();
							    technology.UseCondition_AtmosphereResources.Add(condition, this.game.ItemTypes[condition]);
						    }

						    if (elAllowed.HasAttribute("location-type"))
						    {
                                technology.UseCondition_LocationTypes = new LocationTypes();
							    technology.UseCondition_LocationTypes.Add(this.LoadLocationType(elAllowed));
						    }

                            if (elAllowed.HasAttribute("module-type-group"))
                            {                                
                                technology.UseCondition_ModuleTypesGroup = this.getModuleTypeGroup(elAllowed.GetAttribute("module-type-group"));
                            }
					    }
                    }
                    catch (KeyNotFoundException ex)
                    {
                        throw new FileLoadException("Tried to load technology " + technology.Name, ex);
                    }
				}
			}
			#endregion

			#region moduletypes
			foreach (XmlElement el in this.confDocument.SelectNodes("/data/module/entry"))
			{
				ModuleType moduleType;
				if (loadStub)
				{
					moduleType = new ModuleType(el.GetAttribute("name"));
				}
				else
				{
					moduleType = this.game.ModuleTypes[el.GetAttribute("name")];
					this.assignNames(el, moduleType);
					this.assignNamesMultiple(el, moduleType);
					try
					{
						moduleType.Group = this.getModuleTypeGroup(el.GetAttribute("group"));
						moduleType.Mass = this.XMLAssignDouble(el.GetAttribute("mass"), 0);
						moduleType.Size = this.XMLAssignDouble(el.GetAttribute("size"), 0);
						moduleType.Capacity = this.XMLAssignDouble(el.GetAttribute("capacity"), 0);
						moduleType.TechnologyCapacity = this.XMLAssignInteger(el.GetAttribute("technology-capacity"), 0);

						moduleType.CrewRequired = this.XMLAssignInteger(el.GetAttribute("crew"), 0);
						moduleType.EnergyRequired = this.XMLAssignInteger(el.GetAttribute("energy"), 0);

						moduleType.HitPoints = this.XMLAssignInteger(el.GetAttribute("hit-points"), System.Convert.ToInt32((moduleType.Mass + moduleType.Size) / 20));
						moduleType.Attack = this.XMLAssignInteger(el.GetAttribute("attack"), 0);
						moduleType.Defense = this.XMLAssignInteger(el.GetAttribute("defense"), 0);
						moduleType.Damage = this.XMLAssignInteger(el.GetAttribute("damage"), 0);

						this.assignItemStacks(el.SelectNodes("upkeep"), moduleType.Upkeep);

                        #region fuel
                        foreach (XmlElement elFuel in el.SelectNodes("fuel"))
                        {
                            moduleType.FuelDuration = this.XMLAssignInteger(elFuel.GetAttribute("duration"), 1);
                            this.assignItemStacks(elFuel.SelectNodes("item"), moduleType.Fuel);
                        }
                        #endregion

                        #region movement
                        MoveMode moveMode;
						foreach (XmlElement elMove in el.SelectNodes("move"))
						{
							moveMode = new MoveMode();

							if (elMove.HasAttribute("mode"))
							{
								switch (elMove.GetAttribute("mode"))
								{
									case "space":
										moveMode.Mode = EMoveMode.space;
										break;
									default:
										moveMode.Mode = EMoveMode.ground;
										break;
								}
							}
							else
							{
								moveMode.Mode = EMoveMode.ground;
							}
							moveMode.Speed = this.XMLAssignDouble(elMove.GetAttribute("speed"), 1);
                            moveMode.MassCapacity = this.XMLAssignDouble(elMove.GetAttribute("mass-capacity"), 0);

							moduleType.MoveModes.Add(moveMode.Mode, moveMode);
						}

						#endregion

						#region settlement module
						moduleType.PopulationMaximum = this.XMLAssignInteger(el.GetAttribute("population-maximum"), 0);
						#endregion
                        
                        #region research module
						
						#endregion

						#region production module
                        this.assignItemStacks(el.SelectNodes("energy-consume"), moduleType.ProduceEnergyConsume);

						this.assignItemStacks(el.SelectNodes("consume"), moduleType.Consume);
						ItemStack item = null;
						foreach (XmlElement elProduce in el.SelectNodes("produce"))
						{
							try
							{
								if (elProduce.HasAttribute("item"))
								{
									item = new ItemStack(ItemType.All[elProduce.GetAttribute("item")]);
									item.Quantity = this.XMLAssignInteger(elProduce.GetAttribute("quantity"), 1);
									moduleType.ItemsProduction.Add(item);
									moduleType.ProduceDuration = this.XMLAssignInteger(elProduce.GetAttribute("duration"), 1);
								}
								else if (elProduce.HasAttribute("energy"))
								{
									moduleType.EnergyProduction = this.XMLAssignInteger(elProduce.GetAttribute("energy"), 0);
									moduleType.ProduceDuration= this.XMLAssignInteger(elProduce.GetAttribute("duration"), 13);
								}
								else if (elProduce.HasAttribute("effect"))
								{
									int duration = -1;
									if (elProduce.HasAttribute("duration"))
									{
										if (elProduce.GetAttribute("duration") != "permanent")
										{
											duration = this.XMLAssignInteger(elProduce.GetAttribute("duration"), -1);
										}
									}
									string effect = this.XMLAssignString(elProduce.GetAttribute("effect"), "effect");
									switch (effect)
									{
										case "region cost reduction":
											moduleType.EffectsProduction.Add(EProducableEffects.regionCostReduction);
											break;
									}
								}
								else
								{
									throw new ArgumentException("Unknown production type");
								}
							}
							catch (KeyNotFoundException ex)
							{
								throw new FileLoadException("Tried to load produce item " + item.ItemType.ReportName, ex);
							}
						}
						#endregion

						#region conditions
						foreach (XmlElement elAllowed in el.SelectNodes("operation-allowed-in"))
						{
							string condition;							
							if (elAllowed.HasAttribute("planet-atmosphere"))
							{
								condition = elAllowed.GetAttribute("planet-atmosphere");
								moduleType.OperationCondition_AtmosphereResources.Add(condition, this.game.ItemTypes[condition]);
							}

							if (elAllowed.HasAttribute("location-type"))
							{
								moduleType.OperationCondition_LocationTypes.Add(this.LoadLocationType(elAllowed));
							}
						}

                        foreach (XmlElement elAllowedUse in el.SelectNodes("use"))
                        {
                            if (elAllowedUse.HasAttribute("location-type"))
                            {
                                moduleType.UseCondition_LocationTypes.Add(this.LoadLocationType(elAllowedUse));
                            }
                            if (elAllowedUse.HasAttribute("efficiency-multiplier"))
                            {
                                moduleType.UseCondition_EfficiencyMultiplier = this.XMLAssignDouble(elAllowedUse.GetAttribute("efficiency-multiplier"), 1);
                            }

                            if (elAllowedUse.HasAttribute("require-fuel"))
                            {
                                moduleType.UseCondition_RequireFuel = this.XMLAssignBoolean(elAllowedUse.GetAttribute("require-fuel"), false);
                            }
                        }

						#endregion
					}
					catch (Exception ex)
					{
						throw new Exception("Tried to load module type " + moduleType.ReportName, ex);
					}
				}
			}
			#endregion
		}

		private void assignItemStacks(XmlNodeList elements, ItemStacks itemStacks)
		{
			ItemStack itemStack = null;
			foreach (XmlElement element in elements)
			{
				itemStack = new ItemStack(this.game.ItemTypes[element.GetAttribute("type")]);
				itemStack.Quantity = this.XMLAssignInteger(element.GetAttribute("quantity"), 1);
				itemStacks.Add(itemStack);
			}
		}

		private EModuleTypesGroup getModuleTypeGroup(string groupName)
		{
			EModuleTypesGroup group;
			switch (groupName)
			{
				case "agricultural":
					group = EModuleTypesGroup.agricultural;
					break;
				case "command":
					group = EModuleTypesGroup.command;
					break;
				case "energy":
					group = EModuleTypesGroup.energy;
					break;
				case "extraction":
					group = EModuleTypesGroup.extraction;
					break;
				case "frigate":
					group = EModuleTypesGroup.frigate;
					break;
				case "habitat":
					group = EModuleTypesGroup.habitat;
					break;
				case "infantry":
					group = EModuleTypesGroup.infantry;
					break;
				case "military":
					group = EModuleTypesGroup.military;
					break;
				case "production":
					group = EModuleTypesGroup.production;
					break;
				case "propulsion":
					group = EModuleTypesGroup.propulsion;
					break;
                case "research":
                    group = EModuleTypesGroup.research;
                    break;
                case "settlement":
					group = EModuleTypesGroup.settlement;
					break;
				case "spacecraft":
					group = EModuleTypesGroup.spacecraft;
					break;
				case "space station":
					group = EModuleTypesGroup.spaceStation;
					break;
				case "storage":
					group = EModuleTypesGroup.storage;
					break;
				case "vehicle":
					group = EModuleTypesGroup.vehicle;
					break;
				default:
					throw new KeyNotFoundException("Unknown moduletype group " + groupName);
			}
			return group;
		}

		private void assignNames(XmlElement element, NamedObject namedObject)
		{
			namedObject.FullName = element.GetAttribute("name-en");
			if (element.HasAttribute("description"))
			{
				namedObject.Description = element.GetAttribute("description");
			}
		}

		private void assignNamesMultiple(XmlElement element, IMultiple namedObject)
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
				faction.Options.TextReport = this.XMLAssignBoolean(elFaction.GetAttribute("text-report"), true);
				faction.Options.ReportLineLength = this.XMLAssignInteger(elFaction.GetAttribute("text-report-line-length"), ReportLine.LineLength);
				faction.Options.XmlReport = this.XMLAssignBoolean(elFaction.GetAttribute("xml-report"), true);

				faction.Bank.Balance = this.XMLAssignInteger(elFaction.GetAttribute("balance"), 0);
				faction.Bank.CreditLine = this.XMLAssignInteger(elFaction.GetAttribute("credit-line"), 0);
				faction.Bank.CreditRate = this.XMLAssignDouble(elFaction.GetAttribute("credit-rate"), 0);
				faction.Bank.DepositRate = this.XMLAssignDouble(elFaction.GetAttribute("deposit-rate"), 0);

				//foreach (XmlElement el in elFaction.SelectNodes("shown-item"))
				//    f.ShownItems.Add(ItemType.Get(el.GetAttribute("name")));
				//foreach (XmlElement el in elFaction.SelectNodes("shown-skill"))
				//    f.ShownSkills.Add(SkillType.Get(el.GetAttribute("name")));
				//foreach (XmlElement el in elFaction.SelectNodes("shown-building"))
				//    f.ShownBuildings.Add(BuildingType.Get(el.GetAttribute("name")));

				// Attitudes
				//foreach (XmlElement elAttitude in elFaction.SelectNodes("attitude"))
				//{
				//    Attitude a = (Attitude)Convert.ToInt32(elAttitude.GetAttribute("level"));
				//    int fnum = Convert.ToInt32(elAttitude.GetAttribute("faction"));
				//    f.Attitudes.Add(fnum, a);
				//}
			}
		}

		public void LoadTurnNumber()
		{
			XmlElement el = (XmlElement)gameDocument.SelectSingleNode("/game");
			Game.Turn = Convert.ToInt32(el.GetAttribute("turn"));
		}


		public void LoadOrders()
		{
			XmlElement elOrders = (XmlElement)gameDocument.SelectSingleNode("/game/orders");

			Order order = null;
			IOrderable subject = null;
			string subjectType, subjectName = string.Empty;

			foreach (XmlElement elOrder in elOrders.SelectNodes("order"))
			{
				if (elOrder.HasAttribute("subject"))
				{
					subjectType = elOrder.GetAttribute("subject");
					subjectName = elOrder.GetAttribute("name");
					switch (subjectType)
					{
						//case "region":
						//    subject = Region.All[subjectName];
						//    break;
						case "faction":
							subject = Faction.All[subjectName];
						    break;
						case "modulestack":
							subject = ModuleStack.All[subjectName];
							break;
						case "person":
							subject = Person.All[subjectName];
							break;
						default:
							throw new Exception("Unknown subject type for order.");
					}
					if (subject == null)
						throw new Exception("could not find the order subject: " + subjectType + " named: " + subjectName);
				}
					
				switch (elOrder.FirstChild.Name)
				{
					case "active":
                        order = new ActiveOrder(subject);
						break;
					case "alias":
                        order = new AliasOrder(subject);
						break;
					case "buy":
                        order = new BuyOrder(subject);
						break;
                    case "copy":
                        order = new CopyOrder(subject);
                        break;
					case "form":
                        order = new FormOrder(subject);
						break;
					case "get":
                        order = new GetOrder(subject);
						break;
					case "give":
                        order = new GiveOrder(subject);
						break;
                    case "has":
                        order = new HasOrder(subject);
                        break;
                    case "move":
						order = new MoveOrder(subject);						
						break;
					case "name":
                        order = new NameOrder(subject);
						break;
                    case "produce":
                        order = new ProduceOrder(subject);
                        break;
                    case "sell":
                        order = new SellOrder(subject);
                        break;
                    case "set":
                        order = new SetOrder(subject);
                        break;
                    case "stack":
                        order = new StackOrder(subject);
                        break;
                    case "train":
                        order = new TrainOrder(subject);
                        break;
                    case "transfer":
                        order = new TransferOrder(subject);
						break;
					case "use":
                        order = new UseOrder(subject);
						break;
					default:
                        throw new Exception("Unknown order. " + elOrder.FirstChild.Name);
				}
				order.LoadXml(elOrder);
			}

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
				capacity.Group = this.getModuleTypeGroup(elCapacity.GetAttribute("group"));
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
				elFaction.SetAttribute("text-report", faction.Options.TextReport.ToString());
				elFaction.SetAttribute("text-report-line-length", faction.Options.ReportLineLength.ToString());
				elFaction.SetAttribute("xml-report", faction.Options.XmlReport.ToString());
				elFaction.SetAttribute("balance", faction.Bank.Balance.ToString());
				elFaction.SetAttribute("credit-line", faction.Bank.CreditLine.ToString());
				elFaction.SetAttribute("credit-rate", faction.Bank.CreditRate.ToString());
				elFaction.SetAttribute("deposit-rate", faction.Bank.DepositRate.ToString());

				//foreach (ItemType it in f.ShownItems)
				//    SaveItemType(it, elFaction, "shown-item");
				//foreach (SkillType st in f.ShownSkills)
				//    SaveSkillType(st, elFaction, "shown-skill");
				//foreach (BuildingType bt in f.ShownBuildings)
				//    SaveBuildingType(bt, elFaction, "shown-building");


				// Attitudes
				//foreach (int num in f.Attitudes.Keys)
				//{
				//    if (Faction.Get(num) == null)
				//        continue;
				//    XmlElement elAttitude = (XmlElement)doc.CreateElement("attitude");
				//    elFaction.AppendChild(elAttitude);
				//    elAttitude.SetAttribute("level", ((int)f.Attitudes[num]).ToString());
				//    elAttitude.SetAttribute("faction", num.ToString());
				//}
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
			XmlElement elOrders = doc.CreateElement("orders");
			doc.DocumentElement.AppendChild(elOrders);

			XmlElement elOrder;
			foreach (Faction faction in Faction.All.Values)
			{
				if (factionXMLreport != null & faction != factionXMLreport)
					continue;

				foreach (Order order in faction.Orders)
				{
					if (order.Level == 0)
					{
						elOrder = order.SaveXml(doc, "faction");
						elOrders.AppendChild(elOrder);
					}
				}
			}
			
			foreach (ModuleStack moduleStack in ModuleStack.All.Values)
			{
				if (factionXMLreport != null & moduleStack.Owner != factionXMLreport)
					continue;

				foreach (Order order in moduleStack.Orders)
				{
					if (order.Level == 0)
					{
						elOrder = order.SaveXml(doc, "modulestack");
						elOrders.AppendChild(elOrder);
					}
				}
			}

			foreach (Person person in Person.All.Values)
			{
				if (factionXMLreport != null & person.Owner != factionXMLreport)
					continue;

				foreach (Order order in person.Orders)
				{
					if (order.Level == 0)
					{
						elOrder = order.SaveXml(doc, "person");
						elOrders.AppendChild(elOrder);
					}
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
					switch (capacity.Group)
					{
						case EModuleTypesGroup.agricultural:
							elCapacity.SetAttribute("group", "agricultural");
							break;
						case EModuleTypesGroup.command:
							elCapacity.SetAttribute("group", "command");
							break;
						case EModuleTypesGroup.energy:
							elCapacity.SetAttribute("group", "energy");
							break;
						case EModuleTypesGroup.extraction:
							elCapacity.SetAttribute("group", "extraction");
							break;
						case EModuleTypesGroup.frigate:
							elCapacity.SetAttribute("group", "frigate");
							break;
						case EModuleTypesGroup.habitat:
							elCapacity.SetAttribute("group", "habitat");
							break;
						case EModuleTypesGroup.infantry:
							elCapacity.SetAttribute("group", "infantry");
							break;
						case EModuleTypesGroup.military:
							elCapacity.SetAttribute("group", "military");
							break;
						case EModuleTypesGroup.production:
							elCapacity.SetAttribute("group", "production");
							break;
						case EModuleTypesGroup.propulsion:
							elCapacity.SetAttribute("group", "propulsion");
							break;
						case EModuleTypesGroup.settlement:
							elCapacity.SetAttribute("group", "settlement");
							break;
						case EModuleTypesGroup.spacecraft:
							elCapacity.SetAttribute("group", "spacecraft");
							break;
						case EModuleTypesGroup.spaceStation:
							elCapacity.SetAttribute("group", "space station");
							break;
						case EModuleTypesGroup.storage:
							elCapacity.SetAttribute("group", "storage");
							break;
						case EModuleTypesGroup.vehicle:
							elCapacity.SetAttribute("group", "vehicle");
							break;
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
