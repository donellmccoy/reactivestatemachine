using System;
using System.Reactive.Linq;
using System.Threading;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class EntryActionTests : AbstractReactiveStateMachineTest
    {
        IDisposable _stateChangedSubscription;

        [Test]
        public void SingleEntryActionIsCalled()
        {
            var evt = new ManualResetEvent(false);
            var entryActionCalled = false;

            Action entryAction = () => entryActionCalled = true;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn);
            StateMachine.AddEntryAction(TestStates.FadingIn, entryAction);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            evt.WaitOne();

            Assert.True(entryActionCalled);
        }

        [Test]
        public void MultipleEntryActionsAreCalled()
        {
            var evt = new ManualResetEvent(false);
            const int numEntryActionsToCall = 10;
            var numEntryActionsCalled = 0;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn);

            for (int i = 0; i < numEntryActionsToCall; i++)
            {
                Action entryAction = () => numEntryActionsCalled++;
                StateMachine.AddEntryAction(TestStates.FadingIn, entryAction);
            }

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            evt.WaitOne();

            Assert.AreEqual(numEntryActionsToCall, numEntryActionsCalled);
        }

        [Test]
        public void ConditionalEntryActionIsCalled()
        {
            var evt = new ManualResetEvent(false);
            var entryActionCalled = false;

            Action entryAction = () => entryActionCalled = true;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn);
            StateMachine.AddEntryAction(TestStates.FadingIn, entryAction, () => true);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                _stateChangedSubscription.Dispose();
                evt.Set();
            });

            StateMachine.Start();

            evt.WaitOne();

            Assert.True(entryActionCalled);
        }

        [Test]
        public void ConditionalEntryActionIsNotCalled()
        {
            var evt = new ManualResetEvent(false);
            var entryActionCalled = false;

            Action entryAction = () => entryActionCalled = true;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn);
            StateMachine.AddEntryAction(TestStates.FadingIn, entryAction, () => false);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                evt.Set();
                _stateChangedSubscription.Dispose();
            });

            StateMachine.Start();

            evt.WaitOne();

            Assert.False(entryActionCalled);
        }

        [Test]
        public void EntryActionIsCalledOnSpecificTransition()
        {
            var evt = new ManualResetEvent(false);
            var entryActionCalled = false;

            Action entryAction = () => entryActionCalled = true;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn);
            StateMachine.AddEntryAction(TestStates.FadingIn, TestStates.Collapsed, entryAction);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                evt.Set();
                _stateChangedSubscription.Dispose();
            });

            StateMachine.Start();

            evt.WaitOne();

            Assert.True(entryActionCalled);
        }

        [Test]
        public void EntryActionIsNotCalledOnOtherTransitions()
        {
            var evt = new ManualResetEvent(false);
            var entryActionCalled = false;

            Action entryAction = () => entryActionCalled = true;

            StateMachine.AddAutomaticTransition(TestStates.Collapsed, TestStates.FadingIn);
            StateMachine.AddEntryAction(TestStates.FadingIn, TestStates.NotStarted, entryAction);
            StateMachine.AddEntryAction(TestStates.FadingIn, TestStates.FadingOut, entryAction);
            StateMachine.AddEntryAction(TestStates.FadingIn, TestStates.Visible, entryAction);

            _stateChangedSubscription = StateChanged.Where(args => args.ToState == TestStates.FadingIn).Subscribe(args =>
            {
                evt.Set();
                _stateChangedSubscription.Dispose();
            });

            StateMachine.Start();

            evt.WaitOne();

            Assert.False(entryActionCalled);
        }

    }
}
