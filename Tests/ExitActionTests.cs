using System;
using System.Reactive.Linq;
using System.Threading;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class ExitActionTests : AbstractReactiveStateMachineTest
    {
        IDisposable _stateChangedSubscription;

        [Test]
        public void SingleExitActionIsCalled()
        {
            var evt = new ManualResetEvent(false);
            var exitActionCalled = false;

            Action exitAction = () => exitActionCalled = true;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn);
            StateMachine.AddExitAction(TestStates.Collapsed, exitAction);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            evt.WaitOne();

            Assert.True(exitActionCalled);
        }

        [Test]
        public void MultipleExitActionsAreCalled()
        {
            var evt = new ManualResetEvent(false);
            const int numExitActionsToCall = 10;
            var numExitActionsCalled = 0;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn);

            for (int i = 0; i < numExitActionsToCall; i++)
            {
                Action exitAction = () => numExitActionsCalled++;
                StateMachine.AddExitAction(TestStates.Collapsed, exitAction);
            }

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            evt.WaitOne();

            Assert.AreEqual(numExitActionsToCall, numExitActionsCalled);
        }

        [Test]
        public void ConditionalExitActionIsCalled()
        {
            var evt = new ManualResetEvent(false);
            var exitActionCalled = false;

            Action exitAction = () => exitActionCalled = true;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn);
            StateMachine.AddExitAction(TestStates.Collapsed, exitAction, () => true);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            evt.WaitOne();

            Assert.True(exitActionCalled);
        }

        [Test]
        public void ConditionalExitActionIsNotCalled()
        {
            var evt = new ManualResetEvent(false);
            var exitActionCalled = false;

            Action exitAction = () => exitActionCalled = true;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn);
            StateMachine.AddExitAction(TestStates.Collapsed, exitAction, () => false);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                evt.Set();
                _stateChangedSubscription.Dispose();
            });

            StateMachine.Start();

            evt.WaitOne();

            Assert.False(exitActionCalled);
        }

        [Test]
        public void ExitActionIsCalledOnSpecificTransition()
        {
            var evt = new ManualResetEvent(false);
            var exitActionCalled = false;

            Action exitAction = () => exitActionCalled = true;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn);
            StateMachine.AddExitAction(TestStates.Collapsed, TestStates.FadingIn, exitAction);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                evt.Set();
                _stateChangedSubscription.Dispose();
            });

            StateMachine.Start();

            evt.WaitOne();

            Assert.True(exitActionCalled);
        }

        [Test]
        public void ExitActionIsNotCalledOnOtherTransitions()
        {
            var evt = new ManualResetEvent(false);
            var exitActionCalled = false;

            Action exitAction = () => exitActionCalled = true;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn);
            StateMachine.AddExitAction(TestStates.Collapsed, TestStates.NotStarted, exitAction);
            StateMachine.AddExitAction(TestStates.Collapsed, TestStates.FadingOut, exitAction);
            StateMachine.AddExitAction(TestStates.Collapsed, TestStates.Visible, exitAction);
            

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                evt.Set();
                _stateChangedSubscription.Dispose();
            });

            StateMachine.Start();

            evt.WaitOne();

            Assert.False(exitActionCalled);
        }
    }
}
