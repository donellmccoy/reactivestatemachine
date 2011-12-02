using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReactiveStateMachine
{
    public static class ObservableExtensions
    {
        public static IIgnoringObservable<T> AsIgnoringObservable<T>(this IObservable<T> source)
        {
            return new IgnoringObservable<T>(source);
        }
    }
}
