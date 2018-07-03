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
	public class DurationComponentTests
	{
		private readonly DateTime BaseDate = new DateTime(2017, 1, 1);

		[TestMethod()]
		public void GetTimePeriodTest_FutureDuration()
		{
			AssertValidDurationComponent(new Minutes(BaseDate, 1), "for 1 minute", BaseDate);
			AssertValidDurationComponent(new Hours(BaseDate, 1), "for 1 hour", BaseDate);
			AssertValidDurationComponent(new Days(BaseDate, 1), "for 1 day", BaseDate);
			AssertValidDurationComponent(new Weeks(BaseDate, 1), "for 1 week", BaseDate);
			AssertValidDurationComponent(new Months(BaseDate, (YearMonth)BaseDate.Month, 1), "for 1 month", BaseDate);
			AssertValidDurationComponent(new Years(BaseDate, 1), "for 1 year", BaseDate);

			AssertInvalidDurationComponent("for 1 fortnight", BaseDate);
			AssertInvalidDurationComponent("for 1 second", BaseDate);
		}

		[TestMethod()]
		public void GetTimePeriodTest_PastDuration()
		{
			AssertValidDurationComponent(new Minutes(BaseDate.AddMinutes(-5), 5), "the past 5 minutes", BaseDate);
			AssertValidDurationComponent(new Minutes(BaseDate.AddMinutes(-5), 5), "the last 5 minutes", BaseDate);
		}

		private void AssertValidDurationComponent(ITimePeriod expected, string input, DateTime baseDate)
		{
			var components = DurationComponent.GetComponentsFromText(input);
			Assert.IsTrue(components.Any());
			var componentPeriod = components.FirstOrDefault().GetTimePeriod(baseDate);
			Assert.IsNotNull(componentPeriod);
			Assert.AreEqual(expected.Start, componentPeriod.Start);
			Assert.AreEqual(expected.End, componentPeriod.End);
		}

		private void AssertInvalidDurationComponent(string input, DateTime baseDate)
		{
			var components = DurationComponent.GetComponentsFromText(input);
			var componentPeriod = components.Select(c => c.GetTimePeriod(baseDate)).FirstOrDefault();
			Assert.IsNull(componentPeriod);
		}
	}
}