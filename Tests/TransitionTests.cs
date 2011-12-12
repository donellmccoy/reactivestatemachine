using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class TransitionTests : AbstractReactiveStateMachineTest
    {
        IDisposable _stateChangedSubscription;

        #region Automatic Transitions

        [Test]
        public void AutomaticTransitionIsMade()
        {
            var evt = new ManualResetEvent(false);
            var transitionMade = false;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                transitionMade = true;
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            evt.WaitOne();

            Assert.True(transitionMade);
        }

        [Test]
        public void AutomaticTransitionWithConditionIsMade()
        {
            var evt = new ManualResetEvent(false);
            var transitionMade = false;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn, () => true);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                transitionMade = true;
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            evt.WaitOne();

            Assert.True(transitionMade);
        }

        [Test]
        public void AutomaticTransitionWithConditionIsNotMade()
        {
            var evt = new ManualResetEvent(false);
            var transitionMade = false;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn, () => false);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                transitionMade = true;
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            evt.WaitOne(2000);

            Assert.False(transitionMade);
        }

        #endregion

        #region Triggered Transitions

        [Test]
        public void TriggeredTransitionIsMade()
        {
            var trigger = new Subject<Object>();
            var evt = new ManualResetEvent(false);
            var transitionMade = false;

            StateMachine.AddTransition(TestStates.Collapsed, TestStates.FadingIn, trigger);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                transitionMade = true;
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                trigger.OnNext(null);
            });

            evt.WaitOne();

            Assert.True(transitionMade);
        }

        [Test]
        public void TriggeredTransitionWithConditionIsMade()
        {
            var trigger = new Subject<Object>();
            var evt = new ManualResetEvent(false);
            var transitionMade = false;

            StateMachine.AddTransition(TestStates.Collapsed, TestStates.FadingIn, trigger, args => true);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                transitionMade = true;
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                trigger.OnNext(null);
            });

            evt.WaitOne();

            Assert.True(transitionMade);
        }

        [Test]
        public void TriggeredTransitionWithConditionIsNotMade()
        {
            var trigger = new Subject<Object>();
            var evt = new ManualResetEvent(false);
            var transitionMade = false;

            StateMachine.AddTransition(TestStates.Collapsed, TestStates.FadingIn, trigger, args => false);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                transitionMade = true;
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                trigger.OnNext(null);
            });

            evt.WaitOne(2000);

            Assert.False(transitionMade);
        }

        #endregion

        #region Timed Transitions

        [Test]
        public void TimedTransitionIsMade()
        {
            var evt = new ManualResetEvent(false);
            var transitionMade = false;

            StateMachine.AddTimedTransition(TestStates.Collapsed, TestStates.FadingIn, TimeSpan.FromMilliseconds(1000));

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                transitionMade = true;
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            evt.WaitOne();

            Assert.True(transitionMade);
        }

        [Test]
        public void TimedTransitionWithConditionIsMade()
        {
            var evt = new ManualResetEvent(false);
            var transitionMade = false;

            StateMachine.AddTimedTransition(TestStates.Collapsed, TestStates.FadingIn, TimeSpan.FromMilliseconds(1000), () => true);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                transitionMade = true;
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            evt.WaitOne();

            Assert.True(transitionMade);
        }

        [Test]
        public void TimedTransitionWithConditionIsNotMade()
        {
            var evt = new ManualResetEvent(false);
            var transitionMade = false;

            StateMachine.AddTimedTransition(TestStates.Collapsed, TestStates.FadingIn, TimeSpan.FromMilliseconds(1000), () => false);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                transitionMade = true;
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            evt.WaitOne(3000);

            Assert.False(transitionMade);
        }

        #endregion
    }
}
