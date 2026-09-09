using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SpaceAge
{
	public class SkillBonusFormula
	{
		private static readonly HashSet<string> ValidTokens = new HashSet<string>
		{
			"stack-attack",
			"stack-defense",
			"stack-initiative",
			"lab-output",
			"module-attack",
			"module-defense",
			"units",
		};

		private readonly List<Term> terms = new List<Term>();

		private struct Term
		{
			public int Literal;
			public int Percent;
			public string Token;
			public bool IsPercent;
		}

		public bool IsNonZero
		{
			get
			{
				foreach (Term term in this.terms)
				{
					if (term.Literal != 0 || term.IsPercent)
					{
						return true;
					}
				}
				return false;
			}
		}

		public static SkillBonusFormula Zero
		{
			get { return new SkillBonusFormula(); }
		}

		public static SkillBonusFormula Parse(string text)
		{
			SkillBonusFormula formula = new SkillBonusFormula();
			if (string.IsNullOrEmpty(text))
			{
				return formula;
			}

			text = text.Trim();
			if (Regex.IsMatch(text, @"^-?\d+$"))
			{
				formula.terms.Add(new Term { Literal = int.Parse(text) });
				return formula;
			}

			string[] parts = text.Split('+');
			for (int i = 0; i < parts.Length; i++)
			{
				string part = parts[i].Trim();
				if (part == string.Empty)
				{
					continue;
				}

				Match percentMatch = Regex.Match(part, @"^(-?\d+)%(.+)$");
				if (percentMatch.Success)
				{
					string token = percentMatch.Groups[2].Value.Trim();
					SkillBonusFormula.validateToken(token);
					formula.terms.Add(new Term
					{
						Percent = int.Parse(percentMatch.Groups[1].Value),
						Token = token,
						IsPercent = true,
					});
					continue;
				}

				Match multiplyMatch = Regex.Match(part, @"^(-?\d+)×(.+)$");
				if (multiplyMatch.Success)
				{
					string token = multiplyMatch.Groups[2].Value.Trim();
					SkillBonusFormula.validateToken(token);
					formula.terms.Add(new Term
					{
						Literal = int.Parse(multiplyMatch.Groups[1].Value),
						Token = token,
						IsPercent = false,
					});
					continue;
				}

				if (Regex.IsMatch(part, @"^-?\d+$"))
				{
					formula.terms.Add(new Term { Literal = int.Parse(part) });
					continue;
				}

				throw new Exception("Invalid skill bonus formula part: " + part);
			}

			return formula;
		}

		private static void validateToken(string token)
		{
			if (!ValidTokens.Contains(token))
			{
				throw new Exception("Unknown skill formula scale token: " + token);
			}
		}

		public int Evaluate(ModuleStack host, ModuleStack root)
		{
			int total = 0;
			foreach (Term term in this.terms)
			{
				if (term.IsPercent)
				{
					int scale = SkillFormationStats.ScaleValue(term.Token, host, root);
					total += term.Percent * scale / 100;
				}
				else if (term.Token != null)
				{
					total += term.Literal * SkillFormationStats.ScaleValue(term.Token, host, root);
				}
				else
				{
					total += term.Literal;
				}
			}
			return total;
		}
	}
}
