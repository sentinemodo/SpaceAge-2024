using System;

namespace SpaceAge
{
	public static class SkillEffects
	{
		public const string MoveDuration = "move duration";
		public const string ProductionDuration = "production duration";
		public const string MassThrust = "mass thrust";
		public const string Capacity = "capacity";
		public const string AgriculturalOutput = "agricultural output";
		public const string ExtractionOutput = "extraction output";

		public static int ApplyDurationPercent(int baseWeeks, int percent)
		{
			if (percent >= 100 || baseWeeks <= 0)
			{
				return baseWeeks;
			}
			return Math.Max(1, (int)Math.Ceiling(baseWeeks * percent / 100.0 - 1e-9));
		}

		public static int ApplyCapacityPercent(int baseCapacity, int percent)
		{
			if (percent <= 100 || baseCapacity <= 0)
			{
				return baseCapacity;
			}
			return (int)Math.Floor(baseCapacity * percent / 100.0);
		}

		public static void ApplyOutputPercent(ItemStacks stacks, int percent)
		{
			if (percent <= 100 || stacks == null)
			{
				return;
			}
			foreach (ItemStack itemStack in stacks.Values)
			{
				itemStack.Quantity = (int)Math.Floor(itemStack.Quantity * percent / 100.0);
			}
		}

		public static int MoveDurationPercent(ModuleStack root)
		{
			int best = 100;
			collectMoveDurationPercent(root, ref best);
			return best;
		}

		private static void collectMoveDurationPercent(ModuleStack stack, ref int best)
		{
			if (stack == null)
			{
				return;
			}

			ModuleStack root = stack.RootModuleStack;
			foreach (Person person in stack.People.Values)
			{
				foreach (Skill skill in person.Skills.Values)
				{
					if (skill.Experience < 1)
					{
						continue;
					}
					SkillType skillType = skill.SkillType;
					if (!skillType.PercentProduces.HasEffect(MoveDuration))
					{
						continue;
					}
					ModuleStack host = person.Parent as ModuleStack;
					if (host == null || !skillType.AppliesTo(host, root))
					{
						continue;
					}
					int percent = skillType.PercentProduces.GetPercent(MoveDuration);
					if (percent < best)
					{
						best = percent;
					}
				}
			}

			foreach (ModuleStack nested in stack.ModuleStacks.Values)
			{
				collectMoveDurationPercent(nested, ref best);
			}
		}

		public static int ProductionDurationPercent(ModuleStack producer)
		{
			int best = 100;
			if (producer == null)
			{
				return best;
			}
			ModuleStack root = producer.RootModuleStack;
			foreach (Person person in producer.People.Values)
			{
				foreach (Skill skill in person.Skills.Values)
				{
					if (skill.Experience < 1)
					{
						continue;
					}
					SkillType skillType = skill.SkillType;
					if (!skillType.PercentProduces.HasEffect(ProductionDuration))
					{
						continue;
					}
					ModuleStack host = person.Parent as ModuleStack;
					if (host == null || !skillType.AppliesTo(host, root))
					{
						continue;
					}
					int percent = skillType.PercentProduces.GetPercent(ProductionDuration);
					if (percent < best)
					{
						best = percent;
					}
				}
			}
			return best;
		}

		public static int MassThrustPercent(ModuleStack root)
		{
			int best = 100;
			collectMassThrustPercent(root, ref best);
			return best;
		}

		private static void collectMassThrustPercent(ModuleStack stack, ref int best)
		{
			if (stack == null || stack.ModuleType == null)
			{
				return;
			}

			ModuleStack root = stack.RootModuleStack;
			if (stack.ModuleType.Group == EModuleTypesGroup.propulsion)
			{
				foreach (Person person in stack.People.Values)
				{
					foreach (Skill skill in person.Skills.Values)
					{
						if (skill.Experience < 1)
						{
							continue;
						}
						SkillType skillType = skill.SkillType;
						if (!skillType.PercentProduces.HasEffect(MassThrust))
						{
							continue;
						}
						ModuleStack host = person.Parent as ModuleStack;
						if (host == null || !skillType.AppliesTo(host, root))
						{
							continue;
						}
						int percent = skillType.PercentProduces.GetPercent(MassThrust);
						if (percent > best)
						{
							best = percent;
						}
					}
				}
			}

			foreach (ModuleStack nested in stack.ModuleStacks.Values)
			{
				collectMassThrustPercent(nested, ref best);
			}
		}

		public static int CapacityPercent(ModuleStack stack)
		{
			int best = 100;
			if (stack == null)
			{
				return best;
			}
			ModuleStack root = stack.RootModuleStack;
			foreach (Person person in stack.People.Values)
			{
				foreach (Skill skill in person.Skills.Values)
				{
					if (skill.Experience < 1)
					{
						continue;
					}
					SkillType skillType = skill.SkillType;
					if (!skillType.PercentProduces.HasEffect(Capacity))
					{
						continue;
					}
					ModuleStack host = person.Parent as ModuleStack;
					if (host == null || !skillType.AppliesTo(host, root))
					{
						continue;
					}
					int percent = skillType.PercentProduces.GetPercent(Capacity);
					if (percent > best)
					{
						best = percent;
					}
				}
			}
			return best;
		}

		public static int ItemOutputPercent(ModuleStack producer)
		{
			if (producer == null || producer.ModuleType == null)
			{
				return 100;
			}

			string effect;
			switch (producer.ModuleType.Group)
			{
				case EModuleTypesGroup.agricultural:
					effect = AgriculturalOutput;
					break;
				case EModuleTypesGroup.extraction:
					effect = ExtractionOutput;
					break;
				default:
					return 100;
			}

			int totalBonus = 0;
			ModuleStack root = producer.RootModuleStack;
			foreach (Person person in producer.People.Values)
			{
				foreach (Skill skill in person.Skills.Values)
				{
					if (skill.Experience < 1)
					{
						continue;
					}
					SkillType skillType = skill.SkillType;
					if (!skillType.PercentProduces.HasEffect(effect))
					{
						continue;
					}
					ModuleStack host = person.Parent as ModuleStack;
					if (host == null || !skillType.AppliesTo(host, root))
					{
						continue;
					}
					totalBonus += skillType.PercentProduces.GetPercent(effect) - 100;
				}
			}
			return 100 + totalBonus;
		}
	}
}
