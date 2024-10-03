# ADR 004: Asynchronous Inter-Module Communication

## Status

Accepted

## Context

We need a way for modules to communicate asynchronously for eventually consistent operations.

## Decision

We will use a message bus for asynchronous inter-module communication.

## Consequences

### Positive
- Decouples modules
- Supports eventual consistency
- Improves scalability

### Negative
- Increased complexity
- Potential for message failures

## Example

```csharp
public class OrderCreatedMessage : IMessage
{
    public string OrderId { get; set; }
    public string ProductId { get; set; }
    public int Quantity { get; set; }
}

// In the Order module
await _messageBroker.PublishAsync(new OrderCreatedMessage { ... });

// In the Inventory module
public class OrderCreatedMessageHandler : IMessageHandler<OrderCreatedMessage>
{
    public Task Handle(OrderCreatedMessage message, CancellationToken cancellationToken)
    {
        // Update inventory
    }
}
```

This allows for asynchronous communication between the distinct modules.