using System;
using System.Linq;
using System.Windows.Input;
using ReactiveStateMachine;

namespace TouchStateMachine
{
    public class WindowsTouchStateMachine<T> : TrackingStateMachine<T>
    {
        public WindowsTouchStateMachine(String name, T startState) : base(name, startState, new WindowsTouchTracker())
        {
            
        }

        public int Count
        { 
            get
            {
                return InputTracker.Count;
            }
        }

        public bool Contains(TouchDevice point)
        {
            return InputTracker.Contains(point);
        }

        public bool First(TouchDevice point)
        {
            return InputTracker.First(point);
        }

        public bool Initial(TouchDevice point)
        {
            return InputTracker.Initial(point);
        }

        public bool Intermediate(TouchDevice point)
        {
            return InputTracker.Intermediate(point);
        }

        public bool Subsequent(TouchDevice point)
        {
            return InputTracker.Subsequent(point);
        }

        public bool Last(TouchDevice point)
        {
            return InputTracker.Last(point);
        }

        public bool AtPosition(TouchDevice point, int position)
        {
            return InputTracker.AtPosition(point, position);
        }

        public TouchDevice[] ActivePoints
        {
            get { return InputTracker.ActivePoints.Cast<TouchDevice>().ToArray(); }
        }
    }
}
