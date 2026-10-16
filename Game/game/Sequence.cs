using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{

	public class Sequence
	{
        public class RollDescription
        {
            public int Roll { get; set; }
            public string Description { get; set; }

            public RollDescription(int roll, string description)
            { this.Roll = roll; this.Description = description; }

        }

        public static List<RollDescription> Rolls = new List<RollDescription>();

        private static Stack<int> ints = new Stack<int>();
        public static Stack<int> Ints
		{
			get { return Sequence.ints; }
		}

		/// <summary>Test-injected rolls consumed only during an active battle scope.</summary>
		private static Stack<int> battleInts = new Stack<int>();
		public static Stack<int> BattleInts
		{
			get { return Sequence.battleInts; }
		}

		private static Stack<int> savedOutsideInts;
		private static Random savedOutsideRandom;
		private static Stack<int> battleRollInts;
		private static Random battleRandom;
		private static bool battleScopeActive;

		public static void Reset()
		{
			Sequence.Reset(1);
		}

		public static void Reset(int seed)
		{
			Sequence.ints.Clear();
			Sequence.battleInts.Clear();
			Sequence.Rolls.Clear();
			Sequence.randomGenerator = new Random(seed);
			Sequence.ExitBattleScope();
		}

        private static Random randomGenerator = new Random(1);

		public static int ComputeBattleSeed(int week, string locationName, string attackerStackName, string defenderStackName)
		{
			unchecked
			{
				int hash = 5381;
				hash = ((hash << 5) + hash) ^ week;
				hash = ((hash << 5) + hash) ^ (locationName ?? string.Empty).GetHashCode();
				hash = ((hash << 5) + hash) ^ (attackerStackName ?? string.Empty).GetHashCode();
				hash = ((hash << 5) + hash) ^ (defenderStackName ?? string.Empty).GetHashCode();
				if (hash == 0)
				{
					hash = 1;
				}
				return Math.Abs(hash);
			}
		}

		public static void EnterBattleScope(int seed)
		{
			if (Sequence.battleScopeActive)
			{
				throw new InvalidOperationException("Battle RNG scope is already active.");
			}

			Sequence.savedOutsideInts = Sequence.CloneStack(Sequence.ints);
			Sequence.savedOutsideRandom = Sequence.randomGenerator;
			Sequence.battleRollInts = Sequence.CloneStack(Sequence.battleInts);
			Sequence.battleInts.Clear();
			Sequence.battleScopeActive = true;

			if (Sequence.battleRollInts.Count > 0)
			{
				Sequence.battleRandom = Sequence.savedOutsideRandom;
			}
			else if (Sequence.savedOutsideInts.Count == 0)
			{
				Sequence.battleRandom = Sequence.savedOutsideRandom;
			}
			else
			{
				Sequence.battleRandom = new Random(seed);
			}
		}

		public static void ExitBattleScope()
		{
			if (!Sequence.battleScopeActive)
			{
				return;
			}

			Sequence.ints = Sequence.CloneStack(Sequence.savedOutsideInts);
			Sequence.randomGenerator = Sequence.savedOutsideRandom;
			Sequence.battleRollInts = null;
			Sequence.battleRandom = null;
			Sequence.savedOutsideInts = null;
			Sequence.savedOutsideRandom = null;
			Sequence.battleScopeActive = false;
		}

		private static Stack<int> CloneStack(Stack<int> source)
		{
			Stack<int> clone = new Stack<int>();
			if (source == null || source.Count == 0)
			{
				return clone;
			}

			int[] items = source.ToArray();
			for (int i = items.Length - 1; i >= 0; i--)
			{
				clone.Push(items[i]);
			}
			return clone;
		}

        public static string GenerateRandomString(int length, string description = "")
        {
            string randomName = string.Empty;
            if (Sequence.battleScopeActive)
            {
				if (Sequence.battleRollInts != null && Sequence.battleRollInts.Count > 0)
				{
					randomName = Sequence.battleRollInts.Pop().ToString();
				}
				else
				{
					for (int i = 0; i < length; i++)
					{
						randomName = string.Concat(randomName + Sequence.battleRandom.Next(0, 10));
					}
				}
            }
            else if (Sequence.Ints.Count == 0)
            {
                for (int i = 0; i < length; i++)
                {
                    randomName = string.Concat(randomName + Sequence.randomGenerator.Next(0, 10));
                }
            }
            else
            {
                randomName = Sequence.Ints.Pop().ToString();
            }

            Sequence.Rolls.Add(new RollDescription(Convert.ToInt32(randomName), description));

            return randomName;
        }

        public static int GenerateRandomInt(int minimum, int maximum, string description = "")
        {
            int randomInt = 0;
            if (Sequence.battleScopeActive)
            {
				if (Sequence.battleRollInts != null && Sequence.battleRollInts.Count > 0)
				{
					randomInt = Sequence.battleRollInts.Pop();
				}
				else
				{
					randomInt = Sequence.battleRandom.Next(minimum, maximum);
				}
            }
            else if (Sequence.Ints.Count == 0)
            {
                randomInt = Sequence.randomGenerator.Next(minimum, maximum);
            }
            else
            {
                randomInt = Sequence.Ints.Pop();
            }

            Sequence.Rolls.Add(new RollDescription(randomInt, description));

            return randomInt;
        }

    }
}
