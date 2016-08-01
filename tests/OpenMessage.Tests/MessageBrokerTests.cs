using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace OpenMessage.Tests
{
    public class MessageBrokerTests
    {
        public class Constructor
        {
            [Fact]
            public void GivenANullObserverCollectionThrowArgumentNullException()
            {
                Action act = () => new MessageBroker<string>(null, Enumerable.Empty<IObservable<string>>(), new Mock<ILogger<ManagedObservable<string>>>().Object);

                act.ShouldThrow<ArgumentNullException>();
            }

            [Fact]
            public void GivenANullObservableCollectionThrowArgumentNullException()
            {
                Action act = () => new MessageBroker<string>(Enumerable.Empty<IObserver<string>>(), null, new Mock<ILogger<ManagedObservable<string>>>().Object);

                act.ShouldThrow<ArgumentNullException>();
            }

            [Fact]
            public void GivenANullLoggerThrowArgumentNullException()
            {
                Action act = () => new MessageBroker<string>(Enumerable.Empty<IObserver<string>>(), Enumerable.Empty<IObservable<string>>(), null);

                act.ShouldThrow<ArgumentNullException>();
            }
        }

        public class Subscribe
        {
            private readonly MessageBroker<string> _target = new MessageBroker<string>(Enumerable.Empty<IObserver<string>>(), Enumerable.Empty<IObservable<string>>(), new Mock<ILogger<ManagedObservable<string>>>().Object);

            [Fact]
            public void GivenANullObserverThrowArgumentNullException()
            {
                Action act = () => _target.Subscribe(null);

                act.ShouldThrow<ArgumentNullException>();
            }

            [Fact]
            public void GivenASubscriptionIsAddedThenDoesNotReturnNull()
            {
                _target.Subscribe(new Mock<IObserver<string>>().Object).Should().NotBeNull();
            }

            [Fact]
            public void GivenMultipleSubscriptionsThenCallsEachSubscriberOnNextMessage()
            {
                var subscriber1 = new Mock<IObserver<string>>();
                var subscriber2 = new Mock<IObserver<string>>();
                var producer = new FakeProducer();

                using (var target = new MessageBroker<string>(new[] { subscriber1.Object, subscriber2.Object }, new[] { producer }, new Mock<ILogger<ManagedObservable<string>>>().Object))
                    producer.Trigger();

                subscriber1.Verify(x => x.OnNext("test"), Times.Once);
                subscriber2.Verify(x => x.OnNext("test"), Times.Once);
            }

            [Fact]
            public void GivenMultipleProducersThenCallsEachSubscriberOnNextMessage()
            {
                var subscriber1 = new Mock<IObserver<string>>();
                var subscriber2 = new Mock<IObserver<string>>();
                var producer1 = new FakeProducer();
                var producer2 = new FakeProducer();

                using (var target = new MessageBroker<string>(new[] { subscriber1.Object, subscriber2.Object }, new[] { producer1, producer2 }, new Mock<ILogger<ManagedObservable<string>>>().Object))
                {
                    producer1.Trigger("1");
                    producer2.Trigger("2");
                }

                subscriber1.Verify(x => x.OnNext("1"), Times.Once);
                subscriber2.Verify(x => x.OnNext("1"), Times.Once);
                subscriber1.Verify(x => x.OnNext("2"), Times.Once);
                subscriber2.Verify(x => x.OnNext("2"), Times.Once);
            }

            private class FakeProducer : IObservable<string>
            {
                private HashSet<IObserver<string>> _observers = new HashSet<IObserver<string>>();

                public IDisposable Subscribe(IObserver<string> observer)
                {
                    _observers.Add(observer);
                    return new Disposable(() => _observers.Remove(observer));
                }

                internal void Trigger(string message = "test")
                {
                    foreach (var observer in _observers)
                        observer.OnNext(message);
                }
            }
        }

        public class Dispose
        {
            [Fact]
            public void GivenMultiplePiecesOfTelemetryRecordedThenRecordsOnDispose()
            {
                var observer = new Mock<IObserver<string>>();
                var _target = new MessageBroker<string>(new[] { observer.Object }, Enumerable.Empty<IObservable<string>>(), new Mock<ILogger<ManagedObservable<string>>>().Object);

                using (_target.Subscribe(observer.Object))
                    _target.Dispose();

                observer.Verify(x => x.OnCompleted(), Times.Once);
            }
        }
    }
}
