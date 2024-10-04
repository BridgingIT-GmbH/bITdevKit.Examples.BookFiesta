cs![bITDevKit](https://raw.githubusercontent.com/bridgingIT/bITdevKit.Examples.GettingStarted/main/bITDevKit_Logo.png)
=====================================

# Architecture overview

> An application built using .NET 8 and following a Domain-Driven Design approach by using the
> BridgingIT DevKit.

## Features

- Application Commands/Queries: encapsulate business logic and data access
- Application Models and Contracts: specifies the public API can be exposed to clients
- Application Module Clients: expose a public API for other modules to use
- Application Query Services: provide complex queries not suitable for repositories
- Domain Model, ValueObjects, Events, Rules, TypedIds, Repositories: the building blocks to
  implement the domain model
- Modules: encapsulates related functionality into separate modules
- Messaging: provides asynchronous communication between modules based on messages
- Presentation Endpoints: expose an external HTTP API for the application
- Unit & Integration Tests: ensure the reliability of the application

## Frameworks

- [.NET 8](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8/overview)
- [C#](https://learn.microsoft.com/en-us/dotnet/csharp/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [ASP.NET Core](https://dotnet.microsoft.com/en-us/apps/aspnet)
- [Serilog](https://serilog.net/)
- [xUnit.net](https://xunit.net/), [NSubstitute](https://nsubstitute.github.io/), [Shouldly](https://docs.shouldly.org/)

## Architecture

The architecture is structured around key DDD principles, ensuring that the domain model is at the
core of the application.
The solution is divided into several layers:

- **Domain**: Contains the core business logic and domain model.
- **Application**: Handles application-specific logic, including commands and queries.
- **Infrastructure**: Manages data access, external services, and other infrastructure concerns.
- **Presentation**: Provides the user interface and API endpoints.
- **SharedKernel**: Contains shared concepts, such as value objects, rules, and interfaces.

### Architecture Decision Record (ADR)

An [Architecture Decision Record](https://github.com/joelparkerhenderson/architecture-decision-record?tab=readme-ov-file)
(ADR) is a document that captures an important architectural decision made along with its context
and consequences.

These ADRs outline key architectural decisions for the application, focusing on a modular monolith
structure with clear boundaries between modules, rich domain models, and a mix of synchronous and
asynchronous communication between modules:

- [adr-001-modular-monolith.md](docs%2Fadrs%2Fadr-001-modular-monolith.md)
- [adr-002-http-api.md](docs%2Fadrs%2Fadr-002-http-api.md)
- [adr-003-sync-module-clients.md](docs%2Fadrs%2Fadr-003-sync-module-clients.md)
- [adr-004-async-messaging.md](docs%2Fadrs%2Fadr-004-async-messaging.md)
- [adr-005-rich-domain-models.md](docs%2Fadrs%2Fadr-005-rich-domain-models.md)
- [adr-006-database-choice.md](docs%2Fadrs%2Fadr-006-database-choice.md)
- [adr-007-logging-monitoring.md](docs%2Fadrs%2Fadr-007-logging-monitoring.md)
- [adr-008-modularization-strategy.md](docs%2Fadrs%2Fadr-008-modularization-strategy.md)

## Modules

```mermaid
graph TD
  SK[SharedKernel]
  C[Catalog Module]
  I[Inventory Module]
  O[Organization Module]
  C --> SK
  I --> SK
  O --> SK
  C -- Public API --> I
```

### Organization Module

[see](./src/Modules/Organization/Organization-README.md)

### Catalog Module

[see](./src/Modules/Catalog/Catalog-README.md)

### Inventory Module

[see](./src/Modules/Inventory/Inventory-README.md)

## Getting Started

TODO

### Prerequisites

- Docker Desktop
- Visual Studio (Code)

### Running the Application

The supporting containers should first be started with `docker-compose up` or
`docker-compose up -d`.
Then the Presentation.Web.Server project can be set as the startup project.
On `CTRL+F5` this will start the host at [https://localhost:7144](https://localhost:7144).

- [SQL Server](https://learn.microsoft.com/en-us/sql/sql-server/?view=sql-server-ver16) details:
  `Server=127.0.0.1,14339;Database=bit_devkit_bookfiesta;User=sa;Password=Abcd1234!;Trusted_Connection=False;TrustServerCertificate=True;MultipleActiveResultSets=true;encrypt=false;`
- [Swagger UI](https://swagger.io/docs/) is
  available [here](https://localhost:7144/swagger/index.html).
- [Seq](https://docs.datalust.co/docs/an-overview-of-seq) Dashboard is
  available [here](http://localhost:15349).

### Architecture Overview

The solution, powered by bITDevKit, is structured around key architectural layers and the references
between them:

```mermaid
graph TD
  subgraph Presentation
    CP[Module Definition]
    REG[DI Registrations]
    CA[Endpoint Routings]
    MP[Object Mappings]
    RP[Razor Pages/<br>Views/etc]
  end

  subgraph Application
    SV[Services/<br>Jobs/ Tasks]
    CQ[Commands/<br>Queries/ <br>Validators]
    DRR[Rules/<br>Policies]
    VM[ViewModels/<br>DTOs]
    MA[Messages/<br>Adapter<br>Interfaces]
  end

  subgraph Domain
    DMM[Domain Model]
    EN[Entities/<br>Aggregates/<br>Value Objects]
    RS[Repository<br>Interfaces/<br>Specifications]
    DR[Domain Rules/<br>Domain Policies]
  end

  subgraph Infrastructure
    DC[DbContext/<br>Migrations/<br>Data Entities]
    DA[Domain +<br>Application<br>Interface<br>Implementations]
  end

  Presentation -->|references| Application
  Application -->|references| Domain
  Infrastructure -->|references| Domain
```

Key Points:

- Domain layer remains independent, not referencing other layers
- Infrastructure layer doesn't directly reference the Application or Domain layer
- Adheres to the DI principle: high-level layers (Domain, Application) don't depend on low-level
  layer (Infrastructure), but both depend on abstractions

This layering structure enforces clean architecture principles, ensuring separation of concerns and
maintaining the independence of core business logic.

#### Request Processing Flow

```mermaid
sequenceDiagram
  participant C as Client
  participant EA as External API
  participant CM as Command
  participant CV as CommandValidator
  participant CH as CommandHandler
  participant M as Mapper
  participant R as Repository
  C ->> EA: Web Request
  EA ->> CM: Create Command (with Model)
  CM ->> CV: Validate
  CV -->> CM: Validation Result
  CM ->> CH: Process
  CH ->> R: FindOneResultAsync(EntityId)
  R -->> CH: Result<Domain Entity>
  CH ->> M: Map Model to existing Domain Entity
  M -->> CH: Updated Domain Entity
  CH ->> CH: Apply Domain Rules
  CH ->> R: UpdateAsync(Domain Entity)
  R -->> CH: Updated Domain Entity
  CH -->> EA: Result<Domain Entity>
  EA ->> M: Map Domain Entity to Model
  M -->> EA: Model
  EA ->> EA: Format Response
  E -->> C: Web Response
```

### Solution Structure

<img src="./assets/image-20240426112716841.png" alt="image-20240426112716841" style="zoom:50%;" />

### Application

Contains commands, queries, and their respective handlers.

#### Commands

([CustomerCreateCommand.cs](./src/Application/Commands/CustomerCreateCommand.cs))

```csharp
public class CustomerCreateCommand
    : CommandRequestBase<Customer>
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public override ValidationResult Validate() =>
        new Validator().Validate(this);

    public class Validator : AbstractValidator<CustomerCreateCommand>
    {
        public Validator()
        {
            this.RuleFor(c => c.FirstName).NotNull().NotEmpty().WithMessage("Must not be empty.");
            this.RuleFor(c => c.LastName).NotNull().NotEmpty().WithMessage("Must not be empty.");
        }
    }
}
```

([CustomerCreateCommandHandler.cs](./src/Application/Commands/CustomerCreateCommandHandler.cs))

```csharp
public class CustomerCreateCommandHandler
    : CommandHandlerBase<CustomerCreateCommand, Customer>
{
    private readonly IGenericRepository<Customer> repository;

    public CustomerCreateCommandHandler(
        ILoggerFactory loggerFactory,
        IGenericRepository<Customer> repository)
        : base(loggerFactory)
    {
        this.repository = repository;
    }

    public override async Task<CommandResponse<Customer>> Process(
        CustomerCreateCommand request,
        CancellationToken cancellationToken)
    {
        var customer = new Customer { FirstName = request.FirstName, LastName = request.LastName };
        await this.repository.UpsertAsync(customer, cancellationToken).AnyContext();

        return new CommandResponse<Customer> // TODO: use .For?
        {
            Result = customer
        };
    }
}
```

#### Queries

### Domain

Defining your core business logic with domain models and aggregates.

#### Aggregates

```csharp
public class Customer : AggregateRoot<Guid>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
```

### Infrastructure

Providing the backbone with a DbContext setup and repository implementations.

#### DbContext

([AppDbContext.cs](./src/Infrastructure/EntityFramework/AppDbContext.cs))

````csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
}
````

### Presentation

Serves as the entry point for external interactions, focusing on delivering data and services to
clients.

#### CompositeRoot (Registrations)

([Program.cs](./src/Presentation.Web.Server/Program.cs))

```csharp
builder.Services.AddCommands();
builder.Services.AddQueries();

builder.Services
    .AddSqlServerDbContext<CoreDbContext>(o => o
        .UseConnectionString(builder.Configuration.GetConnectionString("Default")))
    .With
gratorService();
```

#### ViewModels

([CustomerViewModel.cs](./src/Presentation/ViewModels/CustomerViewModel.cs))

````csharp
public class CustomerViewModel
{
    public string Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }
}
````

#### Web API (Controllers)

([CustomersController.cs](./src/Presentation/Web/Controllers/CustomersController.cs))

````csharp
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IMediator mediator;

    public CustomersController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerViewModel>>> GetAsync()
    {
        var query = new CustomerFindAllQuery();
        var result = await this.mediator.Send(query);

        return this.Ok(result?.Result?.Select(e =>
            new CustomerViewModel
            {
                Id = e.Id.ToString(),
                FirstName = e.FirstName,
                LastName = e.LastName
            }));
    }

    [HttpPost]
    public async Task<ActionResult> PostAsync([FromBody] CustomerViewModel model)
    {
        if (model == null)
        {
            return this.BadRequest();
        }

        var command = new CustomerCreateCommand()
        {
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var result = await this.mediator.Send(command);

        return this.Created($"/api/customers/{result.Result.Id}", null);
    }
}
````

### Testing the API

Ensuring reliability through comprehensive unit, integration, and HTTP tests.

#### Swagger UI

Start the application (CTRL-F5) and use the following UI to test the API:

[Swagger UI](https://localhost:7144/swagger/index.html)

![image-20240426112042343](./assets/image-20240426112042343.png)

#### Unit Tests

<img src="./assets/image-20240426111823428.png" alt="image-20240426111823428" style="zoom:50%;" />

#### Integration Tests

<img src="./assets/image-20240426111718058.png" alt="image-20240426111718058" style="zoom:50%;" />

#### Http Tests

Start the application (CTRL-F5) and use the following HTTP requests to test the API:
[API.http](./API.http)

![image-20240426112136837](./assets/image-20240426112136837.png)