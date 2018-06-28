using System;
using Improbable.Context;
using Improbable.Worker;
using Moq;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class DynamicFlagTest
    {
        private Mock<IConnection> connection;
        private Mock<IDispatcher> dispatcher;

        [SetUp]
        public void SetUp()
        {
            connection = new Mock<IConnection>();
            dispatcher = new Mock<IDispatcher>();
        }

        [Test]
        public void Should_RegisterFlagUpdateOp()
        {
            var flag = intFlag();

            dispatcher.Verify(m => m.OnFlagUpdate(It.IsAny<Action<FlagUpdateOp>>()));
        }

        [Test]
        public void Should_AssumeDefaultValue()
        {
            var flag = intFlag();

            Assert.AreEqual(1337, flag.Value);
        }

        [Test]
        public void Should_ParseValueFromConnection()
        {
            connection.Setup(m => m.GetWorkerFlag("thing")).Returns(new Improbable.Collections.Option<string>("100"));
            var flag = intFlag();

            Assert.AreEqual(100, flag.Value);
        }

        [Test]
        public void Should_ProcessUpdates()
        {
            Action<FlagUpdateOp> callback = null;
            dispatcher.Setup(m => m.OnFlagUpdate(It.IsAny<Action<FlagUpdateOp>>())).Callback<Action<FlagUpdateOp>>(cb => callback = cb);
            var flag = intFlag();

            Assert.AreEqual(1337, flag.Value);

            var flagUpdate = new FlagUpdateOp
            {
                Name = "thing",
                Value = "20"
            };

            callback(flagUpdate);
            Assert.AreEqual(flag.Value, 20);
        }

        [Test]
        public void Should_IgnoreUpdatesToOtherVariables()
        {
            Action<FlagUpdateOp> callback = null;
            dispatcher.Setup(m => m.OnFlagUpdate(It.IsAny<Action<FlagUpdateOp>>())).Callback<Action<FlagUpdateOp>>(cb => callback = cb);
            var flag = intFlag();

            Assert.AreEqual(1337, flag.Value);

            var flagUpdate = new FlagUpdateOp
            {
                Name = "not_thing",
                Value = "20"
            };

            callback(flagUpdate);
            Assert.AreEqual(flag.Value, 1337);
        }

        [Test]
        public void Should_ReturnDefaultForTwoArgs()
        {
            connection.Setup(mn => mn.GetWorkerFlag("thing")).Returns("10");
            var flag = twoArgFlag();

            Assert.AreEqual(1337, flag.Value);
        }

        [Test]
        public void Should_ParseForTwoArgs()
        {
            connection.Setup(mn => mn.GetWorkerFlag(It.IsAny<string>())).Returns<string>(s =>
            {
                if (s == "thing1") return new Improbable.Collections.Option<string>("10");
                if (s == "thing2") return new Improbable.Collections.Option<string>("5");
                return new Improbable.Collections.Option<string>(null);
            });
            var flag = twoArgFlag();

            Assert.AreEqual(50, flag.Value);
        }

        [Test]
        public void Should_ProcessUpdatesForTwoArgs()
        {
            Action<FlagUpdateOp> callback = null;
            dispatcher.Setup(m => m.OnFlagUpdate(It.IsAny<Action<FlagUpdateOp>>())).Callback<Action<FlagUpdateOp>>(cb => callback = cb);
            var flag = twoArgFlag();

            callback(new FlagUpdateOp { Name = "thing1", Value = "10" });
            Assert.AreEqual(1337, flag.Value);

            callback(new FlagUpdateOp { Name = "thing2", Value = "5" });
            Assert.AreEqual(50, flag.Value);

            // should revert to default when we unset one of the params
            callback(new FlagUpdateOp { Name = "thing1", Value = new Improbable.Collections.Option<string>(null) });
            Assert.AreEqual(1337, flag.Value);
        }

        [Test]
        public void Should_CopeWithLateAttachedConnection()
        {
            var UnconnectedFlag = new DynamicFlag<int>(dispatcher.Object, "thing", Convert.ToInt32, 1337);
            connection.Setup(m => m.GetWorkerFlag("thing")).Returns(new Improbable.Collections.Option<string>("100"));
            Assert.AreEqual(1337, UnconnectedFlag.Value, "Flag has default value before connection");
            UnconnectedFlag.AttachConnection(connection.Object);
            Assert.AreEqual(100, UnconnectedFlag.Value, "Flag establises new value after connection");
            UnconnectedFlag.DetachConnection(connection.Object);
            Assert.AreEqual(100, UnconnectedFlag.Value, "Flags retain value after disconnection");
        }

        [Test]
        public void Should_TriggerCallbacks()
        {
            Action<FlagUpdateOp> callback = null;
            int lastValue = 0;
            dispatcher.Setup(m => m.OnFlagUpdate(It.IsAny<Action<FlagUpdateOp>>())).Callback<Action<FlagUpdateOp>>(cb => callback = cb);
            bool updated = false;
            var BasicFlag = new DynamicFlag<int>(dispatcher.Object, "thing", Convert.ToInt32, 1337);
            connection.Setup(m => m.GetWorkerFlag("thing")).Returns(new Improbable.Collections.Option<string>("100"));
            BasicFlag.OnChange((value) => { lastValue = value; updated = true; });

            BasicFlag.AttachConnection(connection.Object);
            Assert.AreEqual(lastValue, 100, "Updates on connection");
            Assert.AreEqual(updated, true);
            updated = false;

            callback(new FlagUpdateOp { Name = "thing", Value = "10" });
            Assert.AreEqual(lastValue, 10, "Updates on changes");
            Assert.AreEqual(updated, true);
        }

        private DynamicFlag<int> intFlag()
        {
            return new DynamicFlag<int>(connection.Object, dispatcher.Object, "thing", Convert.ToInt32, 1337);
        }

        private DynamicFlag<int> twoArgFlag()
        {
            return new DynamicFlag<int>(connection.Object, dispatcher.Object, "thing1", "thing2", (x, y) => int.Parse(x) * int.Parse(y), 1337);
        }
    }
}
