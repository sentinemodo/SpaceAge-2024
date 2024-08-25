using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
    public interface IOrbitHolder : ILocationsHolder, ISingular, IBattleReporting
	{
		Orbit Orbit { get; }
        double DistanceTo(SpaceSystemObject spaceSystemObject);
        double DistanceTo(Orbit orbit);
	}
}
