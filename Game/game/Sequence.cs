using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{

	public class Sequence
	{
		// TODO: add random class to the game, to have a single randomizer and a single method to call a seqeuncer and turn rerun seeds
		private static Stack<int> ints = new Stack<int>();
		public static Stack<int> Ints
		{
			get { return Sequence.ints; }
		}
	}	
}
