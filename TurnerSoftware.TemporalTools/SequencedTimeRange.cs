using Itenso.TimePeriod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurnerSoftware.TemporalTools
{
	public class SequencedTimeRange : TimeRange, ISequencedTimeRange
	{
		private ITimeRange sequenceInterval;

		public SequencedTimeRange(TimeSpan sequenceInterval, bool isReadOnly = false) : this(sequenceInterval, TimeSpec.MinPeriodDate, TimeSpec.MaxPeriodDate, isReadOnly) { }
		public SequencedTimeRange(TimeSpan sequenceInterval, DateTime start, bool isReadOnly = false) : this(sequenceInterval, start, TimeSpec.MaxPeriodDate, isReadOnly) { }
		public SequencedTimeRange(TimeSpan sequenceInterval, DateTime start, DateTime end, bool isReadOnly = false) : base(start, end, isReadOnly)
		{
			if (sequenceInterval < TimeSpec.MinPeriodDuration)
			{
				throw new ArgumentOutOfRangeException("sequenceInterval");
			}

			this.sequenceInterval = new CalendarTimeRange(start, sequenceInterval);
		}

		public SequencedTimeRange(ITimeRange sequenceInterval, bool isReadOnly = false) : this(sequenceInterval, TimeSpec.MaxPeriodDate, isReadOnly) { }
		public SequencedTimeRange(ITimeRange sequenceInterval, DateTime end, bool isReadOnly = false) : base(sequenceInterval.Start, end, isReadOnly)
		{
			this.sequenceInterval = sequenceInterval;
		}

		public ITimeRange SequenceInterval
		{
			get
			{
				return sequenceInterval;
			}
		}

		public override void Setup(DateTime newStart, DateTime newEnd)
		{
			base.Setup(newStart, newEnd);

			if (sequenceInterval is YearTimeRange)
			{
				var yearCount = (sequenceInterval as YearTimeRange).YearCount;
				sequenceInterval = new Years(newStart, yearCount);
			}
			else if (sequenceInterval is MonthTimeRange)
			{
				var monthCount = (sequenceInterval as MonthTimeRange).MonthCount;
				sequenceInterval = new Months(newStart, (YearMonth)newStart.Month, monthCount);
			}
			else
			{
				var duration = sequenceInterval.Duration + TimeSpec.MinPositiveDuration;
				sequenceInterval = new CalendarTimeRange(newStart, duration);
			}
		}

		private long GetLCDSequenceInterval()
		{
			if (sequenceInterval is YearTimeRange)
			{
				return (sequenceInterval as YearTimeRange).YearCount;
			}
			else if (sequenceInterval is MonthTimeRange)
			{
				return (sequenceInterval as MonthTimeRange).MonthCount;
			}
			else
			{
				return sequenceInterval.Duration.Ticks;
			}
		}

		private long GetLCDFromDate(DateTime date)
		{
			if (sequenceInterval is YearTimeRange)
			{
				return date.Year;
			}
			else if (sequenceInterval is MonthTimeRange)
			{
				return date.Year * 12 + date.Month;
			}
			else
			{
				return date.Ticks;
			}
		}

		private DateTime GetDateFromLCD(long lowestCommonDenominator)
		{
			if (sequenceInterval is YearTimeRange)
			{
				return new DateTime((int)lowestCommonDenominator, Start.Month, Start.Day);
			}
			else if (sequenceInterval is MonthTimeRange)
			{
				var rawMonth = lowestCommonDenominator % 12;
				var rawYear = (lowestCommonDenominator - rawMonth) / 12;
				var month = Math.Min((int)rawMonth, 1);
				var year = Math.Min((int)rawYear, 1);
				var maxDays = DateTime.DaysInMonth(year, month);
				var day = Math.Min(Start.Day, maxDays);
				return new DateTime(year, month, day);
			}
			else
			{
				return new DateTime(lowestCommonDenominator);
			}
		}

		public ITimeBlock GetClosestMoment(DateTime date)
		{
			if (date < Start)
			{
				return GetNextMoment(date);
			}
			else if (date > End)
			{
				return GetPreviousMoment(date);
			}
			else
			{
				var lcdValue = GetLCDFromDate(date);
				var interval = GetLCDSequenceInterval();
				var timeToAnyMoment = (lcdValue % interval);
				var intervalMidpoint = interval / 2;
				var newLcdValue = lcdValue;

				if (timeToAnyMoment >= intervalMidpoint)
				{
					newLcdValue += timeToAnyMoment;
				}
				else if (timeToAnyMoment > 0)
				{
					newLcdValue -= timeToAnyMoment;
				}
				
				var closestMoment = GetDateFromLCD(newLcdValue);
				return new TimeBlock(closestMoment, true);
			}
		}

		public ITimeBlock GetNextMoment(ITimePeriod after)
		{
			return GetNextMoment(after.End);
		}

		public ITimeBlock GetNextMoment(DateTime after)
		{
			if (after > End)
			{
				return null;
			}

			if (after < Start)
			{
				after = Start;
			}
			
			var lcdValue = GetLCDFromDate(after);
			var interval = GetLCDSequenceInterval();
			var timeToNextMoment = (lcdValue % interval) + interval;
			var nextLcdValue = lcdValue + timeToNextMoment;
			var nextMoment = GetDateFromLCD(nextLcdValue);

			return new TimeBlock(nextMoment, true);
		}

		public ITimeBlock GetPreviousMoment(ITimePeriod before)
		{
			return GetNextMoment(before.Start);
		}

		public ITimeBlock GetPreviousMoment(DateTime before)
		{
			if (before < Start)
			{
				return null;
			}

			if (before > End)
			{
				before = End;
			}

			var lcdValue = GetLCDFromDate(before);
			var interval = GetLCDSequenceInterval();
			var timeToPreviousMoment = (lcdValue % interval) - interval;
			var previousLcdValue = lcdValue + timeToPreviousMoment;
			var previousMoment = GetDateFromLCD(previousLcdValue);

			return new TimeBlock(previousMoment, true);
		}

		public IEnumerable<ITimeBlock> GetSequence()
		{
			return GetSequence(Start, End);
		}

		public IEnumerable<ITimeBlock> GetSequence(ITimePeriod between)
		{
			return GetSequence(between.Start, between.End);
		}

		public IEnumerable<ITimeBlock> GetSequence(DateTime start, DateTime end)
		{
			ITimeBlock nextMoment = new TimeBlock(start);

			if (!IsMomentInSequence(nextMoment))
			{
				nextMoment = GetNextMoment(start);
			}

			while (IsMomentInSequence(nextMoment) && nextMoment.Start < end)
			{
				yield return nextMoment;
				nextMoment = GetNextMoment(nextMoment);
			}
		}

		public bool IsMomentInSequence(ITimePeriod period)
		{
			if (period == null)
			{
				return false;
			}
			return IsMomentInSequence(period.Start);
		}

		public bool IsMomentInSequence(DateTime date)
		{
			if (date < Start || date > End)
			{
				return false;
			}

			var lcdValue = GetLCDFromDate(date);
			var interval = GetLCDSequenceInterval();
			var timeToAnyMoment = (lcdValue % interval);
			return timeToAnyMoment == 0;
		}
	}
}
