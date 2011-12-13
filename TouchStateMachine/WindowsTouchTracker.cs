using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ReactiveStateMachine;

namespace TouchStateMachine
{
    public class WindowsTouchTracker : ITouchTracker
    {
        private readonly List<Tuple<int, TouchDevice>> _activeContacts = new List<Tuple<int,TouchDevice>>();

        public void Track(EventArgs e)
        {
            if (!(e is TouchEventArgs))
                return;

            var args = e as TouchEventArgs;

            var contactAction = args.TouchDevice.GetTouchPoint(null).Action;

            if (contactAction == TouchAction.Down)
                _activeContacts.Add(new Tuple<int, TouchDevice>(args.Timestamp,args.TouchDevice));
            else if (contactAction == TouchAction.Up)
            {
                var tuple = _activeContacts.Where(t => t.Item2 == args.TouchDevice).SingleOrDefault();
                if(tuple != null)
                    _activeContacts.Remove(tuple);
            }
        }

        public int Count
        {
            get { return _activeContacts.Count; }
        }

        object IInputPointTracker.OldestPoint
        {
            get { return _activeContacts.OrderBy(t => t.Item1).Select(t => t.Item2).FirstOrDefault(); }
        }

        object IInputPointTracker.NewestPoint
        {
            get { return _activeContacts.OrderByDescending(t => t.Item1).Select(t => t.Item2).FirstOrDefault(); }
        }

        public bool ContainsContact(object contact)
        {
            return _activeContacts.Where(tuple => tuple.Item2 == contact).Count() == 1;
        }

        public object[] ActivePoints
        {
            get { return _activeContacts.Select(t => t.Item2).ToArray(); }
        }
    }
}
