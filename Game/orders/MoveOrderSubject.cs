using System;

namespace SpaceAge
{
	internal sealed class MoveOrderSubject
	{
		private static readonly ItemStacks EmptyFuel = new ItemStacks();

		private readonly ModuleStack stack;
		private readonly Person person;

		private MoveOrderSubject(ModuleStack moduleStack)
		{
			this.stack = moduleStack;
		}

		private MoveOrderSubject(Person personSubject)
		{
			this.person = personSubject;
		}

		public static MoveOrderSubject From(IOrderable orderable)
		{
			ModuleStack moduleStack = orderable as ModuleStack;
			if (moduleStack != null)
			{
				return new MoveOrderSubject(moduleStack);
			}

			Person personSubject = orderable as Person;
			if (personSubject != null)
			{
				return new MoveOrderSubject(personSubject);
			}

			throw new InvalidOperationException("MOVE applies to module stacks or persons only.");
		}

		public bool IsPerson
		{
			get { return this.person != null; }
		}

		public ModuleStack Stack
		{
			get { return this.stack; }
		}

		public Person Person
		{
			get { return this.person; }
		}

		public IEffectable Effectable
		{
			get { return (IEffectable)(this.stack ?? (object)this.person); }
		}

		public IMoveable Moveable
		{
			get { return (IMoveable)(this.stack ?? (object)this.person); }
		}

		public Faction Owner
		{
			get { return this.stack != null ? this.stack.Owner : this.person.Owner; }
		}

		public Effects Effects
		{
			get { return this.Effectable.Effects; }
		}

		public EventReports EventReports
		{
			get
			{
				if (this.stack != null)
				{
					return this.stack.EventReports;
				}
				return this.person.EventReports;
			}
		}

		public Location Location
		{
			get
			{
				if (this.stack != null)
				{
					return this.stack.Location;
				}
				return this.person.Location;
			}
		}

		public IHolder Parent
		{
			get
			{
				if (this.stack != null)
				{
					return this.stack.Parent;
				}
				return this.person.Parent;
			}
			set
			{
				if (this.stack != null)
				{
					this.stack.Parent = value;
				}
				else
				{
					this.person.Parent = value;
				}
			}
		}

		public IHolder MovingTo
		{
			get
			{
				if (this.stack != null)
				{
					return this.stack.MovingTo;
				}
				return this.person.MovingTo;
			}
			set
			{
				if (this.stack != null)
				{
					this.stack.MovingTo = value;
				}
				else
				{
					this.person.MovingTo = value;
				}
			}
		}

		public bool IsInTransit
		{
			get
			{
				if (this.stack != null)
				{
					return this.stack.IsInTransit;
				}
				return this.person.IsInTransit;
			}
		}

		public string ReportName
		{
			get
			{
				if (this.stack != null)
				{
					return this.stack.ReportName;
				}
				return this.person.ReportName;
			}
		}

		public bool IsFormed
		{
			get
			{
				if (this.stack != null)
				{
					return this.stack.IsFormed;
				}
				return this.person.IsFormed;
			}
		}

		public bool IsActive
		{
			get
			{
				if (this.stack != null)
				{
					return this.stack.IsActive;
				}
				return this.person.IsMoveActive;
			}
		}

		public bool IsPartiallyDisabled
		{
			get { return this.stack != null && this.stack.IsPartiallyDisabled; }
		}

		public bool IsLivingUnit
		{
			get { return this.stack != null && this.stack.IsLivingUnit; }
		}

		public MoveModes MoveModes
		{
			get { return this.Moveable.MoveModes; }
		}

		public ItemStacks Fuel
		{
			get
			{
				ItemStacks fuel = this.Moveable.Fuel;
				if (fuel == null)
				{
					return EmptyFuel;
				}
				return fuel;
			}
		}

		public ModuleType ModuleType
		{
			get { return this.stack != null ? this.stack.ModuleType : null; }
		}

		public ModuleStack RootModuleStack
		{
			get { return this.stack != null ? this.stack.RootModuleStack : null; }
		}

		public bool HasItemsAvailableTo(ModuleStack receiver, ItemStacks need)
		{
			if (this.stack == null)
			{
				return false;
			}
			return this.stack.HasItemsAvailableTo(receiver, need);
		}

		public IItemStacksHolder FindItemHolderAvailableTo(ModuleStack receiver, ItemStacks need)
		{
			if (this.stack == null)
			{
				return null;
			}
			return this.stack.FindItemHolderAvailableTo(receiver, need);
		}

		public void CancelDeliveringPurchases(int week)
		{
			if (this.stack != null)
			{
				DeliveringPurchase.CancelAll(this.stack, week);
				DeliveringPurchase.CancelReturns(this.stack, week);
			}
		}

		public void PrepareFootDeparture(int week)
		{
			if (this.person == null || this.person.IsInTransit)
			{
				return;
			}

			ModuleStack host = this.person.Parent as ModuleStack;
			if (host == null)
			{
				return;
			}

			Location region = host.Location;
			if (region == null)
			{
				return;
			}

			this.person.EventReports.Add(
				week,
				string.Format("left {0} on foot.", host.ReportName));
			this.person.Parent = region;
		}

		public override string ToString()
		{
			return this.ReportName;
		}
	}
}
