using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using NUnit.Framework;
using ReactiveStateMachine.Triggers;

namespace Tests
{
    [TestFixture]
    public class ConfigurationTests : AbstractReactiveStateMachineTest
    {
        [Test]
        public void StartStateIsSetCorrectly()
        {
            Assert.AreEqual(StateMachine.StartState, TestStates.Collapsed);
        }

        [Test]
        public void FluentApiWorks()
        {
            var trigger = new Subject<object>();

            var evt = new ManualResetEvent(false);
            var startedEvt = new ManualResetEvent(false);

            var transitionActionCalled = false;

            StateMachine.AddTransition(trigger)
                        .From(TestStates.Collapsed)
                        .To(TestStates.FadingIn)
                        .Where(o => true)
                        .Do(o => transitionActionCalled = true);

            StateMachine.StateChanged += (sender, args) => evt.Set();

            StateMachine.StateMachineStarted += (sender, args) => startedEvt.Set();
            
            StateMachine.Start();

            startedEvt.WaitOne();

            trigger.OnNext(null);

            evt.WaitOne();

            Assert.That(transitionActionCalled);
        }

    }
}
