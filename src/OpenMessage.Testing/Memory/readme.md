# OpenMessage.Testing.Memory

For testing purposes it is sometimes beneficial to wait for messages to be consumed before returning from the dispatcher.

To do this you need to override the existing `MemoryDispatcher<>`:

```csharp
services.AddAwaitableMemoryDispatcher<T>();
```

This functionality relies on the `ISupportAcknowledgment.AcknowledgeAsync()` method on the message to be called when the consumer finishes executing.

`ISupportAcknowledgment.AcknowledgeAsync()` functionality is provided by `AutoAcknowledgeMiddleware<>`, which is present in the default pipeline (`Pipeline.CreateDefaultBuilder<>()`)