namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Presentation;

using System.Reflection;
using BridgingIT.DevKit.Application;
using BridgingIT.DevKit.Application.JobScheduling;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookStore.Application;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Application;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;
using BridgingIT.DevKit.Examples.BookStore.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class CatalogModule : WebModuleBase
{
    public override IServiceCollection Register(IServiceCollection services, IConfiguration configuration = null, IWebHostEnvironment environment = null)
    {
        var moduleConfiguration = this.Configure<CatalogModuleConfiguration, CatalogModuleConfiguration.Validator>(services, configuration);

        services.AddJobScheduling()
            .WithJob<EchoJob>(CronExpressions.Every5Minutes); // .WithSingletonJob<EchoJob>(CronExpressions.Every5Minutes)
                                                              //.WithJob<HealthCheckJob>(CronExpressions.EveryMinute);

        services.AddStartupTasks()
            .WithTask<CatalogDomainSeederTask>(o => o
                .Enabled(environment?.IsDevelopment() == true)
                .StartupDelay(moduleConfiguration.SeederTaskStartupDelay));

        services.AddSqlServerDbContext<CatalogDbContext>(o => o
                .UseConnectionString(moduleConfiguration.ConnectionStrings["Default"])
                .UseLogger(true, environment?.IsDevelopment() == true),
                c => c
                    .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                    .CommandTimeout(30))
            .WithHealthChecks()
            .WithDatabaseCreatorService(o => o
                .Enabled(environment?.IsDevelopment() == true)
                .DeleteOnStartup())
            //.WithDatabaseMigratorService(o => o
            //    .Enabled(environment?.IsDevelopment() == true)
            //   .DeleteOnStartup());
            .WithOutboxDomainEventService(o => o
                .ProcessingInterval("00:00:30").StartupDelay("00:00:15").PurgeOnStartup().ProcessingModeImmediate());

        services.AddEntityFrameworkRepository<Customer, CatalogDbContext>()
            .WithTransactions<NullRepositoryTransaction<Customer>>()
            .WithBehavior<RepositoryTracingBehavior<Customer>>()
            .WithBehavior<RepositoryLoggingBehavior<Customer>>()
            .WithBehavior<RepositoryConcurrentBehavior<Customer>>()
            .WithBehavior<RepositoryAuditStateBehavior<Customer>>()
            .WithBehavior<RepositoryDomainEventBehavior<Customer>>()
            .WithBehavior<RepositoryDomainEventPublisherBehavior<Customer>>();

        services.AddEntityFrameworkRepository<Tag, CatalogDbContext>()
            .WithTransactions<NullRepositoryTransaction<Tag>>()
            .WithBehavior<RepositoryTracingBehavior<Tag>>()
            .WithBehavior<RepositoryLoggingBehavior<Tag>>()
            .WithBehavior<RepositoryConcurrentBehavior<Tag>>();

        services.AddEntityFrameworkRepository<Category, CatalogDbContext>()
            .WithTransactions<NullRepositoryTransaction<Category>>()
            .WithBehavior<RepositoryTracingBehavior<Category>>()
            .WithBehavior<RepositoryLoggingBehavior<Category>>()
            .WithBehavior<RepositoryConcurrentBehavior<Category>>()
            .WithBehavior<RepositoryAuditStateBehavior<Category>>();

        services.AddEntityFrameworkRepository<Book, CatalogDbContext>()
            .WithTransactions<NullRepositoryTransaction<Book>>()
            .WithBehavior<RepositoryTracingBehavior<Book>>()
            .WithBehavior<RepositoryLoggingBehavior<Book>>()
            .WithBehavior<RepositoryConcurrentBehavior<Book>>()
            .WithBehavior<RepositoryAuditStateBehavior<Book>>()
            .WithBehavior<RepositoryDomainEventBehavior<Book>>()
            .WithBehavior<RepositoryDomainEventPublisherBehavior<Book>>();

        services.AddEntityFrameworkRepository<Publisher, CatalogDbContext>()
            .WithTransactions<NullRepositoryTransaction<Publisher>>()
            .WithBehavior<RepositoryTracingBehavior<Publisher>>()
            .WithBehavior<RepositoryLoggingBehavior<Publisher>>()
            .WithBehavior<RepositoryConcurrentBehavior<Publisher>>()
            .WithBehavior<RepositoryAuditStateBehavior<Publisher>>()
            .WithBehavior<RepositoryDomainEventBehavior<Publisher>>()
            .WithBehavior<RepositoryDomainEventPublisherBehavior<Publisher>>();

        services.AddEntityFrameworkRepository<Author, CatalogDbContext>()
            .WithTransactions<NullRepositoryTransaction<Author>>()
            .WithBehavior<RepositoryTracingBehavior<Author>>()
            .WithBehavior<RepositoryLoggingBehavior<Author>>()
            .WithBehavior<RepositoryConcurrentBehavior<Author>>()
            .WithBehavior<RepositoryAuditStateBehavior<Author>>()
            .WithBehavior<RepositoryDomainEventBehavior<Author>>()
            .WithBehavior<RepositoryDomainEventPublisherBehavior<Author>>();

        return services;
    }
}