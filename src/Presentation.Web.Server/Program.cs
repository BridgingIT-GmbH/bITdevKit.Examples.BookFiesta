using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.GettingStarted.Application;
using BridgingIT.DevKit.Examples.GettingStarted.Domain;
using BridgingIT.DevKit.Examples.GettingStarted.Infrastructure;
using BridgingIT.DevKit.Examples.GettingStarted.Presentation;
using BridgingIT.DevKit.Presentation;
using BridgingIT.DevKit.Presentation.Web;
using Hellang.Middleware.ProblemDetails;
using Microsoft.EntityFrameworkCore;

// ===============================================================================================
// Configure the host
var builder = WebApplication.CreateBuilder(args);
// ===v DevKit registrations v===
builder.Host.ConfigureLogging();
// ===^ DevKit registrations ^===

// ===============================================================================================
// Configure the services
// ===v DevKit registrations v===
builder.Services.AddMediatR();
builder.Services.AddMapping().WithMapster<MapperRegister>();
builder.Services.AddCommands();
builder.Services.AddQueries();

builder.Services.AddStartupTasks(o => o.Enabled().StartupDelay("00:00:5"))
    .WithTask<CoreDomainSeederTask>(o => o
        .Enabled(builder.Environment.IsDevelopment()));

builder.Services.AddSqlServerDbContext<CoreDbContext>(o => o
        .UseConnectionString(builder.Configuration.GetConnectionString("Default"))
        .UseLogger(true, builder.Environment.IsDevelopment()),
        c => c
            .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
            .CommandTimeout(30))
    .WithDatabaseCreatorService(o => o
        .Enabled(builder.Environment.IsDevelopment())
        .DeleteOnStartup());
    //.WithDatabaseMigratorService(o => o
    //    .Enabled(builder.Environment.IsDevelopment())
    //   .DeleteOnStartup());

builder.Services.AddEntityFrameworkRepository<Customer, CoreDbContext>()
    .WithTransactions<NullRepositoryTransaction<Customer>>()
    .WithBehavior<RepositoryTracingBehavior<Customer>>()
    .WithBehavior<RepositoryLoggingBehavior<Customer>>()
    .WithBehavior<RepositoryConcurrentBehavior<Customer>>()
    .WithBehavior<RepositoryAuditStateBehavior<Customer>>()
    .WithBehavior<RepositoryDomainEventPublisherBehavior<Customer>>();

builder.Services.AddEntityFrameworkRepository<Tag, CoreDbContext>()
    .WithTransactions<NullRepositoryTransaction<Tag>>()
    .WithBehavior<RepositoryTracingBehavior<Tag>>()
    .WithBehavior<RepositoryLoggingBehavior<Tag>>()
    .WithBehavior<RepositoryConcurrentBehavior<Tag>>();

builder.Services.AddEntityFrameworkRepository<Category, CoreDbContext>()
    .WithTransactions<NullRepositoryTransaction<Category>>()
    .WithBehavior<RepositoryTracingBehavior<Category>>()
    .WithBehavior<RepositoryLoggingBehavior<Category>>()
    .WithBehavior<RepositoryConcurrentBehavior<Category>>()
    .WithBehavior<RepositoryAuditStateBehavior<Category>>();

builder.Services.AddEntityFrameworkRepository<Book, CoreDbContext>()
    .WithTransactions<NullRepositoryTransaction<Book>>()
    .WithBehavior<RepositoryTracingBehavior<Book>>()
    .WithBehavior<RepositoryLoggingBehavior<Book>>()
    .WithBehavior<RepositoryConcurrentBehavior<Book>>()
    .WithBehavior<RepositoryAuditStateBehavior<Book>>()
    .WithBehavior<RepositoryDomainEventPublisherBehavior<Book>>();

builder.Services.AddEntityFrameworkRepository<Author, CoreDbContext>()
    .WithTransactions<NullRepositoryTransaction<Author>>()
    .WithBehavior<RepositoryTracingBehavior<Author>>()
    .WithBehavior<RepositoryLoggingBehavior<Author>>()
    .WithBehavior<RepositoryConcurrentBehavior<Author>>()
    .WithBehavior<RepositoryAuditStateBehavior<Author>>()
    .WithBehavior<RepositoryDomainEventPublisherBehavior<Author>>();
// ===^ DevKit registrations ^===

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails(o =>
    Configure.ProblemDetails(o, builder.Environment.IsDevelopment()));
// builder.Services.AddProblemDetails(Configure.ProblemDetails);

// ===============================================================================================
// Configure the HTTP request pipeline
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ===v DevKit registrations v===
app.UseRequestCorrelation();
app.UseRequestLogging();
// ===^ DevKit registrations ^===

app.UseProblemDetails();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program
{
    // this partial class is needed to set the accessibilty for the Program class to public
    // needed for endpoint testing when using the webapplicationfactory  https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0#basic-tests-with-the-default-webapplicationfactory
}