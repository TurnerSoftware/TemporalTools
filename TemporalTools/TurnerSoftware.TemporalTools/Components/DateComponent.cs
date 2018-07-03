using Itenso.TimePeriod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TurnerSoftware.TemporalTools.Components
{
	public class DateComponent : TemporalComponent
	{
		private static readonly Regex DateMatch = new Regex(@"\d{1,4}[-/]\d{1,2}[-/]\d{2,4}(?#
)|(on |after |before |until )?((?-i)[A-Z][a-z]+(?i) |the )?(\d{1,2}(st|nd|rd|th)(((,)? \d{4})| of (?-i)[A-Z][a-z]+(?i))?|\d{4})(?#
)|\d{1,2} (?-i)[A-Z][a-z]+(?i)( \d{4})?(?#
)|((?-i)[A-Z][a-z]+(?i)) \d{1,2}, \d{4}(?#
)|on ((?-i)[A-Z][a-z]+(?i))", RegexOptions.IgnoreCase);

		private static readonly Regex NameRetrieve = new Regex(@"[A-Z][a-z]+");
		private static readonly Regex WeekdayNameRetrieve = new Regex(@"(?<=on )((?-i)[A-Z][a-z]+(?i))", RegexOptions.IgnoreCase);

		private static readonly Regex NumericDate = new Regex(@"^\d{1,4}[-/]\d{1,2}[-/]\d{2,4}$");

		private static readonly Regex LongFormatDate = new Regex(@"^(on |after |before |until )?((?-i)[A-Z][a-z]+(?i) |the )?(\d{1,2}(st|nd|rd|th)(((,)? \d{4})| of (?-i)[A-Z][a-z]+(?i))?|\d{4})$", RegexOptions.IgnoreCase);
		private static readonly Regex LongFormatRetrieveDay = new Regex(@"\d{1,2}(?=st|nd|rd|th)", RegexOptions.IgnoreCase);
		private static readonly Regex LongFormatRetrieveYear = new Regex(@"\d{4}$");

		private static readonly Regex FormalDayFirstDate = new Regex(@"^\d{1,2} (?-i)[A-Z][a-z]+(?i)( \d{4})?$", RegexOptions.IgnoreCase);
		private static readonly Regex FormalMonthFirstDate = new Regex(@"^((?-i)[A-Z][a-z]+(?i)) \d{1,2}, \d{4}$", RegexOptions.IgnoreCase);

		private static readonly Regex OnWeekdayDate = new Regex(@"^on ((?-i)[A-Z][a-z]+(?i))$", RegexOptions.IgnoreCase);

		public static readonly string[] Days = new[]
		{
			"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"
		};

		public static readonly string[] Months = new[]
		{
			"January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"
		};

		internal DateComponent(Match match) : base(match) { }

		public static IEnumerable<DateComponent> GetComponentsFromText(string text)
		{
			return DateMatch.Matches(text).Cast<Match>().Select(m => new DateComponent(m));
		}

		public static DateTime GetNextDateFromWeekDay(string weekDay, DateTime baseDate)
		{
			var weekDayIndex = GetWeekDayFromName(weekDay);
			if (weekDayIndex == -1)
			{
				return baseDate;
			}
			var daysAhead = (weekDayIndex - ((int)baseDate.DayOfWeek + 1) + 7) % 7;
			if (daysAhead == 0)
			{
				daysAhead = 7;
			}
			var result = baseDate.AddDays(daysAhead).Date;
			return result;
		}
		public static int GetWeekDayFromName(string weekDay)
		{
			if (weekDay.Length > 2)
			{
				weekDay = weekDay.ToLower();
				var daysList = Days.Select(d => d.ToLower()).ToList();
				var weekDayIndex = daysList.IndexOf(weekDay);
				if (weekDayIndex == -1)
				{
					var partialMatch = daysList.Where(d => d.StartsWith(weekDay)).FirstOrDefault();
					if (partialMatch == null)
					{
						return -1;
					}
					weekDayIndex = daysList.IndexOf(partialMatch);
				}
				return weekDayIndex + 1;
			}
			return -1;
		}

		public static DateTime GetNextDateFromMonth(string month, DateTime baseDate)
		{
			var monthIndex = GetMonthFromName(month);
			if (monthIndex == -1)
			{
				return baseDate;
			}
			var monthsAhead = (monthIndex - (int)baseDate.Month + 12) % 12;
			if (monthsAhead == 0)
			{
				monthsAhead = 12;
			}
			var result = new DateTime(baseDate.Year, baseDate.Month, 1).AddMonths(monthsAhead);
			return result;
		}
		public static int GetMonthFromName(string month)
		{
			if (month.Length > 2)
			{
				month = month.ToLower();
				var monthsList = Months.Select(m => m.ToLower()).ToList();
				var monthIndex = monthsList.IndexOf(month);
				if (monthIndex == -1)
				{
					var partialMatch = monthsList.Where(m => m.StartsWith(month)).FirstOrDefault();
					if (partialMatch == null)
					{
						return -1;
					}
					monthIndex = monthsList.IndexOf(partialMatch);
				}
				return monthIndex + 1;
			}
			return -1;
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
			ITimePeriod result = null;

			if (NumericDate.IsMatch(Text))
			{
				DateTime pointInTime;
				var dateParts = Text.Split('-', '/');
				var commonDate = string.Join("-", dateParts);
				var formats = new string[]
				{
					"yyyy-MM-dd",
					"d-M-yyyy",
					"M-d-yyyy"
				};

				if (!DateTime.TryParseExact(commonDate, formats, null, System.Globalization.DateTimeStyles.None, out pointInTime))
				{
					return null;
				}

				result = new Day(pointInTime);
			}
			else if (LongFormatDate.IsMatch(Text))
			{
				var retrievedDay = LongFormatRetrieveDay.Match(Text).Value;
				var parsedDay = 1;
				if (retrievedDay.Length > 0)
				{
					//Apply date rule if value contains a date
					parsedDay = int.Parse(retrievedDay);
				}

				//Enforce valid date
				if (parsedDay > 31)
				{
					return null;
				}

				var retrievedMonth = "";
				var parsedMonth = 1;
				var matchedNames = NameRetrieve.Matches(Text).Cast<Match>();
				foreach (var match in matchedNames)
				{
					if (match.Value.Length > 2)
					{
						//Apply next weekday rule if value looks like it has weekday details
						//Note: This makes the assumption that the sentence infers a future date
						var weekDay = GetNextDateFromWeekDay(match.Value, baseDate);
						if (weekDay > baseDate)
						{
							parsedDay = weekDay.Day;
							continue;
						}

						//Apply month rule if value looks like it has month details
						var month = GetMonthFromName(match.Value);
						if (month != -1)
						{
							retrievedMonth = match.Value;
							parsedMonth = month;
							continue;
						}
					}
				}

				var retrievedYear = LongFormatRetrieveYear.Match(Text).Value;
				var parsedYear = baseDate.Year;
				if (retrievedYear.Length > 0)
				{
					//Apply year rule if the value contains a year
					parsedYear = int.Parse(retrievedYear);
				}

				//Apply base month if no month or year details were found
				if (retrievedMonth.Length == 0 && retrievedYear.Length == 0)
				{
					parsedMonth = baseDate.Month;
				}

				//Handle that the parsed date is allowed for the given month
				var lastDayOfMonth = new Month(parsedYear, (YearMonth)parsedMonth).End;
				if (parsedDay > lastDayOfMonth.Day)
				{
					return null;
				}

				var leadingDate = new DateTime(parsedYear, parsedMonth, parsedDay);
				var trailingDate = leadingDate;

				//When value is specific to the...
				if (retrievedDay.Length > 0)
				{
					//Day
					trailingDate = new Day(trailingDate).End;
				}
				else if (retrievedMonth.Length > 0)
				{
					//Month
					trailingDate = new Month(trailingDate).End;
				}
				else if (retrievedYear.Length > 0)
				{
					//Year
					trailingDate = new Year(trailingDate).End;
				}

				//Apply rules for open-ended date
				if (Text.ToLower().StartsWith("before"))
				{
					result = new TimeRange(TimeSpec.MinPeriodDate, leadingDate);
				}
				else if (Text.ToLower().StartsWith("after"))
				{
					result = new TimeRange(trailingDate, TimeSpec.MaxPeriodDate);
				}
				else
				{
					result = new TimeRange(leadingDate, trailingDate);
				}
			}
			else if (FormalDayFirstDate.IsMatch(Text) || FormalMonthFirstDate.IsMatch(Text))
			{
				var dateParts = Text.Split(new[] { ", ", " " }, StringSplitOptions.RemoveEmptyEntries);
				var retrievedDay = "";
				var retrievedMonth = "";

				//Work out the order of day/month
				if (FormalDayFirstDate.IsMatch(Text))
				{
					retrievedDay = dateParts[0];
					retrievedMonth = dateParts[1];
				}
				else
				{
					retrievedDay = dateParts[1];
					retrievedMonth = dateParts[0];
				}
				
				var parsedDay = int.Parse(retrievedDay);

				//Check if the retrieved month can at least partial match a month
				var nameMatch = NameRetrieve.Match(retrievedMonth).Value;
				var month = Months.Where(m => m.StartsWith(nameMatch)).FirstOrDefault();
				if (month == null)
				{
					return null;
				}
				var parsedMonth = GetMonthFromName(month);

				var parsedYear = baseDate.Year;
				if (dateParts.Length == 3)
				{
					parsedYear = int.Parse(dateParts[2]);
				}
				
				//Handle that the parsed date is allowed for the given month
				var lastDayOfMonth = new Month(parsedYear, (YearMonth)parsedMonth).End;
				if (parsedDay > lastDayOfMonth.Day)
				{
					return null;
				}

				var pointInTime = new DateTime(parsedYear, parsedMonth, parsedDay);
				result = new TimeBlock(pointInTime, new Day(pointInTime).End);
			}
			else if (OnWeekdayDate.IsMatch(Text))
			{
				var retrievedWeekday = WeekdayNameRetrieve.Match(Text).Value;
				var weekDay = Days.Where(d => d.StartsWith(retrievedWeekday)).FirstOrDefault();
				if (weekDay != null)
				{
					//Note: This makes the assumption that the sentence infers a future date
					var pointInTime = GetNextDateFromWeekDay(weekDay, baseDate);
					result = new TimeBlock(pointInTime, new Day(pointInTime).End);
				}
				else
				{
					return null;
				}
			}
			else
			{
				//Must match one of the specific date formats
				return null;
			}

			return result;
		}
	}
}