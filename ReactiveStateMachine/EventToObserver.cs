﻿using System.Reflection;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace ReactiveStateMachine
{
    public class EventToObserver : TriggerAction<FrameworkElement>
    {
        private MethodInfo _onNextMethod;

        protected override void Invoke(object parameter)
        {
            _onNextMethod?.Invoke(Observer, new[] { parameter });
        }

        #region Observer

        /// <summary>
        /// Observer Dependency Property
        /// </summary>
        public static readonly DependencyProperty ObserverProperty =
            DependencyProperty.Register("Observer", typeof(object), typeof(EventToObserver),
                new FrameworkPropertyMetadata(null, OnObserverChanged));

        /// <summary>
        /// Gets or sets the Observer property. This dependency property 
        /// indicates ....
        /// </summary>
        public object Observer
        {
            get { return GetValue(ObserverProperty); }
            set { SetValue(ObserverProperty, value); }
        }

        /// <summary>
        /// Handles changes to the Observer property.
        /// </summary>
        private static void OnObserverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (EventToObserver)d;
            var oldObserver = e.OldValue;
            var newObserver = target.Observer;
            target.OnObserverChanged(oldObserver, newObserver);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Observer property.
        /// </summary>
        protected virtual void OnObserverChanged(object oldObserver, object newObserver)
        {
            if (newObserver != null)
            {
                _onNextMethod = Observer.GetType().GetMethod("OnNext");
            }
        }

        #endregion
    }
}
