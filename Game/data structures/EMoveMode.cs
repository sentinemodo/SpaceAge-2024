using System;
using System.IO;

namespace SpaceAge
{
	public enum EMoveMode
	{
		ground,
		space,
		naval
	}

	public static class MoveModeXml
	{
		public static EMoveMode Parse(string token)
		{
			switch (token)
			{
				case "ground":
					return EMoveMode.ground;
				case "space":
					return EMoveMode.space;
				case "naval":
					return EMoveMode.naval;
				default:
					throw new FileLoadException("Tried to load move mode " + token);
			}
		}

		public static string ToToken(EMoveMode mode)
		{
			switch (mode)
			{
				case EMoveMode.ground:
					return "ground";
				case EMoveMode.space:
					return "space";
				case EMoveMode.naval:
					return "naval";
				default:
					throw new Exception("Unknown move mode");
			}
		}
	}
}
