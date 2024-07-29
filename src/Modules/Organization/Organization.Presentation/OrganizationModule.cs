namespace BridgingIT.DevKit.Examples.BookFiesta.Organization.Presentation;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookFiesta.Organization.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Organization.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.Organization.Infrastructure;
using BridgingIT.DevKit.Examples.BookFiesta.Organization.Presentation.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class OrganizationModule : WebModuleBase
{
    public override IServiceCollection Register(IServiceCollection services, IConfiguration configuration = null, IWebHostEnvironment environment = null)
    {
        var moduleConfiguration = this.Configure<OrganizationModuleConfiguration, OrganizationModuleConfiguration.Validator>(services, configuration);

        //services.AddScoped<IOrganizationQueryService, OrganizationQueryService>();

        //services.AddJobScheduling()
        //    .WithJob<EchoJob>(CronExpressions.Every5Minutes); // .WithSingletonJob<EchoJob>(CronExpressions.Every5Minutes)
        //                                                      //.WithJob<HealthCheckJob>(CronExpressions.EveryMinute);

        services.AddStartupTasks()
            .WithTask<OrganizationDomainSeederTask>(o => o
                .Enabled(environment?.IsDevelopment() == true)
                .StartupDelay(moduleConfiguration.SeederTaskStartupDelay)); // TODO: should run before any other seeder task because of tenant dependencies (ids)

        services.AddSqlServerDbContext<OrganizationDbContext>(o => o
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

        return services;
    }

    public override IEndpointRouteBuilder Map(IEndpointRouteBuilder app, IConfiguration configuration = null, IWebHostEnvironment environment = null)
    {
        new OrganizationCompanyEndpoints().Map(app);
        new OrganizationTenantEndpoints().Map(app);

        return app;
    }
}