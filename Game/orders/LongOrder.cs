using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public abstract class LongOrder : Order
	{
		public LongOrder(IOrderable subject)
			: base(subject)
		{
		}

        protected int durationInitial;
        public virtual int DurationInitial
        {
            get { return this.durationInitial; }
            set { this.durationInitial = value; }
        }

		protected int durationLeft = Int32.MaxValue;
		public virtual int DurationLeft
		{
			get { return this.durationLeft; }
			set { this.durationLeft = value; }
		}

		private int left;
		public int Left
		{
			get { return this.left; }
			set { this.left = value; }
		}

		public override void Execute(int week)
		{
			this.Subject.ExecutedLongOrder = true;
			if (this.DurationLeft <= 0)
			{
				this.durationLeft = Int32.MaxValue;
				this.Executing = false;
				this.Repeat--;
			}

			if (this.Repeat == 0)
			{
				this.Executed = true;
			}
		}

		public bool CanOperate(int week)
		{
			if (this.Subject is ModuleStack)
			{
				ModuleStack moduleStack = (ModuleStack)this.Subject;
				if (!moduleStack.HasOperationalModules)
				{
					if (moduleStack.ModuleType == null)
						moduleStack.EventReports.Add(
								week,
								string.Format("{0} failed: {1} is not formed yet.",
										this.type.ToString().ToUpper(),
										moduleStack.ReportName));
					if (moduleStack.CrewRequired + moduleStack.ModuleStacks.CrewRequired() > moduleStack.CrewCurrent + moduleStack.ModuleStacks.CrewCurrent())
						moduleStack.EventReports.Add(
								week,
								string.Format("{0} failed: {1} has not enough crew to operate - {2} required, {3} available.",
										this.type.ToString().ToUpper(),
										moduleStack.ReportName,
										moduleStack.CrewRequired + moduleStack.ModuleStacks.CrewRequired(),
										moduleStack.CrewCurrent + moduleStack.ModuleStacks.CrewCurrent()));
					ModuleStack energyRoot = moduleStack.RootModuleStack;
					int energyRequired = moduleStack.EnergyRequired + moduleStack.ModuleStacks.EnergyRequired();
					int energyAvailable = energyRoot.EnergyProduction + energyRoot.ModuleStacks.EnergyProduction();
					if (energyRequired > energyAvailable)
					{
						moduleStack.EventReports.Add(
								week,
								string.Format("{0} failed: {1} has not enough energy to operate - {2} required, {3} available.",
										this.type.ToString().ToUpper(),
										moduleStack.ReportName,
										energyRequired,
										energyAvailable));
					}
					return false;
				}
				if (moduleStack.ModuleType.OperationCondition_LocationTypes.Count > 0)
				{
					if (!moduleStack.ModuleType.OperationCondition_LocationTypes.Contains(BodyEnvironment.EffectiveLocationType(moduleStack.Location)))
					{
						moduleStack.EventReports.Add(
						week,
						string.Format("{0} failed: {1} cannot operate in {2}.",
								this.type.ToString().ToUpper(),
								moduleStack.ModuleType.ReportName,
								moduleStack.Location.ReportName));
						return false;
					}
				}
				if (moduleStack.ModuleType.OperationCondition_AtmosphereResources.Count > 0
					&& !BodyEnvironment.HasAtmosphereResources(moduleStack.Location, moduleStack.ModuleType.OperationCondition_AtmosphereResources))
				{
					moduleStack.EventReports.Add(
						week,
						string.Format("{0} failed: {1} cannot operate in {2}.",
								this.type.ToString().ToUpper(),
								moduleStack.ModuleType.ReportName,
								moduleStack.Location.ReportName));
					return false;
				}
				if (moduleStack.ModuleType.OperationCondition_PlanetTypes.Count > 0
					&& !BodyEnvironment.MatchesPlanetType(moduleStack.Location, moduleStack.ModuleType.OperationCondition_PlanetTypes))
				{
					moduleStack.EventReports.Add(
						week,
						string.Format("{0} failed: {1} cannot operate in {2}.",
								this.type.ToString().ToUpper(),
								moduleStack.ModuleType.ReportName,
								moduleStack.Location.ReportName));
					return false;
				}
			}
			return true;
		}
	}
}
