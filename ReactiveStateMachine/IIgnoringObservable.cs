using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReactiveStateMachine
{
    public interface IIgnoringObservable<out T> : IObservable<T>
    {
        void Ignore();
        void Resume();
    }
}
