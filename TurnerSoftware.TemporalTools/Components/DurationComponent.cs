using Itenso.TimePeriod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TurnerSoftware.TemporalTools.Components
{
	public class DurationComponent : TemporalComponent
	{
		private static readonly Regex DurationMatch = new Regex(@"(the (next|last|past) |for )?\d+ (minute|hour|day|week|month|year)(s)?", RegexOptions.IgnoreCase);

		private static readonly Regex NumericValue = new Regex(@"\d+", RegexOptions.IgnoreCase);
		private static readonly Regex NamedEntity = new Regex(@"minute|hour|day|week|month|year", RegexOptions.IgnoreCase);

		internal DurationComponent(Match match) : base(match) { }

		public static IEnumerable<DurationComponent> GetComponentsFromText(string text)
		{
			return DurationMatch.Matches(text).Cast<Match>().Select(m => new DurationComponent(m));
		}

		public override ITimePeriod GetTimePeriod(DateTime baseDate)
		{
			var retrievedNumericValue = NumericValue.Match(Text).Value;
			var numericValue = int.Parse(retrievedNumericValue);
			var namedEntity = NamedEntity.Match(Text).Value;
			ITimeRange result = null;

			var lowerText = Text.ToLower();
			var inPast = lowerText.Contains(" last ") || lowerText.Contains(" past ");

			if (numericValue < 0)
			{
				return null;
			}

			if (namedEntity == "minute")
			{
				result = new Minutes(baseDate, numericValue);
				if (inPast)
				{
					result = new Minutes(baseDate - result.Duration, numericValue);
				}
			}
			else if (namedEntity == "hour")
			{
				result = new Hours(baseDate, numericValue);
				if (inPast)
				{
					result = new Hours(baseDate - result.Duration, numericValue);
				}
			}
			else if (namedEntity == "day")
			{
				result = new Days(baseDate, numericValue);
				if (inPast)
				{
					result = new Days(baseDate - result.Duration, numericValue);
				}
			}
			else if (namedEntity == "week")
			{
				result = new Weeks(baseDate, numericValue);
				if (inPast)
				{
					result = new Weeks(baseDate - result.Duration, numericValue);
				}
			}
			else if (namedEntity == "month")
			{
				result = new Months(baseDate, (YearMonth)baseDate.Month, numericValue);
				if (inPast)
				{
					result = new Months(baseDate.AddMonths(-numericValue), (YearMonth)baseDate.Month, numericValue);
				}
			}
			else if (namedEntity == "year")
			{
				result = new Years(baseDate, numericValue);
				if (inPast)
				{
					result = new Years(baseDate.AddYears(-numericValue), numericValue);
				}
			}
			
			return result;
		}
	}
}
