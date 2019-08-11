# OpenMessage

OpenMessage aims to simplify the service bus paradigm by using pre-existing patterns to build an extensible architecture.

Designed for the generic hosting model that .Net Core 3 supports, the library aims to be able to cater for a wide range of scenarios, including receiving the same type from multiple sources - aiding a whole host of scenarios.

The core library `OpenMessage` ships with an InMemory provider and a JSON serializer from the AspNetCore team (`System.Text.Json`).

## Getting Started

The library is based around the `Microsoft.Extensions.*` packages and relies upon the abstractions for dependency injection and logging allowing you the freedom to pick the implementations that best suit your scenario.

_Note: The rest of this guide requires you to be using version 3 of `Microsoft.Extensions.*`._

1 - Install the `OpenMessage` package:

> PM> Install-Package OpenMessage

_You may also any of the providers listed below for this sample as the Memory provider ships out of the box._

2 - Configure your host:

    internal class Program
    {
        private static async Task Main()
        {
            await Host.CreateDefaultBuilder()
                .ConfigureServices(services => services.AddOptions().AddLogging())
                // Configure OpenMessage
                .ConfigureMessaging(host =>
                {
                    // Adds a memory based consumer and dispatcher
                    host.ConfigureMemory<Person>();

                    // Adds a handler that writes the entire message in json format to the console
                    host.ConfigureHandler<Person>(msg => Console.WriteLine($"Hello {msg.Value.Name}"));
                })
                .Build()
                .RunAsync();
        }
    }
    
### Sending Messages

To send messages, inject `IDispatcher<T>` and call `DispatchAsync` and the library will route your message to the configured dispatcher for that type.
    
### Receiving Messages

When a message is received, it flows as follows:

> Message Pump > Channel > Consumer Pump > Pipeline > Handler

This library takes care of everything except the handlers. You have a few choices for implementing a handler, all registered via `.ConfigureHandler`:

1. Use a simple `Action<Message<T>>`
2. Use a simple `Func<Message<T>, Task>`
3. Inherit from `Handler<T>`
4. Implement `IHandler<T>`

By default, after your handler has been run, and assuming the underlying provider supports it, the message is automatically acknowledged. This can be configured by calling `ConfigurePipelineOptions<T>` as well as options for the consumer pump and handler timeout. 

## Serializers

You can add more than one serializer to OpenMessage. In this scenario, all registered serializers are checked to see whether they can deserialize the message. When serializing the last registered serializer is used, service collection provider depending.

Here is a list of the available serializers:

- [x] Hyperion
- [x] Jil
- [x] JsonDotNet
- [x] MessagePack
- [x] MsgPack
- [x] Protobuf
- [x] ServiceStackJson
- [x] Utf8Json
- [x] Wire

## Providers

With OpenMessage you can easily receive from multiple sources in a centralised pipeline whilst providing as much of the underlying providers flexibility as possible. 

Here is a list of the available providers:

- [x] Apache Kafka
- [ ] AWS SQS
- [ ] AWS SNS
- [ ] AWS Kinesis
- [ ] AWS EventBridge
- [ ] Azure Event Hubs
- [ ] Azure Service Bus
- [ ] Eventstore
- [x] InMemory 
- [ ] NATS
- [ ] RabbitMq

_Note: Any unchecked providers are currently a work in progress_.
