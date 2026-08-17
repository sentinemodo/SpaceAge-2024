using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public enum ETactic
	{
		avoid,
		destroy,  // basic tactic, random damage
		capture, // 10% HP / 90% capture pool; command+propulsion double size
		evade, // 50% to-hit; command+propulsion half size; leave after two unhit rounds
		disable, // legacy XML name; treated as destroy when firing
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
