using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Surface.Presentation;
using ReactiveStateMachine;

namespace TouchStateMachine
{
    public class SurfaceTouchTracker : ITouchTracker
    {
        private static PropertyInfo _inputArgsProperty;
        private static PropertyInfo _inputRecordProperty;
        private static PropertyInfo _contactActionProperty;
        private static object _contactAdd;
        private static object _contactRemove;

        public SurfaceTouchTracker()
        {
            var contactType = typeof (Contact);
            _inputArgsProperty = contactType.GetProperty("InputArgs", BindingFlags.NonPublic | BindingFlags.Instance);
            _inputRecordProperty = contactType.Assembly.GetType("Microsoft.Surface.Presentation.RawInputSurfaceEventArgs", true).GetProperty("InputRecord");
            _contactActionProperty = contactType.Assembly.GetType("Microsoft.Surface.Presentation.RawInputContactRecord").GetProperty("Action");

            var enumValues = contactType.Assembly.GetType("Microsoft.Surface.Presentation.RawContactAction").GetEnumValues();

            _contactAdd = enumValues.GetValue(0);
            _contactRemove = enumValues.GetValue(2);
        }

        private readonly List<Contact> _activeContacts = new List<Contact>();

        public void Track(EventArgs e)
        {
            if (!(e is ContactEventArgs))
                return;

            var args = e as ContactEventArgs;

            var rawInputEventArgs = _inputArgsProperty.GetValue(args.Contact, null);
            var inputRecord = _inputRecordProperty.GetValue(rawInputEventArgs, null);
            var contactAction = _contactActionProperty.GetValue(inputRecord, null);

            if (contactAction.Equals(_contactAdd) && !_activeContacts.Contains(args.Contact))
                _activeContacts.Add(args.Contact);
            else if (contactAction.Equals(_contactRemove) && _activeContacts.Contains(args.Contact))
                _activeContacts.Remove(args.Contact);
        }

        public int Count
        {
            get { return _activeContacts.Count; }
        }

        object IInputPointTracker.OldestPoint
        {
            get { return _activeContacts.OrderBy(c => c.FrameTimestamp).FirstOrDefault(); }
        }

        object IInputPointTracker.NewestPoint
        {
            get { return _activeContacts.OrderByDescending(c => c.FrameTimestamp).FirstOrDefault(); }
        }

        public object[] ActivePoints
        {
            get { return _activeContacts.ToArray(); }
        }
    }
}
