using System;

namespace ReactiveStateMachine
{
    public interface IIgnoringObservable<out T> : IObservable<T>
    {
        void Ignore();
        void Resume();
    }
}
