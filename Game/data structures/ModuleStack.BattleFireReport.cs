using System;

namespace SpaceAge
{
	public partial class ModuleStack
	{
		public bool UsesCompositeBattleFireReporting()
		{
			foreach (ModuleStack modulestack in this.ModuleStacks.Values)
			{
				if (modulestack.IsHangarCraft)
				{
					continue;
				}
				if (modulestack.ModuleType != null && modulestack.ModuleType.IsDroneBay)
				{
					continue;
				}
				if (modulestack.GetFiringModules().Count > 0)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasCustomBattleFireName()
		{
			return !string.IsNullOrEmpty(this.FullName);
		}

		public bool UsesPluralBattleFireOpening()
		{
			return !this.HasCustomBattleFireName() && this.QuantityActive > 1;
		}

		public bool UsesSingleModuleBattleDetail()
		{
			return this.IsFormed && this.modules.Count == 1;
		}

		/// <summary>
		/// Flat root target already named in the fire line (no nested modulestacks, hit label equals stack name).
		/// Multi-module stacks keep #N in hit lines even when only one module remains.
		/// </summary>
		public bool UsesSimplifiedHitLocationReportingFor(Module hitModule)
		{
			if (hitModule == null || hitModule.Parent != this || this.ModuleStacks.Count > 0)
			{
				return false;
			}
			string label = hitModule.ReportHitLocationLabel;
			if (label.StartsWith("#", StringComparison.Ordinal))
			{
				return false;
			}
			return label == this.ReportName;
		}

		public string BattleFireAttackerPhrase()
		{
			if (this.HasCustomBattleFireName())
			{
				return this.ReportName;
			}
			if (this.QuantityActive > 1 && this.moduleType != null)
			{
				string pluralLabel = string.IsNullOrEmpty(this.moduleType.FullNameMultiple)
					? this.moduleType.FullName
					: this.moduleType.FullNameMultiple;
				return string.Format("{0} {1} [{2}]",
					this.QuantityActive,
					pluralLabel,
					this.name);
			}
			return this.ReportName;
		}

		public string BattleFireSubjectVerbOn()
		{
			bool plural = this.QuantityActive > 1;
			string phrase = this.BattleFireAttackerPhrase();
			if (this.HasCustomBattleFireName())
			{
				return plural
					? string.Concat(phrase, " fire on")
					: string.Concat(phrase, " fires on");
			}
			if (plural)
			{
				return string.Concat(phrase, " fire on");
			}
			if (this.IsLivingUnit)
			{
				return string.Concat(phrase, " attacks");
			}
			return string.Concat(phrase, " fires on");
		}

		public string FormatBattleFireOpening(ModuleStack target, string weaponName)
		{
			if (this.UsesCompositeBattleFireReporting())
			{
				return string.Format("{0} fires {1} on {2}",
					this.ReportName,
					weaponName,
					target.ReportName);
			}
			return string.Format("{0} {1}",
				this.BattleFireSubjectVerbOn(),
				target.ReportName);
		}

		public string AppendSingleModuleBattleDetailToStatsLine(string line)
		{
			if (!this.UsesSingleModuleBattleDetail())
			{
				return line;
			}
			Module module = this.modules[0];
			if (module.CaptureDamage > 0)
			{
				line = string.Format("{0}, capture: {1}", line, module.CaptureDamage);
			}
			if (module.DamageStatus != EDamageStatus.undamaged)
			{
				line = string.Format("{0}, {1}", line, module.ReportDamage);
			}
			if (!module.IsActive)
			{
				line = string.Format("{0}, {1}", line, module.ReportActive);
			}
			else if (!this.IsModuleOperational(module))
			{
				line = string.Concat(line, ", inactive");
			}
			return line;
		}
	}
}
