using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class ConfigurationTest : AbstractReactiveStateMachineTest
    {
        [Test]
        public void StartStateIsSetCorrectly()
        {
            StateMachine = new ReactiveStateMachine.ReactiveStateMachine<TestStates>(TestStates.Collapsed);

            Assert.AreEqual(StateMachine.StartState, TestStates.Collapsed);
        }

    }
}
