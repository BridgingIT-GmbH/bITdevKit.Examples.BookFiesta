# ADR 003: Synchronous Inter-Module Communication

## Status

`Accepted`

## Context

We need a way for modules in our modular monolith to communicate synchronously.

## Decision

We will use ModuleClients for synchronous inter-module communication.

## Consequences

### Positive
- Clear module boundaries
- Compile-time type safety
- Easier refactoring and testing

### Negative
- Additional code for client interfaces
- Small performance overhead

## Example

```csharp
public interface IInventoryModuleClient
{
    Task<Result<StockModel>> StockFindOne(string tenantId, string id);
    Task<Result<IEnumerable<StockModel>>> StockFindAll(string tenantId);
    // ...
}
```

Another module can use this client to interact with the module synchronously.