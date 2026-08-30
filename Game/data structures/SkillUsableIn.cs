using System;

namespace SpaceAge
{
	public class SkillUsableIn
	{
		public EModuleTypesGroup? ModuleGroup { get; set; }

		public int? ModuleStackSize { get; set; }

		public bool Matches(ModuleStack root)
		{
			if (root == null || root.ModuleType == null)
			{
				return false;
			}

			if (this.ModuleGroup.HasValue && root.ModuleType.Group != this.ModuleGroup.Value)
			{
				return false;
			}

			if (this.ModuleStackSize.HasValue && root.QuantityActive != this.ModuleStackSize.Value)
			{
				return false;
			}

			return true;
		}
	}
}
