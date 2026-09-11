using System.Collections.Generic;

namespace SpaceAge
{
	public class BattleSimItemTemplate
	{
		public string Type;
		public int Quantity = 1;
	}

	public class BattleSimStackTemplate
	{
		public string Id;
		public string Type;
		public string Name;
		public int Quantity = 1;
		public string Tactic = "destroy";
		public List<BattleSimItemTemplate> Items = new List<BattleSimItemTemplate>();
		public List<BattleSimStackTemplate> Nested = new List<BattleSimStackTemplate>();
	}

	public class BattleSimTemplate
	{
		public string Id;
		public string DisplayName;
		public string Role;
		public BattleSimStackTemplate Stack;
	}
}
