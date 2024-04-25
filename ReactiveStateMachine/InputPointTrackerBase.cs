using System;
using System.Collections.Generic;
using System.Linq;

namespace ReactiveStateMachine
{
    public class InputPointTrackerBase : IInputPointTracker
    {
        private readonly List<object> _activePoints = new List<object>();

        private readonly object _activePointsLocker = new object();

        public virtual void Track(EventArgs e)
        {
            
        }

        protected void AddPoint(object point)
        {
            if (point == null)
                throw new ArgumentNullException("point");

            lock(_activePointsLocker)
            {
                if (_activePoints.Count == 0)
                    _initial = point;

                _activePoints.Add(point);
            }
        }

        protected void RemovePoint(object point)
        {
            if (point == null)
                throw new ArgumentNullException("point");

            lock (_activePointsLocker)
            {
                _activePoints.Remove(point);
            }
        }

        private object _initial;

        public int Count
        {
            get
            {
                lock(_activePointsLocker)
                    return _activePoints.Count;
            }
        }

        public bool Contains(object point)
        {
            if (point == null)
                throw new ArgumentNullException("point");

            lock (_activePointsLocker)
                return _activePoints.Contains(point);
        }

        public bool First(object point)
        {
            if (point == null)
                throw new ArgumentNullException("point");

            lock (_activePointsLocker)
                return _activePoints.FirstOrDefault() == point;
        }

        public bool Initial(object point)
        {
            if (point == null)
                throw new ArgumentNullException("point");

            lock (_activePointsLocker)
                return _initial == point;
        }

        public bool Intermediate(object point)
        {
            if (point == null)
                throw new ArgumentNullException("point");

            lock (_activePointsLocker)
                return _activePoints.FirstOrDefault() != point && _activePoints.LastOrDefault() != point && _activePoints.Contains(point);
        }

        public bool Subsequent(object point)
        {
            if (point == null)
                throw new ArgumentNullException("point");

            lock (_activePointsLocker)
                return _activePoints.Contains(point) && _activePoints.FirstOrDefault() != point;
        }

        public bool Last(object point)
        {
            if (point == null)
                throw new ArgumentNullException("point");

            lock (_activePointsLocker)
                return _activePoints.LastOrDefault() == point;
        }

        public bool AtPosition(object point, int position)
        {
            if (point == null)
                throw new ArgumentNullException("point");

            lock (_activePointsLocker)
                return _activePoints.ElementAtOrDefault(position) == point;
        }

        public object[] ActivePoints
        {
            get
            {
                lock (_activePointsLocker)
                    return _activePoints.ToArray();
            }
        }
    }
}
