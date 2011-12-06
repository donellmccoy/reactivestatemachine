using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class TransitionActionTests : AbstractReactiveStateMachineTest
    {
        IDisposable _stateChangedSubscription;

        #region Automatic Transitions

        [Test]
        public void TransitionActionOfAutomaticTransitionIsCalled()
        {
            var evt = new AutoResetEvent(false);
            var transitionActionCalled = false;

            Action transitionAction = () => transitionActionCalled = true;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn, transitionAction);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            evt.WaitOne();

            Assert.True(transitionActionCalled);
        }

        [Test]
        public void TransitionActionOfAutomaticTransitionWithConditionIsCalled()
        {
            var evt = new AutoResetEvent(false);
            var transitionActionCalled = false;

            Action transitionAction = () => transitionActionCalled = true;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn, () => true, transitionAction);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            evt.WaitOne();

            Assert.True(transitionActionCalled);
        }

        [Test]
        public void TransitionActionOfAutomaticTransitionWithConditionIsNotCalled()
        {
            var evt = new AutoResetEvent(false);
            var transitionActionCalled = false;

            Action transitionAction = () => transitionActionCalled = true;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn, () => false, transitionAction);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            evt.WaitOne(2000);

            Assert.False(transitionActionCalled);
        }

        #endregion

        #region Triggered Transitions

        [Test]
        public void TransitionActionOfTriggeredTransitionIsCalled()
        {
            var trigger = new Subject<object>();
            var evt = new AutoResetEvent(false);
            var transitionActionCalled = false;

            Action<object> transitionAction = u => transitionActionCalled = true;

            StateMachine.AddTransition(TestStates.Collapsed, TestStates.FadingIn, trigger, transitionAction);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
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

            Assert.True(transitionActionCalled);
        }

        [Test]
        public void TransitionActionOfTriggeredTransitionWithConditionIsCalled()
        {
            var trigger = new Subject<object>();
            var evt = new AutoResetEvent(false);
            var transitionActionCalled = false;

            Action<object> transitionAction = u => transitionActionCalled = true;

            StateMachine.AddTransition(TestStates.Collapsed, TestStates.FadingIn, trigger, args => true, transitionAction);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
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

            Assert.True(transitionActionCalled);
        }

        [Test]
        public void TransitionActionOfTriggeredTransitionWithConditionIsNotCalled()
        {
            var trigger = new Subject<object>();
            var evt = new AutoResetEvent(false);
            var transitionActionCalled = false;

            Action<object> transitionAction = u => transitionActionCalled = true;

            StateMachine.AddTransition(TestStates.Collapsed, TestStates.FadingIn, trigger, args => false, transitionAction);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
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

            Assert.False(transitionActionCalled);
        }

        #endregion

        #region Timed Transitions

        [Test]
        public void TransitionActionOfTimedTransitionIsCalled()
        {
            var evt = new AutoResetEvent(false);
            var transitionActionCalled = false;

            Action transitionAction = () => transitionActionCalled = true;

            StateMachine.AddTransition(TestStates.Collapsed, TestStates.FadingIn, TimeSpan.FromMilliseconds(1000), transitionAction);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            evt.WaitOne();

            Assert.True(transitionActionCalled);
        }

        [Test]
        public void TransitionActionOfTimedTransitionWithConditionIsCalled()
        {
            var evt = new AutoResetEvent(false);
            var transitionActionCalled = false;

            Action transitionAction = () => transitionActionCalled = true;

            StateMachine.AddTransition(TestStates.Collapsed, TestStates.FadingIn, TimeSpan.FromMilliseconds(1000), () => true, transitionAction);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            evt.WaitOne();

            Assert.True(transitionActionCalled);
        }

        [Test]
        public void TransitionActionOfTimedTransitionWithConditionIsNotCalled()
        {
            var evt = new AutoResetEvent(false);
            var transitionActionCalled = false;

            Action transitionAction = () => transitionActionCalled = true;

            StateMachine.AddTransition(TestStates.Collapsed, TestStates.FadingIn, TimeSpan.FromMilliseconds(1000), () => false, transitionAction);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            evt.WaitOne(4000);

            Assert.False(transitionActionCalled);
        }

        #endregion

    }
}
