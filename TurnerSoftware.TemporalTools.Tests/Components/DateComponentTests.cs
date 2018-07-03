using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Itenso.TimePeriod;
using TurnerSoftware.TemporalTools.Components;

namespace TurnerSoftware.TemporalTools.Tests.Components
{
	[TestClass()]
	public class DateComponentTests
	{
		private readonly DateTime BaseDate = new DateTime(2017, 1, 1);

		[TestMethod()]
		public void GetNextDateFromWeekDayTest()
		{
			Assert.AreEqual(new DateTime(2017, 1, 8), DateComponent.GetNextDateFromWeekDay("sunday", BaseDate));
			Assert.AreEqual(new DateTime(2017, 1, 2), DateComponent.GetNextDateFromWeekDay("monday", BaseDate));
			Assert.AreEqual(new DateTime(2017, 1, 3), DateComponent.GetNextDateFromWeekDay("tuesday", BaseDate));
			Assert.AreEqual(new DateTime(2017, 1, 4), DateComponent.GetNextDateFromWeekDay("wednesday", BaseDate));
			Assert.AreEqual(new DateTime(2017, 1, 5), DateComponent.GetNextDateFromWeekDay("thursday", BaseDate));
			Assert.AreEqual(new DateTime(2017, 1, 6), DateComponent.GetNextDateFromWeekDay("friday", BaseDate));
			Assert.AreEqual(new DateTime(2017, 1, 7), DateComponent.GetNextDateFromWeekDay("saturday", BaseDate));

			Assert.AreEqual(BaseDate, DateComponent.GetNextDateFromWeekDay("invalid", BaseDate));
		}

		[TestMethod()]
		public void GetWeekDayFromNameTest()
		{
			Assert.AreEqual(1, DateComponent.GetWeekDayFromName("sunday"));
			Assert.AreEqual(2, DateComponent.GetWeekDayFromName("monday"));
			Assert.AreEqual(3, DateComponent.GetWeekDayFromName("tuesday"));
			Assert.AreEqual(4, DateComponent.GetWeekDayFromName("wednesday"));
			Assert.AreEqual(5, DateComponent.GetWeekDayFromName("thursday"));
			Assert.AreEqual(6, DateComponent.GetWeekDayFromName("friday"));
			Assert.AreEqual(7, DateComponent.GetWeekDayFromName("saturday"));

			//Abbreviation
			Assert.AreEqual(-1, DateComponent.GetWeekDayFromName("s"));
			Assert.AreEqual(-1, DateComponent.GetWeekDayFromName("su"));
			Assert.AreEqual(1, DateComponent.GetWeekDayFromName("sun"));

			//Case-sensitivity
			Assert.AreEqual(4, DateComponent.GetWeekDayFromName("WedNeSdAy"));

			//Invalid
			Assert.AreEqual(-1, DateComponent.GetWeekDayFromName("invalid"));
			Assert.AreEqual(-1, DateComponent.GetMonthFromName(" monday"));
			Assert.AreEqual(-1, DateComponent.GetMonthFromName("thursday "));
		}

		[TestMethod()]
		public void GetNextDateFromMonthTest()
		{
			Assert.AreEqual(new DateTime(2018, 1, 1), DateComponent.GetNextDateFromMonth("january", BaseDate));
			Assert.AreEqual(new DateTime(2017, 2, 1), DateComponent.GetNextDateFromMonth("february", BaseDate));
			Assert.AreEqual(new DateTime(2017, 3, 1), DateComponent.GetNextDateFromMonth("march", BaseDate));
			Assert.AreEqual(new DateTime(2017, 4, 1), DateComponent.GetNextDateFromMonth("april", BaseDate));
			Assert.AreEqual(new DateTime(2017, 5, 1), DateComponent.GetNextDateFromMonth("may", BaseDate));
			Assert.AreEqual(new DateTime(2017, 6, 1), DateComponent.GetNextDateFromMonth("june", BaseDate));
			Assert.AreEqual(new DateTime(2017, 7, 1), DateComponent.GetNextDateFromMonth("july", BaseDate));
			Assert.AreEqual(new DateTime(2017, 8, 1), DateComponent.GetNextDateFromMonth("august", BaseDate));
			Assert.AreEqual(new DateTime(2017, 9, 1), DateComponent.GetNextDateFromMonth("september", BaseDate));
			Assert.AreEqual(new DateTime(2017, 10, 1), DateComponent.GetNextDateFromMonth("october", BaseDate));
			Assert.AreEqual(new DateTime(2017, 11, 1), DateComponent.GetNextDateFromMonth("november", BaseDate));
			Assert.AreEqual(new DateTime(2017, 12, 1), DateComponent.GetNextDateFromMonth("december", BaseDate));

			Assert.AreEqual(BaseDate, DateComponent.GetNextDateFromMonth("invalid", BaseDate));
		}

		[TestMethod()]
		public void GetMonthFromNameTest()
		{
			Assert.AreEqual(1, DateComponent.GetMonthFromName("january"));
			Assert.AreEqual(2, DateComponent.GetMonthFromName("february"));
			Assert.AreEqual(3, DateComponent.GetMonthFromName("march"));
			Assert.AreEqual(4, DateComponent.GetMonthFromName("april"));
			Assert.AreEqual(5, DateComponent.GetMonthFromName("may"));
			Assert.AreEqual(6, DateComponent.GetMonthFromName("june"));
			Assert.AreEqual(7, DateComponent.GetMonthFromName("july"));
			Assert.AreEqual(8, DateComponent.GetMonthFromName("august"));
			Assert.AreEqual(9, DateComponent.GetMonthFromName("september"));
			Assert.AreEqual(10, DateComponent.GetMonthFromName("october"));
			Assert.AreEqual(11, DateComponent.GetMonthFromName("november"));
			Assert.AreEqual(12, DateComponent.GetMonthFromName("december"));

			//Abbreviation
			Assert.AreEqual(-1, DateComponent.GetMonthFromName("j"));
			Assert.AreEqual(-1, DateComponent.GetMonthFromName("ju"));
			Assert.AreEqual(6, DateComponent.GetMonthFromName("jun"));

			//Case-sensitivity
			Assert.AreEqual(4, DateComponent.GetMonthFromName("ApRiL"));

			//Invalid
			Assert.AreEqual(-1, DateComponent.GetMonthFromName("invalid"));
			Assert.AreEqual(-1, DateComponent.GetMonthFromName(" march"));
			Assert.AreEqual(-1, DateComponent.GetMonthFromName("may "));
		}

		[TestMethod()]
		public void GetTimePeriodTest_NumericDates()
		{
			var expected = new Day(new DateTime(2017, 1, 15));
			AssertValidDateComponent(expected, "2017-01-15", BaseDate);
			AssertValidDateComponent(expected, "1-15-2017", BaseDate);
			AssertValidDateComponent(expected, "15-1-2017", BaseDate);
		}

		[TestMethod()]
		public void GetTimePeriodTest_LongFormatDates()
		{
			ITimePeriod period;
			period = new Day(new DateTime(2017, 5, 15));
			AssertValidDateComponent(new TimeRange(period.End, TimeSpec.MaxPeriodDate), "after the 15th of May", BaseDate);
			AssertValidDateComponent(new Month(new DateTime(2017, 6, 1)), "Jun 2017", BaseDate);
			AssertValidDateComponent(new Day(new DateTime(2017, 1, 22)), "the 22nd", BaseDate);
			AssertValidDateComponent(new Year(new DateTime(1928, 1, 1)), "1928", BaseDate);
			period = new Year(new DateTime(2017, 1, 1));
			AssertValidDateComponent(new TimeRange(period.End, TimeSpec.MaxPeriodDate), "after 2017", BaseDate);
			AssertValidDateComponent(new Day(new DateTime(2017, 7, 17)), "July 17th", BaseDate);
			AssertValidDateComponent(new Day(new DateTime(2017, 1, 16)), "On the 16th", BaseDate);
			AssertValidDateComponent(new Day(new DateTime(1842, 10, 12)), "October 12th, 1842", BaseDate);
			period = new Day(new DateTime(2017, 1, 20));
			AssertValidDateComponent(new TimeRange(TimeSpec.MinPeriodDate, period.Start), "before the 20th", BaseDate);
		}

		[TestMethod]
		public void GetTimePeriodTest_FormalDate()
		{
			AssertValidDateComponent(new Day(new DateTime(2017, 10, 17)), "17 Oct", BaseDate);
			AssertValidDateComponent(new Day(new DateTime(2020, 12, 25)), "25 Dec 2020", BaseDate);
			AssertInvalidDateComponent("30 Feb 2017", BaseDate);
			AssertInvalidDateComponent("10 Nop 2017", BaseDate);
		}

		[TestMethod]
		public void GetTimePeriodTest_WeekdayDate()
		{
			AssertValidDateComponent(new Day(new DateTime(2017, 1, 3)), "on Tuesday", BaseDate);
			AssertValidDateComponent(new Day(new DateTime(2017, 1, 4)), "On Wednesday", BaseDate);
			AssertInvalidDateComponent("on Earth", BaseDate);
		}

		private void AssertValidDateComponent(ITimePeriod expected, string input, DateTime baseDate)
		{
			var components = DateComponent.GetComponentsFromText(input);
			Assert.IsTrue(components.Any());
			var componentPeriod = components.FirstOrDefault().GetTimePeriod(baseDate);
			Assert.IsNotNull(componentPeriod);
			Assert.AreEqual(expected.Start, componentPeriod.Start);
			Assert.AreEqual(expected.End, componentPeriod.End);
		}

		private void AssertInvalidDateComponent(string input, DateTime baseDate)
		{
			var components = DateComponent.GetComponentsFromText(input);
			var componentPeriod = components.Select(c => c.GetTimePeriod(baseDate)).FirstOrDefault();
			Assert.IsNull(componentPeriod);
		}
	}
}