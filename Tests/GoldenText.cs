using System;
using System.Text;
using SpaceAge;

namespace UnitTests
{
	/// <summary>
	/// Normalizes golden fixture text so engine version bumps do not fail unrelated report diffs.
	/// </summary>
	public static class GoldenText
	{
		public static string NormalizeLine(string line)
		{
			if (line == null)
			{
				return string.Empty;
			}
			if (line.StartsWith("SpaceAge Engine Version:", StringComparison.Ordinal))
			{
				return string.Format("SpaceAge Engine Version: {0}.", Program.EngineVersion);
			}
			if (line.StartsWith("SpaceAge Battle Simulator v", StringComparison.Ordinal))
			{
				return "SpaceAge Battle Simulator v" + Program.EngineVersion;
			}
			return line;
		}

		public static string NormalizeDocument(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return text ?? string.Empty;
			}
			string normalized = text.Replace("\r\n", "\n");
			string[] lines = normalized.Split('\n');
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < lines.Length; i++)
			{
				if (i > 0)
				{
					builder.Append('\n');
				}
				builder.Append(NormalizeLine(lines[i]));
			}
			return builder.ToString();
		}
	}
}
