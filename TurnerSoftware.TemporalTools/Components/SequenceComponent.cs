using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Itenso.TimePeriod;

namespace TurnerSoftware.TemporalTools.Components
{
	public class SequenceComponent : TemporalComponent
	{
		private static readonly Regex SequenceMatch = new Regex(@"every( (\d{1,2}|(?-i)[A-Z][a-z]+(?i)))?( (minute|hour|day|week|month|year)(s)?)?", RegexOptions.IgnoreCase);

		private static readonly Regex EveryRetrieve = new Regex(@"(?<=every )(?-i)[A-Z][a-z]+(?i)", RegexOptions.IgnoreCase);
		private static readonly Regex NumericIntervalRetrieve = new Regex(@"(?<=every )\d{1,2}", RegexOptions.IgnoreCase);
		private static readonly Regex GenericSequenceIntervalRetrieve = new Regex(@"(?<=every( (\d{1,2}|(?-i)[A-Z][a-z]+(?i)))? )[a-z]+", RegexOptions.IgnoreCase);
		
		internal SequenceComponent(Match match) : base(match) { }

		public static IEnumerable<SequenceComponent> GetComponentsFromText(string text)
		{
			return SequenceMatch.Matches(text).Cast<Match>().Select(m => new SequenceComponent(m));
		}

		public override ITimePeriod GetTimePeriod(DateTime baseDate)
		{
			var startPoint = baseDate;
			CalendarTimeRange sequenceInterval = null;

			//Handle "every" as name/string value
			var retrievedEvery = EveryRetrieve.Match(Text).Value;
			if (retrievedEvery.Length > 0)
			{
				if (DateComponent.GetWeekDayFromName(retrievedEvery) != -1)
				{
					startPoint = DateComponent.GetNextDateFromWeekDay(retrievedEvery, baseDate);
					sequenceInterval = new CalendarTimeRange(startPoint, new TimeSpan(7, 0, 0, 0));
				}
				else if (DateComponent.GetMonthFromName(retrievedEvery) != -1)
				{
					startPoint = DateComponent.GetNextDateFromMonth(retrievedEvery, baseDate);
					sequenceInterval = new Months(startPoint, (YearMonth)startPoint.Month, 12);
				}
				else
				{
					return null;
				}
			}
			
			if (sequenceInterval == null)
			{
				var retrievedGenericSequence = GenericSequenceIntervalRetrieve.Match(Text).Value;
				var singularGenericSequence = retrievedGenericSequence.TrimEnd('s').ToLower();

				//Handle "every" as numeric value
				var retrievedNumericInterval = NumericIntervalRetrieve.Match(Text).Value;
				var parsedNumericInterval = 1;
				if (retrievedNumericInterval.Length > 0)
				{
					parsedNumericInterval = int.Parse(retrievedNumericInterval);
				}

				TimeSpan duration;
				
				switch (singularGenericSequence)
				{
					case "minute":
						duration = new TimeSpan(0, parsedNumericInterval, 0);
						sequenceInterval = new CalendarTimeRange(startPoint, duration);
						break;
					case "hour":
						duration = new TimeSpan(parsedNumericInterval, 0, 0);
						sequenceInterval = new CalendarTimeRange(startPoint, duration);
						break;
					case "day":
						duration = new TimeSpan(parsedNumericInterval, 0, 0, 0);
						sequenceInterval = new CalendarTimeRange(startPoint, duration);
						break;
					case "week":
						duration = new TimeSpan(7 * parsedNumericInterval, 0, 0, 0);
						sequenceInterval = new CalendarTimeRange(startPoint, duration);
						break;
					case "month":
						sequenceInterval = new Months(startPoint, (YearMonth)startPoint.Month, parsedNumericInterval);
						break;
					case "year":
						sequenceInterval = new Years(startPoint, parsedNumericInterval);
						break;
					default:
						return null;
				}
			}
			
			var result = new SequencedTimeRange(sequenceInterval);
			return result;
		}
	}
}
