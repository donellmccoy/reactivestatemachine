using System;
using System.Reactive.Linq;

namespace ReactiveStateMachine.Triggers
{
    public class EventTrigger<T> : Trigger<T> where T:EventArgs
    {
        public EventTrigger(object target, string eventName) : base(Observable.FromEventPattern<T>(target, eventName).Select(evt => evt.EventArgs).AsIgnoringObservable())
        {
            
        }
    }
}
