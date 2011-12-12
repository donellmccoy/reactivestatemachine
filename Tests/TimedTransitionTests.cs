using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class TimedTransitionTests : AbstractReactiveStateMachineTest
    {
        IDisposable _stateChangedSubscription;

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

        [Test]
        public void TransitionActionOfTimedTransitionIsCalled()
        {
            var evt = new ManualResetEvent(false);
            var transitionActionCalled = false;

            Action transitionAction = () => transitionActionCalled = true;

            StateMachine.AddTimedTransition(TestStates.Collapsed, TestStates.FadingIn, TimeSpan.FromMilliseconds(1000), transitionAction);

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
            var evt = new ManualResetEvent(false);
            var transitionActionCalled = false;

            Action transitionAction = () => transitionActionCalled = true;

            StateMachine.AddTimedTransition(TestStates.Collapsed, TestStates.FadingIn, TimeSpan.FromMilliseconds(1000), () => true, transitionAction);

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
            var evt = new ManualResetEvent(false);
            var transitionActionCalled = false;

            Action transitionAction = () => transitionActionCalled = true;

            StateMachine.AddTimedTransition(TestStates.Collapsed, TestStates.FadingIn, TimeSpan.FromMilliseconds(1000), () => false, transitionAction);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            evt.WaitOne(3000);

            Assert.False(transitionActionCalled);
        }

        [Test]
        public void ExceptionInTransitionActionOfTimedTransitionIsHandledAndReported()
        {
            var evt = new ManualResetEvent(false);

            var exceptionHandledAndReported = false;

            var transitionAction = new Action(() => { throw new Exception(); });

            StateMachine.AddTimedTransition(TestStates.Collapsed, TestStates.FadingIn, TimeSpan.FromMilliseconds(1000), transitionAction);

            StateMachine.StateChanged += (sender, args) => evt.Set();

            StateMachine.StateMachineException += (sender, args) => exceptionHandledAndReported = true;

            StateMachine.Start();

            evt.WaitOne();

            Assert.True(exceptionHandledAndReported);
        }

        [Test]
        public void ExceptionInConditionOfTimedTransitionIsHandledAndReported()
        {
            var evt = new ManualResetEvent(false);

            var exceptionHandledAndReported = false;

            var condition = new Func<bool>(() => { throw new Exception(); });

            StateMachine.AddTimedTransition(TestStates.Collapsed, TestStates.FadingIn, TimeSpan.FromMilliseconds(1000), condition);

            StateMachine.StateMachineException += (sender, args) => exceptionHandledAndReported = true;

            StateMachine.Start();

            evt.WaitOne(3000);

            Assert.AreEqual(StateMachine.CurrentState, TestStates.Collapsed);
            Assert.True(exceptionHandledAndReported);
        }


    }
}
