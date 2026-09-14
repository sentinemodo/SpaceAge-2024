using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class OrderParseResult
	{
		public bool Ok { get; set; }
		public List<string> Errors { get; private set; }
		public List<string> Warnings { get; private set; }

		public OrderParseResult()
		{
			this.Errors = new List<string>();
			this.Warnings = new List<string>();
		}

		public string ToJson()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("{\"ok\":");
			sb.Append(this.Ok ? "true" : "false");
			sb.Append(",\"errors\":");
			sb.Append(this.ToJsonArray(this.Errors));
			sb.Append(",\"warnings\":");
			sb.Append(this.ToJsonArray(this.Warnings));
			sb.Append("}");
			return sb.ToString();
		}

		private string ToJsonArray(List<string> values)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("[");
			for (int i = 0; i < values.Count; i++)
			{
				if (i > 0)
				{
					sb.Append(",");
				}
				sb.Append("\"");
				sb.Append(this.EscapeJson(values[i]));
				sb.Append("\"");
			}
			sb.Append("]");
			return sb.ToString();
		}

		private string EscapeJson(string value)
		{
			if (value == null)
			{
				return string.Empty;
			}
			return value
				.Replace("\\", "\\\\")
				.Replace("\"", "\\\"")
				.Replace("\r", "\\r")
				.Replace("\n", "\\n");
		}
	}

	public static class OrderParseRunner
	{
		public static OrderParseResult ParseFile(Game game, string filename)
		{
			OrderParseResult result = new OrderParseResult();
			OrdersReader reader = new OrdersReader(game);

			try
			{
				List<string> commands = reader.ReadOrdersFile(filename);
				commands = reader.RemoveCommentsAndEmptyLines(commands);
				reader.AssignOrders(commands);
				result.Ok = true;
			}
			catch (Exception ex)
			{
				result.Ok = false;
				AddExceptionMessages(result.Errors, ex);
			}

			CollectParsingWarnings(game, result.Warnings);
			if (result.Warnings.Count > 0)
			{
				result.Ok = false;
			}

			return result;
		}

		private static void AddExceptionMessages(List<string> target, Exception ex)
		{
			Exception current = ex;
			while (current != null)
			{
				if (!string.IsNullOrEmpty(current.Message))
				{
					target.Add(current.Message);
				}
				current = current.InnerException;
			}
		}

		private static void CollectParsingWarnings(Game game, List<string> warnings)
		{
			foreach (Faction faction in game.Factions.Values)
			{
				foreach (EventReport eventReport in faction.EventReports)
				{
					if (eventReport.Description != null && eventReport.Description.StartsWith("PARSING:"))
					{
						warnings.Add(eventReport.Description);
					}
				}
			}
		}
	}
}
