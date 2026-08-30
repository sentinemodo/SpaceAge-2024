using System;

namespace SpaceAge
{
	public static class SkillFormationStats
	{
		public static int ScaleValue(string token, ModuleStack host, ModuleStack root)
		{
			if (root == null)
			{
				return 0;
			}

			switch (token)
			{
				case "stack-attack":
					return FormationBaseAttack(root);
				case "stack-defense":
					return FormationBaseDefense(root);
				case "stack-initiative":
					return FormationBaseInitiative(root);
				case "lab-output":
					return LabOutput(root);
				case "module-attack":
					return root.ModuleType != null ? root.ModuleType.Attack : 0;
				case "module-defense":
					return root.ModuleType != null ? root.ModuleType.Defense : 0;
				case "units":
					return root.QuantityActive;
				default:
					throw new Exception("Unknown skill formula scale token: " + token);
			}
		}

		public static int FormationBaseAttack(ModuleStack root)
		{
			int attack = 0;
			collectFormationBaseAttack(root, ref attack);
			return attack;
		}

		private static void collectFormationBaseAttack(ModuleStack stack, ref int attack)
		{
			if (stack.IsFormed && stack.ModuleType != null)
			{
				attack += stack.QuantityActive * stack.ModuleType.Attack;
				attack += stack.Technologies.Attack;
				attack += stack.ItemStacks.CombatAttack(stack.ModuleType.Group, stack.QuantityActive);
			}

			foreach (ModuleStack nested in stack.ModuleStacks.Values)
			{
				collectFormationBaseAttack(nested, ref attack);
			}
		}

		public static int FormationBaseDefense(ModuleStack root)
		{
			int defense = 0;
			collectFormationBaseDefense(root, ref defense);
			return defense;
		}

		private static void collectFormationBaseDefense(ModuleStack stack, ref int defense)
		{
			if (stack.IsFormed && stack.ModuleType != null)
			{
				defense += stack.QuantityActive * stack.ModuleType.Defense;
				defense += stack.Technologies.Defense;
				defense += stack.ItemStacks.CombatDefense(stack.ModuleType.Group, stack.QuantityActive);
			}

			foreach (ModuleStack nested in stack.ModuleStacks.Values)
			{
				collectFormationBaseDefense(nested, ref defense);
			}
		}

		public static int FormationBaseInitiative(ModuleStack root)
		{
			int initiative = 0;
			collectFormationBaseInitiative(root, ref initiative);
			return initiative;
		}

		private static void collectFormationBaseInitiative(ModuleStack stack, ref int initiative)
		{
			if (stack.IsFormed && stack.ModuleType != null)
			{
				initiative += stack.ModuleType.Initiative;
				initiative += stack.Technologies.Initiative;
				initiative += stack.ItemStacks.CombatInitiative(stack.ModuleType.Group, stack.QuantityActive);
			}

			foreach (ModuleStack nested in stack.ModuleStacks.Values)
			{
				collectFormationBaseInitiative(nested, ref initiative);
			}
		}

		public static int LabOutput(ModuleStack root)
		{
			if (root == null || root.ModuleType == null)
			{
				return 0;
			}
			return root.ModuleType.ResearchOutput * root.Modules.Count;
		}
	}
}
