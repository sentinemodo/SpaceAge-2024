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
				faction.LoadXml(elFaction);
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
			XmlElement elGalaxy = (XmlElement)this.gameDocument.SelectSingleNode("/game/galaxy");
			this.game.Galaxy.LoadXml(elGalaxy, this);
			this.game.Galaxy.LoadExits(elGalaxy, this);
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

				doc.DocumentElement.AppendChild(faction.SaveXml(doc));
			}
			#endregion

			#region contracts
			if (Contract.All.Count > 0)
			{
				doc.DocumentElement.AppendChild(Contract.All.SaveXml(doc));
			}
			#endregion

			doc.DocumentElement.AppendChild(this.game.Galaxy.SaveXml(doc, factionXMLreport));

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

		public void SaveOrders(XmlDocument doc, Faction factionXMLreport = null)
		{
			OrderXml.SaveAll(doc, factionXMLreport);
		}

	}	
}
