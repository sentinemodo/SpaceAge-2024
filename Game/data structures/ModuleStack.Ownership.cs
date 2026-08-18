namespace SpaceAge
{
	public partial class ModuleStack
	{
		public void SetOwnerRecursive(Faction newOwner)
		{
			this.owner = newOwner;
			foreach (ModuleStack child in this.ModuleStacks.Values)
			{
				child.SetOwnerRecursive(newOwner);
			}
		}

		public Technologies CollectTechnologiesRecursive()
		{
			Technologies collected = new Technologies();
			foreach (Technology technology in this.technologies)
			{
				if (!collected.Contains(technology.Name))
				{
					collected.Add(technology);
				}
			}
			foreach (ModuleStack child in this.ModuleStacks.Values)
			{
				foreach (Technology technology in child.CollectTechnologiesRecursive())
				{
					if (!collected.Contains(technology.Name))
					{
						collected.Add(technology);
					}
				}
			}
			return collected;
		}
	}
}
