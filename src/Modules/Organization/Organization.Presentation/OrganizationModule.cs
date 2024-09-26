// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Presentation;

using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Infrastructure;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Presentation.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

/// <summary>
///     Represents the module for managing the organization within the BookFiesta application.
///     Inherits from WebModuleBase to provide web-specific module behavior.
/// </summary>
public class OrganizationModule : WebModuleBase
{
    public override IServiceCollection Register(
        IServiceCollection services,
        IConfiguration configuration = null,
        IWebHostEnvironment environment = null)
    {
        var moduleConfiguration =
            this.Configure<OrganizationModuleConfiguration, OrganizationModuleConfiguration.Validator>(services, configuration);

        Log.Information("+++++ SQL: " + moduleConfiguration.ConnectionStrings.First().Value);
        //services.AddScoped<IOrganizationQueryService, OrganizationQueryService>();

        //services.AddJobScheduling()
        //    .WithJob<EchoJob>(CronExpressions.Every5Minutes); // .WithSingletonJob<EchoJob>(CronExpressions.Every5Minutes)
        //                                                      //.WithJob<HealthCheckJob>(CronExpressions.EveryMinute);

        services.AddScoped<IOrganizationModuleClient, OrganizationModuleClient>();

        services.AddStartupTasks()
            .WithTask<OrganizationDomainSeederTask>(o => o
                .Enabled(environment?.IsDevelopment() == true)
                .StartupDelay(moduleConfiguration.SeederTaskStartupDelay)); // organization seed has to be done first to accomodate for the tenant FKs

        services.AddSqlServerDbContext<OrganizationDbContext>(o => o
                    .UseConnectionString(moduleConfiguration.ConnectionStrings["Default"])
                    .UseLogger(true, environment?.IsDevelopment() == true),
                o => o
                    .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                    .CommandTimeout(30))
            //.WithHealthChecks(timeout: TimeSpan.Parse("00:00:30"))
            //.WithDatabaseCreatorService(o => o
            //    .Enabled(environment?.IsDevelopment() == true)
            //    .DeleteOnStartup())
            .WithDatabaseMigratorService(o => o
                .StartupDelay("00:00:05") // organization schema has to be created first to accomodate for the tenant FKs
                .Enabled(environment?.IsDevelopment() == true)
                .DeleteOnStartup(false))
            .WithOutboxDomainEventService(o => o
                .ProcessingInterval("00:00:30")
                .StartupDelay("00:00:30")
                .PurgeOnStartup()
                .ProcessingModeImmediate());

        services.AddEntityFrameworkRepository<Company, OrganizationDbContext>()
            .WithTransactions<NullRepositoryTransaction<Company>>()
            .WithBehavior<RepositoryTracingBehavior<Company>>()
            .WithBehavior<RepositoryLoggingBehavior<Company>>()
            .WithBehavior<RepositoryConcurrentBehavior<Company>>()
            .WithBehavior<RepositoryAuditStateBehavior<Company>>()
            //.WithBehavior<RepositoryDomainEventBehavior<Company>>()
            .WithBehavior<RepositoryDomainEventPublisherBehavior<Company>>();

        services.AddEntityFrameworkRepository<Tenant, OrganizationDbContext>()
            .WithTransactions<NullRepositoryTransaction<Tenant>>()
            .WithBehavior<RepositoryTracingBehavior<Tenant>>()
            .WithBehavior<RepositoryLoggingBehavior<Tenant>>()
            .WithBehavior<RepositoryConcurrentBehavior<Tenant>>()
            .WithBehavior<RepositoryAuditStateBehavior<Tenant>>()
            //.WithBehavior<RepositoryDomainEventBehavior<Tenant>>()
            .WithBehavior<RepositoryDomainEventPublisherBehavior<Tenant>>();

        // see below (Map)
        //services.AddEndpoints<OrganizationCompanyEndpoints>();
        //services.AddEndpoints<OrganizationTenantEndpoints>();

        return services;
    }

    public override IEndpointRouteBuilder Map(
        IEndpointRouteBuilder app,
        IConfiguration configuration = null,
        IWebHostEnvironment environment = null)
    {
        app.MapGet(
            "/hello",
            async context =>
            {
                await context.Response.WriteAsync("Hello world");
            });

        // ODER endpoints (grouping)

        new OrganizationCompanyEndpoint().Map(app);
        new OrganizationTenantEndpoints().Map(app);

        return app;
    }
}