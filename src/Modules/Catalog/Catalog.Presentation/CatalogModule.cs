// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Presentation;

using BridgingIT.DevKit.Application;
using BridgingIT.DevKit.Application.JobScheduling;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.AuthorFiesta.Modules.Catalog.Presentation.Web;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application.Messages;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Infrastructure;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Presentation.Web;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

public class CatalogModule : WebModuleBase
{
    public override IServiceCollection Register(
        IServiceCollection services,
        IConfiguration configuration = null,
        IWebHostEnvironment environment = null)
    {
        var moduleConfiguration =
            this.Configure<CatalogModuleConfiguration, CatalogModuleConfiguration.Validator>(services, configuration);

        Log.Information("+++++ SQL: " + moduleConfiguration.ConnectionStrings.First().Value);
        services.AddScoped<ICatalogQueryService, CatalogQueryService>();

        // services // INFO incase the Organization module is a seperate webservice use refit ->
        //     .AddRefitClient<IOrganizationModuleClient>()
        //     .ConfigureHttpClient(c =>
        //     {
        //         c.BaseAddress = new Uri(configuration["Modules:OrganizationModule:ServiceUrl"]);
        //     });

        services.AddJobScheduling()
            .WithJob<EchoJob>(CronExpressions.Every5Minutes);
        // .WithSingletonJob<EchoJob>(CronExpressions.Every5Minutes)
        //.WithJob<HealthCheckJob>(CronExpressions.EveryMinute);

        services.AddStartupTasks()
            .WithTask<CatalogDomainSeederTask>(o => o
                .Enabled(environment?.IsDevelopment() == true)
                .StartupDelay(moduleConfiguration.SeederTaskStartupDelay));

        services.AddMessaging()
            .WithSubscription<StockCreatedMessage, StockCreatedMessageHandler>()
            .WithSubscription<StockUpdatedMessage, StockUpdatedMessageHandler>();

        services.AddSqlServerDbContext<CatalogDbContext>(o => o
                    .UseConnectionString(moduleConfiguration.ConnectionStrings["Default"])
                    .UseLogger(true, environment?.IsDevelopment() == true),
                o => o
                    .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                    .CommandTimeout(30))
            //.WithHealthChecks(timeout: TimeSpan.Parse("00:00:30"))
            //.WithDatabaseCreatorService(o => o
            //    .StartupDelay("00:00:05")
            //    .Enabled(environment?.IsDevelopment() == true)
            //    .DeleteOnStartup(false))
            .WithDatabaseMigratorService(o => o
                .StartupDelay("00:00:10")
                .Enabled(environment?.IsDevelopment() == true)
                .DeleteOnStartup(false));
        // .WithOutboxDomainEventService(o => o
        //     .ProcessingInterval("00:00:30")
        //     .StartupDelay("00:00:30")
        //     .PurgeOnStartup()
        //     .ProcessingModeImmediate());

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

        // see below (Map)
        //services.AddEndpoints<CatalogCustomerEndpoints>();
        //services.AddEndpoints<CatalogBookEndpoints>();
        //services.AddEndpoints<CatalogCategoryEndpoints>();
        //services.AddEndpoints<CatalogPublisherEndpoints>();

        return services;
    }

    public override IEndpointRouteBuilder Map(
        IEndpointRouteBuilder app,
        IConfiguration configuration = null,
        IWebHostEnvironment environment = null)
    {
        new CatalogCustomerEndpoints().Map(app);
        new CatalogAuthorEndpoints().Map(app);
        new CatalogBookEndpoints().Map(app);
        new CatalogCategoryEndpoints().Map(app);
        new CatalogPublisherEndpoints().Map(app);

        return app;
    }
}