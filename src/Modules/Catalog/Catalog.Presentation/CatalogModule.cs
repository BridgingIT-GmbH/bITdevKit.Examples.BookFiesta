namespace BridgingIT.DevKit.Examples.BookFiesta.Catalog.Presentation;
using BridgingIT.DevKit.Application;
using BridgingIT.DevKit.Application.JobScheduling;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookFiesta.Catalog.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Catalog.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.Catalog.Infrastructure;
using BridgingIT.DevKit.Examples.BookFiesta.Catalog.Presentation.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class CatalogModule : WebModuleBase
{
    public override IServiceCollection Register(IServiceCollection services, IConfiguration configuration = null, IWebHostEnvironment environment = null)
    {
        var moduleConfiguration = this.Configure<CatalogModuleConfiguration, CatalogModuleConfiguration.Validator>(services, configuration);

        services.AddScoped<ICatalogQueryService, CatalogQueryService>();

        services.AddJobScheduling()
            .WithJob<EchoJob>(CronExpressions.Every5Minutes); // .WithSingletonJob<EchoJob>(CronExpressions.Every5Minutes)
                                                              //.WithJob<HealthCheckJob>(CronExpressions.EveryMinute);

        //services.AddStartupTasks()
        //    .WithTask<CatalogDomainSeederTask>(o => o
        //        .Enabled(environment?.IsDevelopment() == true)
        //        .StartupDelay(moduleConfiguration.SeederTaskStartupDelay));

        services.AddSqlServerDbContext<CatalogDbContext>(o => o
                .UseConnectionString(moduleConfiguration.ConnectionStrings["Default"])
                .UseLogger(true, environment?.IsDevelopment() == true),
                o => o
                    .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                    .CommandTimeout(30))
            .WithHealthChecks()
            //.WithDatabaseCreatorService(o => o
            //    .StartupDelay("00:00:05") // organization schema has to be created first to accomodate for the tenant FKs
            //    .Enabled(environment?.IsDevelopment() == true)
            //    .DeleteOnStartup(false))
            .WithDatabaseMigratorService(o => o
                .StartupDelay("00:00:35") // organization schema has to be created first to accomodate for the tenant FKs
                .Enabled(environment?.IsDevelopment() == true)
                .DeleteOnStartup(false))
            .WithOutboxDomainEventService(o => o
                .ProcessingInterval("00:00:30")
                .StartupDelay("00:00:15")
                .PurgeOnStartup()
                .ProcessingModeImmediate());

        services.AddEntityFrameworkRepository<Customer, CatalogDbContext>()
            .WithTransactions<NullRepositoryTransaction<Customer>>()
            .WithBehavior<RepositoryTracingBehavior<Customer>>()
            .WithBehavior<RepositoryLoggingBehavior<Customer>>()
            .WithBehavior<RepositoryConcurrentBehavior<Customer>>()
            .WithBehavior<RepositoryAuditStateBehavior<Customer>>()
            //.WithBehavior<RepositoryDomainEventBehavior<Customer>>()
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
            //.WithBehavior<RepositoryDomainEventBehavior<Book>>()
            .WithBehavior<RepositoryDomainEventPublisherBehavior<Book>>();

        services.AddEntityFrameworkRepository<Publisher, CatalogDbContext>()
            .WithTransactions<NullRepositoryTransaction<Publisher>>()
            .WithBehavior<RepositoryTracingBehavior<Publisher>>()
            .WithBehavior<RepositoryLoggingBehavior<Publisher>>()
            .WithBehavior<RepositoryConcurrentBehavior<Publisher>>()
            .WithBehavior<RepositoryAuditStateBehavior<Publisher>>()
            //.WithBehavior<RepositoryDomainEventBehavior<Publisher>>()
            .WithBehavior<RepositoryDomainEventPublisherBehavior<Publisher>>();

        services.AddEntityFrameworkRepository<Author, CatalogDbContext>()
            .WithTransactions<NullRepositoryTransaction<Author>>()
            .WithBehavior<RepositoryTracingBehavior<Author>>()
            .WithBehavior<RepositoryLoggingBehavior<Author>>()
            .WithBehavior<RepositoryConcurrentBehavior<Author>>()
            .WithBehavior<RepositoryAuditStateBehavior<Author>>()
            //.WithBehavior<RepositoryDomainEventBehavior<Author>>()
            .WithBehavior<RepositoryDomainEventPublisherBehavior<Author>>();

        return services;
    }

    public override IEndpointRouteBuilder Map(IEndpointRouteBuilder app, IConfiguration configuration = null, IWebHostEnvironment environment = null)
    {
        //new CatalogBookEndpoints().Map(app);
        //new CatalogCategoryEndpoints().Map(app);
        //new CatalogPublisherEndpoints().Map(app);

        return app;
    }
}