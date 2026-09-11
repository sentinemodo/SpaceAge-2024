using System;
using System.Collections.Generic;

namespace SpaceAge
{
	public static class OrderFactory
	{
		private delegate Order Factory(IOrderable subject);

		private static readonly Dictionary<string, Factory> ByVerb = new Dictionary<string, Factory>(StringComparer.OrdinalIgnoreCase)
		{
			{ "active", subject => new ActiveOrder(subject) },
			{ "alias", subject => new AliasOrder(subject) },
			{ "attack", subject => new AttackOrder(subject) },
			{ "capture", subject => new CaptureOrder(subject) },
			{ "buy", subject => new BuyOrder(subject) },
			{ "copy", subject => new CopyOrder(subject) },
			{ "contract", subject => new ContractOrder(subject) },
			{ "press", subject => new PressOrder(subject) },
			{ "declare", subject => new DeclareOrder(subject) },
			{ "deposit", subject => new DepositOrder(subject) },
			{ "form", subject => new FormOrder(subject) },
			{ "get", subject => new GetOrder(subject) },
			{ "give", subject => new GiveOrder(subject) },
			{ "has", subject => new HasOrder(subject) },
			{ "jump", subject => new JumpOrder(subject) },
			{ "move", subject => new MoveOrder(subject) },
			{ "name", subject => new NameOrder(subject) },
			{ "produce", subject => new ProduceOrder(subject) },
			{ "repair", subject => new RepairOrder(subject) },
			{ "research", subject => new ResearchOrder(subject) },
			{ "see", subject => new SeeOrder(subject) },
			{ "sell", subject => new SellOrder(subject) },
			{ "set", subject => new SetOrder(subject) },
			{ "stack", subject => new StackOrder(subject) },
			{ "tactic", subject => new TacticOrder(subject) },
			{ "train", subject => new TrainOrder(subject) },
			{ "transfer", subject => new TransferOrder(subject) },
			{ "use", subject => new UseOrder(subject) },
			{ "withdraw", subject => new WithdrawOrder(subject) },
		};

		public static string NormalizeTextVerb(string token)
		{
			return token.TrimStart('@', '+', '-').ToLowerInvariant();
		}

		public static bool IsKnownVerb(string verb)
		{
			return ByVerb.ContainsKey(verb);
		}

		public static Order Create(string verb, IOrderable subject)
		{
			Factory factory;
			if (!ByVerb.TryGetValue(verb, out factory))
			{
				throw new Exception("Unknown order. " + verb);
			}
			return factory(subject);
		}

		public static Order CreateFromTextToken(string token, IOrderable subject, string fullCommand)
		{
			string verb = NormalizeTextVerb(token);
			Factory factory;
			if (!ByVerb.TryGetValue(verb, out factory))
			{
				throw new Exception("Unknown order: " + fullCommand);
			}
			return factory(subject);
		}
	}
}
