using Itenso.TimePeriod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurnerSoftware.TemporalTools
{
	public interface ISequencedTimeRange : ITimeRange
	{
		ITimeBlock GetClosestMoment(DateTime date);
		bool IsMomentInSequence(DateTime date);
		bool IsMomentInSequence(ITimePeriod period);
		ITimeBlock GetNextMoment(DateTime after);
		ITimeBlock GetNextMoment(ITimePeriod after);
		ITimeBlock GetPreviousMoment(DateTime before);
		ITimeBlock GetPreviousMoment(ITimePeriod before);
		IEnumerable<ITimeBlock> GetSequence();
		IEnumerable<ITimeBlock> GetSequence(DateTime start, DateTime end);
		IEnumerable<ITimeBlock> GetSequence(ITimePeriod between);
	}
}
