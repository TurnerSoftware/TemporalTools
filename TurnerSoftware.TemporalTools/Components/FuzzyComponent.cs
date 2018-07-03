using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Itenso.TimePeriod;

namespace TurnerSoftware.TemporalTools.Components
{
	public class FuzzyComponent : TemporalComponent
	{
		private static readonly Regex FuzzyMatch = new Regex(@"(next|last|this) (week|month|year|(?-i)[A-Z][a-z]+(?i))|today|tonight|last night|(yesterday|tomorrow)( morning| afternoon| night)?|((?-i)[A-Z][a-z]+(?i))(?= at)", RegexOptions.IgnoreCase);
		
		private static readonly Regex NamedDate = new Regex(@"[A-Z][a-z]+");
		
		internal FuzzyComponent(Match match) : base(match) { }

		public static IEnumerable<FuzzyComponent> GetComponentsFromText(string text)
		{
			return FuzzyMatch.Matches(text).Cast<Match>().Select(m => new FuzzyComponent(m));
		}

		public override ITimePeriod GetTimePeriod(DateTime baseDate)
		{
			var date = baseDate.Date;
			var time = baseDate.TimeOfDay;
			var namedEntity = string.Empty;
			ITimeRange result = null;
			
			var lowerText = Text.ToLower();

			if (lowerText == "today")
			{
				result = new Day(date);
			}
			else if (lowerText == "tonight")
			{
				result = GetTimeOfDayPeriod(TimeOfDay.Night, date);
			}
			else if (lowerText == "last night")
			{
				result = GetTimeOfDayPeriod(TimeOfDay.Night, date.AddDays(-1));
			}
			else if (lowerText.StartsWith("yesterday"))
			{
				if (lowerText == "yesterday")
				{
					result = new Day(date.AddDays(-1));
				}
				else if (lowerText == "yesterday morning")
				{
					result = GetTimeOfDayPeriod(TimeOfDay.Morning, date.AddDays(-1));
				}
				else if (lowerText == "yesterday afternoon")
				{
					result = GetTimeOfDayPeriod(TimeOfDay.Afternoon, date.AddDays(-1));
				}
			}
			else if (lowerText.StartsWith("tomorrow"))
			{
				if (lowerText == "tomorrow")
				{
					result = new Day(date.AddDays(1));
				}
				else if (lowerText == "tomorrow morning")
				{
					result = GetTimeOfDayPeriod(TimeOfDay.Morning, date.AddDays(1));
				}
				else if (lowerText == "tomorrow afternoon")
				{
					result = GetTimeOfDayPeriod(TimeOfDay.Afternoon, date.AddDays(1));
				}
				else if (lowerText == "tomorrow night")
				{
					result = GetTimeOfDayPeriod(TimeOfDay.Night, date.AddDays(1));
				}
			}
			else if (lowerText.StartsWith("next ") || lowerText.StartsWith("last ") || lowerText.StartsWith("this"))
			{
				var pieces = lowerText.Split(' ');
				if (pieces.Length == 2)
				{
					var inPast = pieces[0] == "last";
					var inFuture = pieces[0] == "next";
					namedEntity = pieces[1];

					if (namedEntity == "week")
					{
						result = new Week(date);
						if (inPast)
						{
							result = (result as Week).GetPreviousWeek();
						}
						else if (inFuture)
						{
							result = (result as Week).GetNextWeek();
						}
					}
					else if (namedEntity == "month")
					{
						result = new Month(date);
						if (inPast)
						{
							result = (result as Month).GetPreviousMonth();
						}
						else if (inFuture)
						{
							result = (result as Month).GetNextMonth();
						}
					}
					else if (namedEntity == "year")
					{
						result = new Year(date);
						if (inPast)
						{
							result = (result as Year).GetPreviousYear();
						}
						else if (inFuture)
						{
							result = (result as Year).GetNextYear();
						}
					}
				}
			}
			else if (NamedDate.IsMatch(Text))
			{
				namedEntity = NamedDate.Match(Text).Value;
			}

			//Handle named entities (eg. "Wednesday" or "January")
			if (!string.IsNullOrEmpty(namedEntity))
			{
				var inPast = lowerText.StartsWith("last ");

				//TODO: Support "this" named entity (eg. "this March" is the March in the baseDate year)

				if (DateComponent.GetWeekDayFromName(namedEntity) != -1)
				{
					var moment = DateComponent.GetNextDateFromWeekDay(namedEntity, date);
					result = new Day(moment);
					if (inPast)
					{
						result = new Day(moment.AddDays(-7));
					}
				}
				else if (DateComponent.GetMonthFromName(namedEntity) != -1)
				{
					var moment = DateComponent.GetNextDateFromMonth(namedEntity, date);
					result = new Month(moment);
					if (inPast)
					{
						result = new Month(moment.AddMonths(-12));
					}
				}
			}

			return result;
		}

		public static ITimeRange GetTimeOfDayPeriod(TimeOfDay time, DateTime baseDate)
		{
			var startPoint = baseDate.Date;
			if (time == TimeOfDay.Morning)
			{
				return new TimeRange(startPoint, new TimeSpan(12, 0, 0));
			}
			else if (time == TimeOfDay.Afternoon)
			{
				startPoint = startPoint.AddHours(12);
				return new TimeRange(startPoint, new TimeSpan(6, 0, 0));
			}
			else
			{
				startPoint = startPoint.AddHours(18);
				return new TimeRange(startPoint, new TimeSpan(6, 0, 0));
			}
		}
	}

	public enum TimeOfDay
	{
		Morning,
		Afternoon,
		Night
	}
}
