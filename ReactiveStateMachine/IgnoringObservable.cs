using System;
using System.Reactive.Linq;

namespace ReactiveStateMachine
{
    public class IgnoringObservable<T> : IIgnoringObservable<T>
    {
        private bool _ignoring = true;

        private readonly IObservable<T> _source;

        public IgnoringObservable(IObservable<T> source)
        {
            _source = source;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _source.Where(t => !Ignoring()).Subscribe(observer);
        }

        private bool Ignoring()
        {
            return _ignoring;
        }

        public void Ignore()
        {
            _ignoring = true;
        }

        public void Resume()
        {
            _ignoring = false;
        }
    }
}
