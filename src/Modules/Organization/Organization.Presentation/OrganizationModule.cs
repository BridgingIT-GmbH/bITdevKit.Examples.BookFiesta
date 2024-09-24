// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Presentation;

using Application;
using Common;
using DevKit.Domain.Repositories;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Web;

/// <summary>
/// Represents the module for managing the organization within the BookFiesta application.
/// Inherits from WebModuleBase to provide web-specific module behavior.
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

        //services.AddScoped<IOrganizationQueryService, OrganizationQueryService>();

        //services.AddJobScheduling()
        //    .WithJob<EchoJob>(CronExpressions.Every5Minutes); // .WithSingletonJob<EchoJob>(CronExpressions.Every5Minutes)
        //                                                      //.WithJob<HealthCheckJob>(CronExpressions.EveryMinute);

        services.AddScoped<IOrganizationModuleClient, OrganizationModuleClient>();

        services.AddStartupTasks()
            .WithTask<OrganizationDomainSeederTask>(
                o => o.Enabled(environment?.IsDevelopment() == true)
                    .StartupDelay(
                        moduleConfiguration
                            .SeederTaskStartupDelay)); // TODO: should run before any other seeder task because of tenant dependencies (ids)

        services.AddSqlServerDbContext<OrganizationDbContext>(
                o => o.UseConnectionString(moduleConfiguration.ConnectionStrings["Default"])
                    .UseLogger(true, environment?.IsDevelopment() == true),
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery).CommandTimeout(30))
            .WithHealthChecks(timeout: TimeSpan.Parse("00:00:30"))
            //.WithDatabaseCreatorService(o => o
            //    .Enabled(environment?.IsDevelopment() == true)
            //    .DeleteOnStartup())
            .WithDatabaseMigratorService(o => o.Enabled(environment?.IsDevelopment() == true).DeleteOnStartup(false))
            .WithOutboxDomainEventService(
                o => o.ProcessingInterval("00:00:30")
                    .StartupDelay("00:00:15")
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
        new OrganizationCompanyEndpoint().Map(app);
        new OrganizationTenantEndpoints().Map(app);

        return app;
    }
}