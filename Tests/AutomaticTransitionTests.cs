using System;
using System.Reactive.Linq;
using System.Threading;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class AutomaticTransitionTests : AbstractReactiveStateMachineTest
    {
        private IDisposable _stateChangedSubscription;

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

        [Test]
        public void TransitionActionOfAutomaticTransitionIsCalled()
        {
            var evt = new ManualResetEvent(false);
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
            var evt = new ManualResetEvent(false);
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
            var evt = new ManualResetEvent(false);
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

        [Test]
        public void ExceptionInTransitionActionOfAutomaticTransitionIsHandledAndReported()
        {
            var evt = new ManualResetEvent(false);

            var exceptionHandledAndReported = false;

            var transitionAction = new Action(() => { throw new Exception(); });

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn, transitionAction);

            StateMachine.StateChanged += (sender, args) => evt.Set();
            
            StateMachine.StateMachineException += (sender, args) => exceptionHandledAndReported = true;

            StateMachine.Start();

            evt.WaitOne();

            Assert.True(exceptionHandledAndReported);
        }

        [Test]
        public void ExceptionInConditionOfAutomaticTransitionIsHandledAndReported()
        {
            var evt = new ManualResetEvent(false);

            var exceptionHandledAndReported = false;

            var condition = new Func<bool>(() => { throw new Exception(); });

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn, condition);

            StateMachine.StateMachineException += (sender, args) => exceptionHandledAndReported = true;

            StateMachine.Start();

            evt.WaitOne(2000);

            Assert.AreEqual(StateMachine.CurrentState,TestStates.Collapsed);
            Assert.True(exceptionHandledAndReported);
        }

    }
}
