using Itenso.TimePeriod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TurnerSoftware.TemporalTools.Components;

namespace TurnerSoftware.TemporalTools
{
	public class TemporalParser
	{
		private class ProcessedComponent
		{
			public TemporalComponent Component { get; set; }
			public ITimePeriod TimePeriod { get; set; }
		}

		public IEnumerable<ITimePeriod> GetTimePeriodsFromText(string text, TemporalParserOptions options)
		{
			var components = GetComponentsFromText(text);
			var groupedComponents = TemporalComponent.GroupComponents(components);
			var result = new List<ITimePeriod>();

			foreach (var componentGroup in groupedComponents)
			{
				var tmpTimePeriod = GetTimePeriodFromComponents(componentGroup, options);
				if (tmpTimePeriod != null)
				{
					result.Add(tmpTimePeriod);
				}
			}

			return result;
		}

		private IEnumerable<TemporalComponent> GetComponentsFromText(string text)
		{
			var components = new List<TemporalComponent>();

			var dateComponents = DateComponent.GetComponentsFromText(text);
			components.AddRange(dateComponents);

			var timeComponents = TimeComponent.GetComponentsFromText(text);
			components.AddRange(timeComponents);
			
			var sequenceComponents = SequenceComponent.GetComponentsFromText(text);
			components.AddRange(sequenceComponents);

			var fuzzyComponents = FuzzyComponent.GetComponentsFromText(text);
			components.AddRange(fuzzyComponents);

			var durationComponents = DurationComponent.GetComponentsFromText(text);
			components.AddRange(durationComponents);

			var relativeComponents = RelativeDateTimeComponent.GetComponentsFromText(text);
			components.AddRange(relativeComponents);

			return components;
		}

		private ITimePeriod GetTimePeriodFromComponents(IEnumerable<TemporalComponent> components, TemporalParserOptions options)
		{
			//TODO: Overhaul this so it actually is smart! It currently can't:
			//		- Match all component types together
			//		- Handle variations of types (eg. open-ended date with open-ended time)
			var processedComponents = components.Select(c => new ProcessedComponent
			{
				Component = c,
				TimePeriod = c.GetTimePeriod(options.BaseDate)
			}).Where(c => c.TimePeriod != null).ToList();
			
			if (processedComponents.Count() == 0)
			{
				return null;
			}
			else if (processedComponents.Count() == 1)
			{
				return processedComponents.FirstOrDefault().TimePeriod;
			}

			var dateComponents = processedComponents.Where(c => c.Component is DateComponent);
			var timeComponents = processedComponents.Where(c => c.Component is TimeComponent);
			var sequenceComponents = processedComponents.Where(c => c.Component is SequenceComponent);
			var fuzzyComponents = processedComponents.Where(c => c.Component is FuzzyComponent);
			//var durationComponents = processedComponents.Where(c => c.Component is DurationComponent);
			//var relativeComponents = processedComponents.Where(c => c.Component is RelativeDateTimeComponent);

			if (components.Any(c => c is SequenceComponent))
			{
				var dateComponent = dateComponents.FirstOrDefault();
				var timeComponent = timeComponents.FirstOrDefault();
				var sequenceComponent = sequenceComponents.FirstOrDefault();
				if (sequenceComponent != null)
				{
					return TransformSequenceDateTime(
						sequenceComponent,
						dateComponent,
						timeComponent
					);
				}
			}
			else if (components.Count() == 2 && components.Any(c => c is DateComponent) && components.Any(c => c is TimeComponent))
			{
				var dateComponent = dateComponents.FirstOrDefault();
				var timeComponent = timeComponents.FirstOrDefault();
				if (dateComponent != null && timeComponent != null)
				{
					return TransformDateTimeComponents(dateComponent.TimePeriod, timeComponent.TimePeriod);
				}
			}
			else if (components.Count() == 2 && components.Any(c => c is FuzzyComponent) && components.Any(c => c is TimeComponent))
			{
				var fuzzyComponent = fuzzyComponents.FirstOrDefault();
				var timeComponent = timeComponents.FirstOrDefault();
				if (fuzzyComponent != null && timeComponent != null)
				{
					return TransformDateTimeComponents(fuzzyComponent.TimePeriod, timeComponent.TimePeriod);
				}
			}

			return null;
		}

		/// <summary>
		/// Roughly converts a date period with a time period into one period
		/// </summary>
		/// <param name="datePeriod"></param>
		/// <param name="timePeriod"></param>
		/// <returns></returns>
		private ITimePeriod TransformDateTimeComponents(ITimePeriod datePeriod, ITimePeriod timePeriod)
		{
			var pointInTime = datePeriod.Start.Add(timePeriod.Start.TimeOfDay);
			if (timePeriod.HasStart && timePeriod.HasEnd)
			{
				return new Minute(pointInTime);
			}
			else if (timePeriod.HasStart)
			{
				return new TimeRange(pointInTime, new Day(pointInTime).End);
			}
			else if (timePeriod.HasEnd)
			{
				return new TimeRange(pointInTime, timePeriod.End.TimeOfDay);
			}
			return datePeriod;
		}
		
		/// <summary>
		/// Roughly converts sequence, date and time components into a single time period
		/// </summary>
		/// <param name="sequenceComponent"></param>
		/// <param name="dateComponent"></param>
		/// <param name="timeComponent"></param>
		/// <returns></returns>
		private ITimePeriod TransformSequenceDateTime(ProcessedComponent sequenceComponent, ProcessedComponent dateComponent = null, ProcessedComponent timeComponent = null)
		{
			ITimePeriod sequencePeriod = sequenceComponent.TimePeriod;
			DateTime sequenceStart = sequenceComponent.TimePeriod.Start;
			DateTime sequenceEnd = sequenceComponent.TimePeriod.End;

			if (dateComponent != null)
			{
				if ((dateComponent.Component as DateComponent).IsUntil)
				{
					sequenceEnd = dateComponent.TimePeriod.Start;
				}
				else if (dateComponent.TimePeriod.HasStart && !dateComponent.TimePeriod.HasEnd)
				{
					//aka. after [Date]
					sequencePeriod = sequenceComponent.Component.GetTimePeriod(dateComponent.TimePeriod.Start);
					sequenceStart = sequencePeriod.Start;
				}
			}

			if (timeComponent != null)
			{
				if ((timeComponent.Component as TimeComponent).IsUntil)
				{
					sequenceEnd = sequenceEnd + timeComponent.TimePeriod.Duration;
				}
				else
				{
					sequenceStart = sequenceStart + timeComponent.TimePeriod.Start.TimeOfDay;
				}
			}

			sequencePeriod.Setup(sequenceStart, sequenceEnd);

			return sequencePeriod;
		}
	}

	public class TemporalParserOptions
	{
		public DateTime BaseDate { get; set; } = DateTime.UtcNow;
	}
}
