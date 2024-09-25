// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

#pragma warning disable SA1200 // Using directives should be placed correctly
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Monitor.OpenTelemetry.Exporter;
using BridgingIT.DevKit.Application.Commands;
using BridgingIT.DevKit.Application.JobScheduling;
using BridgingIT.DevKit.Application.Messaging;
using BridgingIT.DevKit.Application.Queries;
using BridgingIT.DevKit.Application.Utilities;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Infrastructure;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Presentation;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Presentation;
using BridgingIT.DevKit.Examples.BookFiesta.Presentation.Web.Client.Pages;
using BridgingIT.DevKit.Examples.BookFiesta.Presentation.Web.Server;
using BridgingIT.DevKit.Examples.BookFiesta.Presentation.Web.Server.Components;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Application;
using BridgingIT.DevKit.Infrastructure.EntityFramework;
using BridgingIT.DevKit.Presentation;
using BridgingIT.DevKit.Presentation.Web;
using BridgingIT.DevKit.Presentation.Web.JobScheduling;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MudBlazor.Services;
using NSwag;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors.Security;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

#pragma warning restore SA1200 // Using directives should be placed correctly

// ===============================================================================================
// Create the webhost
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Host.ConfigureAppConfiguration();
builder.Host.ConfigureLogging(builder.Configuration);

// ===============================================================================================
// Configure the modules
builder.Services.AddModules(builder.Configuration, builder.Environment)
    .WithModule<OrganizationModule>()
    .WithModule<CatalogModule>()
    .WithModuleContextAccessors()
    .WithRequestModuleContextAccessors()
    .WithModuleControllers(c => c.AddJsonOptions(ConfigureJsonOptions));

builder.Services.Configure<JsonOptions>(ConfigureJsonOptions); // configure json for minimal apis

// ===============================================================================================
// Configure the services
builder.Services.AddMediatR(); // or AddDomainEvents()?
builder.Services.AddMapping().WithMapster();

builder.Services.AddCommands()
    .WithBehavior(typeof(ModuleScopeCommandBehavior<,>))
    //.WithBehavior(typeof(ChaosExceptionCommandBehavior<,>))
    .WithBehavior(typeof(RetryCommandBehavior<,>))
    .WithBehavior(typeof(TimeoutCommandBehavior<,>))
    .WithBehavior(typeof(TenantAwareCommandBehavior<,>));
builder.Services.AddQueries()
    .WithBehavior(typeof(ModuleScopeQueryBehavior<,>))
    //.WithBehavior(typeof(ChaosExceptionQueryBehavior<,>))
    .WithBehavior(typeof(RetryQueryBehavior<,>))
    .WithBehavior(typeof(TimeoutQueryBehavior<,>))
    .WithBehavior(typeof(TenantAwareQueryBehavior<,>));

builder.Services.AddJobScheduling(
        o => o.StartupDelay(builder.Configuration["JobScheduling:StartupDelay"]),
        builder.Configuration)
    //.WithJob<HealthCheckJob>(CronExpressions.Every10Seconds)
    .WithBehavior<ModuleScopeJobSchedulingBehavior>()
    //.WithBehavior<ChaosExceptionJobSchedulingBehavior>()
    .WithBehavior<RetryJobSchedulingBehavior>()
    .WithBehavior<TimeoutJobSchedulingBehavior>();

builder.Services.AddStartupTasks(o => o.Enabled().StartupDelay(builder.Configuration["StartupTasks:StartupDelay"]))
    .WithTask<EchoStartupTask>(o => o.Enabled(builder.Environment.IsDevelopment()).StartupDelay("00:00:03"))
    .WithTask<
        JobSchedulingSqlServerSeederStartupTask>() // uses quartz configuration from appsettings JobScheduling:Quartz:quartz...
    .WithBehavior<ModuleScopeStartupTaskBehavior>()
    //.WithBehavior<ChaosExceptionStartupTaskBehavior>()
    .WithBehavior<RetryStartupTaskBehavior>()
    .WithBehavior<TimeoutStartupTaskBehavior>();

builder.Services
    .AddMessaging(builder.Configuration, o => o.StartupDelay(builder.Configuration["Messaging:StartupDelay"]))
    .WithBehavior<ModuleScopeMessagePublisherBehavior>()
    .WithBehavior<ModuleScopeMessageHandlerBehavior>()
    .WithBehavior<MetricsMessagePublisherBehavior>()
    .WithBehavior<MetricsMessageHandlerBehavior>()
    //.WithBehavior<ChaosExceptionMessageHandlerBehavior>()
    .WithBehavior<RetryMessageHandlerBehavior>()
    .WithBehavior<TimeoutMessageHandlerBehavior>()
    .WithOutbox<CatalogDbContext>(
        o => o // registers the outbox publisher behavior and worker service at once
            .ProcessingInterval("00:00:30")
            .ProcessingModeImmediate() // forwards the outbox message, through a queue, to the outbox worker
            .StartupDelay("00:00:15")
            .PurgeOnStartup())
    .WithInProcessBroker(); //.WithRabbitMQBroker();

ConfigureHealth(builder.Services);

builder.Services
    .AddMetrics(); // TOOL: dotnet-counters monitor -n BridgingIT.DevKit.Examples.BookFiesta.Presentation.Web.Server --counters bridgingit_devkit
builder.Services.Configure<ApiBehaviorOptions>(ConfiguraApiBehavior);
builder.Services.AddSingleton<IConfigurationRoot>(builder.Configuration);
builder.Services.AddScoped<ICurrentUserAccessor, FakeCurrentUserAccessor>();
builder.Services.AddProblemDetails(o => Configure.ProblemDetails(o, true));
//builder.Services.AddProblemDetails(Configure.ProblemDetails); // TODO: replace this with the new .NET8 error handling with IExceptionHandler https://www.milanjovanovic.tech/blog/global-error-handling-in-aspnetcore-8 and AddProblemDetails https://youtu.be/4NfflZilTvk?t=596
//builder.Services.AddExceptionHandler();
//builder.Services.AddProblemDetails();

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveWebAssemblyComponents();

//builder.Services.AddHostedService<Service1>();
//builder.Services.AddHostedService<Service2>();

builder.Services.AddLocalization();
builder.Services.AddMudServices();
builder.Services.AddSignalR();

builder.Services.AddEndpoints<SystemEndpoints>(builder.Environment.IsDevelopment());
builder.Services.AddEndpoints<JobSchedulingEndpoints>(builder.Environment.IsDevelopment());
//builder.Services.AddEndpoints();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(ConfigureOpenApiDocument);

if (!builder.Environment.IsDevelopment())
{
    builder.Services
        .AddApplicationInsightsTelemetry(); // https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core
}

builder.Logging.AddOpenTelemetry(
    logging =>
    {
        logging.IncludeFormattedMessage = true;
        logging.IncludeScopes = true;
    });
builder.Services.AddOpenTelemetry().WithMetrics(ConfigureMetrics).WithTracing(ConfigureTracing);

// ===============================================================================================
// Configure the HTTP request pipeline
var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", true);
    app.UseHsts();
}

//app.UseResponseCompression();
app.UseHttpsRedirection();

app.UseProblemDetails();
//app.UseExceptionHandler();

app.UseRequestCorrelation();
app.UseRequestModuleContext();
app.UseRequestLogging();

app.UseOpenApi();
app.UseSwaggerUi();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseModules();

app.UseAuthentication(); // TODO: move to IdentityModule
app.UseAuthorization(); // TODO: move to IdentityModule

if (builder.Configuration["Metrics:Prometheus:Enabled"].To<bool>())
{
    app.UseOpenTelemetryPrometheusScrapingEndpoint();
}

app.MapModules();
app.MapControllers();
app.MapEndpoints(); // adds the endpoints to the application
app.MapHealthChecks();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    //.AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Counter).Assembly);

app.MapHub<NotificationHub>("/signalrhub");

app.Run();

void ConfiguraApiBehavior(ApiBehaviorOptions options)
{
    options.SuppressModelStateInvalidFilter = true;
}

void ConfigureJsonOptions(JsonOptions options)
{
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
}

void ConfigureHealth(IServiceCollection services)
{
    var seqServerUrl = builder.Configuration.GetSection("Serilog:WriteTo")
        .GetChildren()
        .FirstOrDefault(x => x["Name"] == "Seq")?["Args:serverUrl"];

    services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy(), ["self"])
        .AddSeqPublisher(s => s.Endpoint = seqServerUrl);
    //.AddCheck<RandomHealthCheck>("random")
    //.AddApplicationInsightsPublisher()

    //services.Configure<HealthCheckPublisherOptions>(options =>
    //{
    //    options.Delay = TimeSpan.FromSeconds(5);
    //});

    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
    services.AddHealthChecksUI() // https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/blob/master/README.md
        .AddInMemoryStorage();
    //.AddSqliteStorage($"Data Source=data_health.db");
}

void ConfigureMetrics(MeterProviderBuilder provider)
{
    provider.AddRuntimeInstrumentation()
        .AddMeter(
            "Microsoft.AspNetCore.Hosting",
            "Microsoft.AspNetCore.Server.Kestrel",
            "System.Net.Http",
            "BridgingIT.DevKit");

    if (builder.Configuration["Metrics:Prometheus:Enabled"].To<bool>())
    {
        Log.Logger.Information("{LogKey} prometheus exporter enabled (endpoint={MetricsEndpoint})", "MET", "/metrics");
        provider.AddPrometheusExporter();
    }
}

void ConfigureTracing(TracerProviderBuilder provider)
{
    // TODO: multiple per module tracer needed? https://github.com/open-telemetry/opentelemetry-dotnet/issues/2040
    // https://opentelemetry.io/docs/instrumentation/net/getting-started/
    var serviceName = Assembly.GetExecutingAssembly().GetName().Name; //TODO: use ModuleExtensions.ServiceName

    if (builder.Environment.IsDevelopment())
    {
        provider.SetSampler(new AlwaysOnSampler());
    }
    else
    {
        provider.SetSampler(new TraceIdRatioBasedSampler(1));
    }

    provider
        //.AddSource(ModuleExtensions.Modules.Select(m => m.Name).Insert(serviceName).ToArray()) // TODO: provide a nice (module) extension for this -> .AddModuleSources() // NOT NEEDED, * will add all activitysources
        .AddSource("*")
        .SetErrorStatusOnException()
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName)
                .AddTelemetrySdk()
                .AddAttributes(
                    new Dictionary<string, object>
                    {
                        ["host.name"] = Environment.MachineName,
                        ["os.description"] = RuntimeInformation.OSDescription,
                        ["deployment.environment"] = builder.Environment.EnvironmentName.ToLowerInvariant()
                    }))
        .SetErrorStatusOnException()
        .AddAspNetCoreInstrumentation(
            opts =>
            {
                opts.RecordException = true;
                opts.Filter = context => !context.Request.Path.ToString()
                    .EqualsPatternAny(new RequestLoggingOptions().PathBlackListPatterns);
            })
        .AddHttpClientInstrumentation(
            opts =>
            {
                opts.RecordException = true;
                opts.FilterHttpRequestMessage = req
                    => !req.RequestUri.PathAndQuery.EqualsPatternAny(
                        new RequestLoggingOptions().PathBlackListPatterns.Insert("*api/events/raw"));
            })
        .AddSqlClientInstrumentation(
            opts =>
            {
                opts.Filter = cmd
                    => // https://github.com/open-telemetry/opentelemetry-dotnet-contrib/blob/main/src/OpenTelemetry.Instrumentation.SqlClient/README.md#filter
                {
                    if (cmd is SqlCommand command)
                    {
                        return !command.CommandText.Contains("QRTZ_") &&
                            !command.CommandText.Contains("__MigrationsHistory") &&
                            !command.CommandText.Equals("SELECT 1;");
                    }

                    return false;
                };
                opts.RecordException = true;
                opts.EnableConnectionLevelAttributes = true;
                opts.SetDbStatementForText = true;
            });

    if (builder.Configuration["Tracing:Jaeger:Enabled"].To<bool>())
    {
        Log.Logger.Information(
            "{LogKey} jaeger exporter enabled (host={JaegerHost})",
            "TRC",
            builder.Configuration["Tracing:Jaeger:AgentHost"]);
        provider.AddJaegerExporter(
            opts =>
            {
                opts.AgentHost = builder.Configuration["Tracing:Jaeger:AgentHost"];
                opts.AgentPort = Convert.ToInt32(builder.Configuration["Tracing:Jaeger:AgentPort"]);
                opts.ExportProcessorType = ExportProcessorType.Simple;
            });
    }

    if (builder.Configuration["Tracing:Console:Enabled"].To<bool>())
    {
        Log.Logger.Information("{LogKey} console exporter enabled", "TRC");
        provider.AddConsoleExporter();
    }

    if (builder.Configuration["Tracing:AzureMonitor:Enabled"].To<bool>())
    {
        Log.Logger.Information("{LogKey} azuremonitor exporter enabled", "TRC");
        provider.AddAzureMonitorTraceExporter(
            o =>
            {
                o.ConnectionString = builder.Configuration["Tracing:AzureMonitor:ConnectionString"].EmptyToNull() ??
                    Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
            });
    }

    var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

    if (useOtlpExporter)
    {
        builder.Services.AddOpenTelemetry().UseOtlpExporter();
    }
}

void ConfigureOpenApiDocument(AspNetCoreOpenApiDocumentGeneratorSettings settings)
{
    settings.DocumentName = "v1";
    settings.Version = "v1";
    settings.Title = "Backend API";
    settings.AddSecurity(
        "bearer",
        [],
        new OpenApiSecurityScheme
        {
            Type = OpenApiSecuritySchemeType.OAuth2,
            Flow = OpenApiOAuth2Flow.Implicit,
            Description = "Oidc Authentication",
            Flows = new OpenApiOAuthFlows
            {
                Implicit = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = $"{builder.Configuration["Oidc:Authority"]}/protocol/openid-connect/auth",
                    TokenUrl = $"{builder.Configuration["Oidc:Authority"]}/protocol/openid-connect/token",
                    Scopes = new Dictionary<string, string>
                    {
                        //{"openid", "openid"},
                    }
                }
            }
        });
    settings.OperationProcessors.Add(new AuthorizeRolesSummaryOperationProcessor());
    settings.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("bearer"));
    settings.OperationProcessors.Add(new AuthorizationOperationProcessor("bearer"));
}

public partial class Program
{
    // this partial class is needed to set the accessibilty for the Program class to public
    // needed for testing with a test fixture https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0#basic-tests-with-the-default-webapplicationfactory
}

//#pragma warning disable SA1402 // File may only contain a single type
//public class Service1 : IHostedService
//{
//    private readonly ILogger<Service1> logger;

//    public Service1(ILogger<Service1> logger)
//    {
//        this.logger = logger;
//    }

//    public async Task StartAsync(CancellationToken cancellationToken)
//    {
//        this.logger.LogInformation("Service1 starting at: {time}", DateTimeOffset.UtcNow);
//        await Task.Delay(5000, cancellationToken);
//        this.logger.LogInformation("Service1 delay completed at: {time}", DateTimeOffset.UtcNow);
//    }

//    public Task StopAsync(CancellationToken cancellationToken)
//    {
//        this.logger.LogInformation("Service1 stopping.");
//        return Task.CompletedTask;
//    }
//}

//public class Service2 : IHostedService
//{
//    private readonly ILogger<Service2> logger;

//    public Service2(ILogger<Service2> logger)
//    {
//        this.logger = logger;
//    }

//    public Task StartAsync(CancellationToken cancellationToken)
//    {
//        this.logger.LogInformation("Service2 starting at: {time}", DateTimeOffset.UtcNow);
//        return Task.CompletedTask;
//    }

//    public Task StopAsync(CancellationToken cancellationToken)
//    {
//        this.logger.LogInformation("Service2 stopping.");
//        return Task.CompletedTask;
//    }
//}
//#pragma warning restore SA1402 // File may only contain a single type