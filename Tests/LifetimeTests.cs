using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class LifetimeTests : AbstractReactiveStateMachineTest
    {

        [Test]
        public void StartingTwiceThrows()
        {
            StateMachine.Start();

            Assert.Throws<InvalidOperationException>( () => StateMachine.Start());
        }

        [Test]
        public void CurrentStateEqualsStartStateAfterStart()
        {
            StateMachine.Start();

            //TODO: wait for the StateChangedEvent to occur
            Thread.Sleep(100);

            Assert.AreEqual(StateMachine.CurrentState, StateMachine.StartState);
        }

        [Test]
        public void RestartingMachineDoesNotThrow()
        {
            StateMachine.Start();
            StateMachine.Stop();

            Assert.DoesNotThrow(() => StateMachine.Start());
        }

    }
}
