using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public partial class Battle
	{
		private void resetCaptureDamage()
		{
			this.resetCaptureDamage(this.attackers);
			this.resetCaptureDamage(this.defenders);
		}

		private void resetCaptureDamage(ModuleStacks stacks)
		{
			if (stacks == null)
			{
				return;
			}
			foreach (ModuleStack stack in stacks.Values)
			{
				this.resetCaptureDamage(stack);
			}
		}

		private void resetCaptureDamage(ModuleStack stack)
		{
			if (stack == null)
			{
				return;
			}
			foreach (Module module in stack.Modules)
			{
				module.CaptureDamage = 0;
			}
			foreach (ModuleStack nested in stack.ModuleStacks.Values)
			{
				this.resetCaptureDamage(nested);
			}
		}

		private void processEvadeLeaves()
		{
			List<ModuleStack> combatants = new List<ModuleStack>();
			foreach (ModuleStack stack in this.attackers.Values)
			{
				combatants.Add(stack);
			}
			foreach (ModuleStack stack in this.defenders.Values)
			{
				combatants.Add(stack);
			}
			foreach (ModuleStack stack in combatants)
			{
				if (!stack.HasEvade)
				{
					continue;
				}
				if (this.hitThisRound.Contains(stack))
				{
					this.unhitRounds[stack] = 0;
					continue;
				}
				int count = 0;
				if (this.unhitRounds.ContainsKey(stack))
				{
					count = this.unhitRounds[stack];
				}
				count++;
				this.unhitRounds[stack] = count;
				if (count >= 2)
				{
					string line = string.Concat(stack.ReportName, " evades and leaves combat.");
					this.report(line);
					this.removeFromBattle(stack);
				}
			}
		}

		public void ApplyCaptures(int week)
		{
			this.applyCompleteCaptures(this.attackers, week);
			this.applyCompleteCaptures(this.defenders, week);
			List<Module> captures = new List<Module>(this.pendingCaptures);
			foreach (Module module in captures)
			{
				if (module.IsWrecked || !module.IsCaptureComplete)
				{
					continue;
				}
				this.transferCapturedModule(module, week);
			}
			this.pendingCaptures.Clear();
		}

		private enum EVictoryDisposition
		{
			none,
			destroy,
			scavenge,
			capture
		}

		public void ApplyVictoryResolution(int week)
		{
			bool attackersWin = this.attackers.Count > 0 && this.defenders.Count == 0;
			bool defendersWin = this.attackers.Count == 0 && this.defenders.Count > 0;
			if (!attackersWin && !defendersWin)
			{
				return;
			}

			ModuleStacks winningSide = attackersWin ? this.attackers : this.defenders;
			EVictoryDisposition disposition = this.resolveVictoryDisposition(winningSide);
			if (disposition == EVictoryDisposition.none)
			{
				return;
			}

			Faction winnerOwner;
			ModuleStack winnerInitiator;
			if (attackersWin)
			{
				winnerOwner = this.attacker.Owner;
				winnerInitiator = this.attacker;
			}
			else
			{
				winnerInitiator = this.battleDefender;
				winnerOwner = this.battleDefender != null ? this.battleDefender.Owner : null;
				foreach (ModuleStack stack in winningSide.Values)
				{
					winnerOwner = stack.Owner;
					winnerInitiator = stack;
					break;
				}
			}
			if (winnerOwner == null || winnerInitiator == null)
			{
				return;
			}

			List<ModuleStack> scavengers = this.collectScavengers(winningSide);
			List<ModuleStack> lootRecipients = this.collectLootRecipients(winningSide);
			HashSet<Faction> loserOwners = this.collectLoserOwners(winnerOwner);
			Location location = winnerInitiator.Location;
			if (location == null)
			{
				return;
			}

			List<ModuleStack> loserStacks = new List<ModuleStack>();
			foreach (ModuleStack stack in stacksAtLocation(location))
			{
				if (loserOwners.Contains(stack.Owner))
				{
					loserStacks.Add(stack);
				}
			}
			foreach (ModuleStack stack in loserStacks)
			{
				this.applyVictoryToStack(stack, disposition, week, winnerOwner, scavengers, lootRecipients, loserOwners);
			}
		}

		private EVictoryDisposition resolveVictoryDisposition(ModuleStacks winningSide)
		{
			if (winningSide == null || winningSide.Count == 0)
			{
				return EVictoryDisposition.none;
			}

			bool allAvoid = true;
			bool hasDestroy = false;
			bool hasScavenge = false;
			bool hasCapture = false;
			foreach (ModuleStack stack in winningSide.Values)
			{
				if (!stack.IsAvoiding)
				{
					allAvoid = false;
				}
				if (stack.HasDestroy)
				{
					hasDestroy = true;
				}
				if (stack.HasScavenge)
				{
					hasScavenge = true;
				}
				if (stack.HasCapture)
				{
					hasCapture = true;
				}
			}

			if (allAvoid)
			{
				return EVictoryDisposition.none;
			}
			if (hasDestroy)
			{
				return EVictoryDisposition.destroy;
			}
			if (hasScavenge)
			{
				return EVictoryDisposition.scavenge;
			}
			if (hasCapture)
			{
				return EVictoryDisposition.capture;
			}
			return EVictoryDisposition.none;
		}

		private List<ModuleStack> collectScavengers(ModuleStacks winningSide)
		{
			List<ModuleStack> scavengers = new List<ModuleStack>();
			foreach (ModuleStack stack in winningSide.Values)
			{
				if (stack.HasScavenge)
				{
					scavengers.Add(stack);
				}
			}
			return scavengers;
		}

		private List<ModuleStack> collectLootRecipients(ModuleStacks winningSide)
		{
			List<ModuleStack> recipients = new List<ModuleStack>();
			if (winningSide == null)
			{
				return recipients;
			}
			foreach (ModuleStack stack in winningSide.Values)
			{
				if (stack.IsFormed && stack.IsRootModuleStack)
				{
					recipients.Add(stack);
				}
			}
			return recipients;
		}

		internal void SplitLootAmongRecipients(List<ModuleStack> recipients, ItemStack itemStack)
		{
			this.splitAmongRecipients(recipients, itemStack);
		}

		internal void ReportFaunaLoot(ItemStack itemStack)
		{
			if (itemStack == null || itemStack.Quantity <= 0)
			{
				return;
			}
			this.report(string.Format("  Fauna wreckage yielded {0}.", itemStack.ReportName));
		}

		private HashSet<Faction> collectLoserOwners(Faction winnerOwner)
		{
			HashSet<Faction> loserOwners = new HashSet<Faction>();
			if (this.attacker != null && this.attacker.Owner != winnerOwner)
			{
				loserOwners.Add(this.attacker.Owner);
			}
			if (this.battleDefender != null && this.battleDefender.Owner != winnerOwner)
			{
				loserOwners.Add(this.battleDefender.Owner);
			}
			return loserOwners;
		}

		private void applyVictoryToStack(
			ModuleStack stack,
			EVictoryDisposition disposition,
			int week,
			Faction winnerOwner,
			List<ModuleStack> scavengers,
			List<ModuleStack> lootRecipients,
			HashSet<Faction> loserOwners)
		{
			List<Module> modules = new List<Module>(stack.Modules);
			foreach (Module module in modules)
			{
				if (module.IsWrecked || module.IsActive)
				{
					continue;
				}
				switch (disposition)
				{
					case EVictoryDisposition.destroy:
						this.destroyDisabledModule(module, week, winnerOwner, null, lootRecipients);
						break;
					case EVictoryDisposition.scavenge:
						this.destroyDisabledModule(module, week, winnerOwner, scavengers, lootRecipients);
						break;
					case EVictoryDisposition.capture:
						this.transferVictoryCapturedModule(module, week, winnerOwner);
						break;
				}
			}
			foreach (ModuleStack nested in stack.ModuleStacks.Values)
			{
				if (loserOwners.Contains(nested.Owner))
				{
					this.applyVictoryToStack(nested, disposition, week, winnerOwner, scavengers, lootRecipients, loserOwners);
				}
			}
		}

		private void destroyDisabledModule(
			Module module,
			int week,
			Faction winnerOwner,
			List<ModuleStack> scavengers,
			List<ModuleStack> lootRecipients)
		{
			ModuleStack source = module.Parent;
			int originalCount = source.Quantity;
			if (originalCount < 1)
			{
				return;
			}
			int index = source.Modules.IndexOf(module);
			if (index < 0)
			{
				return;
			}

			if (scavengers != null && scavengers.Count > 0)
			{
				this.giveScavengeBuildResources(source.ModuleType, scavengers);
				this.transferProportionalItemsToRecipients(
					source,
					scavengers,
					1,
					originalCount,
					false);
			}
			else
			{
				this.removeProportionalItems(source, 1, originalCount, false);
			}

			List<ModuleStack> faunaLootRecipients = scavengers != null && scavengers.Count > 0
				? scavengers
				: lootRecipients;
			FaunaBattleLoot.DropFromDestroyedModule(this, source, faunaLootRecipients, week);

			this.destroyProportionalNested(source, 1, originalCount, winnerOwner);
			string moduleReportName = module.ReportName;
			source.RemoveModule(index);

			string line = string.Format("  {0} is destroyed.", moduleReportName);
			this.report(line);
			this.reportObserver(line);
			source.EventReports.Add(week, string.Format("lost {0} after defeat.", moduleReportName));

			this.cleanupDestroyedStack(source, winnerOwner);
		}

		private void giveScavengeBuildResources(ModuleType moduleType, List<ModuleStack> scavengers)
		{
			Technology technology = Technology.All.FindProducerFor(moduleType);
			if (technology == null || technology.UseConsumeItems == null)
			{
				return;
			}
			foreach (ItemStack template in technology.UseConsumeItems.Values)
			{
				int half = template.Quantity / 2;
				if (half <= 0)
				{
					continue;
				}
				this.splitAmongRecipients(
					scavengers,
					new ItemStack(template.ItemType, half));
			}
		}

		private void splitAmongRecipients(List<ModuleStack> recipients, ItemStack itemStack)
		{
			if (recipients == null || recipients.Count == 0 || itemStack.Quantity <= 0)
			{
				return;
			}
			int remaining = itemStack.Quantity;
			int index = 0;
			while (remaining > 0)
			{
				ModuleStack recipient = recipients[index % recipients.Count];
				recipient.ItemStacks.Add(new ItemStack(itemStack.ItemType, 1));
				remaining--;
				index++;
			}
		}

		private void transferProportionalItemsToRecipients(
			ModuleStack source,
			List<ModuleStack> recipients,
			int taken,
			int originalCount,
			bool crewCasualties)
		{
			if (recipients == null || recipients.Count == 0)
			{
				return;
			}
			List<ItemStack> snapshot = new List<ItemStack>();
			foreach (ItemStack itemStack in source.ItemStacks.Values)
			{
				snapshot.Add(itemStack);
			}
			foreach (ItemStack itemStack in snapshot)
			{
				int move = (itemStack.Quantity * taken) / originalCount;
				if (move <= 0)
				{
					continue;
				}
				if (crewCasualties && itemStack.ItemType.Group == EItemTypesGroup.crew)
				{
					this.transferCrewCasualties(source, recipients[0], itemStack.ItemType, move);
					continue;
				}
				source.ItemStacks.Remove(new ItemStack(itemStack.ItemType, move));
				this.splitAmongRecipients(recipients, new ItemStack(itemStack.ItemType, move));
			}
		}

		private void removeProportionalItems(ModuleStack source, int taken, int originalCount, bool crewCasualties)
		{
			List<ItemStack> snapshot = new List<ItemStack>();
			foreach (ItemStack itemStack in source.ItemStacks.Values)
			{
				snapshot.Add(itemStack);
			}
			foreach (ItemStack itemStack in snapshot)
			{
				int remove = (itemStack.Quantity * taken) / originalCount;
				if (remove <= 0)
				{
					continue;
				}
				if (crewCasualties && itemStack.ItemType.Group == EItemTypesGroup.crew)
				{
					source.ItemStacks.Remove(new ItemStack(itemStack.ItemType, remove));
					int killed = remove / 4;
					if (killed > 0)
					{
						this.report(string.Format("  {0} killed.", new ItemStack(itemStack.ItemType, killed).ReportName));
					}
					continue;
				}
				source.ItemStacks.Remove(new ItemStack(itemStack.ItemType, remove));
			}
		}

		private void destroyProportionalNested(ModuleStack source, int taken, int originalCount, Faction winnerOwner)
		{
			List<ModuleStack> nested = new List<ModuleStack>();
			foreach (ModuleStack child in source.ModuleStacks.Values)
			{
				nested.Add(child);
			}
			foreach (ModuleStack child in nested)
			{
				int moveCount = (child.Quantity * taken) / originalCount;
				if (moveCount <= 0)
				{
					continue;
				}
				for (int i = 0; i < moveCount; i++)
				{
					if (child.Quantity == 0)
					{
						break;
					}
					child.RemoveModule(0);
				}
				this.cleanupDestroyedStack(child, winnerOwner);
			}
		}

		private void cleanupDestroyedStack(ModuleStack stack, Faction winnerOwner)
		{
			if (stack.Quantity > 0)
			{
				return;
			}
			ModuleStack root = stack.RootModuleStack;
			stack.ModuleType = null;
			ModuleStack.All.Remove(stack);
			if (root != null && !root.HasIntactModules())
			{
				Contract.All.NotifyStackDestroyed(winnerOwner, root);
			}
		}

		private void transferVictoryCapturedModule(Module module, int week, Faction recipientOwner)
		{
			ModuleStack source = module.Parent;
			int originalCount = source.Quantity;
			if (originalCount < 1 || source.Owner == recipientOwner)
			{
				return;
			}
			if (module.IsWrecked || module.IsActive)
			{
				return;
			}

			int index = source.Modules.IndexOf(module);
			if (index < 0)
			{
				return;
			}

			ModuleStack captured = new ModuleStack(source.Parent, recipientOwner, source.ModuleType, this.capturedStackName(source));
			source.RemoveModule(index);
			captured.AddModule(module);

			this.transferProportionalItems(source, captured, 1, originalCount, source.ModuleType.Group == EModuleTypesGroup.command);
			this.transferProportionalNested(source, captured, 1, originalCount, recipientOwner);
			this.transferProportionalPeople(source, captured, 1, originalCount, recipientOwner);

			source.EventReports.Add(week, string.Format("lost {0} to capture.", module.ReportName));
			captured.EventReports.Add(week, string.Format("captured {0} from {1}.", captured.ModuleType.ReportName, source.ReportName));

			string line = string.Format("  {0} captured by {1}.", captured.ReportName, recipientOwner.ReportName);
			this.report(line);
			this.reportObserver(line);

			if (source.ModuleType != null && source.ModuleType.Group == EModuleTypesGroup.command)
			{
				ModuleStack root = source.RootModuleStack;
				Faction originalOwner = source.Owner;
				if (root.Owner == originalOwner
					&& this.countCommandModules(root, originalOwner) == 0)
				{
					this.captureParentByOwnership(root, recipientOwner);
				}
			}

			this.cleanupDestroyedStack(source, recipientOwner);
		}

		private void applyCompleteCaptures(ModuleStacks stacks, int week)
		{
			if (stacks == null)
			{
				return;
			}
			List<ModuleStack> stackList = new List<ModuleStack>(stacks.Values);
			foreach (ModuleStack stack in stackList)
			{
				this.applyCompleteCaptures(stack, week);
			}
		}

		private void applyCompleteCaptures(ModuleStack stack, int week)
		{
			List<Module> modules = new List<Module>(stack.Modules);
			foreach (Module module in modules)
			{
				if (module.IsWrecked || !module.IsCaptureComplete)
				{
					continue;
				}
				this.transferCapturedModule(module, week);
			}
			foreach (ModuleStack nested in stack.ModuleStacks.Values)
			{
				this.applyCompleteCaptures(nested, week);
			}
		}

		private void transferCapturedModule(Module module, int week)
		{
			this.transferCapturedModule(module, week, this.attacker.Owner);
		}

		private void transferCapturedModule(Module module, int week, Faction recipientOwner)
		{
			ModuleStack source = module.Parent;
			int originalCount = source.Quantity;
			Faction originalOwner = source.Owner;
			if (originalCount < 1 || recipientOwner == null)
			{
				return;
			}
			if (originalOwner == recipientOwner)
			{
				return;
			}

			int index = source.Modules.IndexOf(module);
			if (index < 0)
			{
				return;
			}

			ModuleStack captured = new ModuleStack(source.Parent, recipientOwner, source.ModuleType, this.capturedStackName(source));
			source.RemoveModule(index);
			captured.AddModule(module);

			this.transferProportionalItems(source, captured, 1, originalCount, source.ModuleType.Group == EModuleTypesGroup.command);
			this.transferProportionalNested(source, captured, 1, originalCount, recipientOwner);
			this.transferProportionalPeople(source, captured, 1, originalCount, recipientOwner);

			source.EventReports.Add(week, string.Format("lost {0} to capture.", module.ReportName));
			captured.EventReports.Add(week, string.Format("captured {0} from {1}.", captured.ModuleType.ReportName, source.ReportName));

			if (source.ModuleType.Group == EModuleTypesGroup.command)
			{
				ModuleStack root = source.RootModuleStack;
				if (root.Owner == originalOwner
					&& this.countCommandModules(root, originalOwner) == 0)
				{
					this.captureParentByOwnership(root, recipientOwner);
				}
			}

			if (source.Modules.Count == 0 || source.Quantity < 1)
			{
				this.removeFromBattle(source);
			}
		}

		private int countCommandModules(ModuleStack stack, Faction owner)
		{
			int count = 0;
			if (stack.Owner == owner && stack.ModuleType != null
				&& stack.ModuleType.Group == EModuleTypesGroup.command)
			{
				count += stack.Modules.Count;
			}
			foreach (ModuleStack child in stack.ModuleStacks.Values)
			{
				count += this.countCommandModules(child, owner);
			}
			return count;
		}

		private void captureParentByOwnership(ModuleStack root, Faction recipientOwner)
		{
			this.changeOwnerRecursive(root, recipientOwner);
			string line = string.Format("  {0} captured by {1}.",
				root.ReportName,
				recipientOwner.ReportName);
			this.report(line);
			this.reportObserver(line);
			this.removeFromBattle(root);
		}

		private void changeOwnerRecursive(ModuleStack stack, Faction owner)
		{
			stack.Owner = owner;
			foreach (Person person in stack.People.Values)
			{
				person.Owner = owner;
			}
			foreach (ModuleStack child in stack.ModuleStacks.Values)
			{
				this.changeOwnerRecursive(child, owner);
			}
		}

		private string capturedStackName(ModuleStack source)
		{
			int serial = 1;
			string name = this.formatCapturedName(serial);
			while (ModuleStack.All.ContainsKey(name))
			{
				serial++;
				name = this.formatCapturedName(serial);
			}
			return name;
		}

		private string formatCapturedName(int serial)
		{
			int digits = NamedObject.MaxNameLength - 1;
			return string.Concat("c", serial.ToString().PadLeft(digits, '0'));
		}

		private void transferProportionalItems(ModuleStack source, ModuleStack dest, int taken, int originalCount, bool crewCasualties)
		{
			List<ItemStack> snapshot = new List<ItemStack>();
			foreach (ItemStack itemStack in source.ItemStacks.Values)
			{
				snapshot.Add(itemStack);
			}
			foreach (ItemStack itemStack in snapshot)
			{
				int move = (itemStack.Quantity * taken) / originalCount;
				if (move <= 0)
				{
					continue;
				}
				if (crewCasualties && itemStack.ItemType.Group == EItemTypesGroup.crew)
				{
					this.transferCrewCasualties(source, dest, itemStack.ItemType, move);
					continue;
				}
				source.ItemStacks.Remove(new ItemStack(itemStack.ItemType, move));
				dest.ItemStacks.Add(new ItemStack(itemStack.ItemType, move));
			}
		}

		private void transferCrewCasualties(ModuleStack source, ModuleStack dest, ItemType crewType, int move)
		{
			int killed = move / 4;
			int wounded = move / 2;
			int healthy = move - killed - wounded;
			source.ItemStacks.Remove(new ItemStack(crewType, move));
			if (killed > 0)
			{
				this.report(string.Format("  {0} killed.", new ItemStack(crewType, killed).ReportName));
			}
			if (wounded > 0)
			{
				ItemType woundedType = ItemType.All.ContainsKey("wndtrn") ? ItemType.All["wndtrn"] : crewType;
				dest.ItemStacks.Add(new ItemStack(woundedType, wounded));
				this.report(string.Format("  {0} wounded.", new ItemStack(crewType, wounded).ReportName));
			}
			if (healthy > 0)
			{
				dest.ItemStacks.Add(new ItemStack(crewType, healthy));
				this.report(string.Format("  {0} captured.", new ItemStack(crewType, healthy).ReportName));
			}
		}

		private void transferProportionalNested(ModuleStack source, ModuleStack dest, int taken, int originalCount, Faction recipientOwner)
		{
			List<ModuleStack> nested = new List<ModuleStack>();
			foreach (ModuleStack child in source.ModuleStacks.Values)
			{
				nested.Add(child);
			}
			foreach (ModuleStack child in nested)
			{
				int moveCount = (child.Quantity * taken) / originalCount;
				if (moveCount <= 0)
				{
					continue;
				}
				ModuleStack moved = new ModuleStack(dest, recipientOwner, child.ModuleType);
				for (int i = 0; i < moveCount; i++)
				{
					if (child.Quantity == 0)
					{
						break;
					}
					Module nestedModule = child.RemoveModule(0);
					moved.AddModule(nestedModule);
				}
			}
		}

		private void transferProportionalPeople(ModuleStack source, ModuleStack dest, int taken, int originalCount, Faction recipientOwner)
		{
			People people = source.People;
			int moveCount = (people.Count * taken) / originalCount;
			if (moveCount <= 0)
			{
				return;
			}

			List<Person> toMove = new List<Person>();
			foreach (Person person in people.Values)
			{
				if (toMove.Count >= moveCount)
				{
					break;
				}
				toMove.Add(person);
			}
			toMove.Sort(People.CompareByNames);

			int killed = moveCount / 4;
			int wounded = moveCount / 2;
			Race woundedRace = Race.All.ContainsKey("wndtrn") ? Race.All["wndtrn"] : null;
			int index = 0;
			foreach (Person person in toMove)
			{
				string personName = person.ReportName;
				if (index < killed)
				{
					this.report(string.Format("  {0} is killed.", personName));
					Person.All.Remove(person);
				}
				else
				{
					person.Parent = dest;
					person.Owner = recipientOwner;
					if (index < killed + wounded && woundedRace != null)
					{
						person.Race = woundedRace;
						this.report(string.Format("  {0} is wounded.", personName));
					}
					else
					{
						this.report(string.Format("  {0} is captured.", personName));
					}
				}
				index++;
			}
		}

		private void announceLocationEvent(int week)
		{
			if (this.attacker == null || this.attacker.Location == null)
			{
				return;
			}
			Location location = this.attacker.Location;
			string attackers = this.factionList(this.attackers);
			string defenders = this.factionList(this.defenders);
			string line = string.Format(
				"battle at {0}, attackers: {1}, defenders: {2}.",
				location.ReportName,
				attackers,
				defenders);
			foreach (Faction faction in Faction.All.Values)
			{
				if (location.HasPresence(faction))
				{
					faction.EventReports.Add(week, line);
				}
			}
		}

		private string factionList(ModuleStacks stacks)
		{
			List<string> names = new List<string>();
			foreach (Faction faction in stacks.Owners)
			{
				names.Add(faction.ReportName);
			}
			return string.Join(", ", names.ToArray());
		}

		private void considerRetreat(ModuleStack modulestack)
		{
			if (!modulestack.IsAvoiding)
			{
				return;
			}

			string line;
			if (modulestack.IsImmobile)
			{
				line = string.Concat(modulestack.ReportName, " is immobile and cannot escape.");
			}
			else
			{
				line = string.Concat(modulestack.ReportName, " tries to escape but failed.");
			}
			this.report(line);
		}
	}
}
