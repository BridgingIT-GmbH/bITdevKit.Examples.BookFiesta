# ADR 005: Rich Domain Models vs Anemic CRUD Entities

## Status

Accepted

## Context

We need to decide how to structure our domain models and where to place business logic within our application architecture.

## Decision

We will use rich domain models that encapsulate business logic rather than anemic CRUD entities.

## Consequences

### Positive
- Business logic is centralized in the domain layer
- Improved encapsulation and data integrity
- Better alignment with Domain-Driven Design principles
- Easier to maintain and extend as the application grows

### Negative
- Steeper learning curve for developers used to anemic models
- Potential for increased complexity in simple CRUD operations

## Example

```csharp
public class Book : AggregateRoot<BookId>
{
    public string Title { get; private set; }
    public Money Price { get; private set; }
    public List<AuthorId> AuthorIds { get; private set; }

    public void UpdatePrice(Money newPrice)
    {
        if (newPrice.Amount < 0)
            throw new InvalidOperationException("Price cannot be negative");
        
        Price = newPrice;
        AddDomainEvent(new BookPriceUpdatedEvent(Id, newPrice));
    }

    public void AddAuthor(AuthorId authorId)
    {
        if (AuthorIds.Count >= 5)
            throw new InvalidOperationException("A book cannot have more than 5 authors");
        
        AuthorIds.Add(authorId);
    }
}
```

This rich domain model encapsulates business rules (e.g., price validation, author limit) and raises domain events, unlike a simple CRUD entity.
