using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Race : NamedObject, IMultiple
	{

		public static Races All = new Races();

		public Race(string name)
			: base(name) 
		{
            if (Race.All.ContainsKey(name))
                throw new Exception("Race with name [" + name + "] already exists");

            Race.All.Add(this.name, this);
		}

		public ItemType ItemType
		{
			get { return ItemType.All[this.name]; }
		}

		private int officerTrainingDuration = 0;
		public int OfficerTrainingDuration
		{
			get { return this.officerTrainingDuration; }
			set { this.officerTrainingDuration = value; }
		}

		private double mass;
		public double Mass
		{
			get { return this.mass; }
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				this.mass = value;
			}
		}

		private double size;
		public double Size
		{
			get { return this.size; }
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				this.size = value;
			}
		}

		private double capacity;
		public double Capacity
		{
			get { return this.capacity; }
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				this.capacity = value;
			}
		}

		private ItemStacks upkeep = new ItemStacks();
		public ItemStacks Upkeep
		{
			get { return this.upkeep; }
		}

		private ItemStacks consume = new ItemStacks();
		public ItemStacks Consume
		{
			get { return this.consume; }
		}

		public string NoConsumeEffect { get; set; }
		public int NoConsumeChance { get; set; }
		public string NoUpkeepEffect { get; set; }
		public int NoUpkeepChance { get; set; }

		protected string fullNameMultiple = string.Empty;
		public string FullNameMultiple
		{
			get { return this.fullNameMultiple; }
			set { this.fullNameMultiple = value; }
		}

		public string ReportNameMultiple
		{
			get { return this.fullNameMultiple + " [" + this.name + "]"; }
		}

        #region movement
        private MoveModes moveModes = new MoveModes();
        public MoveModes MoveModes
        {
            get { return this.moveModes; }
        }
        #endregion

	}
}
