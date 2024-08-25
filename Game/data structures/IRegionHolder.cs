using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
    public interface IRegionHolder : ILocationsHolder, IBattleReporting, ISingular
	{
		Regions Regions { get; }
	}
}
