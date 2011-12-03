using System;

namespace ReactiveStateMachine
{
    public interface IInputPointTracker
    {
        void Track(EventArgs e);
        int Count { get; }
        object OldestPoint {get;}
        object NewestPoint { get; }
        object[] ActivePoints { get; }
    }
}
