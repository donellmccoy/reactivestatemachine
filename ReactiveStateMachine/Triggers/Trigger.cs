using System;

namespace ReactiveStateMachine.Triggers
{
    public class Trigger<TTrigger>
    {
        public Trigger(IObservable<TTrigger> source)
        {
            if (source is IIgnoringObservable<TTrigger> observable)
            {
                Sequence = observable;
            }
            else
            {
                Sequence = new IgnoringObservable<TTrigger>(source);
            }
        }

        public IIgnoringObservable<TTrigger> Sequence { get; private set; }
    }
}
