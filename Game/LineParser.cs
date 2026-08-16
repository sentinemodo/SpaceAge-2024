using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class LineParser
	{
		public static string GetToken(ref string s)
		{
			string res = "";
			while (s.Length > 0 && s[0] != ' ')
			{
				res += s.Substring(0, 1);
				s = s.Substring(1);
			}
			s = s.TrimStart();
			return res;
		}

		public static string GetQuotedToken(ref string s)
		{
			if (s.Length > 0 && s[0] == '"')
			{
				string res = "";
				s = s.Substring(1);
				while (s.Length > 0 && s[0] != '"')
				{
					res += s.Substring(0, 1);
					s = s.Substring(1);
				}
				if (s.Length > 0) s = s.Substring(1);
				s = s.TrimStart();
				return res;
			}
			else
				return GetToken(ref s);
		}

		public static string GetValidString(string s)
		{
			string res = "";
			for (int i = 0; i < s.Length; i++)
			{
				if ((s[i] >= 'A' && s[i] <= 'Z') || (s[i] >= 'a' && s[i] <= 'z')
					|| (s[i] >= '0' && s[i] <= '9')
					|| (s[i] >= 'À' && s[i] <= 'ß') || (s[i] >= 'à' && s[i] <= 'ÿ')
					|| s[i] == '¨' || s[i] == '¸'
					|| s[i] == ' ' || s[i] == '_' || s[i] == '\'' || s[i] == '-'
					|| s[i] == '+' || s[i] == '.' || s[i] == ',')
					res += s[i];
			}
			return res;
		}

		public static bool IsNumber(string s)
		{
			try
			{
				return (Convert.ToInt32(s) >= 0);
			}
			catch (FormatException)
			{
				return false;
			}
		}

		// Index of the first comment marker in the line, or -1 if none.
		// Both ';' and '//' start a comment; the earliest one wins.
		public static int CommentIndex(string s)
		{
			int semicolon = s.IndexOf(';');
			int doubleSlash = s.IndexOf("//");

			if (semicolon >= 0 && doubleSlash >= 0)
				return Math.Min(semicolon, doubleSlash);
			if (semicolon >= 0)
				return semicolon;
			return doubleSlash;
		}

		public static string Uncomment(string s)
		{
			int commentStart = CommentIndex(s);
			if (commentStart >= 0)
				return s.Substring(0, commentStart);
			else
				return s;
		}
	}
}
