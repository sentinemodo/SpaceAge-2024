using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Module : IEffectable
	{
		public Module(ModuleStack parent)
		{
			this.parent = parent;
			this.damage = 0;
			this.online = true;
		}

		public Module(ModuleStack parent, int damage)
		{
			this.parent = parent;
			this.damage = damage;
			this.online = true;
		}

		private ModuleStack parent;
		public ModuleStack Parent
		{
			get { return this.parent; }
			set { this.parent = value; }
		}

		public string ReportID
		{
			get
			{
				return string.Concat("#", this.id);
			}
		}

		public string ReportName
		{
			get
			{
				return string.Concat(this.ReportID, " ", this.parent.ModuleType.ReportName);
			}
		}

		public int id
		{
			get
			{
				int i = 0;
				foreach (Module module in this.Parent.Modules)
				{
					i++;
					if (this is Module)
					{
						return i;
					}
				}
				throw new Exception("The module is not in the parent collection. This should not ever happen.");
			}
		}

		private int damage;
		public int Damage
		{
			get { return this.damage; }
			set 
			{
				this.damage = value;			
			}
		}

		private int hitPoints
		{
			get { return this.parent.ModuleType.DamageCapacity; }
		}

		public int HitPoints
		{
			get { return this.hitPoints; }
		}

		private int captureDamage;
		public int CaptureDamage
		{
			get { return this.captureDamage; }
			set { this.captureDamage = value; }
		}

		public bool HasPersistedState
		{
			get { return this.damage > 0 || this.captureDamage > 0 || !this.online; }
		}

		public bool IsWrecked
		{
			get { return this.Damage >= this.HitPoints; }
		}

		public bool IsCaptureComplete
		{
			get { return !this.IsWrecked && (this.Damage + this.CaptureDamage) >= this.HitPoints; }
		}

		public EDamageStatus DamageStatus
		{
			get
			{
				EDamageStatus status = EDamageStatus.undamaged;
				if (this.damage >= this.hitPoints) 
				{ 
					status = EDamageStatus.destroyed;
				} else if (this.damage * 4 > this.hitPoints * 3)
				{
					status = EDamageStatus.criticallyDamaged;
				} else if (this.damage * 2 > this.hitPoints) 
				{ 
					status = EDamageStatus.heavilyDamaged;
				}
				else if (this.damage * 4 > this.hitPoints)
				{
					status = EDamageStatus.lightlyDamaged;
				}
				return status;
			}
		}

		public string ReportDamage
		{
			get
			{
				string line = string.Empty;
				switch (this.DamageStatus)
				{
					case EDamageStatus.undamaged:
						line = "not damaged";
						break;
					case EDamageStatus.lightlyDamaged:
						line = "lightly damaged";
						break;
					case EDamageStatus.heavilyDamaged:
						line = "heavily damaged";
						break;
					case EDamageStatus.criticallyDamaged:
						line = "critically damaged";
						break;
					case EDamageStatus.destroyed:
						line = "destroyed";
						break;
				}
				return line;
			}
		}

		private bool online;
		public bool Online
		{
			get { return this.online; }
			set { this.online = value; }
		}

		public bool IsActive
		{
			get
			{
				// the individual modules will be switched off on missing crew or energy
				if (!this.online)
					return false;
				if (this.DamageStatus == EDamageStatus.heavilyDamaged |
					this.DamageStatus == EDamageStatus.criticallyDamaged | 
					this.DamageStatus == EDamageStatus.destroyed)
					return false;
				return true;
			}
		}

		public string ReportActive
		{
			get
			{
				string line = string.Empty;
				if (!this.online)
				{
					line = "deactivated";
				} else if (this.IsActive)
				{
					line = "active";
				} else 
				{
					line = "disabled";					
				}
				return line;
			}
		}

		#region IEffectable Members

		private Effects effects = new Effects();
		public Effects Effects
		{
			get { return this.effects; }
		}

		#endregion
	}
}
