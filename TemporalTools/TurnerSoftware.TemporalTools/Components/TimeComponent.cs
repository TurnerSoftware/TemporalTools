using Itenso.TimePeriod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TurnerSoftware.TemporalTools.Components
{
	public class TimeComponent : TemporalComponent
	{
		private static readonly Regex TimeMatch = new Regex(@"(at |before |after |until )?\d{1,2}([.:]\d{2}(( )?(am|pm))?| o'clock|( )?(am|pm))", RegexOptions.IgnoreCase);
		private static readonly Regex RetrieveHours = new Regex(@"(?<![.:](\d{1,2})?)\d{1,2}(?=[.:]| o|( )?(a|p))", RegexOptions.IgnoreCase);
		private static readonly Regex RetrieveMinutes = new Regex(@"(?<=[.:])\d{2}");

		internal TimeComponent(Match match) : base(match) { }

		public static IEnumerable<TimeComponent> GetComponentsFromText(string text)
		{
			return TimeMatch.Matches(text).Cast<Match>().Select(m => new TimeComponent(m));
		}

		public bool IsUntil
		{
			get
			{
				return Text.ToLower().StartsWith("until ");
			}
		}

		public override ITimePeriod GetTimePeriod(DateTime baseDate)
		{
			ITimePeriod result = new TimeBlock();
			var value = Text.ToLower();

			var retrievedHour = RetrieveHours.Match(value).Value;
			var parsedHour = int.Parse(retrievedHour);

			var retrievedMinutes = RetrieveMinutes.Match(value).Value;
			var parsedMinutes = 0;
			if (retrievedMinutes.Length > 0)
			{
				parsedMinutes = int.Parse(retrievedMinutes);
			}

			//Enforce 12-hour time
			if ((value.Contains("am") || value.Contains("pm")) && parsedHour > 12)
			{
				return null;
			}
			else if (value.Contains("am") && parsedHour == 0)
			{
				return null;
			}

			//Handle 12-hour time
			if (value.Contains("am") && parsedHour >= 12)
			{
				parsedHour -= 12;
			}
			else if (value.Contains("pm") && parsedHour < 12)
			{
				parsedHour += 12;
			}

			//Enforce 24-hour time
			if (parsedHour > 23)
			{
				return null;
			}

			//Enforce valid minutes
			if (parsedMinutes > 59)
			{
				return null;
			}

			var timespan = new TimeSpan(parsedHour, parsedMinutes, 0);
			var pointInTime = baseDate + timespan;

			//Apply rules for open-ended date
			if (value.StartsWith("before"))
			{
				result = new TimeRange(TimeSpec.MinPeriodDate, pointInTime);
			}
			else if (value.StartsWith("after"))
			{
				result = new TimeRange(pointInTime, TimeSpec.MaxPeriodDate);
			}
			else
			{
				result = new Minute(pointInTime);
			}

			return result;
		}
	}
}
