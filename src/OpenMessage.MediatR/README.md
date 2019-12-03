# OpenMessage.MediatR

Because MediatR expects all message types to implement `INotification`, OpenMessage will automatically wrap messages in `MediatRMessage<>` or `MediatRBatchMessage<>` before passing them to MediatR to be handled.

Ensure that all `INotificationHandler<>` listen for `INotificationHandler<MediatRBatchMessage<T>>` or `INotificationHandler<MediatRMessage<T>>`

## Usage

``` csharp
Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddMediatR(/** Configuration here **/);
    })
    .ConfigureMessaging(builder =>
    {
        builder
            .ConfigurePipeline<T>()
            .RunMediatR();
    });
```
