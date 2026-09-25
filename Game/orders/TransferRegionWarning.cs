using System;

namespace SpaceAge
{
	public static class TransferRegionWarning
	{
		public static void WarnIfOtherRegion(
			IItemStacksHolder subject,
			IItemStacksHolder other,
			string otherRole)
		{
			if (subject == null || other == null || !subject.IsFormed || IsNewAlias(other))
			{
				return;
			}

			Location here = subject.Location;
			if (here == null)
			{
				return;
			}

			if (other.IsFormed)
			{
				Location there = other.Location;
				if (there != null && there == here)
				{
					return;
				}
			}

			Faction owner = OwnerOf(subject);
			if (owner == null)
			{
				return;
			}

			owner.EventReports.Add(string.Format(
				"WARNING: {0} unit [{1}] is not in this region.",
				otherRole,
				other.Name));
		}

		private static Faction OwnerOf(IItemStacksHolder holder)
		{
			ModuleStack stack = holder as ModuleStack;
			if (stack != null)
			{
				return stack.Owner;
			}

			Person person = holder as Person;
			if (person != null)
			{
				return person.Owner;
			}

			return null;
		}

		private static bool IsNewAlias(IItemStacksHolder holder)
		{
			if (holder.IsFormed)
			{
				return false;
			}
			string alias = holder.Alias ?? string.Empty;
			int split = alias.IndexOf('_');
			string raw = split >= 0 ? alias.Substring(split + 1) : alias;
			return raw.StartsWith("new");
		}
	}
}
