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
	public class SequenceComponentTests
	{
		private readonly DateTime BaseDate = new DateTime(2017, 1, 1);

		[TestMethod()]
		public void GetTimePeriodTest_Every()
		{
			SequencedTimeRange sequencedTimeRange;

			sequencedTimeRange = new SequencedTimeRange(new Minutes(BaseDate, 5));
			AssertValidSequenceComponent(sequencedTimeRange, "every 5 minutes", BaseDate);

			sequencedTimeRange = new SequencedTimeRange(new Hours(BaseDate, 1));
			AssertValidSequenceComponent(sequencedTimeRange, "every hour", BaseDate);

			sequencedTimeRange = new SequencedTimeRange(new Months(BaseDate, (YearMonth)BaseDate.Month, 2));
			AssertValidSequenceComponent(sequencedTimeRange, "every 2 months", BaseDate);

			sequencedTimeRange = new SequencedTimeRange(new Days(BaseDate, 1));
			AssertValidSequenceComponent(sequencedTimeRange, "every day", BaseDate);

			sequencedTimeRange = new SequencedTimeRange(new Months(new DateTime(2017, 8, 1), YearMonth.August, 12));
			AssertValidSequenceComponent(sequencedTimeRange, "every August", BaseDate);
			
			sequencedTimeRange = new SequencedTimeRange(new TimeSpan(7, 0,0,0), new DateTime(2017, 1, 3));
			AssertValidSequenceComponent(sequencedTimeRange, "every Tuesday", BaseDate);

			AssertInvalidSequenceComponent("every MadeUpDay", BaseDate);
			AssertInvalidSequenceComponent("every second", BaseDate);
			AssertInvalidSequenceComponent("every 123 minutes", BaseDate);
		}

		private void AssertValidSequenceComponent(SequencedTimeRange expected, string input, DateTime baseDate)
		{
			var components = SequenceComponent.GetComponentsFromText(input);
			Assert.IsTrue(components.Any());
			var componentPeriod = components.FirstOrDefault().GetTimePeriod(baseDate) as SequencedTimeRange;
			Assert.IsNotNull(componentPeriod);
			Assert.AreEqual(expected.Start, componentPeriod.Start);
			Assert.AreEqual(expected.End, componentPeriod.End);

			var expectedSequence = expected.SequenceInterval;
			var parsedSequence = componentPeriod.SequenceInterval;
			Assert.AreEqual(expectedSequence.Start, parsedSequence.Start);
			Assert.AreEqual(expectedSequence.End, parsedSequence.End);
		}

		private void AssertInvalidSequenceComponent(string input, DateTime baseDate)
		{
			var components = SequenceComponent.GetComponentsFromText(input);
			var componentPeriod = components.Select(c => c.GetTimePeriod(baseDate)).FirstOrDefault();
			Assert.IsNull(componentPeriod);
		}
	}
}