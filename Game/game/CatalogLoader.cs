using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace SpaceAge
{
	public class CatalogLoader
	{
		private readonly DataFile dataFile;

		public CatalogLoader(DataFile dataFile)
		{
			this.dataFile = dataFile;
		}

		public void LoadItems(XmlDocument confDocument, Game game, bool loadStub)
		{
			string name;

			// Stars
			foreach (XmlElement el in confDocument.SelectNodes("/data/star/entry"))
			{
				if (loadStub)
				{
					name = el.GetAttribute("name");
					game.StarTypes.Add(name, new StarType(name));
				}
				else
				{
					StarType star = game.StarTypes[el.GetAttribute("name")];
					star.LoadXml(el);
				}
			}

			// Planets
			foreach (XmlElement el in confDocument.SelectNodes("/data/planet/entry"))
			{
				if (loadStub)
				{
					name = el.GetAttribute("name");
					game.PlanetTypes.Add(name, new PlanetType(name));
				}
				else
				{
					PlanetType planet = game.PlanetTypes[el.GetAttribute("name")];
					planet.LoadXml(el);
				}
			}

			// Moons
			foreach (XmlElement el in confDocument.SelectNodes("/data/moon/entry"))
			{
				if (loadStub)
				{
					name = el.GetAttribute("name");
					game.MoonTypes.Add(name, new MoonType(name));
				}
				else
				{
					MoonType moon = game.MoonTypes[el.GetAttribute("name")];
					moon.LoadXml(el);
				}
			}

			// Regions 
			foreach (XmlElement el in confDocument.SelectNodes("/data/region/entry"))
			{
				if (loadStub)
				{
					name = el.GetAttribute("name");
                    game.RegionTypes.Add(name, new RegionType(name));
				}
				else
				{
					RegionType region = game.RegionTypes[el.GetAttribute("name")];
					region.LoadXml(el);
                    if (el.HasAttribute("location-type"))
                    {
                        region.LocationType = this.dataFile.LoadLocationType(el);
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
			foreach (XmlElement el in confDocument.SelectNodes("/data/item/entry"))
			{
				ItemType itemType;
				if (loadStub)
				{
                    itemType = new ItemType(el.GetAttribute("name"));
				}
				else
				{
					itemType = ItemType.All[el.GetAttribute("name")];
					itemType.LoadXml(el);
					itemType.LoadMultipleNames(el);

					itemType.Mass = this.dataFile.XMLAssignDouble(el.GetAttribute("mass"), 0);
					itemType.Size = this.dataFile.XMLAssignDouble(el.GetAttribute("size"), 0);
					itemType.Attack = this.dataFile.XMLAssignInteger(el.GetAttribute("attack"), 0);
					itemType.Damage = this.dataFile.XMLAssignInteger(el.GetAttribute("damage"), 0);
					itemType.Defense = this.dataFile.XMLAssignInteger(el.GetAttribute("defense"), 0);
					itemType.Initiative = this.dataFile.XMLAssignInteger(el.GetAttribute("initiative"), 0);
					itemType.NominalValue = this.dataFile.XMLAssignInteger(el.GetAttribute("value"), 0);

					foreach (XmlElement elAllowedBy in el.SelectNodes("use-allowed-by"))
					{
						if (elAllowedBy.HasAttribute("module-type-group"))
						{
							itemType.UseAllowedModuleTypesGroup = ModuleTypeGroupXml.Parse(elAllowedBy.GetAttribute("module-type-group"));
						}
					}

					#region upkeep
					ItemStack item = null;
					foreach (XmlElement elUpkeep in el.SelectNodes("upkeep"))
					{
						item = new ItemStack(game.ItemTypes[elUpkeep.GetAttribute("type")]);
						item.Quantity = this.dataFile.XMLAssignInteger(elUpkeep.GetAttribute("quantity"), 1);
						itemType.Upkeep.Add(item);
					}
					#endregion
					#region consume
					foreach (XmlElement elConsume in el.SelectNodes("consume"))
					{
						item = new ItemStack(game.ItemTypes[elConsume.GetAttribute("type")]);
						item.Quantity = this.dataFile.XMLAssignInteger(elConsume.GetAttribute("quantity"), 1);
						itemType.Consume.Add(item);
					}
					#endregion
				}
			}
			#endregion

			#region races
			// races 
			foreach (XmlElement el in confDocument.SelectNodes("/data/race/entry"))
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
					itemType.LoadXml(el);
					itemType.LoadMultipleNames(el);
					itemType.Group = EItemTypesGroup.crew;
					itemType.Mass = this.dataFile.XMLAssignDouble(el.GetAttribute("mass"), 0);
					itemType.Size = this.dataFile.XMLAssignDouble(el.GetAttribute("size"), 0);
					itemType.NominalValue = this.dataFile.XMLAssignInteger(el.GetAttribute("value"), 0);

					#region upkeep
					ItemStack item = null;
					foreach (XmlElement elUpkeep in el.SelectNodes("upkeep"))
					{
						if (elUpkeep.GetAttribute("crew-type") == "crew")
						{
							item = new ItemStack(game.ItemTypes[elUpkeep.GetAttribute("type")]);
							item.Quantity = this.dataFile.XMLAssignInteger(elUpkeep.GetAttribute("quantity"), 1);
							itemType.Upkeep.Add(item);
						}
					}
					#endregion
					#region consume
					foreach (XmlElement elConsume in el.SelectNodes("consume"))
					{
						item = new ItemStack(game.ItemTypes[elConsume.GetAttribute("type")]);
						item.Quantity = this.dataFile.XMLAssignInteger(elConsume.GetAttribute("quantity"), 1);
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
					race.LoadXml(el);

					race.Mass = this.dataFile.XMLAssignDouble(el.GetAttribute("mass"), 0);
					race.Size = this.dataFile.XMLAssignDouble(el.GetAttribute("size"), 0);
					race.Capacity = this.dataFile.XMLAssignDouble(el.GetAttribute("capacity"), 0);
					race.OfficerTrainingDuration = this.dataFile.XMLAssignInteger(el.GetAttribute("officer-training-duration"), 0);

					#region upkeep
					ItemStack item = null;
					foreach (XmlElement elUpkeep in el.SelectNodes("upkeep"))
					{
						if (elUpkeep.GetAttribute("crew-type") == "officer")
						{
							item = new ItemStack(game.ItemTypes[elUpkeep.GetAttribute("type")]);
							item.Quantity = this.dataFile.XMLAssignInteger(elUpkeep.GetAttribute("quantity"), 1);
							race.Upkeep.Add(item);
						}
					}
					#endregion

					#region consume
					foreach (XmlElement elConsume in el.SelectNodes("consume"))
					{
						item = new ItemStack(game.ItemTypes[elConsume.GetAttribute("type")]);
						item.Quantity = this.dataFile.XMLAssignInteger(elConsume.GetAttribute("quantity"), 1);
						race.Consume.Add(item);
					}
					#endregion
					#region no-consume
					foreach (XmlElement elNoConsume in el.SelectNodes("no-consume"))
					{
						race.NoConsumeEffect = elNoConsume.GetAttribute("effect");
						race.NoConsumeChance = this.dataFile.XMLAssignInteger(elNoConsume.GetAttribute("chance"), 0);
					}
					#endregion
					#region no-upkeep
					foreach (XmlElement elNoUpkeep in el.SelectNodes("no-upkeep"))
					{
						race.NoUpkeepEffect = elNoUpkeep.GetAttribute("effect");
						race.NoUpkeepChance = this.dataFile.XMLAssignInteger(elNoUpkeep.GetAttribute("chance"), 0);
					}
					#endregion
				}

			}
			#endregion

			#region skills
			// skills 
			foreach (XmlElement el in confDocument.SelectNodes("/data/skill/entry"))
			{
				SkillType skillType;
				if (loadStub)
				{
					skillType = new SkillType(el.GetAttribute("name"));
				}
				else
				{
					skillType = SkillType.All[el.GetAttribute("name")];
					skillType.LoadXml(el);

					skillType.TrainingDuration = this.dataFile.XMLAssignInteger(el.GetAttribute("training-duration"), 1);
					skillType.AttackFormula = SkillBonusFormula.Parse(el.GetAttribute("attack"));
					skillType.DefenseFormula = SkillBonusFormula.Parse(el.GetAttribute("defense"));
					skillType.InitiativeFormula = SkillBonusFormula.Parse(el.GetAttribute("initiative"));
					skillType.CureChanceFormula = SkillBonusFormula.Parse(el.GetAttribute("cure-chance"));

					foreach (XmlElement elUsableIn in el.SelectNodes("usable-in"))
					{
						SkillUsableIn usableIn = new SkillUsableIn();
						string moduleGroup = elUsableIn.GetAttribute("module-group");
						if (moduleGroup != string.Empty)
						{
							usableIn.ModuleGroup = ModuleTypeGroupXml.Parse(moduleGroup);
						}
						else
						{
							string moduleTypeGroup = elUsableIn.GetAttribute("module-type-group");
							if (moduleTypeGroup != string.Empty)
							{
								usableIn.ModuleGroup = ModuleTypeGroupXml.Parse(moduleTypeGroup);
							}
						}

						string moduleStackSize = elUsableIn.GetAttribute("modulestack-size");
						if (moduleStackSize != string.Empty)
						{
							usableIn.ModuleStackSize = this.dataFile.XMLAssignInteger(moduleStackSize, 0);
						}

						skillType.UsableIn.Add(usableIn);
					}

					foreach (XmlElement elProduce in el.SelectNodes("produce"))
					{
						string effect = elProduce.GetAttribute("effect");
						string value = elProduce.GetAttribute("value");
						if (CatalogLoader.isPercentProduceValue(value))
						{
							skillType.PercentProduces.Add(new SkillPercentProduce
							{
								Effect = effect,
								Percent = int.Parse(value.Substring(0, value.Length - 1)),
							});
						}
						else
						{
							skillType.ProduceEffect = effect;
							skillType.ProduceTarget = elProduce.GetAttribute("target");
							skillType.ProduceFormula = SkillBonusFormula.Parse(value);
						}
					}
				}
			}
			#endregion

			#region technologies
			Technology technology;
			foreach (XmlElement el in confDocument.SelectNodes("/data/technology/entry"))
			{
				if (loadStub)
				{
					name = el.GetAttribute("name");
					technology = new Technology(name);
				}
				else
				{
					technology = game.Technologies[el.GetAttribute("name")];
					technology.LoadXml(el);
					technology.Level = this.dataFile.XMLAssignInteger(el.GetAttribute("level"), 0);
					technology.UseTime = this.dataFile.XMLAssignInteger(el.GetAttribute("use-time"), 1);

					if (el.HasAttribute("cost"))
					{
						technology.Cost = this.dataFile.XMLAssignInteger(el.GetAttribute("cost"), technology.Cost);
					}
					technology.LoadTags(el.GetAttribute("tags"));
					if (el.HasAttribute("requires"))
					{
						technology.Requires = game.Technologies[el.GetAttribute("requires")];
					}

					technology.Attack = this.dataFile.XMLAssignInteger(el.GetAttribute("attack"), 0);
					technology.Defense = this.dataFile.XMLAssignInteger(el.GetAttribute("defense"), 0);
					technology.Initiative = this.dataFile.XMLAssignInteger(el.GetAttribute("initiative"), 0);

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
								item.Quantity = this.dataFile.XMLAssignInteger(elConsume.GetAttribute("quantity"), 1);
                                items.Add(item);
                                technology.UseConsumeItems = items;
							}
							else if (elConsume.HasAttribute("module"))
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
							    item = new ItemStack(game.ItemTypes[elProduce.GetAttribute("item")]);
							    item.Quantity = this.dataFile.XMLAssignInteger(elProduce.GetAttribute("quantity"), 1);
                                items.Add(item);
                                technology.UseProduceItems = items;
							    technology.ProductionType = EProductionType.Items;
						    }
						    else if (elProduce.HasAttribute("module"))
						    {
							    module = game.ModuleTypes[elProduce.GetAttribute("module")];
							    technology.UseProduceModules = module;
							    technology.ProductionType = EProductionType.Modules;
						    }
						    else if (elProduce.HasAttribute("effect"))
						    {
							    string reason = this.dataFile.XMLAssignString(elProduce.GetAttribute("effect"), "effect");
							    if (elProduce.HasAttribute("target") == false || elProduce.HasAttribute("change") == false)
							    {
								    throw new FileLoadException("tried to parse effect " + reason);
							    }
							    technology.UseProduceEffectName = reason;
							    technology.UseProduceTarget = elProduce.GetAttribute("target");
							    technology.UseProduceChange = this.dataFile.XMLAssignInteger(elProduce.GetAttribute("change"), 0);
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
                                technology.UseCondition_PlanetTypes = new PlanetTypes
                                {
                                    { condition, game.PlanetTypes[condition] }
                                };
						    }

						    if (elAllowed.HasAttribute("planet-atmosphere"))
						    {
							    condition = elAllowed.GetAttribute("planet-atmosphere");
                                technology.UseCondition_AtmosphereResources = new ItemTypes
                                {
                                    { condition, game.ItemTypes[condition] }
                                };
						    }

						    if (elAllowed.HasAttribute("location-type"))
						    {
                                technology.UseCondition_LocationTypes = new LocationTypes
                                {
                                    this.dataFile.LoadLocationType(elAllowed)
                                };
						    }

                            if (elAllowed.HasAttribute("module-type-group"))
                            {                                
                                technology.UseCondition_ModuleTypesGroup = ModuleTypeGroupXml.Parse(elAllowed.GetAttribute("module-type-group"));
                            }
							if (elAllowed.HasAttribute("module"))
							{
								technology.UseCondition_ModuleType = elAllowed.GetAttribute("module");
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
			foreach (XmlElement el in confDocument.SelectNodes("/data/module/entry"))
			{
				ModuleType moduleType;
				if (loadStub)
				{
					moduleType = new ModuleType(el.GetAttribute("name"));
				}
				else
				{
					moduleType = game.ModuleTypes[el.GetAttribute("name")];
					moduleType.LoadXml(el);
					moduleType.LoadMultipleNames(el);
					try
					{
						moduleType.Group = ModuleTypeGroupXml.Parse(el.GetAttribute("group"));
						moduleType.Mass = this.dataFile.XMLAssignDouble(el.GetAttribute("mass"), 0);
						moduleType.Size = this.dataFile.XMLAssignDouble(el.GetAttribute("size"), 0);
						moduleType.Capacity = this.dataFile.XMLAssignDouble(el.GetAttribute("capacity"), 0);
                        moduleType.ResearchOutput = this.dataFile.XMLAssignInteger(el.GetAttribute("research-output"), 0); 
						moduleType.TechnologyCapacity = this.dataFile.XMLAssignInteger(el.GetAttribute("technology-capacity"), 0);

						moduleType.CrewRequired = this.dataFile.XMLAssignInteger(el.GetAttribute("crew"), 0);
						moduleType.EnergyRequired = this.dataFile.XMLAssignInteger(el.GetAttribute("energy"), 0);

						moduleType.DamageCapacity = this.dataFile.XMLAssignInteger(el.GetAttribute("hit-points"), System.Convert.ToInt32((moduleType.Mass + moduleType.Size) / 20));
						moduleType.Attack = this.dataFile.XMLAssignInteger(el.GetAttribute("attack"), 0);
						moduleType.Defense = this.dataFile.XMLAssignInteger(el.GetAttribute("defense"), 0);
						moduleType.Damage = this.dataFile.XMLAssignInteger(el.GetAttribute("damage"), 0);
						moduleType.Initiative = this.dataFile.XMLAssignInteger(el.GetAttribute("initiative"), 0);
						moduleType.NominalValue = this.dataFile.XMLAssignInteger(el.GetAttribute("value"), 0);
						moduleType.WeaponGroup = el.GetAttribute("weapon-group");
						moduleType.Resists = el.GetAttribute("resists");
						moduleType.ArmorModule = el.GetAttribute("armor-module") == "true";

						this.dataFile.assignItemStacks(el.SelectNodes("upkeep"), moduleType.Upkeep);
						foreach (XmlElement elNoUpkeep in el.SelectNodes("no-upkeep"))
						{
							moduleType.NoUpkeepEffect = elNoUpkeep.GetAttribute("effect");
							moduleType.NoUpkeepChance = this.dataFile.XMLAssignInteger(elNoUpkeep.GetAttribute("chance"), 0);
						}

                        #region fuel
                        foreach (XmlElement elFuel in el.SelectNodes("fuel"))
                        {
                            moduleType.FuelDuration = this.dataFile.XMLAssignInteger(elFuel.GetAttribute("duration"), 1);
                            this.dataFile.assignItemStacks(elFuel.SelectNodes("item"), moduleType.Fuel);
                        }
                        #endregion

                        #region movement
                        MoveMode moveMode;
						foreach (XmlElement elMove in el.SelectNodes("move"))
						{
							moveMode = new MoveMode();

							if (elMove.HasAttribute("mode"))
							{
								moveMode.Mode = MoveModeXml.Parse(elMove.GetAttribute("mode"));
							}
							else
							{
								moveMode.Mode = EMoveMode.ground;
							}
							moveMode.Speed = this.dataFile.XMLAssignDouble(elMove.GetAttribute("speed"), 1);
                            moveMode.MassCapacity = this.dataFile.XMLAssignDouble(elMove.GetAttribute("mass-capacity"), 0);

							moduleType.MoveModes.Add(moveMode.Mode, moveMode);
						}

						#endregion

						#region settlement module
						moduleType.PopulationMaximum = this.dataFile.XMLAssignInteger(el.GetAttribute("population-maximum"), 0);
						#endregion
                        
                        #region research module
						
						#endregion

						#region production module
                        this.dataFile.assignItemStacks(el.SelectNodes("energy-consume"), moduleType.ProduceEnergyConsume);

						this.dataFile.assignItemStacks(el.SelectNodes("consume"), moduleType.Consume);
						foreach (XmlElement elNoConsume in el.SelectNodes("no-consume"))
						{
							moduleType.NoConsumeEffect = elNoConsume.GetAttribute("effect");
							moduleType.NoConsumeChance = this.dataFile.XMLAssignInteger(elNoConsume.GetAttribute("chance"), 0);
						}
						foreach (XmlElement elEffect in el.SelectNodes("effect"))
						{
							if (elEffect.GetAttribute("type") != "heal")
							{
								continue;
							}
							if (elEffect.GetAttribute("target") != "wndtrn")
							{
								continue;
							}
							moduleType.HealTarget = elEffect.GetAttribute("target");
							moduleType.HealQuantity = this.dataFile.XMLAssignInteger(elEffect.GetAttribute("value"), 0);
							moduleType.HealWeeks = this.dataFile.XMLAssignInteger(elEffect.GetAttribute("weeks"), 0);
							moduleType.HealQuantityWithItem = this.dataFile.XMLAssignInteger(elEffect.GetAttribute("with-item-value"), 0);
							moduleType.HealWeeksWithItem = this.dataFile.XMLAssignInteger(elEffect.GetAttribute("weeks-with-item"), 0);
							moduleType.HealConsumeItem = elEffect.GetAttribute("consume-item");
							moduleType.HealConsumeQuantity = this.dataFile.XMLAssignInteger(elEffect.GetAttribute("consume-quantity"), 1);
						}
						ItemStack item = null;
						foreach (XmlElement elProduce in el.SelectNodes("produce"))
						{
							try
							{
								if (elProduce.HasAttribute("item"))
								{
									item = new ItemStack(ItemType.All[elProduce.GetAttribute("item")]);
									item.Quantity = this.dataFile.XMLAssignInteger(elProduce.GetAttribute("quantity"), 1);
									moduleType.ItemsProduction.Add(item);
									moduleType.ProduceDuration = this.dataFile.XMLAssignInteger(elProduce.GetAttribute("duration"), 1);
								}
								else if (elProduce.HasAttribute("energy"))
								{
									moduleType.EnergyProduction = this.dataFile.XMLAssignInteger(elProduce.GetAttribute("energy"), 0);
									moduleType.ProduceDuration= this.dataFile.XMLAssignInteger(elProduce.GetAttribute("duration"), 13);
								}
								else if (elProduce.HasAttribute("effect"))
								{
									int duration = -1;
									if (elProduce.HasAttribute("duration"))
									{
										if (elProduce.GetAttribute("duration") != "permanent")
										{
											duration = this.dataFile.XMLAssignInteger(elProduce.GetAttribute("duration"), -1);
										}
									}
									string effect = this.dataFile.XMLAssignString(elProduce.GetAttribute("effect"), "effect");
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
								moduleType.OperationCondition_AtmosphereResources.Add(condition, game.ItemTypes[condition]);
							}

							if (elAllowed.HasAttribute("planet-type"))
							{
								condition = elAllowed.GetAttribute("planet-type");
								moduleType.OperationCondition_PlanetTypes.Add(condition, game.PlanetTypes[condition]);
							}

							if (elAllowed.HasAttribute("location-type"))
							{
								moduleType.OperationCondition_LocationTypes.Add(this.dataFile.LoadLocationType(elAllowed));
							}
						}

                        foreach (XmlElement elAllowedUse in el.SelectNodes("use"))
                        {
                            if (elAllowedUse.HasAttribute("location-type"))
                            {
                                moduleType.UseCondition_LocationTypes.Add(this.dataFile.LoadLocationType(elAllowedUse));
                            }
                            if (elAllowedUse.HasAttribute("efficiency-multiplier"))
                            {
                                moduleType.UseCondition_EfficiencyMultiplier = this.dataFile.XMLAssignDouble(elAllowedUse.GetAttribute("efficiency-multiplier"), 1);
                            }

                            if (elAllowedUse.HasAttribute("require-fuel"))
                            {
                                moduleType.UseCondition_RequireFuel = this.dataFile.XMLAssignBoolean(elAllowedUse.GetAttribute("require-fuel"), false);
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

		private static bool isPercentProduceValue(string value)
		{
			if (string.IsNullOrEmpty(value) || !value.EndsWith("%"))
			{
				return false;
			}
			string digits = value.Substring(0, value.Length - 1);
			int percent;
			return int.TryParse(digits, out percent);
		}
	}
}
