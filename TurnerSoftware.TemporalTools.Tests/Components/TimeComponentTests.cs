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
	public class TimeComponentTests
	{
		private readonly DateTime BaseDate = new DateTime(2017, 1, 1);

		[TestMethod()]
		public void GetTimePeriodTest_24HourTime()
		{
			AssertValidTimeComponent(new Minute(BaseDate), "0:00", BaseDate);

			AssertValidTimeComponent(new Minute(BaseDate.AddHours(17).AddMinutes(20)), "17:20", BaseDate);

			ITimePeriod period;
			period = new TimeRange(BaseDate.AddHours(19), TimeSpec.MaxPeriodDate);
			AssertValidTimeComponent(period, "after 19:00", BaseDate);

			period = new TimeRange(TimeSpec.MinPeriodDate, BaseDate.AddHours(14));
			AssertValidTimeComponent(period, "before 14:00", BaseDate);
			
			AssertInvalidTimeComponent("24:00", BaseDate);
			AssertInvalidTimeComponent("24 o'clock", BaseDate);
		}

		[TestMethod()]
		public void GetTimePeriodTest_12HourTime()
		{
			AssertValidTimeComponent(new Minute(BaseDate), "12am", BaseDate);
			AssertValidTimeComponent(new Minute(BaseDate), "at 12am", BaseDate);
			AssertValidTimeComponent(new Minute(BaseDate.AddHours(12)), "12pm", BaseDate);
			AssertValidTimeComponent(new Minute(BaseDate.AddHours(12)), "12:00", BaseDate);
			AssertValidTimeComponent(new Minute(BaseDate.AddHours(12)), "12 o'clock", BaseDate);

			AssertValidTimeComponent(new Minute(BaseDate.AddHours(16).AddMinutes(15)), "4:15pm", BaseDate);
			AssertValidTimeComponent(new Minute(BaseDate.AddHours(12).AddMinutes(59)), "12:59pm", BaseDate);

			ITimePeriod period;
			period = new TimeRange(BaseDate.AddHours(6), TimeSpec.MaxPeriodDate);
			AssertValidTimeComponent(period, "after 6:00 am", BaseDate);

			period = new TimeRange(TimeSpec.MinPeriodDate, BaseDate.AddHours(1));
			AssertValidTimeComponent(period, "before 1 o'clock", BaseDate);

			AssertInvalidTimeComponent("13am", BaseDate);
			AssertInvalidTimeComponent("0am", BaseDate);
		}

		private void AssertValidTimeComponent(ITimePeriod expected, string input, DateTime baseDate)
		{
			var components = TimeComponent.GetComponentsFromText(input);
			Assert.IsTrue(components.Any());
			var componentPeriod = components.FirstOrDefault().GetTimePeriod(baseDate);
			Assert.IsNotNull(componentPeriod);
			Assert.AreEqual(expected.Start, componentPeriod.Start);
			Assert.AreEqual(expected.End, componentPeriod.End);
		}

		private void AssertInvalidTimeComponent(string input, DateTime baseDate)
		{
			var components = TimeComponent.GetComponentsFromText(input);
			var componentPeriod = components.Select(c => c.GetTimePeriod(baseDate)).FirstOrDefault();
			Assert.IsNull(componentPeriod);
		}
	}
}