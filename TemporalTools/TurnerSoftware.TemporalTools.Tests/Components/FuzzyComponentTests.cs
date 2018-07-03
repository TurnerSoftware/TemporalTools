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
	public class FuzzyComponentTests
	{
		private readonly DateTime BaseDate = new DateTime(2017, 1, 1);

		[TestMethod()]
		public void GetTimePeriodTest_SmallFuzzy()
		{
			AssertValidFuzzyComponent(new Day(BaseDate), "today", BaseDate);

			var yesterday = BaseDate.AddDays(-1);
			var tomorrow = BaseDate.AddDays(1);

			AssertValidFuzzyComponent(new Day(tomorrow), "tomorrow", BaseDate);
			AssertValidFuzzyComponent(new Day(yesterday), "yesterday", BaseDate);

			AssertValidFuzzyComponent(new TimeRange(BaseDate.AddHours(18), tomorrow), "tonight", BaseDate);
			AssertValidFuzzyComponent(new TimeRange(yesterday.AddHours(18), BaseDate), "last night", BaseDate);
			
			AssertValidFuzzyComponent(new TimeRange(tomorrow, tomorrow.AddHours(12)), "tomorrow morning", BaseDate);
			AssertValidFuzzyComponent(new TimeRange(tomorrow.AddHours(12), tomorrow.AddHours(18)), "tomorrow afternoon", BaseDate);
			AssertValidFuzzyComponent(new TimeRange(tomorrow.AddHours(18), BaseDate.AddDays(2)), "tomorrow night", BaseDate);

			AssertValidFuzzyComponent(new TimeRange(yesterday, yesterday.AddHours(12)), "yesterday morning", BaseDate);
			AssertValidFuzzyComponent(new TimeRange(yesterday.AddHours(12), yesterday.AddHours(18)), "yesterday afternoon", BaseDate);
		}

		[TestMethod()]
		public void GetTimePeriodTest_LongFuzzy()
		{
			AssertValidFuzzyComponent(new Week(BaseDate), "this week", BaseDate);
			AssertValidFuzzyComponent(new Week(BaseDate.AddDays(7)), "next week", BaseDate);
			AssertValidFuzzyComponent(new Week(BaseDate.AddDays(-7)), "last week", BaseDate);

			AssertValidFuzzyComponent(new Month(BaseDate), "this month", BaseDate);
			AssertValidFuzzyComponent(new Month(BaseDate.AddMonths(1)), "next month", BaseDate);
			AssertValidFuzzyComponent(new Month(BaseDate.AddMonths(-1)), "last month", BaseDate);

			AssertValidFuzzyComponent(new Year(BaseDate), "this year", BaseDate);
			AssertValidFuzzyComponent(new Year(BaseDate.AddYears(1)), "next year", BaseDate);
			AssertValidFuzzyComponent(new Year(BaseDate.AddYears(-1)), "last year", BaseDate);
		}

		[TestMethod()]
		public void GetTimePeriodTest_NamedFuzzy()
		{
			AssertValidFuzzyComponent(new Day(new DateTime(2016, 12, 27)), "last Tuesday", BaseDate);
			AssertValidFuzzyComponent(new Day(new DateTime(2017, 1, 6)), "next Friday", BaseDate);

			AssertValidFuzzyComponent(new Month(new DateTime(2017, 3, 1)), "next March", BaseDate);
			AssertValidFuzzyComponent(new Month(new DateTime(2016, 8, 1)), "last August", BaseDate);
		}

		[TestMethod()]
		public void GetTimeOfDayPeriodTest()
		{
			var morning = new TimeRange(BaseDate, new TimeSpan(12, 0, 0));
			Assert.AreEqual(morning, FuzzyComponent.GetTimeOfDayPeriod(TimeOfDay.Morning, BaseDate));

			var afternoon = new TimeRange(BaseDate.AddHours(12), new TimeSpan(6, 0, 0));
			Assert.AreEqual(afternoon, FuzzyComponent.GetTimeOfDayPeriod(TimeOfDay.Afternoon, BaseDate));

			var night = new TimeRange(BaseDate.AddHours(18), new TimeSpan(6, 0, 0));
			Assert.AreEqual(night, FuzzyComponent.GetTimeOfDayPeriod(TimeOfDay.Night, BaseDate));
		}

		private void AssertValidFuzzyComponent(ITimePeriod expected, string input, DateTime baseDate)
		{
			var components = FuzzyComponent.GetComponentsFromText(input);
			Assert.IsTrue(components.Any());
			var componentPeriod = components.FirstOrDefault().GetTimePeriod(baseDate);
			Assert.IsNotNull(componentPeriod);
			Assert.AreEqual(expected.Start, componentPeriod.Start);
			Assert.AreEqual(expected.End, componentPeriod.End);
		}
	}
}