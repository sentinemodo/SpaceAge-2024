using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

		public static void Reset()
		{
			Sequence.Reset(1);
		}

		public static void Reset(int seed)
		{
			Sequence.ints.Clear();
			Sequence.Rolls.Clear();
			Sequence.randomGenerator = new Random(seed);
		}

        private static Random randomGenerator = new Random(1);

        public static string GenerateRandomString(int length, string description = "")
        {
            string randomName = string.Empty;
            if (Sequence.Ints.Count == 0)
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
            if (Sequence.Ints.Count == 0)
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
