using Itenso.TimePeriod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TurnerSoftware.TemporalTools.Components
{
	public class RelativeDateTimeComponent : TemporalComponent
	{
		private static readonly Regex RelativeDateTimeMatch = new Regex(@"\d+ (minute|hour|day|week|month|year)(s)? ago", RegexOptions.IgnoreCase);

		private static readonly Regex NumericValue = new Regex(@"\d+", RegexOptions.IgnoreCase);
		private static readonly Regex NamedEntity = new Regex(@"minute|hour|day|week|month|year", RegexOptions.IgnoreCase);

		internal RelativeDateTimeComponent(Match match) : base(match) { }

		public static IEnumerable<RelativeDateTimeComponent> GetComponentsFromText(string text)
		{
			return RelativeDateTimeMatch.Matches(text).Cast<Match>().Select(m => new RelativeDateTimeComponent(m));
		}

		public override ITimePeriod GetTimePeriod(DateTime baseDate)
		{
			var retrievedNumericValue = NumericValue.Match(Text).Value;
			var numericValue = int.Parse(retrievedNumericValue);
			var namedEntity = NamedEntity.Match(Text).Value;
			ITimeRange result = null;

			var lowerText = Text.ToLower();

			if (namedEntity == "minute")
			{
				result = new Minutes(baseDate.AddMinutes(-numericValue), numericValue);
			}
			else if (namedEntity == "hour")
			{
				result = new Hours(baseDate.AddHours(-numericValue), numericValue);
			}
			else if (namedEntity == "day")
			{
				result = new Days(baseDate.AddDays(-numericValue), numericValue);
			}
			else if (namedEntity == "week")
			{
				result = new Weeks(baseDate.AddDays(-numericValue * 7), numericValue);
			}
			else if (namedEntity == "month")
			{
				var moment = baseDate.AddMonths(-numericValue);
				result = new Months(moment, (YearMonth)moment.Month, numericValue);
			}
			else if (namedEntity == "year")
			{
				result = new Years(baseDate.AddYears(-numericValue), numericValue);
			}
			
			return result;
		}
	}
}
