#OpenMessage

Latest Build: ![Build Status](https://im5tu.visualstudio.com/_apis/public/build/definitions/e4fcda74-9f33-4672-b774-b4419099857c/2/badge)

OpenMessage aims to simplify the service bus paradigm by using pre-existing patterns to build an extensible architecture.

##Getting Started

The library is based around the `Microsoft.Extensions.*` packages and relies upon the abstractions for depenency injection and logging allowing you the freedom to pick the implementations that best suit your scenario.

Assuming you want to connect to Azure Service Bus, here is how you configure OpenMessage:

1 - Add the provider: 

    PM> Install-Package OpenMessage.Providers.Azure

2 - Add the serializer (or write your own):

    PM> Install-Package OpenMessage.Serializer.JsonNet
    
3 - Add the services to your service collection: 

    using OpenMessage;
    using OpenMessage.Providers.Azure.Configuration;
    using OpenMessage.Serializer.JsonNet;
    
    ...
    
    IServiceCollection AddServices(IServiceCollection services)
    {
        return services
                .AddOpenMessage()
                .AddJsonNetSerializer()
                .Configure<OpenMessageAzureProviderOptions>(config => {
                    config.ConnectionString = "YOUR CONNECTION STRING HERE");    
                });
    }
    
###Sending Messages

4 - Add either a Queue/Topic dispatcher to the service collection:

    IServiceCollection AddQueueBindings(IServiceCollection services)
    {
        return services.AddQueueDispatcher<MyType>();    
    }
    
5 - Inject an `IDispatcher<T>` into your class of choice:

    internal class CommandGenerator
    {
        private readonly IDispatcher<MyType> _dispatcher;
        
        public CommandGenerator(IDispatcher<MyType> dispatcher)
        {
            _dispatcher = dispatcher;    
        }    
    }
    
###Receiving Messages

4 - Add either a Queue/Subscription observable to the service collection:

    IServiceCollection AddQueueBindings(IServiceCollection services)
    {
        return services.AddQueueObservable<MyType>();    
    }

5 - When done, resolve an `IEnumerable<IBroker>` from the service collection to begin receiving messages.

##Serializers

You can add more than one serializer to OpenMessage. In this scenario, all registered serializers are checked to see whether they can deserialize the message. When serializing the last registered serializer is used, service collection provider depending.

- [x] [Json.Net](http://www.nuget.org/packages/OpenMessage.Serializer.JsonNet/)
- [x] [Jil](http://www.nuget.org/packages/OpenMessage.Serializer.Jil/)
- [x] [Protobuf](http://www.nuget.org/packages/OpenMessage.Serializer.ProtobufNet/)

##Providers

- [x] [Azure Service Bus](http://www.nuget.org/packages/OpenMessage.Providers.Azure/)
- [ ] Azure Event Hubs
- [ ] In Memory
- [ ] Rabbit MQ