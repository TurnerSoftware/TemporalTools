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
	public class RelativeDateTimeComponentTests
	{
		private readonly DateTime BaseDate = new DateTime(2017, 1, 1);

		[TestMethod()]
		public void GetTimePeriodTest()
		{
			AssertValidRelativeDateTimeComponent(new Minutes(BaseDate.AddMinutes(-5), 5), "5 minutes ago", BaseDate);
			AssertValidRelativeDateTimeComponent(new Hours(BaseDate.AddHours(-3), 3), "3 hours ago", BaseDate);
			AssertValidRelativeDateTimeComponent(new Days(BaseDate.AddDays(-4), 4), "4 days ago", BaseDate);
			AssertValidRelativeDateTimeComponent(new Weeks(BaseDate.AddDays(-14), 2), "2 weeks ago", BaseDate);
			AssertValidRelativeDateTimeComponent(new Months(BaseDate.AddMonths(-1), (YearMonth)BaseDate.AddMonths(-1).Month, 1), "1 month ago", BaseDate);
			AssertValidRelativeDateTimeComponent(new Years(BaseDate.AddYears(-10), 10), "10 years ago", BaseDate);

			AssertInvalidRelativeDateTimeComponent("a minute ago", BaseDate);
			AssertInvalidRelativeDateTimeComponent("2 seconds ago", BaseDate);
			AssertInvalidRelativeDateTimeComponent("1 fortnight ago", BaseDate);
		}

		private void AssertValidRelativeDateTimeComponent(ITimePeriod expected, string input, DateTime baseDate)
		{
			var components = RelativeDateTimeComponent.GetComponentsFromText(input);
			Assert.IsTrue(components.Any());
			var componentPeriod = components.FirstOrDefault().GetTimePeriod(baseDate);
			Assert.IsNotNull(componentPeriod);
			Assert.AreEqual(expected.Start, componentPeriod.Start);
			Assert.AreEqual(expected.End, componentPeriod.End);
		}

		private void AssertInvalidRelativeDateTimeComponent(string input, DateTime baseDate)
		{
			var components = RelativeDateTimeComponent.GetComponentsFromText(input);
			var componentPeriod = components.Select(c => c.GetTimePeriod(baseDate)).FirstOrDefault();
			Assert.IsNull(componentPeriod);
		}
	}
}