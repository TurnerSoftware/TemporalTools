using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Itenso.TimePeriod;
using TurnerSoftware.TemporalTools;

namespace TurnerSoftware.TemporalTools.Tests
{
	[TestClass()]
	public class TemporalParserTests
	{
		[TestMethod()]
		public void GetTimePeriodsFromTextTest_DateTime()
		{
			var options = new TemporalParserOptions
			{
				BaseDate = new DateTime(2017, 1, 1)
			};

			AssertValidParsedText(new List<ITimePeriod>
			{
				new Minute(2017, 1, 3, 7, 30)
			}, "At 7:30 on Tuesday", options);
			AssertValidParsedText(new List<ITimePeriod>
			{
				new Minute(2017, 1, 7, 1, 0)
			}, "At 1 o'clock on Saturday", options);
			AssertValidParsedText(new List<ITimePeriod>
			{
				new Minute(2017, 1, 4, 17, 0)
			}, "On Wednesday at 5pm", options);
			AssertValidParsedText(new List<ITimePeriod>
			{
				new Minute(2017, 1, 6, 4, 0)
			}, "Friday at 4am", options);
			AssertValidParsedText(new List<ITimePeriod>
			{
				new TimeRange(new DateTime(2017, 1, 2), new DateTime(2017, 1, 2, 15, 30, 0))
			}, "Before 3:30pm on Monday", options);
			AssertValidParsedText(new List<ITimePeriod>
			{
				new TimeRange(new DateTime(2017, 1, 6, 2, 30, 0), new Day(2017, 1, 6).End)
			}, "After 2:30 on Friday", options);
		}

		[TestMethod()]
		public void GetTimePeriodsFromTextTest_SequenceDateTime()
		{
			var options = new TemporalParserOptions
			{
				BaseDate = new DateTime(2017, 1, 1)
			};

			AssertValidParsedText(new List<ITimePeriod>
			{
				new SequencedTimeRange(new TimeSpan(7, 0, 0, 0), new DateTime(2017, 1, 3), new DateTime(2017, 1, 10))
			}, "Every Tuesday until the 10th", options);

			AssertValidParsedText(new List<ITimePeriod>
			{
				new SequencedTimeRange(new TimeSpan(7, 0, 0, 0), new DateTime(2017, 1, 6, 9, 35, 0))
			}, "Every Friday at 9:35am", options);

			AssertValidParsedText(new List<ITimePeriod>
			{
				new SequencedTimeRange(new TimeSpan(7, 0, 0, 0), new DateTime(2017, 1, 31, 16, 0, 0))
			}, "Every Tuesday after the 26th at 4pm", options);
		}

		private void AssertValidParsedText(IEnumerable<ITimePeriod> expected, string input, TemporalParserOptions options)
		{
			var parser = new TemporalParser();
			var parsedComponents = parser.GetTimePeriodsFromText(input, options).ToList();
			var expectedComponents = expected.ToList();

			Assert.AreEqual(expectedComponents.Count, parsedComponents.Count);

			for (int i = 0, l = parsedComponents.Count; i < l; i++)
			{
				var parsedComponent = parsedComponents[i];
				var expectedComponent = expectedComponents[i];
				
				Assert.AreEqual(expectedComponent.Start, parsedComponent.Start);
				Assert.AreEqual(expectedComponent.End, parsedComponent.End);

				if (expectedComponent is SequencedTimeRange)
				{
					Assert.IsInstanceOfType(parsedComponent, typeof(SequencedTimeRange));
					var expectedSequence = (expectedComponent as SequencedTimeRange).SequenceInterval;
					var parsedSequence = (parsedComponent as SequencedTimeRange).SequenceInterval;
					Assert.AreEqual(expectedSequence.Start, parsedSequence.Start);
					Assert.AreEqual(expectedSequence.End, parsedSequence.End);
				}
			}
		}
	}
}