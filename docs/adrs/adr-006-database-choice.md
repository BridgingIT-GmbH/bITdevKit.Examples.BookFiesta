# ADR 006: Database Choice - SQL Server

## Status

Accepted

## Context

We need a reliable, scalable database solution for the application that can handle complex relationships and support ACID transactions.

## Decision

We will use SQL Server as our primary database.

## Consequences

### Positive
- Strong consistency and ACID compliance
- Powerful querying capabilities with T-SQL
- Good integration with Entity Framework Core
- Robust security features

### Negative
- Licensing costs for commercial use
- May be overkill for simple data storage needs

## Example

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
}
```