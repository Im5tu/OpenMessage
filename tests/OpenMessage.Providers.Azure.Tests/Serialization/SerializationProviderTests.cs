using FluentAssertions;
using Moq;
using OpenMessage.Providers.Azure.Serialization;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenMessage.Providers.Azure.Tests.Serialization
{
    public class SerializationProviderTests
    {
        public class Constructor
        {
            [Fact]
            public void GivenANullProviderEnumerationThrowArgumentNullException()
            {
                Action act = () => new SerializationProvider(null, new Mock<ISerializer>().Object);

                act.ShouldThrow<ArgumentNullException>();
            }

            [Fact]
            public void GivenANullDefaultSerializerThrowArgumentNullException()
            {
                Action act = () => new SerializationProvider(Enumerable.Empty<ISerializer>(), null);

                act.ShouldThrow<ArgumentNullException>();
            }
        }

        public class Serialize
        {
            [Fact]
            public void GivenANullEntityThrowArgumentNullException()
            {
                var target = new SerializationProvider(Enumerable.Empty<ISerializer>(), new Mock<ISerializer>().Object);

                Action act = () => target.Serialize<string>(null);

                act.ShouldThrow<ArgumentNullException>();
            }

            [Fact]
            public void GivenAnEntityThenSerializesWithTheDefaultSerializer()
            {
                var serializer = new Mock<ISerializer>();
                var target = new SerializationProvider(Enumerable.Empty<ISerializer>(), serializer.Object);

                target.Serialize("test")?.Dispose();

                serializer.Verify(s => s.Serialize(It.IsAny<string>()), Times.Once);
            }
        }

        public class Deserialize
        {
            [Fact]
            public void GivenANullEntityThrowArgumentNullException()
            {
                var target = new SerializationProvider(Enumerable.Empty<ISerializer>(), new Mock<ISerializer>().Object);

                Action act = () => target.Deserialize<string>(null);

                act.ShouldThrow<ArgumentNullException>();
            }

            [Fact]
            public void GivenATypeThatCanBeDeserializedThenDeserializesToTheCorrectType()
            {
                var serializer = new Mock<ISerializer>();
                var serializer2 = new Mock<ISerializer>();
                serializer.Setup(s => s.TypeName).Returns("testing");
                serializer.Setup(s => s.Deserialize<string>(It.IsAny<Stream>())).Returns("test");
                serializer2.Setup(s => s.TypeName).Returns("testing2");
                var target = new SerializationProvider(new[] { serializer.Object, serializer2.Object }, serializer.Object);

                target.Deserialize<string>(new Microsoft.ServiceBus.Messaging.BrokeredMessage("test")
                {
                    ContentType = "testing"
                }).Should().Be("test");
            }

            [Fact]
            public void GivenATypeThatCantBeDeserializedThenThrowException()
            {
                var serializer = new Mock<ISerializer>();
                serializer.Setup(s => s.TypeName).Returns("testing");
                var target = new SerializationProvider(new[] { serializer.Object }, serializer.Object);

                Action act = () => target.Deserialize<string>(new Microsoft.ServiceBus.Messaging.BrokeredMessage("test")
                {
                    ContentType = "testing2"
                });

                act.ShouldThrow<Exception>();
            }

            [Fact]
            public void GivenANullContentTypeOnTheBrokeredMessageThenThrowException()
            {
                var target = new SerializationProvider(Enumerable.Empty<ISerializer>(), new Mock<ISerializer>().Object);

                Action act = () => target.Deserialize<string>(new Microsoft.ServiceBus.Messaging.BrokeredMessage());

                act.ShouldThrow<ArgumentException>();
            }
        }
    }
}
