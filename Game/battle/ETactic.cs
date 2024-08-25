using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public enum ETactic
	{
		avoid,
		destroy,  // basic tactic, random damage
		disable, // targets command, power and propulsion modulestacsk for taking over with limited damged to other module types
		split, // split modules from target modulestacks (in space this usually causes death of the crew)
		disarm,	 // targets military modules first to limit own losses
		conquer, // transfer marines on the other modulestack, to claim ownership of the modulestack
		retreat,
		attackStrongest,
		attackWeakest,
		attackBiggest,
		attackSmallest,
		attackFighters,
		attackFrigates,
		attackShips,
		attackBases,
		closeIn,
		longRange,
		support
	}
}
