using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenMessage;
using Xunit;
using FluentAssertions;

namespace OpenMessage.Tests
{
    public class BrokerHostTests
    {

        public class Constructor
        {
            [Fact]
            public void GivenANullBrokerEnumerableThrowArgumentNullException()
            {
                Action act = () => new BrokerHost(null);

                act.ShouldThrow<ArgumentNullException>();
            }
        }

        public class Dispose
        {
            [Fact]
            public void GivenABrokerEnumerationMustCallDisposeOnlyOnce()
            {
                var broker = new DisposableBroker();
                var host = new BrokerHost(new[] { broker });

                host.Dispose();

                broker.Disposed.Should().BeTrue();
                broker.DisposedCount.Should().Be(1);
            }

            [Fact]
            public void GivenMultipleDisposeCallsThenSecondCallShouldThrowObjectDisposedException()
            {
                var host = new BrokerHost(Enumerable.Empty<IBroker>());
                host.Dispose();

                Action act = () => host.Dispose();
                act.ShouldThrow<ObjectDisposedException>();
            }

            [Fact]
            public void GivenMultipleBrokersThenCallDisposeOnEach()
            {
                var brokers = new[] { new DisposableBroker(), new DisposableBroker(), new DisposableBroker() };

                new BrokerHost(brokers).Dispose();

                brokers.All(x => x.Disposed).Should().BeTrue();
                brokers.All(x => x.DisposedCount == 1).Should().BeTrue();
            }
        }


        internal class DisposableBroker : IBroker, IDisposable
        {
            internal bool Disposed { get; private set; }
            internal int DisposedCount { get; private set; }

            public void Dispose()
            {
                Disposed = true;
                DisposedCount += 1;
            }
        }
    }
}
