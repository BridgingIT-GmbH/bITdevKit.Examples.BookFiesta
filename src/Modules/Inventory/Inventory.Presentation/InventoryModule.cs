// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Presentation;

using BridgingIT.DevKit.Application;
using BridgingIT.DevKit.Application.JobScheduling;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Infrastructure;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Presentation.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

public class InventoryModule : WebModuleBase
{
    public override IServiceCollection Register(
        IServiceCollection services,
        IConfiguration configuration = null,
        IWebHostEnvironment environment = null)
    {
        var moduleConfiguration =
            this.Configure<InventoryModuleConfiguration, InventoryModuleConfiguration.Validator>(services, configuration);

        Log.Information("+++++ SQL: " + moduleConfiguration.ConnectionStrings.First().Value);
        services.AddScoped<IInventoryQueryService, InventoryQueryService>();

        services.AddScoped<IInventoryModuleClient, InventoryModuleClient>();
        // services // INFO: incase the Inventory module is a seperate webservice use refit ->
        //     .AddRefitClient<IInventoryModuleClient>()
        //     .ConfigureHttpClient(c =>
        //     {
        //         c.BaseAddress = new Uri(configuration["Modules:InventoryModule:ServiceUrl"]);
        //     });

        services.AddJobScheduling()
            .WithJob<EchoJob>(CronExpressions.Every5Minutes);
        // .WithSingletonJob<EchoJob>(CronExpressions.Every5Minutes)
        //.WithJob<HealthCheckJob>(CronExpressions.EveryMinute);

        services.AddStartupTasks()
            .WithTask<InventoryDomainSeederTask>(o => o
                .Enabled(environment?.IsDevelopment() == true)
                .StartupDelay(moduleConfiguration.SeederTaskStartupDelay));

        services.AddSqlServerDbContext<InventoryDbContext>(o => o
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
                .DeleteOnStartup(false))
            .WithOutboxDomainEventService(o => o
                .ProcessingInterval("00:00:30")
                .StartupDelay("00:00:30")
                .PurgeOnStartup()
                .ProcessingModeImmediate());

        services.AddEntityFrameworkRepository<Stock, InventoryDbContext>()
            .WithTransactions<NullRepositoryTransaction<Stock>>()
            .WithBehavior<RepositoryTracingBehavior<Stock>>()
            .WithBehavior<RepositoryLoggingBehavior<Stock>>()
            .WithBehavior<RepositoryConcurrentBehavior<Stock>>()
            .WithBehavior<RepositoryAuditStateBehavior<Stock>>()
            //.WithBehavior<RepositoryDomainEventBehavior<Stock>>()
            .WithBehavior<RepositoryDomainEventPublisherBehavior<Stock>>();

        services.AddEntityFrameworkRepository<StockSnapshot, InventoryDbContext>()
            .WithTransactions<NullRepositoryTransaction<StockSnapshot>>()
            .WithBehavior<RepositoryTracingBehavior<StockSnapshot>>()
            .WithBehavior<RepositoryLoggingBehavior<StockSnapshot>>()
            .WithBehavior<RepositoryConcurrentBehavior<StockSnapshot>>()
            .WithBehavior<RepositoryAuditStateBehavior<StockSnapshot>>()
            //.WithBehavior<RepositoryDomainEventBehavior<StockSnapshot>>()
            .WithBehavior<RepositoryDomainEventPublisherBehavior<StockSnapshot>>();

        // see below (Map)
        //services.AddEndpoints<InventoryStockEndpoints>();
        //services.AddEndpoints<InventoryStockSnapshotEndpoints>();

        return services;
    }

    public override IEndpointRouteBuilder Map(
        IEndpointRouteBuilder app,
        IConfiguration configuration = null,
        IWebHostEnvironment environment = null)
    {
        new InventoryStockEndpoints().Map(app);
        new InventoryStockSnapshotEndpoints().Map(app);

        return app;
    }
}