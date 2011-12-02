using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

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
            return _source.Where(t => !_ignoring).Subscribe(observer);
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
