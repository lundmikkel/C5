using System;
using System.Collections.Generic;
using System.Linq;

namespace C5.intervals
{
    class IntervalList<T> : CollectionValueBase<IInterval<T>>, IStaticIntervaled<T> where T : IComparable<T>
    {
        private readonly IEnumerable<IInterval<T>> _intervals;

        public IntervalList(IEnumerable<IInterval<T>> intervals)
        {
            _intervals = intervals;
        }

        public override bool IsEmpty
        {
            get { return Count == 0; }
        }

        public override int Count
        {
            get { return _intervals.Count(); }
        }

        public override Speed CountSpeed
        {
            get { return Speed.Constant; }
        }

        public override IInterval<T> Choose()
        {
            if (Count > 0)
                return _intervals.First();

            throw new NoSuchItemException();
        }

        public int CountOverlaps(IInterval<T> query)
        {
            return FindOverlaps(query).Count();
        }

        public override IEnumerator<IInterval<T>> GetEnumerator()
        {
            return _intervals.ToArray().Cast<IInterval<T>>().GetEnumerator();
        }

        public IInterval<T> Span { get; private set; }

        public IEnumerable<IInterval<T>> FindOverlaps(T query)
        {
            return FindOverlaps(new IntervalBase<T>(query));
        }

        public IEnumerable<IInterval<T>> FindOverlaps(IInterval<T> query)
        {
            return _intervals.Where(interval => interval.Overlaps(query));
        }

        public bool OverlapExists(IInterval<T> query)
        {
            return _intervals.Any(interval => interval.Overlaps(query));
        }
    }
}
