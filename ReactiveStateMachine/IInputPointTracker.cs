using System;

namespace ReactiveStateMachine
{
    public interface IInputPointTracker
    {
        void Track(EventArgs e);

        int Count { get; }

        bool Contains(object point);

        bool First(object point);
        bool Initial(object point);
        bool Intermediate(object point);
        bool Subsequent(object point);
        bool Last(object point);
        bool AtPosition(object point, int position);

        object[] ActivePoints { get; }
    }
}
