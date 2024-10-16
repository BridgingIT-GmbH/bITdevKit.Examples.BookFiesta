// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\build\Build.cs
// ----------------------------------------
using System;
using System.IO;
using System.Linq;
using Nuke.Common;
// using Nuke.Common.CI;
// using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.SonarScanner;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Docker.DockerTasks;

// ReSharper disable All
#pragma warning disable CS0618 // Type or member is obsolete

// dotnet tool install Nuke.GlobalTool --global
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.All);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Solution file to build")]
    readonly string SolutionFile;

    [Solution] Solution Solution;

    [Parameter("SonarQube server URL")]
    readonly string SonarServerUrl = "http://localhost:9000";

    [Parameter("SonarQube Docker image")]
    readonly string SonarQubeDockerImage = "sonarqube:latest";

    [Parameter("SonarQube Docker container name")]
    readonly string SonarQubeContainerName = "sonarqube_container_bookfiesta";

    [Parameter("Host path for SonarQube data")]
    readonly AbsolutePath SonarQubeHostPath = RootDirectory / "tools" / "sonarqube" / "data";

    [Parameter("Source directory containing .cs and .md files")]
    readonly AbsolutePath SourceDirectory = RootDirectory / "src";

    [Parameter("Output file path for concatenated files")]
    readonly AbsolutePath CodeDumpOutputFile = RootDirectory / "CodeDump.cs";

    // Default local admin credentials
    readonly string SonarLogin = "admin";
    readonly string SonarPassword = "admin";

    // Use the solution name for both the SonarQube Project Name and Key
    string SonarProjectName => Solution.Name;
    string SonarProjectKey => Solution.Name.Replace(" ", "_").ToLower();

    protected override void OnBuildInitialized()
    {
        if (string.IsNullOrEmpty(SolutionFile))
        {
            Console.WriteLine("Using default solution file.");
        }
        else
        {
            Console.WriteLine($"Using specified solution file: {SolutionFile}");
            Solution = ProjectModelTasks.ParseSolution(SolutionFile);
        }
    }

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            // EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target EnsureSonarQubeContainer => _ => _
        .Executes(() =>
        {
            var isRunning = DockerPs(settings => settings
                .SetQuiet(true)
                .SetFilter($"name={SonarQubeContainerName}"))
                .Any();

            if (!isRunning)
            {
                SonarQubeHostPath.CreateDirectory();

                DockerRun(settings => settings
                    .SetImage(SonarQubeDockerImage)
                    .SetName(SonarQubeContainerName)
                    .SetPublish("9000:9000"));
                    //.SetVolume($"{SonarQubeHostPath}:/opt/sonarqube/data")
                    //.SetDetach(true)
                    //.SetNetwork("host"));

                System.Threading.Thread.Sleep(30000);
            }
            else
            {
                Console.WriteLine("SonarQube container is already running.");
            }
        });

    Target SonarBegin => _ => _
        .DependsOn(EnsureSonarQubeContainer)
        .Before(Compile)
        .Executes(() =>
        {
            SonarScannerTasks.SonarScannerBegin(s => s
                .SetProjectKey(SonarProjectKey)
                .SetName(SonarProjectName)
                .SetServer(SonarServerUrl)
                .SetLogin(SonarLogin)
                .SetPassword(SonarPassword)
                .SetFramework("net5.0"));
        });

    Target SonarEnd => _ => _
        .After(Test)
        .Executes(() =>
        {
            SonarScannerTasks.SonarScannerEnd(s => s
                .SetLogin(SonarLogin)
                .SetPassword(SonarPassword));
        });

    Target SonarAnalysis => _ => _
        .DependsOn(SonarBegin, Test, SonarEnd)
        .Executes(() =>
        {
            Console.WriteLine($"SonarQube analysis complete for project '{SonarProjectName}'.");
            Console.WriteLine($"Project Key: {SonarProjectKey}");
            Console.WriteLine($"Solution analyzed: {Solution.Path}");
            Console.WriteLine($"You can view the results at {SonarServerUrl}");
            Console.WriteLine("Default login credentials are admin/admin. Please change them after first login.");
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore()
                .EnableNoBuild()
                .EnableCollectCoverage()
                .SetCoverletOutputFormat(CoverletOutputFormat.opencover));
                // .SetResultsDirectory(TestResultsDirectory));
        });

    Target CodeDump => _ => _
    .Executes(() =>
    {
        var files = RootDirectory.GlobFiles("**/*.cs", "**/*.md")
            .Where(file =>
                !file.Name.Equals("GlobalSuppressions.cs", StringComparison.OrdinalIgnoreCase) &&
                !file.Name.Equals("AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase) &&
                !file.Name.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) &&
                !file.Name.EndsWith("ApiClient.cs") &&
                !file.Name.EndsWith("GlobalUsings.cs") &&
                !file.Name.EndsWith("CodeDump.cs") &&
                !file.Name.EndsWith("CODE_OF_CONDUCT") &&
                !file.ToString().Contains("GlobalSuppressions.cs") &&
                !file.ToString().Contains("Migrations") &&
                !file.ToString().Contains(".UnitTests") &&
                !file.ToString().Contains(".IntegrationTests")&&
                !file.ToString().Contains(@"\bin\") &&            // Exclude bin folders
                !file.ToString().Contains(@"\obj\"))
            .ToList();

        //files.AddRange(RootDirectory.GlobFiles("*.md"));

        var totalFiles = files.Count;
        var processedFiles = 0;
        var csFiles = 0;
        var mdFiles = 0;
        var totalLines = 0;

        Console.WriteLine($"Starting to process {totalFiles} files (.cs and .md)...");

        using (var writer = new StreamWriter(CodeDumpOutputFile))
        {
            foreach (var file in files)
            {
                processedFiles++;
                var fileName = file.Name;
                var extension = Path.GetExtension(fileName).ToLower();

                Console.WriteLine($"Processing: {fileName} ({processedFiles} of {totalFiles})");

                writer.WriteLine($"// File: {file}");
                writer.WriteLine("// ----------------------------------------");

                var lines = File.ReadAllLines(file);
                var contentStartIndex = FindContentStartIndex(lines);
                totalLines += lines.Length - contentStartIndex;

                for (int i = contentStartIndex; i < lines.Length; i++)
                {
                    writer.WriteLine(lines[i]);
                }

                writer.WriteLine();

                if (extension == ".cs")
                    csFiles++;
                else if (extension == ".md")
                    mdFiles++;
            }
        }

        Console.WriteLine($"All files have been concatenated into {CodeDumpOutputFile}");
        Console.WriteLine($"Total files processed: {totalFiles}");
        Console.WriteLine($"C# files: {csFiles}");
        Console.WriteLine($"Markdown files: {mdFiles}");
        Console.WriteLine($"Total lines: {totalLines}");
    });

private int FindContentStartIndex(string[] lines)
{
    for (var i = 0; i < lines.Length; i++)
    {
        if (!string.IsNullOrWhiteSpace(lines[i]) && !lines[i].TrimStart().StartsWith("//"))
        {
            return i;
        }
    }
    return 0;
}

    Target All => _ => _
        .DependsOn(SonarAnalysis, CodeDump)
        .Executes(() =>
        {
            Console.WriteLine("All tasks completed successfully.");
        });
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\build\Configuration.cs
// ----------------------------------------
using System;
using System.ComponentModel;
using System.Linq;
using Nuke.Common.Tooling;

[TypeConverter(typeof(TypeConverter<Configuration>))]
public class Configuration : Enumeration
{
    public static Configuration Debug = new Configuration { Value = nameof(Debug) };
    public static Configuration Release = new Configuration { Value = nameof(Release) };

    public static implicit operator string(Configuration configuration)
    {
        return configuration.Value;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Presentation.AppHost\HealthCheckAnnotation.cs
// ----------------------------------------
namespace Aspire.Hosting;

using Microsoft.Extensions.Diagnostics.HealthChecks;

/// <summary>
///     An annotation that associates a health check factory with a resource
/// </summary>
/// <param name="healthCheckFactory">A function that creates the health check</param>
public class HealthCheckAnnotation(Func<IResource, CancellationToken, Task<IHealthCheck?>> healthCheckFactory)
    : IResourceAnnotation
{
    public Func<IResource, CancellationToken, Task<IHealthCheck?>> HealthCheckFactory { get; } = healthCheckFactory;

    public static HealthCheckAnnotation Create(Func<string, IHealthCheck> connectionStringFactory)
    {
        return new HealthCheckAnnotation(
            async (resource, token) =>
            {
                if (resource is not IResourceWithConnectionString c)
                {
                    return null;
                }

                if (await c.GetConnectionStringAsync(token) is not string cs)
                {
                    return null;
                }

                return connectionStringFactory(cs);
            });
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Presentation.AppHost\HttpEndpointHealthCheckExtensions.cs
// ----------------------------------------
namespace Aspire.Hosting;

using HealthChecks.Uris;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public static class HttpEndpointHealthCheckExtensions
{
    /// <summary>
    ///     Adds a health check to the resource with HTTP endpoints.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="endpointName">
    ///     The optional name of the endpoint. If not specified, will be the first http or https
    ///     endpoint (based on scheme).
    /// </param>
    /// <param name="path">path to send the HTTP request to. This will be appended to the base URL of the resolved endpoint.</param>
    /// <param name="configure">A callback to configure the options for this health check.</param>
    public static IResourceBuilder<T> WithHealthCheck<T>(
        this IResourceBuilder<T> builder,
        string? endpointName = null,
        string path = "health",
        Action<UriHealthCheckOptions>? configure = null)
        where T : IResourceWithEndpoints
    {
        return builder.WithAnnotation(
            new HealthCheckAnnotation(
                (resource, ct) =>
                {
                    if (resource is not IResourceWithEndpoints resourceWithEndpoints)
                    {
                        return Task.FromResult<IHealthCheck?>(null);
                    }

                    var endpoint = endpointName is null
                        ? resourceWithEndpoints.GetEndpoints().FirstOrDefault(e => e.Scheme is "http" or "https")
                        : resourceWithEndpoints.GetEndpoint(endpointName);

                    var url = endpoint?.Url;

                    if (url is null)
                    {
                        return Task.FromResult<IHealthCheck?>(null);
                    }

                    var options = new UriHealthCheckOptions();

                    options.AddUri(new Uri(new Uri(url), path));

                    configure?.Invoke(options);

                    var client = new HttpClient();

                    return Task.FromResult<IHealthCheck?>(new UriHealthCheck(options, () => client));
                }));
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Presentation.AppHost\Program.cs
// ----------------------------------------
var builder = DistributedApplication.CreateBuilder(args);

//var sqlPassword = builder.AddParameter("sql-password", secret: true);
var sql = builder.AddSqlServer(
        "sql")
        //port: 14329)
        // password: sqlPassword)
    .WithImageTag("latest")
    //.WithDataVolume() // requires persistent password https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/persist-data-volumes
    //.WithHealthCheck()
    .AddDatabase("sqldata");

builder.AddProject<Projects.Presentation_Web_Server>("presentation-web-server")
    .WaitFor(sql)
    .WithReference(sql);

//TODO: add SEQ integration https://learn.microsoft.com/en-us/dotnet/aspire/logging/seq-integration?tabs=dotnet-cli

builder.Build().Run();

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Presentation.AppHost\SqlResourceHealthCheckExtensions.cs
// ----------------------------------------
namespace Aspire.Hosting;

using HealthChecks.SqlServer;

public static class SqlResourceHealthCheckExtensions
{
    /// <summary>
    ///     Adds a health check to the SQL Server resource.
    /// </summary>
    public static IResourceBuilder<SqlServerServerResource> WithHealthCheck(
        this IResourceBuilder<SqlServerServerResource> builder)
    {
        return builder.WithSqlHealthCheck(cs => new SqlServerHealthCheckOptions { ConnectionString = cs });
    }

    /// <summary>
    ///     Adds a health check to the SQL Server database resource.
    /// </summary>
    public static IResourceBuilder<SqlServerDatabaseResource> WithHealthCheck(
        this IResourceBuilder<SqlServerDatabaseResource> builder)
    {
        return builder.WithSqlHealthCheck(cs => new SqlServerHealthCheckOptions { ConnectionString = cs });
    }

    /// <summary>
    ///     Adds a health check to the SQL Server resource with a specific query.
    /// </summary>
    public static IResourceBuilder<SqlServerServerResource> WithHealthCheck(
        this IResourceBuilder<SqlServerServerResource> builder,
        string query)
    {
        return builder.WithSqlHealthCheck(
            cs => new SqlServerHealthCheckOptions { ConnectionString = cs, CommandText = query });
    }

    /// <summary>
    ///     Adds a health check to the SQL Server database resource  with a specific query.
    /// </summary>
    public static IResourceBuilder<SqlServerDatabaseResource> WithHealthCheck(
        this IResourceBuilder<SqlServerDatabaseResource> builder,
        string query)
    {
        return builder.WithSqlHealthCheck(
            cs => new SqlServerHealthCheckOptions { ConnectionString = cs, CommandText = query });
    }

    private static IResourceBuilder<T> WithSqlHealthCheck<T>(
        this IResourceBuilder<T> builder,
        Func<string, SqlServerHealthCheckOptions> healthCheckOptionsFactory)
        where T : IResource
    {
        return builder.WithAnnotation(
            HealthCheckAnnotation.Create(cs => new SqlServerHealthCheck(healthCheckOptionsFactory(cs))));
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Presentation.AppHost\WaitForDependenciesExtensions.cs
// ----------------------------------------
namespace Aspire.Hosting;

using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

public static class WaitForDependenciesExtensions
{
    /// <summary>
    /// Wait for a resource to be running before starting another resource.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="other">The resource to wait for.</param>
    public static IResourceBuilder<T> WaitFor<T>(this IResourceBuilder<T> builder, IResourceBuilder<IResource> other)
        where T : IResource
    {
        builder.ApplicationBuilder.AddWaitForDependencies();
        return builder.WithAnnotation(new WaitOnAnnotation(other.Resource));
    }

    /// <summary>
    /// Wait for a resource to run to completion before starting another resource.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="other">The resource to wait for.</param>
    public static IResourceBuilder<T> WaitForCompletion<T>(this IResourceBuilder<T> builder, IResourceBuilder<IResource> other)
        where T : IResource
    {
        builder.ApplicationBuilder.AddWaitForDependencies();
        return builder.WithAnnotation(new WaitOnAnnotation(other.Resource) { WaitUntilCompleted = true });
    }

    /// <summary>
    /// Adds a lifecycle hook that waits for all dependencies to be "running" before starting resources. If that resource
    /// has a health check, it will be executed before the resource is considered "running".
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    private static IDistributedApplicationBuilder AddWaitForDependencies(this IDistributedApplicationBuilder builder)
    {
        builder.Services.TryAddLifecycleHook<WaitForDependenciesRunningHook>();
        return builder;
    }

    private class WaitOnAnnotation(IResource resource) : IResourceAnnotation
    {
        public IResource Resource { get; } = resource;

#pragma warning disable CS8669 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. Auto-generated code requires an explicit '#nullable' directive in source.
        public string[]? States { get; set; }
#pragma warning restore CS8669 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. Auto-generated code requires an explicit '#nullable' directive in source.

        public bool WaitUntilCompleted { get; set; }
    }

    private class WaitForDependenciesRunningHook(DistributedApplicationExecutionContext executionContext,
        ResourceNotificationService resourceNotificationService) :
        IDistributedApplicationLifecycleHook,
        IAsyncDisposable
    {
        private readonly CancellationTokenSource _cts = new();

        public Task BeforeStartAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken = default)
        {
            // We don't need to execute any of this logic in publish mode
            if (executionContext.IsPublishMode)
            {
                return Task.CompletedTask;
            }

            // The global list of resources being waited on
            var waitingResources = new ConcurrentDictionary<IResource, ConcurrentDictionary<WaitOnAnnotation, TaskCompletionSource>>();

            // For each resource, add an environment callback that waits for dependencies to be running
            foreach (var r in appModel.Resources)
            {
                var resourcesToWaitOn = r.Annotations.OfType<WaitOnAnnotation>().ToLookup(a => a.Resource);

                if (resourcesToWaitOn.Count == 0)
                {
                    continue;
                }

                // Abuse the environment callback to wait for dependencies to be running

                r.Annotations.Add(new EnvironmentCallbackAnnotation(async context =>
                {
                    var dependencies = new List<Task>();

                    // Find connection strings and endpoint references and get the resource they point to
                    foreach (var group in resourcesToWaitOn)
                    {
                        var resource = group.Key;

                        // REVIEW: This logic does not handle cycles in the dependency graph (that would result in a deadlock)

                        // Don't wait for yourself
                        if (resource != r && resource is not null)
                        {
                            var pendingAnnotations = waitingResources.GetOrAdd(resource, _ => new());

                            foreach (var waitOn in group)
                            {
                                var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

                                async Task Wait()
                                {
                                    context.Logger?.LogInformation("Waiting for {Resource}.", waitOn.Resource.Name);

                                    await tcs.Task;

                                    context.Logger?.LogInformation("Waiting for {Resource} completed.", waitOn.Resource.Name);
                                }

                                pendingAnnotations[waitOn] = tcs;

                                dependencies.Add(Wait());
                            }
                        }
                    }

                    await resourceNotificationService.PublishUpdateAsync(r, s => s with
                    {
                        State = new("Waiting", KnownResourceStateStyles.Info)
                    });
                    
                    await Task.WhenAll(dependencies).WaitAsync(context.CancellationToken);
                }));
            }

            _ = Task.Run(async () =>
           {
               var stoppingToken = _cts.Token;

               // These states are terminal but we need a better way to detect that
               static bool IsKnownTerminalState(CustomResourceSnapshot snapshot) =>
                   snapshot.State == "FailedToStart" ||
                   snapshot.State == "Exited" ||
                   snapshot.ExitCode is not null;

               // Watch for global resource state changes
               await foreach (var resourceEvent in resourceNotificationService.WatchAsync(stoppingToken))
               {
                   if (waitingResources.TryGetValue(resourceEvent.Resource, out var pendingAnnotations))
                   {
                       foreach (var (waitOn, tcs) in pendingAnnotations)
                       {
                           if (waitOn.States is string[] states && states.Contains(resourceEvent.Snapshot.State?.Text, StringComparer.Ordinal))
                           {
                               pendingAnnotations.TryRemove(waitOn, out _);

                               _ = DoTheHealthCheck(resourceEvent, tcs);
                           }
                           else if (waitOn.WaitUntilCompleted)
                           {
                               if (IsKnownTerminalState(resourceEvent.Snapshot))
                               {
                                   pendingAnnotations.TryRemove(waitOn, out _);

                                   _ = DoTheHealthCheck(resourceEvent, tcs);
                               }
                           }
                           else if (waitOn.States is null)
                           {
                               if (resourceEvent.Snapshot.State.Text == "Running")
                               {
                                   pendingAnnotations.TryRemove(waitOn, out _);

                                   _ = DoTheHealthCheck(resourceEvent, tcs);
                               }
                               else if (IsKnownTerminalState(resourceEvent.Snapshot))
                               {
                                   pendingAnnotations.TryRemove(waitOn, out _);

                                   tcs.TrySetException(new Exception($"Dependency {waitOn.Resource.Name} failed to start"));
                               }
                           }
                       }
                   }
               }
           },
           cancellationToken);

            return Task.CompletedTask;
        }

        private async Task DoTheHealthCheck(ResourceEvent resourceEvent, TaskCompletionSource tcs)
        {
            var resource = resourceEvent.Resource;

            // REVIEW: Right now, every resource does an independent health check, we could instead cache
            // the health check result and reuse it for all resources that depend on the same resource


#pragma warning disable CS8669 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. Auto-generated code requires an explicit '#nullable' directive in source.
            HealthCheckAnnotation? healthCheckAnnotation = null;
#pragma warning restore CS8669 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. Auto-generated code requires an explicit '#nullable' directive in source.

            // Find the relevant health check annotation. If the resource has a parent, walk up the tree
            // until we find the health check annotation.
            while (true)
            {
                // If we find a health check annotation, break out of the loop
                if (resource.TryGetLastAnnotation(out healthCheckAnnotation))
                {
                    break;
                }

                // If the resource has a parent, walk up the tree
                if (resource is IResourceWithParent parent)
                {
                    resource = parent.Parent;
                }
                else
                {
                    break;
                }
            }

#pragma warning disable CS8669 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. Auto-generated code requires an explicit '#nullable' directive in source.
            Func<CancellationToken, ValueTask>? operation = null;
#pragma warning restore CS8669 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. Auto-generated code requires an explicit '#nullable' directive in source.

            if (healthCheckAnnotation?.HealthCheckFactory is { } factory)
            {
#pragma warning disable CS8669 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. Auto-generated code requires an explicit '#nullable' directive in source.
                IHealthCheck? check;
#pragma warning restore CS8669 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. Auto-generated code requires an explicit '#nullable' directive in source.

                try
                {
                    // TODO: Do need to pass a cancellation token here?
                    check = await factory(resource, default);

                    if (check is not null)
                    {
                        var context = new HealthCheckContext()
                        {
                            Registration = new HealthCheckRegistration("", check, HealthStatus.Unhealthy, [])
                        };

                        operation = async (cancellationToken) =>
                        {
                            var result = await check.CheckHealthAsync(context, cancellationToken);

                            if (result.Exception is not null)
                            {
                                ExceptionDispatchInfo.Throw(result.Exception);
                            }

                            if (result.Status != HealthStatus.Healthy)
                            {
                                throw new Exception("Health check failed");
                            }
                        };
                    }
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);

                    return;
                }
            }

            try
            {
                if (operation is not null)
                {
                    var pipeline = CreateResiliencyPipeline();

                    await pipeline.ExecuteAsync(operation);
                }

                tcs.TrySetResult();
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }

        private static ResiliencePipeline CreateResiliencyPipeline()
        {
            var retryUntilCancelled = new RetryStrategyOptions()
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                BackoffType = DelayBackoffType.Exponential,
                MaxRetryAttempts = 5,
                UseJitter = true,
                MaxDelay = TimeSpan.FromSeconds(30)
            };

            return new ResiliencePipelineBuilder().AddRetry(retryUntilCancelled).Build();
        }

        public ValueTask DisposeAsync()
        {
            _cts.Cancel();
            return default;
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Presentation.ServiceDefaults\Extensions.cs
// ----------------------------------------
namespace Microsoft.Extensions.Hosting;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        //builder.ConfigureOpenTelemetry();

        //builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(
            http =>
            {
                http.AddStandardResilienceHandler();
                http.AddServiceDiscovery();
            });

        return builder;
    }

    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        //builder.Logging.AddOpenTelemetry(logging =>
        //{
        //    logging.IncludeFormattedMessage = true;
        //    logging.IncludeScopes = true;
        //});

        //builder.Services.AddOpenTelemetry()
        //    .WithMetrics(metrics =>
        //    {
        //        metrics.AddAspNetCoreInstrumentation()
        //            .AddHttpClientInstrumentation()
        //            .AddRuntimeInstrumentation();
        //    })
        //    .WithTracing(tracing =>
        //    {
        //        tracing.AddAspNetCoreInstrumentation()
        //            // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
        //            //.AddGrpcClientInstrumentation()
        //            .AddHttpClientInstrumentation();
        //    });

        //builder.AddOpenTelemetryExporters();

        return builder;
    }

    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for app to be considered ready to accept traffic after starting
            app.MapHealthChecks("/health");

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            app.MapHealthChecks("/alive", new HealthCheckOptions { Predicate = r => r.Tags.Contains("live") });
        }

        return app;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        //var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        //if (useOtlpExporter)
        //{
        //    builder.Services.AddOpenTelemetry().UseOtlpExporter();
        //}

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        //{
        //    builder.Services.AddOpenTelemetry()
        //       .UseAzureMonitor();
        //}

        return builder;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Presentation.Web.Client\Program.cs
// ----------------------------------------
#pragma warning disable SA1200 // Using directives should be placed correctly
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using Polly;

#pragma warning restore SA1200 // Using directives should be placed correctly

var builder = WebAssemblyHostBuilder.CreateDefault(args);
var configuration = builder.Configuration.Build();

builder.Services.AddLocalization();
//builder.Services.AddScoped<IApiClient, ApiClient>();
builder.Services.AddHttpClient("backend-api")
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30)));
builder.Services.AddScoped(sp => HttpClientFactory(sp, configuration));

builder.Services.AddMudServices(
    config =>
    {
        config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
        config.SnackbarConfiguration.PreventDuplicates = false;
        config.SnackbarConfiguration.NewestOnTop = true;
        config.SnackbarConfiguration.ShowCloseIcon = true;
        config.SnackbarConfiguration.VisibleStateDuration = 10000;
        config.SnackbarConfiguration.HideTransitionDuration = 500;
        config.SnackbarConfiguration.ShowTransitionDuration = 500;
        config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
    });

await builder.Build().RunAsync();

static HttpClient HttpClientFactory(IServiceProvider serviceProvider, IConfiguration configuration)
{
    var httpClient = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("backend-api");
    httpClient.DefaultRequestHeaders.Accept.Clear();
    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    return httpClient;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Presentation.Web.Server\Program.cs
// ----------------------------------------
using BridgingIT.DevKit.Application.JobScheduling;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Presentation;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Infrastructure;
using BridgingIT.DevKit.Infrastructure.EntityFramework;
using OpenTelemetry.Exporter;
#pragma warning disable SA1200 // Using directives should be placed correctly
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Monitor.OpenTelemetry.Exporter;
using BridgingIT.DevKit.Application.Commands;
using BridgingIT.DevKit.Application.Messaging;
using BridgingIT.DevKit.Application.Queries;
using BridgingIT.DevKit.Application.Utilities;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Presentation;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Presentation;
using BridgingIT.DevKit.Examples.BookFiesta.Presentation.Web.Client.Pages;
using BridgingIT.DevKit.Examples.BookFiesta.Presentation.Web.Server;
using BridgingIT.DevKit.Examples.BookFiesta.Presentation.Web.Server.Components;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Application;
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
    .WithModule<InventoryModule>()
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

builder.Services.AddJobScheduling(o => o
            .StartupDelay(builder.Configuration["JobScheduling:StartupDelay"]),
        builder.Configuration)
    //.WithJob<HealthCheckJob>(CronExpressions.Every10Seconds)
    .WithBehavior<ModuleScopeJobSchedulingBehavior>()
    //.WithBehavior<ChaosExceptionJobSchedulingBehavior>()
    .WithBehavior<RetryJobSchedulingBehavior>()
    .WithBehavior<TimeoutJobSchedulingBehavior>();

builder.Services.AddStartupTasks(o => o
        .Enabled()
        .StartupDelay(builder.Configuration["StartupTasks:StartupDelay"]))
    .WithTask<EchoStartupTask>(o => o
        .Enabled(builder.Environment.IsDevelopment()).StartupDelay("00:00:03"))
    .WithTask<SwaggerGeneratorStartupTask>(o => o
        .Enabled(builder.Environment.IsDevelopment()))
    .WithTask<JobSchedulingSqlServerSeederStartupTask>(o => o // uses quartz configuration from appsettings JobScheduling:Quartz:quartz...
        .Enabled(builder.Environment.IsDevelopment()).StartupDelay("00:00:05"))
    //.WithTask(sp => new JobSchedulingSqlServerSeederStartupTask(sp.GetRequiredService<ILoggerFactory>(), builder.Configuration))
    .WithBehavior<ModuleScopeStartupTaskBehavior>()
    //.WithBehavior<ChaosExceptionStartupTaskBehavior>()
    .WithBehavior<RetryStartupTaskBehavior>()
    .WithBehavior<TimeoutStartupTaskBehavior>();

builder.Services
    .AddMessaging(builder.Configuration,
        o => o
            .StartupDelay(builder.Configuration["Messaging:StartupDelay"]))
    .WithBehavior<ModuleScopeMessagePublisherBehavior>()
    .WithBehavior<ModuleScopeMessageHandlerBehavior>()
    .WithBehavior<MetricsMessagePublisherBehavior>()
    .WithBehavior<MetricsMessageHandlerBehavior>()
    //.WithBehavior<ChaosExceptionMessageHandlerBehavior>()
    .WithBehavior<RetryMessageHandlerBehavior>()
    .WithBehavior<TimeoutMessageHandlerBehavior>()
    // .WithOutbox<OrganizationDbContext>(o => o // registers the outbox publisher behavior and worker service at once
    //     .ProcessingInterval("00:00:30")
    //     .StartupDelay("00:00:30")
    //     .ProcessingModeImmediate() // forwards the outbox message, through a queue, to the outbox worker
    //     .PurgeOnStartup())
    .WithInProcessBroker(); //.WithRabbitMQBroker();

ConfigureHealth(builder.Services);

builder.Services
    .AddMetrics(); // TOOL: dotnet-counters monitor -n BridgingIT.DevKit.Examples.BookFiesta.Presentation.Web.Server --counters bridgingit_devkit
builder.Services.Configure<ApiBehaviorOptions>(ConfiguraApiBehavior);
builder.Services.AddSingleton<IConfigurationRoot>(builder.Configuration);
builder.Services.AddScoped<ICurrentUserAccessor, FakeCurrentUserAccessor>();
builder.Services.AddProblemDetails(o =>
    Configure.ProblemDetails(o, true));
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
builder.Services.Configure<AspNetCoreOpenApiDocumentGeneratorSettings>(ConfigureOpenApiDocument);

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
builder.Services.AddOpenTelemetry()
    .WithMetrics(ConfigureMetrics)
    .WithTracing(ConfigureTracing);

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
        .SetErrorStatusOnException();
        // .AddAspNetCoreInstrumentation(
        //     o =>
        //     {
        //         o.RecordException = true;
        //         o.Filter = context => !context.Request.Path.ToString()
        //             .EqualsPatternAny(new RequestLoggingOptions().PathBlackListPatterns);
        //     })
        // .AddHttpClientInstrumentation(
        //     o =>
        //     {
        //         o.RecordException = true;
        //         o.FilterHttpRequestMessage = req
        //             => !req.RequestUri.PathAndQuery.EqualsPatternAny(
        //                 new RequestLoggingOptions().PathBlackListPatterns.Insert("*api/events/raw"));
        //     });
        // .AddSqlClientInstrumentation(
        //     o =>
        //     {
        //         o.Filter = cmd => // https://github.com/open-telemetry/opentelemetry-dotnet-contrib/blob/main/src/OpenTelemetry.Instrumentation.SqlClient/README.md#filter
        //         {
        //             if (cmd is SqlCommand command)
        //             {
        //                 return !command.CommandText.Contains("QRTZ_") &&
        //                     !command.CommandText.Contains("__MigrationsHistory") &&
        //                     !command.CommandText.Equals("SELECT 1;");
        //             }
        //
        //             return false;
        //         };
        //         o.RecordException = true;
        //         o.EnableConnectionLevelAttributes = true;
        //         o.SetDbStatementForText = true;
        //     });

    if (builder.Configuration["Tracing:Otlp:Enabled"].To<bool>())
    {
        Log.Logger.Information("{LogKey} otlp exporter enabled (endpoint={Endpoint})", "TRC", builder.Configuration["Tracing:Otlp:Endpoint"]);
        provider.AddOtlpExporter(o =>
        {
            o.Endpoint = new Uri(builder.Configuration["Tracing:Otlp:Endpoint"]);
            o.Protocol = OtlpExportProtocol.HttpProtobuf;
            o.Headers = builder.Configuration["Tracing:Otlp:Headers"];
        });
    }

    if (builder.Configuration["Tracing:Jaeger:Enabled"].To<bool>())
    {
        Log.Logger.Information("{LogKey} jaeger exporter enabled (host={JaegerHost})", "TRC", builder.Configuration["Tracing:Jaeger:AgentHost"]);
        provider.AddJaegerExporter(o =>
        {
            o.AgentHost = builder.Configuration["Tracing:Jaeger:AgentHost"];
            o.AgentPort = Convert.ToInt32(builder.Configuration["Tracing:Jaeger:AgentPort"]);
            o.ExportProcessorType = ExportProcessorType.Simple;
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

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Presentation.Web.Client\Resources\Global.Designer.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Presentation.Web.Client.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Global {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Global() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("BridgingIT.DevKit.Examples.BookFiesta.Presentation.Web.Client.Resources.Global", typeof(Global).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The quick brown fox jumps over the lazy dog.
        /// </summary>
        public static string Localization_Title {
            get {
                return ResourceManager.GetString("Localization_Title", resourceCulture);
            }
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Presentation.Web.Server\Hubs\NotificationHub.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Presentation.Web.Server;

using Microsoft.AspNetCore.SignalR;

public class NotificationHub : Hub { }

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Application\ITenantAware.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Application;

public interface ITenantAware
{
    string TenantId { get; }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\CatalogModuleConfiguration.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class CatalogModuleConfiguration
{
    public IReadOnlyDictionary<string, string> ConnectionStrings { get; set; }

    public string SeederTaskStartupDelay { get; set; } = "00:00:05";

    public class Validator : AbstractValidator<CatalogModuleConfiguration>
    {
        public Validator()
        {
            this.RuleFor(c => c.ConnectionStrings)
                .NotNull()
                .NotEmpty()
                .Must(c => c.ContainsKey("Default"))
                .WithMessage("Connection string with name 'Default' is required");

            this.RuleFor(c => c.SeederTaskStartupDelay)
                .NotNull()
                .NotEmpty()
                .WithMessage("SeederTaskStartupDelay cannot be null or empty");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\CatalogSeedEntities.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

public static class CatalogSeedEntities
{
    private static string GetSuffix(long ticks)
    {
        return ticks > 0 ? $"-{ticks}" : string.Empty;
    }

    private static string GetIsbn(long ticks, string isbn)
    {
        return ticks > 0 ? $"978-{new Random().NextInt64(1000000000000, 9999999999999)}" : isbn;
    }

    private static string GetSku(long ticks, string sku)
    {
        return ticks > 0 ? $"{new Random().NextInt64(10000000, 999999999999)}" : sku;
    }

#pragma warning disable SA1202
    public static (Tag[] Tags, Customer[] Customer, Author[] Authors, Publisher[] Publishers, Category[] Categories, Book[] Books) Create(TenantId[] tenantIds, long ticks = 0)
#pragma warning restore SA1202
    {
        var tags = Tags.Create(tenantIds, ticks);
        var customers = Customers.Create(tenantIds, ticks);
        var authors = Authors.Create(tenantIds, tags, ticks);
        var publishers = Publishers.Create(tenantIds, ticks);
        var categories = Categories.Create(tenantIds, ticks);
        var books = Books.Create(tenantIds, tags, categories, publishers, authors, ticks);

        return (tags, customers, authors, publishers, categories, books);
    }

    public static class Tags
    {
        public static Tag[] Create(TenantId[] tenants, long ticks = 0)
        {
            return
            [
                .. new[]
                {
                    Tag.Create(tenants[0], $"SoftwareArchitecture{GetSuffix(ticks)}", $"CatalogBook{GetSuffix(ticks)}"),
                    Tag.Create(tenants[0], $"DomainDrivenDesign{GetSuffix(ticks)}", $"CatalogBook{GetSuffix(ticks)}"),
                    Tag.Create(tenants[0], $"Microservices{GetSuffix(ticks)}", $"CatalogBook{GetSuffix(ticks)}"),
                    Tag.Create(tenants[0], $"CleanArchitecture{GetSuffix(ticks)}", $"CatalogBook{GetSuffix(ticks)}"),
                    Tag.Create(tenants[0], $"DesignPatterns{GetSuffix(ticks)}", $"CatalogBook{GetSuffix(ticks)}"),
                    Tag.Create(tenants[0], $"CloudArchitecture{GetSuffix(ticks)}", $"CatalogBook{GetSuffix(ticks)}"),
                    Tag.Create(
                        tenants[0],
                        $"EnterpriseArchitecture{GetSuffix(ticks)}",
                        $"CatalogBook{GetSuffix(ticks)}"),
                    Tag.Create(
                        tenants[0],
                        $"ArchitecturalPatterns{GetSuffix(ticks)}",
                        $"CatalogBook{GetSuffix(ticks)}"),
                    Tag.Create(tenants[0], $"SystemDesign{GetSuffix(ticks)}", $"CatalogBook{GetSuffix(ticks)}"),
                    Tag.Create(tenants[0], $"SoftwareDesign{GetSuffix(ticks)}", $"CatalogBook{GetSuffix(ticks)}"),
                    Tag.Create(tenants[0], $"Author{GetSuffix(ticks)}", $"CatalogAuthor{GetSuffix(ticks)}")
                }.ForEach(e => e.Id = TagId.Create($"{GuidGenerator.Create($"Tag_{e.Name}_{e.Category}")}"))
            ];
        }
    }

    public static class Customers
    {
        public static Customer[] Create(TenantId[] tenants, long ticks = 0)
        {
            return
            [
                .. new[]
                {
                    Customer.Create(
                        tenants[0],
                        PersonFormalName.Create(["John", "Doe"]),
                        EmailAddress.Create($"john.doe{GetSuffix(ticks)}@example.com"),
                        Address.Create("J. Doe", "Main Street", string.Empty, "17100", "Anytown", "USA")),
                    Customer.Create(
                        tenants[0],
                        PersonFormalName.Create(["Mary", "Jane"]),
                        EmailAddress.Create($"mary.jane{GetSuffix(ticks)}@example.com"),
                        Address.Create("M. Jane", "Maple Street", string.Empty, "17101", "Anytown", "USA"))
                }.ForEach(e => e.Id = CustomerId.Create($"{GuidGenerator.Create($"Customer_{e.Email}")}"))
            ];
        }
    }

    public static class Authors
    {
        public static Author[] Create(TenantId[] tenants, Tag[] tags, long ticks = 0)
        {
            return
            [
                .. new[]
                {
                    Author.Create(
                            tenants[0],
                            PersonFormalName.Create(["Martin", "Fowler"], string.Empty, string.Empty),
                            "Martin Fowler is a British software developer, author and international public speaker on software development, specializing in object-oriented analysis and design, UML, patterns, and agile software development methodologies, including extreme programming.")
                        .AddTag(tags[10]),
                    Author.Create(
                            tenants[0],
                            PersonFormalName.Create(["Robert", "C.", "Martin"], string.Empty, string.Empty),
                            "Robert C. Martin, colloquially called 'Uncle Bob', is an American software engineer, instructor, and best-selling author. He is most recognized for developing many software design principles and for being a founder of the influential Agile Manifesto.")
                        .AddTag(tags[10]),
                    Author.Create(
                            tenants[0],
                            PersonFormalName.Create(["Eric", "Evans"], string.Empty, string.Empty),
                            "Eric Evans is a thought leader in software design and domain modeling. He is the author of 'Domain-Driven Design: Tackling Complexity in the Heart of Software'.")
                        .AddTag(tags[10]),
                    Author.Create(
                            tenants[0],
                            PersonFormalName.Create(["Gregor", "Hohpe"], string.Empty, string.Empty),
                            "Gregor Hohpe is a software architect and author known for his work on enterprise integration patterns and cloud computing.")
                        .AddTag(tags[10]),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Sam", "Newman"], string.Empty, string.Empty),
                        "Sam Newman is a technologist and consultant specializing in cloud computing, continuous delivery, and microservices."),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Vaughn", "Vernon"], string.Empty, string.Empty),
                        "Vaughn Vernon is a software developer and architect with more than 35 years of experience in a broad range of business domains."),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Neal", "Ford"], string.Empty, string.Empty),
                        "Neal Ford is a software architect, programmer, and author. He is an internationally recognized expert on software development and delivery, especially in the intersection of agile engineering techniques and software architecture."),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Mark", "Richards"], string.Empty, string.Empty),
                        "Mark Richards is an experienced, hands-on software architect involved in the architecture, design, and implementation of microservices architectures, service-oriented architectures, and distributed systems."),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Dino", "Esposito"], string.Empty, string.Empty),
                        "Dino Esposito is a well-known web development expert and the author of many popular books on ASP.NET, AJAX, and JavaScript."),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Len", "Bass"], "Dr.", string.Empty),
                        "Len Bass is a senior principal researcher at National ICT Australia Ltd. He has authored numerous books and articles on software architecture, programming, and product line engineering."),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Erich", "Gamma"], "Dr.", string.Empty),
                        "Erich Gamma is a Swiss computer scientist and co-author of the influential software engineering book 'Design Patterns: Elements of Reusable Object-Oriented Software'."),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Richard", "Helm"], string.Empty, string.Empty),
                        "Richard Helm is a co-author of the 'Gang of Four' book on Design Patterns and has extensive experience in object-oriented technology and software architecture."),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Ralph", "Johnson"], "Dr.", string.Empty),
                        "Ralph Johnson is a Research Associate Professor in the Department of Computer Science at the University of Illinois at Urbana-Champaign and a co-author of the 'Gang of Four' Design Patterns book."),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["John", "Vlissides"], string.Empty, string.Empty),
                        "John Vlissides was a software consultant, designer, and implementer with expertise in object-oriented technology and a co-author of Design Patterns."),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Chris", "Richardson"], string.Empty, string.Empty),
                        "Chris Richardson is a developer and architect. He is a Java Champion, a JavaOne rock star and the author of POJOs in Action, which describes how to build enterprise Java applications with frameworks such as Spring and Hibernate."),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Grady", "Booch"], string.Empty, string.Empty),
                        "Grady Booch is an American software engineer, best known for developing the Unified Modeling Language with Ivar Jacobson and James Rumbaugh."),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Ivar", "Jacobson"], "Dr.", string.Empty),
                        "Ivar Jacobson is a Swedish computer scientist and software engineer, known as a major contributor to UML, Objectory, RUP, and Aspect-oriented software development."),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["James", "Rumbaugh"], "Dr.", string.Empty),
                        "James Rumbaugh is an American computer scientist and object-oriented methodologist who is best known for his work in creating the Object Modeling Technique and the Unified Modeling Language."),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Michael", "Feathers"], string.Empty, string.Empty),
                        "Michael Feathers is a consultant and author in the field of software development. He is a specialist in software testing and process improvement."),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Brendan", "Burns"], string.Empty, string.Empty),
                        string.Empty),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["George", "Fairbanks"], string.Empty, string.Empty),
                        string.Empty),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Rebecca", "Parsons"], string.Empty, string.Empty),
                        string.Empty),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Patrick", "Kua"], string.Empty, string.Empty),
                        string.Empty),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Pramod", "Sadalage"], string.Empty, string.Empty),
                        string.Empty),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Paul", "Clements"], string.Empty, string.Empty),
                        string.Empty),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Felix", "Bachmann"], string.Empty, string.Empty),
                        string.Empty),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["David", "Garlan"], string.Empty, string.Empty),
                        string.Empty),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["James", "Ivers"], string.Empty, string.Empty),
                        string.Empty),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Reed", "Little"], string.Empty, string.Empty),
                        string.Empty),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Paulo", "Merson"], string.Empty, string.Empty),
                        string.Empty),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Robert", "Nord"], string.Empty, string.Empty),
                        string.Empty),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Judith", "Stafford"], string.Empty, string.Empty),
                        string.Empty),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Michael", "T.", "Nygard"], string.Empty, string.Empty),
                        string.Empty),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Martin", "L.", "Abbot"], string.Empty, string.Empty),
                        string.Empty),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Michael", "T.", "Fisher"], string.Empty, string.Empty),
                        string.Empty),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Cornelia", "Davies"], string.Empty, string.Empty),
                        string.Empty),
                    Author.Create(
                        tenants[0],
                        PersonFormalName.Create(["Martin", "Kleppmann"], string.Empty, string.Empty),
                        string.Empty) // 36
                }.ForEach(
                    e => e.Id = AuthorId.Create(
                        $"{GuidGenerator.Create($"Author_{e.PersonName.Full}{GetSuffix(ticks)}")}"))
            ];
        }
    }

    public static class Publishers
    {
        public static Publisher[] Create(TenantId[] tenants, long ticks = 0)
        {
            return
            [
                .. new[]
                {
                    Publisher.Create(
                        tenants[0],
                        $"Addison-Wesley Professional{GetSuffix(ticks)}",
                        "Addison-Wesley Professional is a publisher of textbooks and computer literature. It is an imprint of Pearson PLC, a global publishing and education company."),
                    Publisher.Create(
                        tenants[0],
                        $"O'Reilly Media{GetSuffix(ticks)}",
                        "O'Reilly Media is an American learning company established by Tim O'Reilly that publishes books, produces tech conferences, and provides an online learning platform."),
                    Publisher.Create(
                        tenants[0],
                        $"Manning Publications{GetSuffix(ticks)}",
                        "Manning Publications is an American publisher established in 1993 that specializes in computer books for software developers, engineers, architects, system administrators, and managers."),
                    Publisher.Create(
                        tenants[0],
                        $"Packt Publishing{GetSuffix(ticks)}",
                        "Packt Publishing is a publisher of technology books, eBooks and video courses for IT developers, administrators, and users."),
                    Publisher.Create(tenants[0], $"Marschall & Brainerd{GetSuffix(ticks)}", string.Empty)
                }.ForEach(e => e.Id = PublisherId.Create($"{GuidGenerator.Create($"Publisher_{e.Name}")}"))
            ];
        }
    }

    public static class Categories
    {
        public static Category[] Create(TenantId[] tenants, long ticks = 0)
        {
            return
            [
                .. new[]
                {
                    Category.Create(tenants[0], $"Software-Architecture{GetSuffix(ticks)}", "Software Architecture")
                        .AddChild(Category.Create(tenants[0], $"Design-Patterns{GetSuffix(ticks)}", "Design Patterns"))
                        .AddChild(
                            Category.Create(
                                    tenants[0],
                                    $"Architectural-Styles{GetSuffix(ticks)}",
                                    "Architectural Styles",
                                    1)
                                .AddChild(
                                    Category.Create(
                                        tenants[0],
                                        $"Microservices{GetSuffix(ticks)}",
                                        "Microservices Architecture"))
                                .AddChild(
                                    Category.Create(
                                        tenants[0],
                                        $"SOA{GetSuffix(ticks)}",
                                        "Service-Oriented Architecture",
                                        1)))
                        .AddChild(
                            Category.Create(
                                tenants[0],
                                $"Domain-Driven-Design{GetSuffix(ticks)}",
                                "Domain-Driven Design",
                                2)),
                    Category.Create(
                            tenants[0],
                            $"Enterprise-Architecture{GetSuffix(ticks)}",
                            "Enterprise Architecture",
                            1)
                        .AddChild(
                            Category.Create(tenants[0], $"Cloud-Architecture{GetSuffix(ticks)}", "Cloud Architecture"))
                        .AddChild(
                            Category.Create(
                                tenants[0],
                                $"Integration-Patterns{GetSuffix(ticks)}",
                                "Integration Patterns",
                                1)),
                    Category.Create(tenants[0], $"Software-Design{GetSuffix(ticks)}", "Software Design", 2)
                        .AddChild(
                            Category.Create(tenants[0], $"Clean-Architecture{GetSuffix(ticks)}", "Clean Architecture"))
                        .AddChild(
                            Category.Create(tenants[0], $"SOLID-Principles{GetSuffix(ticks)}", "SOLID Principles", 1)),
                    Category.Create(
                            tenants[0],
                            $"Architectural-Practices{GetSuffix(ticks)}",
                            "Architectural Practices",
                            3)
                        .AddChild(Category.Create(tenants[0], $"Scalability{GetSuffix(ticks)}", "Scalability"))
                        .AddChild(Category.Create(tenants[0], $"Security{GetSuffix(ticks)}", "Security", 1))
                        .AddChild(Category.Create(tenants[0], $"Performance{GetSuffix(ticks)}", "Performance", 2))
                }.ForEach(e => e.Id = CategoryId.Create($"{GuidGenerator.Create($"Category_{e.Title}")}"))
            ];
        }
    }

    public static class Books
    {
        public static Book[] Create(
            TenantId[] tenants,
            Tag[] tags,
            Category[] categories,
            Publisher[] publishers,
            Author[] authors,
            long ticks = 0)
        {
            return
            [
                .. new[]
                {
                    Book.Create(
                            tenants[0],
                            $"Domain-Driven Design: Tackling Complexity in the Heart of Software{GetSuffix(ticks)}",
                            string.Empty,
                            "Eric Evans' book on how domain-driven design works in practice.",
                            ProductSku.Create(GetSku(ticks, "0321125217")),
                            BookIsbn.Create(GetIsbn(ticks, "978-0321125217")),
                            Money.Create(54.99m),
                            publishers[0],
                            new DateOnly(2003, 8, 30))
                        .AssignAuthor(authors[2]) // Eric Evans
                        .AddTag(tags[1])
                        .AddTag(tags[0])
                        .AddCategory(categories[0])
                        .AddCategory(categories[0].Children.ToArray()[2])
                        .AddRating(Rating.Excellent())
                        .AddRating(Rating.VeryGood())
                        .AddRating(Rating.VeryGood())
                        .AddChapter("Chapter 1: Putting the Domain Model to Work")
                        .AddChapter("Chapter 2: The Building Blocks of a Model-Driven Design")
                        .AddChapter("Chapter 3: Refactoring Toward Deeper Insight"),
                    Book.Create(
                            tenants[0],
                            $"Clean Architecture: A Craftsman's Guide to Software Structure and Design{GetSuffix(ticks)}",
                            string.Empty,
                            "Robert C. Martin's guide to building robust and maintainable software systems.",
                            ProductSku.Create(GetSku(ticks, "0134494166")),
                            BookIsbn.Create(GetIsbn(ticks, "978-0134494166")),
                            Money.Create(37.49m),
                            publishers[0],
                            new DateOnly(2017, 9, 10))
                        .AssignAuthor(authors[1]) // Robert C. Martin
                        .AddTag(tags[3])
                        .AddTag(tags[0])
                        .AddCategory(categories[2])
                        .AddCategory(categories[2].Children.ToArray()[0])
                        .AddRating(Rating.Poor())
                        .AddRating(Rating.Good())
                        .AddRating(Rating.VeryGood())
                        .AddChapter("Chapter 1: What Is Design and Architecture?")
                        .AddChapter("Chapter 2: A Tale of Two Values")
                        .AddChapter("Chapter 3: Paradigm Overview"),
                    Book.Create(
                            tenants[0],
                            $"Patterns of Enterprise Application Architecture{GetSuffix(ticks)}",
                            string.Empty,
                            "Martin Fowler's guide to enterprise application design patterns.",
                            ProductSku.Create(GetSku(ticks, "0321127426")),
                            BookIsbn.Create(GetIsbn(ticks, "978-0321127426")),
                            Money.Create(59.99m),
                            publishers[0],
                            new DateOnly(2002, 11, 15))
                        .AssignAuthor(authors[0]) // Martin Fowler
                        .AddTag(tags[4])
                        .AddTag(tags[6])
                        .AddCategory(categories[0])
                        .AddCategory(categories[0].Children.ToArray()[0])
                        .AddRating(Rating.VeryGood())
                        .AddRating(Rating.Poor())
                        .AddRating(Rating.Poor())
                        .AddChapter("Chapter 1: Layering")
                        .AddChapter("Chapter 2: Organizing Domain Logic")
                        .AddChapter("Chapter 3: Mapping to Relational Databases"),
                    Book.Create(
                            tenants[0],
                            $"Enterprise Integration Patterns{GetSuffix(ticks)}",
                            string.Empty,
                            "Gregor Hohpe's comprehensive catalog of messaging patterns.",
                            ProductSku.Create(GetSku(ticks, "0321200686")),
                            BookIsbn.Create(GetIsbn(ticks, "978-0321200686")),
                            Money.Create(69.99m),
                            publishers[0],
                            new DateOnly(2003, 10, 10))
                        .AssignAuthor(authors[3]) // Gregor Hohpe
                        .AddTag(tags[6])
                        .AddTag(tags[7])
                        .AddCategory(categories[1])
                        .AddCategory(categories[1].Children.ToArray()[1])
                        .AddRating(Rating.VeryGood())
                        .AddRating(Rating.Poor())
                        .AddRating(Rating.Poor())
                        .AddChapter("Chapter 1: Introduction to Messaging Systems")
                        .AddChapter("Chapter 2: Integration Styles")
                        .AddChapter("Chapter 3: Messaging Systems"),
                    Book.Create(
                            tenants[0],
                            $"Building Microservices{GetSuffix(ticks)}",
                            string.Empty,
                            "Sam Newman's guide to designing fine-grained systems.",
                            ProductSku.Create(GetSku(ticks, "1491950357")),
                            BookIsbn.Create(GetIsbn(ticks, "978-1491950357")),
                            Money.Create(44.99m),
                            publishers[1],
                            new DateOnly(2015, 2, 20))
                        .AssignAuthor(authors[4]) // Sam Newman
                        .AddTag(tags[2])
                        .AddTag(tags[0])
                        .AddCategory(categories[0])
                        .AddCategory(categories[0].Children.ToArray()[1].Children.ToArray()[0])
                        .AddRating(Rating.VeryGood())
                        .AddRating(Rating.Poor())
                        .AddRating(Rating.Poor())
                        .AddChapter("Chapter 1: Microservices")
                        .AddChapter("Chapter 2: The Evolutionary Architect")
                        .AddChapter("Chapter 3: How to Model Services"),
                    Book.Create(
                            tenants[0],
                            $"Implementing Domain-Driven Design{GetSuffix(ticks)}",
                            string.Empty,
                            "Vaughn Vernon's practical guide to DDD implementation.",
                            ProductSku.Create(GetSku(ticks, "0321834577")),
                            BookIsbn.Create(GetIsbn(ticks, "978-0321834577")),
                            Money.Create(59.99m),
                            publishers[0],
                            new DateOnly(2013, 2, 6))
                        .AssignAuthor(authors[5]) // Vaughn Vernon
                        .AddTag(tags[1])
                        .AddTag(tags[0])
                        .AddCategory(categories[0])
                        .AddCategory(categories[0].Children.ToArray()[2])
                        .AddRating(Rating.VeryGood())
                        .AddRating(Rating.Poor())
                        .AddRating(Rating.Poor())
                        .AddChapter("Chapter 1: Getting Started with DDD")
                        .AddChapter("Chapter 2: Domains, Subdomains, and Bounded Contexts")
                        .AddChapter("Chapter 3: Context Maps"),
                    Book.Create(
                            tenants[0],
                            $"Software Architecture in Practice{GetSuffix(ticks)}",
                            string.Empty,
                            "Len Bass's comprehensive examination of software architecture in practice.",
                            ProductSku.Create(GetSku(ticks, "0321815736")),
                            BookIsbn.Create(GetIsbn(ticks, "978-0321815736")),
                            Money.Create(64.99m),
                            publishers[0],
                            new DateOnly(2012, 9, 25))
                        .AssignAuthor(authors[9]) // Len Bass
                        .AddTag(tags[0])
                        .AddTag(tags[9])
                        .AddCategory(categories[0])
                        .AddRating(Rating.VeryGood())
                        .AddChapter("Chapter 1: What Is Software Architecture?")
                        .AddChapter("Chapter 2: Why Is Software Architecture Important?")
                        .AddChapter("Chapter 3: The Many Contexts of Software Architecture"),
                    Book.Create(
                            tenants[0],
                            $"Design Patterns: Elements of Reusable Object-Oriented Software{GetSuffix(ticks)}",
                            string.Empty,
                            "The classic 'Gang of Four' book on design patterns.",
                            ProductSku.Create(GetSku(ticks, "0201633610")),
                            BookIsbn.Create(GetIsbn(ticks, "978-0201633610")),
                            Money.Create(59.99m),
                            publishers[0],
                            new DateOnly(1994, 10, 31))
                        .AssignAuthor(authors[10]) // Erich Gamma
                        .AssignAuthor(authors[11]) // Richard Helm
                        .AssignAuthor(authors[12]) // Ralph Johnson
                        .AssignAuthor(authors[13]) // John Vlissides
                        .AddTag(tags[4])
                        .AddTag(tags[9])
                        .AddCategory(categories[0])
                        .AddCategory(categories[0].Children.ToArray()[0])
                        .AddRating(Rating.Poor())
                        .AddChapter("Chapter 1: Introduction")
                        .AddChapter("Chapter 2: A Case Study: Designing a Document Editor")
                        .AddChapter("Chapter 3: Creational Patterns"),
                    Book.Create(
                            tenants[0],
                            $"Microservices Patterns{GetSuffix(ticks)}",
                            string.Empty,
                            "Chris Richardson's guide to solving challenges in microservices architecture.",
                            ProductSku.Create(GetSku(ticks, "1617294549")),
                            BookIsbn.Create(GetIsbn(ticks, "978-1617294549")),
                            Money.Create(49.99m),
                            publishers[2],
                            new DateOnly(2018, 11, 19))
                        .AssignAuthor(authors[14]) // Chris Richardson
                        .AddTag(tags[2])
                        .AddTag(tags[7])
                        .AddCategory(categories[0])
                        .AddCategory(categories[0].Children.ToArray()[1].Children.ToArray()[0])
                        .AddRating(Rating.VeryGood())
                        .AddRating(Rating.Poor())
                        .AddChapter("Chapter 1: Escaping Monolithic Hell")
                        .AddChapter("Chapter 2: Decomposition Strategies")
                        .AddChapter("Chapter 3: Interprocess Communication in a Microservice Architecture"),
                    Book.Create(
                            tenants[0],
                            $"Object-Oriented Analysis and Design with Applications{GetSuffix(ticks)}",
                            string.Empty,
                            "Grady Booch's classic text on OOAD.",
                            ProductSku.Create(GetSku(ticks, "0201895513")),
                            BookIsbn.Create(GetIsbn(ticks, "978-0201895513")),
                            Money.Create(74.99m),
                            publishers[0],
                            new DateOnly(1994, 9, 30))
                        .AssignAuthor(authors[15]) // Grady Booch
                        .AddTag(tags[9])
                        .AddTag(tags[4])
                        .AddCategory(categories[2])
                        .AddRating(Rating.VeryGood())
                        .AddRating(Rating.Poor())
                        .AddRating(Rating.Poor())
                        .AddChapter("Chapter 1: Complexity")
                        .AddChapter("Chapter 2: The Object Model")
                        .AddChapter("Chapter 3: Classes and Objects"),
                    Book.Create(
                            tenants[0],
                            $"The Unified Modeling Language User Guide{GetSuffix(ticks)}",
                            string.Empty,
                            "Comprehensive guide to UML by its creators.",
                            ProductSku.Create(GetSku(ticks, "0321267974")),
                            BookIsbn.Create(GetIsbn(ticks, "978-0321267974")),
                            Money.Create(64.99m),
                            publishers[0],
                            new DateOnly(1998, 9, 29))
                        .AssignAuthor(authors[15]) // Grady Booch
                        .AssignAuthor(authors[16]) // Ivar Jacobson
                        .AssignAuthor(authors[17]) // James Rumbaugh
                        .AddTag(tags[9])
                        .AddTag(tags[0])
                        .AddCategory(categories[2])
                        .AddRating(Rating.VeryGood())
                        .AddRating(Rating.Poor())
                        .AddRating(Rating.Poor())
                        .AddChapter("Chapter 1: Getting Started")
                        .AddChapter("Chapter 2: Classes")
                        .AddChapter("Chapter 3: Relationships"),
                    Book.Create(
                            tenants[0],
                            $"Working Effectively with Legacy Code{GetSuffix(ticks)}",
                            string.Empty,
                            "Michael Feathers' strategies for dealing with large, untested legacy code bases.",
                            ProductSku.Create(GetSku(ticks, "0131177055")),
                            BookIsbn.Create(GetIsbn(ticks, "978-0131177055")),
                            Money.Create(54.99m),
                            publishers[0],
                            new DateOnly(2004, 9, 1))
                        .AssignAuthor(authors[18]) // Michael Feathers
                        .AddTag(tags[9])
                        .AddTag(tags[0])
                        .AddCategory(categories[2])
                        .AddCategory(categories[3])
                        .AddChapter("Chapter 1: Changing Software")
                        .AddChapter("Chapter 2: Working with Feedback")
                        .AddChapter("Chapter 3: Sensing and Separation"),
                    Book.Create(
                            tenants[0],
                            $"Fundamentals of Software Architecture{GetSuffix(ticks)}",
                            string.Empty,
                            "A comprehensive guide to software architecture fundamentals.",
                            ProductSku.Create(GetSku(ticks, "1492043454")),
                            BookIsbn.Create(GetIsbn(ticks, "978-1492043454")),
                            Money.Create(59.99m),
                            publishers[1],
                            new DateOnly(2020, 2, 1))
                        .AssignAuthor(authors[7]) // Mark Richards
                        .AssignAuthor(authors[6]) // Neal Ford
                        .AddTag(tags[0])
                        .AddTag(tags[9])
                        .AddCategory(categories[0])
                        .AddChapter("Chapter 1: Introduction")
                        .AddChapter("Chapter 2: Architectural Thinking")
                        .AddChapter("Chapter 3: Modularity"),
                    Book.Create(
                            tenants[0],
                            $"Designing Data-Intensive Applications{GetSuffix(ticks)}",
                            string.Empty,
                            "Martin Kleppmann's guide to the principles, practices, and patterns of modern data systems.",
                            ProductSku.Create(GetSku(ticks, "1449373320")),
                            BookIsbn.Create(GetIsbn(ticks, "978-1449373320")),
                            Money.Create(49.99m),
                            publishers[1],
                            new DateOnly(2017, 3, 16))
                        .AssignAuthor(authors[36]) // Martin Kleppmann
                        .AddTag(tags[8])
                        .AddTag(tags[0])
                        .AddCategory(categories[0])
                        .AddCategory(categories[3].Children.ToArray()[2])
                        .AddChapter("Chapter 1: Reliable, Scalable, and Maintainable Applications")
                        .AddChapter("Chapter 2: Data Models and Query Languages")
                        .AddChapter("Chapter 3: Storage and Retrieval"),
                    Book.Create(
                            tenants[0],
                            $"Cloud Native Patterns{GetSuffix(ticks)}",
                            string.Empty,
                            "Designing change-tolerant software for cloud platforms.",
                            ProductSku.Create(GetSku(ticks, "1617294296")),
                            BookIsbn.Create(GetIsbn(ticks, "978-1617294296")),
                            Money.Create(49.99m),
                            publishers[2],
                            new DateOnly(2019, 5, 13))
                        .AssignAuthor(authors[35]) // Cornelia Davis
                        .AddTag(tags[5])
                        .AddTag(tags[7])
                        .AddCategory(categories[1])
                        .AddCategory(categories[1].Children.ToArray()[0])
                        .AddChapter("Chapter 1: You Keep Using That Word")
                        .AddChapter("Chapter 2: Running Cloud-Native Applications in Production")
                        .AddChapter("Chapter 3: The Platform"),
                    Book.Create(
                            tenants[0],
                            $"Refactoring: Improving the Design of Existing Code{GetSuffix(ticks)}",
                            string.Empty,
                            "Martin Fowler's guide to refactoring and improving code design.",
                            ProductSku.Create(GetSku(ticks, "0134757599")),
                            BookIsbn.Create(GetIsbn(ticks, "978-0134757599")),
                            Money.Create(49.99m),
                            publishers[0],
                            new DateOnly(1999, 7, 8))
                        .AssignAuthor(authors[0]) // Martin Fowler
                        .AddTag(tags[9])
                        .AddTag(tags[0])
                        .AddCategory(categories[2])
                        .AddCategory(categories[3])
                        .AddChapter("Chapter 1: Refactoring: A First Example")
                        .AddChapter("Chapter 2: Principles in Refactoring")
                        .AddChapter("Chapter 3: Bad Smells in Code"),
                    Book.Create(
                            tenants[0],
                            $"Clean Code: A Handbook of Agile Software Craftsmanship{GetSuffix(ticks)}",
                            string.Empty,
                            "Robert C. Martin's guide to writing clean, maintainable code.",
                            ProductSku.Create(GetSku(ticks, "0132350884")),
                            BookIsbn.Create(GetIsbn(ticks, "978-0132350884")),
                            Money.Create(44.99m),
                            publishers[0],
                            new DateOnly(2008, 8, 1))
                        .AssignAuthor(authors[1]) // Robert C. Martin
                        .AddTag(tags[9])
                        .AddTag(tags[3])
                        .AddCategory(categories[2])
                        .AddCategory(categories[2].Children.ToArray()[1])
                        .AddChapter("Chapter 1: Clean Code")
                        .AddChapter("Chapter 2: Meaningful Names")
                        .AddChapter("Chapter 3: Functions"),
                    Book.Create(
                            tenants[0],
                            $"The Art of Scalability{GetSuffix(ticks)}",
                            string.Empty,
                            "Principles and practices for scaling web architectures.",
                            ProductSku.Create(GetSku(ticks, "0134032801")),
                            BookIsbn.Create(GetIsbn(ticks, "978-0134032801")),
                            Money.Create(59.99m),
                            publishers[0],
                            new DateOnly(2009, 12, 1))
                        .AssignAuthor(authors[33]) // Martin L. Abbott
                        .AssignAuthor(authors[34]) // Michael T. Fisher
                        .AddTag(tags[0])
                        .AddTag(tags[8])
                        .AddCategory(categories[0])
                        .AddCategory(categories[3].Children.ToArray()[0])
                        .AddChapter("Chapter 1: Scaling Concepts")
                        .AddChapter("Chapter 2: Principles of Scalability")
                        .AddChapter("Chapter 3: Processes for Scalable Architectures"),
                    Book.Create(
                            tenants[0],
                            $"Release It!: Design and Deploy Production-Ready Software{GetSuffix(ticks)}",
                            string.Empty,
                            "Michael T. Nygard's guide to designing and architecting applications for the real world.",
                            ProductSku.Create(GetSku(ticks, "1680502398")),
                            BookIsbn.Create(GetIsbn(ticks, "978-1680502398")),
                            Money.Create(47.99m),
                            publishers[2],
                            new DateOnly(2007, 3, 30))
                        .AssignAuthor(authors[31]) // Michael T. Nygard
                        .AddTag(tags[0])
                        .AddTag(tags[9])
                        .AddCategory(categories[0])
                        .AddCategory(categories[3])
                        .AddChapter("Chapter 1: Living in Production")
                        .AddChapter("Chapter 2: Case Study: The Exception That Grounded an Airline")
                        .AddChapter("Chapter 3: Stability Antipatterns"),
                    Book.Create(
                            tenants[0],
                            $"Documenting Software Architectures: Views and Beyond{GetSuffix(ticks)}",
                            string.Empty,
                            "A comprehensive guide to documenting software architectures.",
                            ProductSku.Create(GetSku(ticks, "0321552686")),
                            BookIsbn.Create(GetIsbn(ticks, "978-0321552686")),
                            Money.Create(69.99m),
                            publishers[0],
                            new DateOnly(2010, 10, 5))
                        .AssignAuthor(authors[24]) // Paul Clements
                        .AssignAuthor(authors[25]) // Felix Bachmann
                        .AssignAuthor(authors[9]) //  Len Bass,
                        .AssignAuthor(authors[26]) // David Garlan
                        .AssignAuthor(authors[27]) // James Ivers
                        .AssignAuthor(authors[28]) // Reed Little
                        .AssignAuthor(authors[29]) // Paulo Merson
                        .AssignAuthor(authors[30]) // Robert Nord
                        .AssignAuthor(authors[31]) // Judith Stafford
                        .AddTag(tags[0])
                        .AddTag(tags[9])
                        .AddCategory(categories[0])
                        .AddCategory(categories[3])
                        .AddChapter("Chapter 1: Introduction")
                        .AddChapter("Chapter 2: Software Architecture Documentation in Practice")
                        .AddChapter("Chapter 3: A System of Views"),
                    Book.Create(
                            tenants[0],
                            $"Building Evolutionary Architectures{GetSuffix(ticks)}",
                            string.Empty,
                            "Support Constant Change.",
                            ProductSku.Create(GetSku(ticks, "1491986360")),
                            BookIsbn.Create(GetIsbn(ticks, "978-1491986360")),
                            Money.Create(39.99m),
                            publishers[1],
                            new DateOnly(2017, 10, 5))
                        .AssignAuthor(authors[6]) //  Neal Ford
                        .AssignAuthor(authors[21]) // Rebecca Parsons
                        .AssignAuthor(authors[22]) // Patrick Kua
                        .AssignAuthor(authors[23]) // Pramod Sadalage
                        .AddTag(tags[0])
                        .AddTag(tags[9])
                        .AddCategory(categories[0])
                        .AddCategory(categories[3])
                        .AddChapter("Chapter 1: Software Architecture")
                        .AddChapter("Chapter 2: Evolutionary Architecture")
                        .AddChapter("Chapter 3: Engineering Incremental Change"),
                    Book.Create(
                            tenants[0],
                            $"Just Enough Software Architecture{GetSuffix(ticks)}",
                            string.Empty,
                            "A Risk-Driven Approach.",
                            ProductSku.Create(GetSku(ticks, "0984618101")),
                            BookIsbn.Create(GetIsbn(ticks, "978-0984618101")),
                            Money.Create(59.99m),
                            publishers[4],
                            new DateOnly(2010, 8, 1))
                        .AssignAuthor(authors[20]) // George Fairbanks
                        .AddTag(tags[0])
                        .AddTag(tags[9])
                        .AddCategory(categories[0])
                        .AddChapter("Chapter 1: Introduction")
                        .AddChapter("Chapter 2: Risk-Driven Model")
                        .AddChapter("Chapter 3: Engineering and Evaluating Software Architectures"),
                    Book.Create(
                            tenants[0],
                            $"Software Systems Architecture{GetSuffix(ticks)}",
                            "Working with Stakeholders Using Viewpoints and Perspectives.",
                            string.Empty,
                            ProductSku.Create(GetSku(ticks, "0321112293")),
                            BookIsbn.Create(GetIsbn(ticks, "978-0321112293")),
                            Money.Create(64.99m),
                            publishers[0],
                            new DateOnly(2005, 4, 1))
                        .AssignAuthor(authors[9]) // Len Bass
                        .AddTag(tags[0])
                        .AddTag(tags[9])
                        .AddCategory(categories[0])
                        .AddChapter("Chapter 1: Introduction")
                        .AddChapter("Chapter 2: Software Architecture Concepts")
                        .AddChapter("Chapter 3: Viewpoints and Views"),
                    Book.Create(
                            tenants[0],
                            $"Domain-Driven Design Distilled{GetSuffix(ticks)}",
                            string.Empty,
                            "Vaughn Vernon's concise guide to the fundamentals of DDD.",
                            ProductSku.Create(GetSku(ticks, "0134434421")),
                            BookIsbn.Create(GetIsbn(ticks, "978-0134434421")),
                            Money.Create(29.99m),
                            publishers[0],
                            new DateOnly(2016, 6, 1))
                        .AssignAuthor(authors[5]) // Vaughn Vernon
                        .AddTag(tags[1])
                        .AddTag(tags[0])
                        .AddCategory(categories[0])
                        .AddCategory(categories[0].Children.ToArray()[2])
                        .AddChapter("Chapter 1: DDD for Me")
                        .AddChapter("Chapter 2: Strategic Design with Bounded Contexts and the Ubiquitous Language")
                        .AddChapter("Chapter 3: Strategic Design with Subdomains"),
                    Book.Create(
                            tenants[0],
                            $"Designing Distributed Systems{GetSuffix(ticks)}",
                            string.Empty,
                            "Patterns and Paradigms for Scalable, Reliable Services.",
                            ProductSku.Create(GetSku(ticks, "1491983645")),
                            BookIsbn.Create(GetIsbn(ticks, "978-1491983645")),
                            Money.Create(39.99m),
                            publishers[1],
                            new DateOnly(2018, 2, 20))
                        .AssignAuthor(authors[19]) // Brendan Burns
                        .AddTag(tags[0])
                        .AddTag(tags[8])
                        .AddCategory(categories[0])
                        .AddCategory(categories[1])
                        .AddChapter("Chapter 1: Introduction")
                        .AddChapter("Chapter 2: Single-Node Patterns")
                        .AddChapter("Chapter 3: Serving Patterns")
                }.ForEach(e => e.Id = BookId.Create($"{GuidGenerator.Create($"Book_{e.Title}")}"))
            ];
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Presentation\CatalogMapperRegister.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Presentation;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using Mapster;

public class CatalogMapperRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Author aggregate mappings
        config.ForType<Author, AuthorModel>();

        config.ForType<AuthorBook, AuthorBookModel>()
            .Map(d => d.Id, s => s.BookId.Value);

        // Book aggregate mappings
        config.ForType<Book, BookModel>()
            .Map(d => d.Sku, s => s.Sku.Value)
            .Map(d => d.Keywords, s => s.Keywords.Select(e => e.Text).ToList())
            .Map(d => d.Isbn, s => s.Isbn.Value);

        config.ForType<BookAuthor, BookAuthorModel>()
            .Map(d => d.Id, s => s.AuthorId.Value);

        config.ForType<BookPublisher, BookPublisherModel>()
            .Map(d => d.Id, s => s.PublisherId.Value);

        // Customer aggregate mappings
        //config.ForType<Category, CategoryModel>()
        //    .Map(d => d.ParentId, s => s.Parent.Id.Value.ToString(), e => e.Parent != null);

        //config.ForType<CustomerModel, CustomerCreateCommand>()
        //    .Map(d => d.AddressName, s => s.Address.Name)
        //    .Map(d => d.AddressLine1, s => s.Address.Line1)
        //    .Map(d => d.AddressLine2, s => s.Address.Line2)
        //    .Map(d => d.AddressPostalCode, s => s.Address.PostalCode)
        //    .Map(d => d.AddressCity, s => s.Address.City)
        //    .Map(d => d.AddressCountry, s => s.Address.Country);

        //config.ForType<CustomerModel, CustomerUpdateCommand>()
        //    .Map(d => d.AddressName, s => s.Address.Name)
        //    .Map(d => d.AddressLine1, s => s.Address.Line1)
        //    .Map(d => d.AddressLine2, s => s.Address.Line2)
        //    .Map(d => d.AddressPostalCode, s => s.Address.PostalCode)
        //    .Map(d => d.AddressCity, s => s.Address.City)
        //    .Map(d => d.AddressCountry, s => s.Address.Country);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Presentation\CatalogModule.cs
// ----------------------------------------
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

        // services.AddJobScheduling()
        //     .WithJob<EchoJob>(CronExpressions.Every5Minutes);
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

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Presentation\CatalogSignalRHub.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Presentation;

using Microsoft.AspNetCore.SignalR;

public class CatalogSignalRHub : Hub
{
    public async Task OnCheckHealth()
    {
        await this.Clients.All.SendAsync("CheckHealth");
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application\InventoryModuleClient.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

using MediatR;

/// <summary>
///     Specifies the public API for this module that will be exposed to other modules
/// </summary>
public class InventoryModuleClient(IMediator mediator, IMapper mapper)
    : IInventoryModuleClient
{
    /// <summary>
    ///     Retrieves the details of a stock based on the ID.
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="id">The unique identifier of the tenant stock.</param>
    /// <returns>
    ///     A task representing the asynchronous operation. The task's result contains a <see cref="Result{T}" /> with the
    ///     stock details when successful; otherwise, an error result.
    /// </returns>
    public async Task<Result<StockModel>> StockFindOne(string tenantId, string id)
    {
        var result = (await mediator.Send(
            new StockFindOneQuery(tenantId, id))).Result;

        return result.IsSuccess
            ? Result<StockModel>.Success(mapper.Map<Stock, StockModel>(result.Value), result.Messages)
            : Result<StockModel>.Failure(result.Messages, result.Errors);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application\InventoryModuleConfiguration.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class InventoryModuleConfiguration
{
    public IReadOnlyDictionary<string, string> ConnectionStrings { get; set; }

    public string SeederTaskStartupDelay { get; set; } = "00:00:05";

    public class Validator : AbstractValidator<InventoryModuleConfiguration>
    {
        public Validator()
        {
            this.RuleFor(c => c.ConnectionStrings)
                .NotNull()
                .NotEmpty()
                .Must(c => c.ContainsKey("Default"))
                .WithMessage("Connection string with name 'Default' is required");

            this.RuleFor(c => c.SeederTaskStartupDelay)
                .NotNull()
                .NotEmpty()
                .WithMessage("SeederTaskStartupDelay cannot be null or empty");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application.Contracts\IInventoryModuleClient.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

using BridgingIT.DevKit.Common;
// using Refit

/// <summary>
///     Specifies the public API for this module that will be exposed to other modules
/// </summary>
public interface IInventoryModuleClient
{
    /// <summary>
    ///     Retrieves the details of a stock based on the ID.
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="id">The unique identifier of the tenant stock.</param>
    /// <returns>
    ///     A task representing the asynchronous operation. The task's result contains a <see cref="Result{T}" /> with the
    ///     stock details when successful; otherwise, an error result.
    /// </returns>
    // INFO incase the Inventory module is a seperate webservice use refit -> [Get("api/tenants/{tenantId}/inventory/stocks/{id}")]
    public Task<Result<StockModel>> StockFindOne(string tenantId, string id);
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Domain\InventorySeedEntities.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

public static class InventorySeedEntities
{
    private static string GetSuffix(long ticks)
    {
        return ticks > 0 ? $"-{ticks}" : string.Empty;
    }

    private static string GetSku(long ticks, string sku)
    {
        return ticks > 0 ? new Random().NextInt64(10000000, 999999999999).ToString() : sku;
    }

#pragma warning disable SA1202
    public static (Stock[] Stocks, StockSnapshot[] StockSnapshots) Create(TenantId[] tenants, long ticks = 0)
#pragma warning restore SA1202
    {
        return (Stocks.Create(tenants, ticks),
            StockSnapshots.Create(tenants, Stocks.Create(tenants, ticks), ticks));
    }

    public static class Stocks
    {
        public static Stock[] Create(TenantId[] tenants, long ticks = 0)
        {
            var random = new Random(42); // Seed for reproducibility

            return
            [
                .. new[]
                {
                    Stock.Create(
                        tenants[0],
                        ProductSku.Create("0321125217"),
                        100,
                        20,
                        50,
                        Money.Create(30.00m),
                        StorageLocation.Create("A", "1", "1")),
                    Stock.Create(
                        tenants[0],
                        ProductSku.Create("0134494166"),
                        75,
                        15,
                        40,
                        Money.Create(25.00m),
                        StorageLocation.Create("A", "1", "2")),
                    Stock.Create(
                        tenants[0],
                        ProductSku.Create("0321127426"),
                        50,
                        10,
                        30,
                        Money.Create(35.00m),
                        StorageLocation.Create("A", "2", "1")),
                    Stock.Create(
                        tenants[0],
                        ProductSku.Create("0321200686"),
                        120,
                        25,
                        60,
                        Money.Create(40.00m),
                        StorageLocation.Create("A", "2", "2")),
                    Stock.Create(
                        tenants[0],
                        ProductSku.Create("1491950357"),
                        80,
                        20,
                        50,
                        Money.Create(28.00m),
                        StorageLocation.Create("B", "1", "1")),
                    Stock.Create(
                        tenants[0],
                        ProductSku.Create("0321834577"),
                        60,
                        15,
                        40,
                        Money.Create(32.00m),
                        StorageLocation.Create("B", "1", "2")),
                    Stock.Create(
                        tenants[0],
                        ProductSku.Create("0321815736"),
                        90,
                        20,
                        50,
                        Money.Create(38.00m),
                        StorageLocation.Create("B", "2", "1")),
                    Stock.Create(
                        tenants[0],
                        ProductSku.Create("0201633610"),
                        70,
                        15,
                        40,
                        Money.Create(33.00m),
                        StorageLocation.Create("B", "2", "2")),
                    Stock.Create(
                        tenants[0],
                        ProductSku.Create("1617294549"),
                        110,
                        25,
                        60,
                        Money.Create(29.00m),
                        StorageLocation.Create("C", "1", "1")),
                    Stock.Create(
                        tenants[0],
                        ProductSku.Create("0201895513"),
                        40,
                        10,
                        30,
                        Money.Create(42.00m),
                        StorageLocation.Create("C", "1", "2"))
                }.Select(stock =>
                {
                    stock.Id = StockId.Create($"{GuidGenerator.Create($"Stock_{stock.Sku.Value}{GetSuffix(ticks)}")}");

                    // Add some random stock movements and adjustments
                    for (var i = 0; i < 5; i++)
                    {
                        var quantity = random.Next(1, 21);
                        var type = random.Next(2) == 0 ? StockMovementType.Addition : StockMovementType.Removal;
                        stock.AddStock(quantity);
                        if (type == StockMovementType.Removal)
                        {
                            stock.RemoveStock(quantity);
                        }

                        var adjustment = random.Next(-10, 11); // Randomly adjust quantity
                        if (adjustment != 0)
                        {
                            stock.AdjustQuantity(adjustment, $"Random quantity adjustment {i + 1}");
                        }

                        if (random.Next(2) != 0) // Randomly adjust unit cost
                        {
                            continue;
                        }

                        // BUG: AdjustUnitCost causes InvalidException when inserted (seed)
                        // System.InvalidCastException
                        //     Unable to cast object of type 'BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain.StockAdjustmentId' to type 'BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain.StockId'.
                        // var costChange = (decimal)((random.NextDouble() * 10) - 5); // Random change between -5 and 5
                        // var newUnitCost = Money.Create(Math.Max(0.01m, stock.UnitCost.Amount + costChange));
                        // stock.AdjustUnitCost(newUnitCost, $"Random unitcost adjustment {i + 1}");
                        // stock.AdjustUnitCost(Money.Zero(), $"Random unitcost adjustment {i + 1}");
                    }

                    return stock;
                })
            ];
        }
    }

    public static class StockSnapshots
    {
        public static StockSnapshot[] Create(TenantId[] tenants, Stock[] stocks, long ticks = 0)
        {
            return
            [
                .. stocks.Select(stock =>
                {
                    var snapshot = StockSnapshot.Create(
                        tenants[0],
                        stock.Id,
                        stock.Sku,
                        stock.QuantityOnHand,
                        stock.QuantityReserved,
                        stock.UnitCost,
                        stock.Location,
                        DateTimeOffset.Parse("2024-01-01T00:00:00Z"));

                    snapshot.Id = StockSnapshotId.Create(
                        $"{GuidGenerator.Create($"StockSnapshot_{stock.Sku.Value}_{snapshot.Timestamp.Ticks}{GetSuffix(ticks)}")}");

                    return snapshot;
                })
            ];
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Presentation\InventoryMapperRegister.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Presentation;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using Mapster;

public class InventoryMapperRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        RegisterStock(config);
    }

    private static void RegisterStock(TypeAdapterConfig config)
    {
        config.ForType<Stock, StockModel>()
            .IgnoreNullValues(true)
            .Map(d => d.Sku, s => s.Sku.Value)
            .Ignore(dest => dest.Adjustments)
            .Ignore(dest => dest.Movements); // TODO: does not work at the moment;

        config.ForType<StockModel, Stock>()
            .IgnoreNullValues(true)
            .ConstructUsing(
                src => Stock.Create(
                    TenantId.Create(src.TenantId),
                    ProductSku.Create(src.Sku),
                    src.QuantityOnHand,
                    src.ReorderThreshold,
                    src.ReorderQuantity,
                    Money.Create(src.UnitCost),
                    src.Location))
            .AfterMapping(
                (src, dest) =>
                {
#pragma warning disable SA1501
                    if (dest.Id != null) { }
#pragma warning restore SA1501
                    else
                    {
                        if (src.QuantityReserved > 0)
                        {
                            dest.ReserveStock(src.QuantityReserved);
                        }
                    }
                });
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Presentation\InventoryModule.cs
// ----------------------------------------
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

        // services.AddJobScheduling()
        //     .WithJob<EchoJob>(CronExpressions.Every5Minutes);
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
                .DeleteOnStartup(false));
            // .WithOutboxDomainEventService(o => o
            //     .ProcessingInterval("00:00:30")
            //     .StartupDelay("00:00:30")
            //     .PurgeOnStartup()
            //     .ProcessingModeImmediate());

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

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\OrganizationModuleClient.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

using MediatR;

public class OrganizationModuleClient(IMediator mediator, IMapper mapper)
    : IOrganizationModuleClient
{
    public async Task<Result<TenantModel>> TenantFindOne(string id)
    {
        var result = (await mediator.Send(new TenantFindOneQuery(id))).Result;
        var m = mapper.Map<Tenant, TenantModel>(result.Value);

        return result.For<Tenant, TenantModel>(mapper);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\OrganizationModuleConfiguration.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class OrganizationModuleConfiguration
{
    public IReadOnlyDictionary<string, string> ConnectionStrings { get; set; }

    public string SeederTaskStartupDelay { get; set; } = "00:00:05";

    public class Validator : AbstractValidator<OrganizationModuleConfiguration>
    {
        public Validator()
        {
            this.RuleFor(c => c.ConnectionStrings)
                .NotNull()
                .NotEmpty()
                .Must(c => c.ContainsKey("Default"))
                .WithMessage("Connection string with name 'Default' is required");

            this.RuleFor(c => c.SeederTaskStartupDelay)
                .NotNull()
                .NotEmpty()
                .WithMessage("SeederTaskStartupDelay cannot be null or empty");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application.Contracts\IOrganizationModuleClient.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

using BridgingIT.DevKit.Common;

/// <summary>
///     Specifies the public API for this module that will be exposed to other modules
/// </summary>
public interface IOrganizationModuleClient
{
    /// <summary>
    ///     Retrieves the details of a tenant based on the tenant ID.
    /// </summary>
    /// <param name="id">The unique identifier of the tenant.</param>
    /// <returns>
    ///     A task representing the asynchronous operation. The task's result contains a <see cref="Result{T}" /> with the
    ///     tenant details when successful; otherwise, an error result.
    /// </returns>
    // INFO incase the Organization module is a seperate webservice use refit -> [Get("api/organization/tenants/{id}")]
    public Task<Result<TenantModel>> TenantFindOne(string id);
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Domain\OrganizationSeedEntities.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

public static class OrganizationSeedEntities
{
    private static string GetSuffix(long ticks)
    {
        return ticks > 0 ? $"-{ticks}" : string.Empty;
    }

#pragma warning disable SA1202
    public static (Company[] Companies, Tenant[] Tenants) Create(long ticks = 0)
#pragma warning restore SA1202
    {
        return (
            Companies.Create(ticks),
            Tenants.Create(Companies.Create(ticks), ticks));
    }

    public static class Companies
    {
        public static Company[] Create(long ticks = 0)
        {
            return
            [
                .. new[]
                {
                    Company.Create(
                            $"Acme Corporation{GetSuffix(ticks)}",
                            "AC123456",
                            EmailAddress.Create($"contact{GetSuffix(ticks)}@acme.com"),
                            Address.Create(
                                $"Acme Corporation{GetSuffix(ticks)}",
                                "123 Business Ave",
                                "Suite 100",
                                "90210",
                                "Los Angeles",
                                "USA"))
                        .SetContactPhone(PhoneNumber.Create("+1234567890"))
                        .SetWebsite(Url.Create("https://www.acme.com"))
                        .SetVatNumber(VatNumber.Create("US12-3456789")),
                    Company.Create(
                            $"TechInnovate GmbH{GetSuffix(ticks)}",
                            "HRB987654",
                            EmailAddress.Create($"info{GetSuffix(ticks)}@techinnovate.de"),
                            Address.Create(
                                $"TechInnovate GmbH{GetSuffix(ticks)}",
                                "Innovationsstraße 42",
                                string.Empty,
                                "10115",
                                "Berlin",
                                "Germany"))
                        .SetContactPhone(PhoneNumber.Create("+49301234567"))
                        .SetWebsite(Url.Create("https://www.techinnovate.de"))
                        .SetVatNumber(VatNumber.Create("DE123456789")),
                    Company.Create(
                            $"Global Trade Ltd{GetSuffix(ticks)}",
                            "GTL789012",
                            EmailAddress.Create($"enquiries{GetSuffix(ticks)}@globaltrade.co.uk"),
                            Address.Create(
                                $"Global Trade Ltd{GetSuffix(ticks)}",
                                "1 Commerce Street",
                                "Floor 15",
                                "EC1A 1BB",
                                "London",
                                "United Kingdom"))
                        .SetContactPhone(PhoneNumber.Create("+442071234567"))
                        .SetWebsite(Url.Create("https://www.globaltrade.co.uk"))
                        .SetVatNumber(VatNumber.Create("GB123456789"))
                }.ForEach(e => e.Id = CompanyId.Create($"{GuidGenerator.Create($"Company_{e.Name}")}"))
            ];
        }
    }

    public static class Tenants
    {
        public static Tenant[] Create(Company[] companies, long ticks = 0)
        {
            return
            [
                .. new[]
                {
                    Tenant.Create(companies[0].Id, $"AcmeBooks{GetSuffix(ticks)}", $"books@acme{GetSuffix(ticks)}.com")
                        .AddSubscription()
                        .SetSchedule(
                            DateSchedule.Create(
                                DateOnly.FromDateTime(new DateTime(2020, 1, 1)),
                                DateOnly.FromDateTime(new DateTime(2022, 12, 31))))
                        .SetPlanType(TenantSubscriptionPlanType.Free)
                        .Tenant.AddSubscription()
                        .SetSchedule(DateSchedule.Create(DateOnly.FromDateTime(new DateTime(2023, 1, 1))))
                        .SetPlanType(TenantSubscriptionPlanType.Basic)
                        .SetBillingCycle(TenantSubscriptionBillingCycle.Yearly)
                        .Tenant.SetBranding(TenantBranding.Create("#000000", "#AAAAAA")),
                    Tenant.Create(
                            companies[0].Id,
                            $"TechBooks{GetSuffix(ticks)}",
                            $"books@techinnovate{GetSuffix(ticks)}.de")
                        .AddSubscription()
                        .SetSchedule(DateSchedule.Create(DateOnly.FromDateTime(new DateTime(2020, 1, 1))))
                        .SetPlanType(TenantSubscriptionPlanType.Premium)
                        .SetBillingCycle(TenantSubscriptionBillingCycle.Yearly)
                        .Tenant.SetBranding(TenantBranding.Create("#000000", "#AAAAAA"))
                }.ForEach(e => e.Id = TenantIdFactory.CreateForName($"Tenant_{e.Name}"))
            ];
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Presentation\OrganizationMapperRegister.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Presentation;

using BridgingIT.DevKit.Domain.Model;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Application;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using Mapster;

public class OrganizationMapperRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        RegisterCompany(config);
        RegisterTenant(config);
    }

    private static void RegisterCompany(TypeAdapterConfig config)
    {
        config.ForType<CompanyModel, Company>()
            .IgnoreNullValues(true)
            .ConstructUsing(
                src => Company.Create(
                    src.Name,
                    src.RegistrationNumber,
                    EmailAddress.Create(src.ContactEmail),
                    MapAddress(src.Address)))
            .AfterMapping(
                (src, dest) =>
                {
                    if (dest.Id != null)
                    {
                        dest.SetName(src.Name);
                        dest.SetRegistrationNumber(src.RegistrationNumber);
                        dest.SetContactEmail(EmailAddress.Create(src.ContactEmail));
                        dest.SetAddress(MapAddress(src.Address));
                    }

                    dest.SetContactPhone(PhoneNumber.Create(src.ContactPhone));
                    dest.SetWebsite(Url.Create(src.Website));
                    dest.SetVatNumber(VatNumber.Create(src.VatNumber));
                });

        config.ForType<AddressModel, Address>() // TODO: move to new SharedKernelMapperRegister
            .MapWith(src => MapAddress(src));
    }

    private static void RegisterTenant(TypeAdapterConfig config)
    {
        config.ForType<Tenant, TenantModel>()
            .IgnoreNullValues(true)
            .Ignore(dest => dest.Subscriptions) // TODO: does not work at the moment
            .Map(dest => dest.IsActive, src => src.IsActive());

        config.ForType<TenantBrandingModel, TenantBrandingModel>()
            .IgnoreNullValues(true);

        config.ForType<TenantModel, Tenant>()
            .IgnoreNullValues(true)
            .ConstructUsing(src => Tenant.Create(src.CompanyId, src.Name, EmailAddress.Create(src.ContactEmail)))
            .AfterMapping(
                (src, dest) =>
                {
                    if (dest.Id != null)
                    {
                        dest.SetCompany(src.CompanyId);
                        dest.SetName(src.Name);
                        dest.SetContactEmail(EmailAddress.Create(src.ContactEmail));
                    }

                    dest.SetDescription(src.Description);
                    MapSubscriptions(src.Subscriptions, dest);
                    MapBranding(src.Branding, dest);
                });

        config.ForType<(TenantSubscriptionModel subscriptionModel, Tenant tenant), TenantSubscription>()
            .IgnoreNullValues(true)
            .ConstructUsing(
                src => TenantSubscription.Create(
                    src.tenant,
                    Enumeration.FromId<TenantSubscriptionPlanType>(src.subscriptionModel.PlanType),
                    DateSchedule.Create(
                        src.subscriptionModel.Schedule.StartDate,
                        src.subscriptionModel.Schedule.EndDate)))
            .AfterMapping(
                (src, dest) =>
                {
                    if (dest.Id != null)
                    {
                        dest.SetPlanType(
                            Enumeration.FromId<TenantSubscriptionPlanType>(src.subscriptionModel.PlanType));
                        dest.SetSchedule(
                            DateSchedule.Create(
                                src.subscriptionModel.Schedule.StartDate,
                                src.subscriptionModel.Schedule.EndDate));
                    }

                    dest.SetStatus(Enumeration.FromId<TenantSubscriptionStatus>(src.subscriptionModel.Status));
                    dest.SetBillingCycle(
                        Enumeration.FromId<TenantSubscriptionBillingCycle>(src.subscriptionModel.BillingCycle));
                });

        config.ForType<TenantBrandingModel, TenantBranding>()
            .IgnoreNullValues(true)
            .ConstructUsing(
                src => TenantBranding.Create(
                    HexColor.Create(src.PrimaryColor),
                    HexColor.Create(src.SecondaryColor),
                    Url.Create(src.LogoUrl),
                    Url.Create(src.FaviconUrl)))
            .AfterMapping(
                (src, dest) =>
                {
                    if (dest.Id != null)
                    {
                        dest.SetPrimaryColor(HexColor.Create(src.PrimaryColor));
                        dest.SetSecondaryColor(HexColor.Create(src.SecondaryColor));
                        dest.SetLogoUrl(Url.Create(src.LogoUrl));
                        dest.SetFaviconUrl(Url.Create(src.FaviconUrl));
                    }

                    dest.SetCustomCss(src.CustomCss);
                });
    }

    private static Address MapAddress(AddressModel source)
    {
        return source == null
            ? null
            : Address.Create(source.Name, source.Line1, source.Line2, source.PostalCode, source.City, source.Country);
    }

    private static void MapSubscriptions(TenantSubscriptionModel[] sources, Tenant destination)
    {
        var existingSubscriptions = destination.Subscriptions.ToList();
        var newSubscriptionModels = sources ?? [];

        foreach (var existingSubscription in existingSubscriptions)
        {
            if (!newSubscriptionModels.Any(s => s.Id == existingSubscription.Id.Value.ToString()))
            {
                destination.RemoveSubscription(existingSubscription);
            }
        }

        foreach (var subscriptionModel in newSubscriptionModels)
        {
            var existingSubscription = existingSubscriptions.Find(s => s.Id.Value.ToString() == subscriptionModel.Id);
            if (existingSubscription == null)
            {
                var newSubscription =
                    (subscriptionModel, destination).Adapt<TenantSubscription>(); // use destination too (tenant)
                destination.AddSubscription(newSubscription);
            }
            else
            {
                subscriptionModel.Adapt(existingSubscription);
            }
        }
    }

    private static void MapBranding(TenantBrandingModel source, Tenant destination)
    {
        if (source == null)
        {
            destination.SetBranding(null);

            return;
        }

        var branding = destination.Branding ?? TenantBranding.Create();
        source.Adapt(branding);
        destination.SetBranding(branding);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Presentation\OrganizationModule.cs
// ----------------------------------------
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

        // services.AddJobScheduling()
        //     .WithJob<EchoJob>(CronExpressions.Every5Minutes);
        // .WithSingletonJob<EchoJob>(CronExpressions.Every5Minutes)
        //.WithJob<HealthCheckJob>(CronExpressions.EveryMinute);

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
                .DeleteOnStartup(false));
            // .WithOutboxDomainEventService(o => o
            //     .ProcessingInterval("00:00:30")
            //     .StartupDelay("00:00:30")
            //     .PurgeOnStartup()
            //     .ProcessingModeImmediate());

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

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Presentation\OrganizationSignalRHub.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Presentation;

using Microsoft.AspNetCore.SignalR;

public class OrganizationSignalRHub : Hub
{
    public async Task OnCheckHealth()
    {
        await this.Clients.All.SendAsync("CheckHealth");
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Application\Commands\TenantAwareCommandBehavior.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Application;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;
using MediatR;

/// <summary>
///     A behavior for handling tenant-aware commands in the application. This class is responsible for
///     checking if a request implements the ITenantAware interface and ensuring the tenant exists and is active
///     before allowing the request to proceed.
/// </summary>
/// <typeparam name="TRequest">The type of request implementing IRequest.</typeparam>
/// <typeparam name="TResponse">The type of response returned by the request.</typeparam>
public class TenantAwareCommandBehavior<TRequest, TResponse>(
    ILoggerFactory loggerFactory,
    IOrganizationModuleClient organizationModuleClient)
    : CommandBehaviorBase<TRequest, TResponse>(loggerFactory)
    where TRequest : class, IRequest<TResponse>
{
    protected override bool CanProcess(TRequest request)
    {
        return request is ITenantAware;
    }

    protected override async Task<TResponse> Process(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ITenantAware instance)
        {
            return await next().AnyContext();
        }

        if (!(await organizationModuleClient.TenantFindOne(instance.TenantId)).Value?.IsActive == false)
        {
            throw new Exception("Tenant does not exists or inactive");
        }

        return await next().AnyContext();
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Application\Queries\TenantAwareQueryBehavior.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Application;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;
using MediatR;

/// <summary>
///     A behavior for handling tenant-aware queries in the application. This class is responsible for
///     checking if a request implements the ITenantAware interface and ensuring the tenant exists and is active
///     before allowing the request to proceed.
/// </summary>
/// <typeparam name="TRequest">The type of request implementing IRequest.</typeparam>
/// <typeparam name="TResponse">The type of response returned by the request.</typeparam>
public class TenantAwareQueryBehavior<TRequest, TResponse>(
    ILoggerFactory loggerFactory,
    IOrganizationModuleClient organizationModuleClient)
    : QueryBehaviorBase<TRequest, TResponse>(loggerFactory)
    where TRequest : class, IRequest<TResponse>
{
    protected override bool CanProcess(TRequest request)
    {
        return request is ITenantAware;
    }

    protected override async Task<TResponse> Process(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ITenantAware instance)
        {
            return await next().AnyContext();
        }

        if (!(await organizationModuleClient.TenantFindOne(instance.TenantId)).Value?.IsActive == false)
        {
            throw new Exception("Tenant does not exists or inactive");
        }

        return await next().AnyContext();
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Application.Contracts\Models\AddressModel.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Application;

public class AddressModel
{
    public string Name { get; set; }

    public string Line1 { get; set; }

    public string Line2 { get; set; }

    public string PostalCode { get; set; }

    public string City { get; set; }

    public string Country { get; set; }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Application.Contracts\Models\DateScheduleModel.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Application;

public class DateScheduleModel
{
    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Application.Contracts\Models\PersonFormalNameModel.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Application;

public class PersonFormalNameModel
{
    public string Title { get; set; }

    public string[] Parts { get; set; }

    public string Suffix { get; set; }

    public string Full { get; set; }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Domain\Model\Address.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

public class Address : ValueObject
{
    private Address() { } // Private constructor required by EF Core

    private Address(string name, string line1, string line2, string postalCode, string city, string country)
    {
        this.Name = name;
        this.Line1 = line1;
        this.Line2 = line2;
        this.PostalCode = postalCode;
        this.City = city;
        this.Country = country;
    }

    public string Name { get; }

    public string Line1 { get; }

    public string Line2 { get; }

    public string PostalCode { get; }

    public string City { get; }

    public string Country { get; }

    public static Address Create(
        string name,
        string line1,
        string line2,
        string postalCode,
        string city,
        string country)
    {
        var address = new Address(name, line1, line2, postalCode, city, country);
        if (!IsValid(address))
        {
            throw new DomainRuleException("Invalid address");
        }

        return address;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Name;
        yield return this.Line1;
        yield return this.Line2;
        yield return this.PostalCode;
        yield return this.City;
        yield return this.Country;
    }

    private static bool IsValid(Address address)
    {
        return !string.IsNullOrEmpty(address.Name) &&
            !string.IsNullOrEmpty(address.Line1) &&
            !string.IsNullOrEmpty(address.PostalCode) &&
            !string.IsNullOrEmpty(address.Country) &&
            !string.IsNullOrEmpty(address.Country);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Domain\Model\AverageRating.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

[DebuggerDisplay("Value={Value}, Amount={Amount}")]
public class AverageRating : ValueObject
{
    private double value;

    private AverageRating() { }

    private AverageRating(double value, int amount)
    {
        this.Value = value;
        this.Amount = amount;
    }

    public double? Value
    {
        get => this.Amount > 0 ? this.value : null;
        private set => this.value = value!.Value;
    }

    public int Amount { get; private set; }

    public static implicit operator double(AverageRating rating)
    {
        return rating.Value ?? 0;
    }

    public static AverageRating Create(double value = 0, int amount = 0)
    {
        return new AverageRating(value, amount);
    }

    public void Add(Rating rating)
    {
        // ReSharper disable once ArrangeRedundantParentheses
        this.Value = ((this.value * this.Amount) + rating.Value) / ++this.Amount;
    }

    public void Remove(Rating rating)
    {
        if (this.Amount == 0)
        {
            return;
        }

        // ReSharper disable once ArrangeRedundantParentheses
        this.Value = ((this.Value * this.Amount) - rating.Value) / --this.Amount;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Domain\Model\Currency.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

[DebuggerDisplay("Code={Code}")]
public class Currency : ValueObject
{
    private static readonly Dictionary<string, string> Currencies;

    static Currency()
    {
        // source: https://www.xe.com/symbols/
        Currencies = new Dictionary<string, string>
        {
            { "ALL", "Lek" },
            { "AFN", "؋" },
            { "ARS", "$" },
            { "AWG", "ƒ" },
            { "AUD", "$" },
            { "AZN", "₼" },
            { "BSD", "$" },
            { "BBD", "$" },
            { "BYN", "Br" },
            { "BZD", "BZ$" },
            { "BMD", "$" },
            { "BOB", "$b" },
            { "BAM", "KM" },
            { "BWP", "P" },
            { "BGN", "лв" },
            { "BRL", "R$" },
            { "BND", "$" },
            { "KHR", "៛" },
            { "CAD", "$" },
            { "KYD", "$" },
            { "CLP", "$" },
            { "CNY", "¥" },
            { "COP", "$" },
            { "CRC", "₡" },
            { "HRK", "kn" },
            { "CUP", "₱" },
            { "CZK", "Kč" },
            { "DKK", "kr" },
            { "DOP", "RD$" },
            { "XCD", "$" },
            { "EGP", "£" },
            { "SVC", "$" },
            { "EUR", "€" },
            { "FKP", "£" },
            { "FJD", "$" },
            { "GHS", "¢" },
            { "GIP", "£" },
            { "GTQ", "Q" },
            { "GGP", "£" },
            { "GYD", "$" },
            { "HNL", "L" },
            { "HKD", "$" },
            { "HUF", "Ft" },
            { "ISK", "kr" },
            { "INR", "₹" },
            { "IDR", "Rp" },
            { "IRR", "﷼" },
            { "IMP", "£" },
            { "ILS", "₪" },
            { "JMD", "J$" },
            { "JPY", "¥" },
            { "JEP", "£" },
            { "KZT", "лв" },
            { "KPW", "₩" },
            { "KRW", "₩" },
            { "KGS", "лв" },
            { "LAK", "₭" },
            { "LBP", "£" },
            { "LRD", "$" },
            { "MKD", "ден" },
            { "MYR", "RM" },
            { "MUR", "₨" },
            { "MXN", "$" },
            { "MNT", "₮" },
            { "MZN", "MT" },
            { "NAD", "$" },
            { "NPR", "₨" },
            { "ANG", "ƒ" },
            { "NZD", "$" },
            { "NIO", "C$" },
            { "NGN", "₦" },
            { "NOK", "kr" },
            { "OMR", "﷼" },
            { "PKR", "₨" },
            { "PAB", "B/." },
            { "PYG", "Gs" },
            { "PEN", "S/." },
            { "PHP", "₱" },
            { "PLN", "zł" },
            { "QAR", "﷼" },
            { "RON", "lei" },
            { "RUB", "₽" },
            { "SHP", "£" },
            { "SAR", "﷼" },
            { "RSD", "Дин." },
            { "SCR", "₨" },
            { "SGD", "$" },
            { "SBD", "$" },
            { "SOS", "S" },
            { "ZAR", "R" },
            { "LKR", "₨" },
            { "SEK", "kr" },
            { "CHF", "CHF" },
            { "SRD", "$" },
            { "SYP", "£" },
            { "TWD", "NT$" },
            { "THB", "฿" },
            { "TTD", "TT$" },
            { "TRY", "₺" },
            { "TVD", "$" },
            { "UAH", "₴" },
            { "GBP", "£" },
            { "USD", "$" },
            { "UYU", "$U" },
            { "UZS", "лв" },
            { "VEF", "Bs" },
            { "VND", "₫" },
            { "YER", "﷼" },
            { "ZWD", "Z$" }
        };
    }

    private Currency() { } // Private constructor required by EF Core

    private Currency(string code)
    {
        this.Code = code;
    }

    public static Currency AlbaniaLek
        => Create("ALL");

    public static Currency AfghanistanAfghani
        => Create("AFN");

    public static Currency ArgentinaPeso
        => Create("ARS");

    public static Currency ArubaGuilder
        => Create("AWG");

    public static Currency AustraliaDollar
        => Create("AUD");

    public static Currency AzerbaijanManat
        => Create("AZN");

    public static Currency BahamasDollar
        => Create("BSD");

    public static Currency BarbadosDollar
        => Create("BBD");

    public static Currency BelarusRuble
        => Create("BYN");

    public static Currency BelizeDollar
        => Create("BZD");

    public static Currency BermudaDollar
        => Create("BMD");

    public static Currency BoliviaBolíviano
        => Create("BOB");

    public static Currency BosniaandHerzegovinaMark
        => Create("BAM");

    public static Currency BotswanaPula
        => Create("BWP");

    public static Currency BulgariaLev
        => Create("BGN");

    public static Currency BrazilReal
        => Create("BRL");

    public static Currency BruneiDarussalamDollar
        => Create("BND");

    public static Currency CambodiaRiel
        => Create("KHR");

    public static Currency CanadaDollar
        => Create("CAD");

    public static Currency CaymanIslandsDollar
        => Create("KYD");

    public static Currency ChilePeso
        => Create("CLP");

    public static Currency ChinaYuanRenminbi
        => Create("CNY");

    public static Currency ColombiaPeso
        => Create("COP");

    public static Currency CostaRicaColon
        => Create("CRC");

    public static Currency CroatiaKuna
        => Create("HRK");

    public static Currency CubaPeso
        => Create("CUP");

    public static Currency CzechRepublicKoruna
        => Create("CZK");

    public static Currency DenmarkKrone
        => Create("DKK");

    public static Currency DominicanRepublicPeso
        => Create("DOP");

    public static Currency EastCaribbeanDollar
        => Create("XCD");

    public static Currency EgyptPound
        => Create("EGP");

    public static Currency ElSalvadorColon
        => Create("SVC");

    public static Currency Euro
        => Create("EUR");

    public static Currency FalklandIslands
        => Create("FKP");

    public static Currency FijiDollar
        => Create("FJD");

    public static Currency GhanaCedi
        => Create("GHS");

    public static Currency GibraltarPound
        => Create("GIP");

    public static Currency GuatemalaQuetzal
        => Create("GTQ");

    public static Currency GuernseyPound
        => Create("GGP");

    public static Currency GuyanaDollar
        => Create("GYD");

    public static Currency HondurasLempira
        => Create("HNL");

    public static Currency HongKongDollar
        => Create("HKD");

    public static Currency HungaryForint
        => Create("HUF");

    public static Currency IcelandKrona
        => Create("ISK");

    public static Currency IndiaRupee
        => Create("INR");

    public static Currency IndonesiaRupiah
        => Create("IDR");

    public static Currency IranRial
        => Create("IRR");

    public static Currency IsleofManPound
        => Create("IMP");

    public static Currency IsraelShekel
        => Create("ILS");

    public static Currency JamaicaDollar
        => Create("JMD");

    public static Currency JapanYen
        => Create("JPY");

    public static Currency JerseyPound
        => Create("JEP");

    public static Currency KazakhstanTenge
        => Create("KZT");

    public static Currency KoreaNorth
        => Create("KPW");

    public static Currency KoreaSouth
        => Create("KRW");

    public static Currency KyrgyzstanSom
        => Create("KGS");

    public static Currency LaosKip
        => Create("LAK");

    public static Currency LebanonPound
        => Create("LBP");

    public static Currency LiberiaDollar
        => Create("LRD");

    public static Currency MacedoniaDenar
        => Create("MKD");

    public static Currency MalaysiaRinggit
        => Create("MYR");

    public static Currency MauritiusRupee
        => Create("MUR");

    public static Currency MexicoPeso
        => Create("MXN");

    public static Currency MongoliaTughrik
        => Create("MNT");

    public static Currency MozambiqueMetical
        => Create("MZN");

    public static Currency NamibiaDollar
        => Create("NAD");

    public static Currency NepalRupee
        => Create("NPR");

    public static Currency NetherlandsAntillesGuilder
        => Create("ANG");

    public static Currency NewZealandDollar
        => Create("NZD");

    public static Currency NicaraguaCordoba
        => Create("NIO");

    public static Currency NigeriaNaira
        => Create("NGN");

    public static Currency NorwayKrone
        => Create("NOK");

    public static Currency OmanRial
        => Create("OMR");

    public static Currency PakistanRupee
        => Create("PKR");

    public static Currency PanamaBalboa
        => Create("PAB");

    public static Currency ParaguayGuarani
        => Create("PYG");

    public static Currency PeruSol
        => Create("PEN");

    public static Currency PhilippinesPeso
        => Create("PHP");

    public static Currency PolandZloty
        => Create("PLN");

    public static Currency QatarRiyal
        => Create("QAR");

    public static Currency RomaniaLeu
        => Create("RON");

    public static Currency RussiaRuble
        => Create("RUB");

    public static Currency SaintHelenaPound
        => Create("SHP");

    public static Currency SaudiArabiaRiyal
        => Create("SAR");

    public static Currency SerbiaDinar
        => Create("RSD");

    public static Currency SeychellesRupee
        => Create("SCR");

    public static Currency SingaporeDollar
        => Create("SGD");

    public static Currency SolomonIslandsDollar
        => Create("SBD");

    public static Currency SomaliaShilling
        => Create("SOS");

    public static Currency SouthAfricaRand
        => Create("ZAR");

    public static Currency SriLankaRupee
        => Create("LKR");

    public static Currency SwedenKrona
        => Create("SEK");

    public static Currency SwitzerlandFranc
        => Create("CHF");

    public static Currency SurinameDollar
        => Create("SRD");

    public static Currency SyriaPound
        => Create("SYP");

    public static Currency TaiwanNewDollar
        => Create("TWD");

    public static Currency ThailandBaht
        => Create("THB");

    public static Currency TrinidadandTobagoDollar
        => Create("TTD");

    public static Currency TurkeyLira
        => Create("TRY");

    public static Currency TuvaluDollar
        => Create("TVD");

    public static Currency UkraineHryvnia
        => Create("UAH");

    public static Currency GBPound
        => Create("GBP");

    public static Currency USDollar
        => Create("USD");

    public static Currency UruguayPeso
        => Create("UYU");

    public static Currency UzbekistanSom
        => Create("UZS");

    public static Currency VenezuelaBolivar
        => Create("VEF");

    public static Currency VietNamDong
        => Create("VND");

    public static Currency YemenRial
        => Create("YER");

    public static Currency ZimbabweDollar
        => Create("ZWD");

    public string Code { get; private set; }

    public string Symbol
        => Currencies.First(c => c.Key == this.Code).Value;

    public static implicit operator string(Currency currency)
    {
        return currency?.Code;
        // allows a Currency value to be implicitly converted to a string.
    }

    public static implicit operator Currency(string value)
    {
        return new Currency(value);
        // allows a string value to be implicitly converted to a Currency object.
    }

    public static Currency Create(string code)
    {
        if (!Currencies.ContainsKey(code.SafeNull()))
        {
            throw new DomainRuleException($"Invalid currency code: {code}");
        }

        return new Currency(code); //Currencies.First(c => c.Key == code).Value; //Currencies[code];
    }

    public override string ToString()
    {
        return $"{this.Symbol}";
        // https://social.technet.microsoft.com/wiki/contents/articles/27931.currency-formatting-in-c.aspx
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Code;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Domain\Model\DateSchedule.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using System.Globalization;

[DebuggerDisplay("StartDate={StartDate}, EndDate={EndDate}")]
public class DateSchedule : ValueObject, IComparable<DateSchedule>
{
    private DateSchedule() { } // Private constructor required by EF Core

    private DateSchedule(DateOnly startDate, DateOnly? endDate)
    {
        this.StartDate = startDate;
        this.EndDate = endDate;
    }

    public DateOnly StartDate { get; private set; }

    public DateOnly? EndDate { get; private set; }

    public bool IsOpenEnded
        => !this.EndDate.HasValue;

    public static bool operator <(DateSchedule left, DateSchedule right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(DateSchedule left, DateSchedule right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(DateSchedule left, DateSchedule right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(DateSchedule left, DateSchedule right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static DateSchedule Create(DateOnly startDate, DateOnly? endDate = null)
    {
        if (endDate <= startDate)
        {
            throw new DomainRuleException("End date must be after start date");
        }

        return new DateSchedule(startDate, endDate);
    }

    public bool IsActive(DateOnly date)
    {
        return date >= this.StartDate && (!this.EndDate.HasValue || date <= this.EndDate.Value);
    }

    public bool OverlapsWith(DateSchedule other)
    {
        if (other == null)
        {
            return false;
        }

        if (this.IsOpenEnded || other.IsOpenEnded)
        {
            return this.StartDate <= (other.EndDate ?? DateOnly.MaxValue) &&
                other.StartDate <= (this.EndDate ?? DateOnly.MaxValue);
        }

        return this.StartDate <= other.EndDate.Value && other.StartDate <= this.EndDate.Value;
    }

    public override string ToString()
    {
        const string dateFormat = "dd-MM-yyyy";

        return this.EndDate.HasValue
            ? $"{this.StartDate.ToString(dateFormat, CultureInfo.InvariantCulture)} to {this.EndDate.Value.ToString(dateFormat, CultureInfo.InvariantCulture)}"
            : $"{this.StartDate.ToString(dateFormat, CultureInfo.InvariantCulture)} (Open-ended)";
    }

    public int CompareTo(DateSchedule other)
    {
        if (other == null)
        {
            return 1;
        }

        return this.StartDate.CompareTo(other.StartDate);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.StartDate;
        yield return this.EndDate;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Domain\Model\EmailAddress.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using System.Text.RegularExpressions;

[DebuggerDisplay("Value={Value}")]
public partial class EmailAddress : ValueObject
{
    private EmailAddress() { } // Private constructor required by EF Core

    private EmailAddress(string email)
    {
        this.Value = email;
    }

    public string Value { get; private set; }

    public static implicit operator string(EmailAddress email)
    {
        return email.Value;
    }

    public static implicit operator EmailAddress(string email)
    {
        return Create(email);
    }

    public static EmailAddress Create(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null; //throw new DomainRuleException("EmailAddress cannot be empty.");
        }

        value = Normalize(value);
        if (!IsValid(value))
        {
            throw new DomainRuleException("Invalid email address");
        }

        return new EmailAddress(value);
    }

    public override string ToString()
    {
        return this.Value;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }

    private static string Normalize(string value)
    {
        return value?.Trim().ToLowerInvariant() ?? string.Empty;
    }

    [GeneratedRegex(
        @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        "en-US")]
    private static partial Regex IsValidRegex();

    private static bool IsValid(string email)
    {
        return !string.IsNullOrEmpty(email) && email.Length <= 255 && IsValidRegex().IsMatch(email);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Domain\Model\HexColor.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using System.Text.RegularExpressions;

[DebuggerDisplay("Value={Value}")]
public class HexColor : ValueObject
{
    private static readonly Regex HexColorRegex = new(@"^#(?:[0-9a-fA-F]{3}){1,2}$", RegexOptions.Compiled);

    private HexColor() { } // Private constructor required by EF Core

    private HexColor(string value)
    {
        this.Value = value;
    }

    public string Value { get; }

    public static implicit operator string(HexColor color)
    {
        return color.Value;
    }

    public static implicit operator HexColor(string value)
    {
        return Create(value);
    }

    public static HexColor Create(string value)
    {
        value = Normalize(value);
        if (!IsValid(value))
        {
            throw new DomainRuleException($"Invalid hex color format: {value}. Use the format #RGB or #RRGGBB.");
        }

        return new HexColor(value);
    }

    public static HexColor Create(byte r, byte g, byte b)
    {
        return Create($"#{r:X2}{g:X2}{b:X2}");
    }

    public static bool IsValid(string value)
    {
        return string.IsNullOrWhiteSpace(value) || HexColorRegex.IsMatch(value);
    }

    public override string ToString()
    {
        return this.Value;
    }

    public (byte R, byte G, byte B) ToRgb()
    {
        var hex = this.Value.TrimStart('#');

        return (Convert.ToByte(hex.Substring(0, 2), 16), Convert.ToByte(hex.Substring(2, 2), 16),
            Convert.ToByte(hex.Substring(4, 2), 16));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }

    private static string Normalize(string value)
    {
        value = value?.ToUpperInvariant() ?? string.Empty;
        if (value.Length == 4) // #RGB format
        {
            return $"#{value[1]}{value[1]}{value[2]}{value[2]}{value[3]}{value[3]}";
        }

        return value;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Domain\Model\Money.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using System.Globalization;
using EnsureThat;

[DebuggerDisplay("Currency={Currency}, Amount={Amount}")]
public class Money : DecimalValueObject
{
    private int? cachedHashCode;

    private Money() { } // Private constructor required by EF Core

    private Money(decimal amount, Currency currency)
        : base(amount)
    {
        this.Currency = currency;
    }

    public Currency Currency { get; private set; }

    public static Money Zero()
    {
        return Create(0);
    }

    public static Money Zero(Currency currency)
    {
        return Create(0, currency);
    }

    public bool IsZero()
    {
        return this.Amount == 0;
    }

#pragma warning disable SA1201 // Elements should appear in the correct order
    public static implicit operator decimal(Money money)
    {
        return money?.Amount ?? 0;
        // allows a Money value to be implicitly converted to a decimal.
    }

    public static implicit operator string(Money money)
    {
        return money?.ToString() ?? string.Empty;
        // allows a Money value to be implicitly converted to a string.
    }
#pragma warning restore SA1201 // Elements should appear in the correct order

    public static bool operator ==(Money a, Money b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
        {
            return false;
        }

        return a.Amount.Equals(b.Amount) && a.Currency.Equals(b.Currency);
    }

    public static bool operator !=(Money a, Money b)
    {
        return !(a == b);
    }

    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
        {
            throw new InvalidOperationException("Cannot calculate money with different currencies");
        }

        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        if (a.Currency != b.Currency)
        {
            throw new InvalidOperationException("Cannot calculate money with different currencies");
        }

        return new Money(a.Amount - b.Amount, a.Currency);
    }

    public static Money Create(decimal amount)
    {
        return new Money(amount, Currency.USDollar);
    }

    public static Money Create(decimal amount, Currency currency)
    {
        EnsureArg.IsNotNull(currency, nameof(currency));

        return new Money(amount, currency);
    }

    public override bool Equals(object obj)
    {
        if (obj == null || obj.GetType() != this.GetType())
        {
            return false;
        }

        return this.Equals((Money)obj);
    }

    /// <summary>
    ///     Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
    /// </returns>
    public override int GetHashCode()
    {
        return this.cachedHashCode ??= this.GetAtomicValues()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    public override string ToString()
    {
        return this.Format(this.Amount, this.Currency.Code);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Currency.Code;
        yield return this.Amount;
    }

    protected override IEnumerable<IComparable> GetComparableAtomicValues()
    {
        yield return this.Currency.Code;
        yield return this.Amount;
    }

    private string Format(decimal amount, string currencyCode)
    {
        EnsureArg.IsNotNullOrEmpty(currencyCode, nameof(currencyCode));

        var culture = (from c in CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            let r = this.CreateRegionInfo(c.Name)
            where r != null && string.Equals(r.ISOCurrencySymbol, currencyCode, StringComparison.OrdinalIgnoreCase)
            select c).FirstOrDefault();

        if (culture == null)
        {
            return amount.ToString("0.00");
        }

        return string.Format(culture, "{0:C}", amount);
    }

    private RegionInfo CreateRegionInfo(string cultureName)
    {
        RegionInfo region;

        try
        {
            region = new RegionInfo(cultureName);
        }
        catch (CultureNotFoundException)
        {
            return default;
        }

        return region;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Domain\Model\PersonFormalName.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using System.Text.RegularExpressions;

[DebuggerDisplay("Name={ToString()}")]
public partial class PersonFormalName : ValueObject
{
    private static readonly Regex NamePartRegex = Regexes.NamePartRegex();
    private static readonly Regex TitleSuffixRegex = Regexes.TitleSuffixRegex();

    private PersonFormalName() { } // Private constructor required by EF Core

    private PersonFormalName(string[] parts, string title = null, string suffix = null)
    {
        Validate(parts, title, suffix);

        this.Title = title;
        this.Parts = parts;
        this.Suffix = suffix;
    }

    public string Title { get; private set; }

    public IEnumerable<string> Parts { get; }

    public string Suffix { get; private set; }

    public string Full
    {
        get => this.ToString();
        set // needs to be private
            => _ = value;
    }

    public static implicit operator string(PersonFormalName name)
    {
        return name?.ToString();
        // allows a PersonFormalName value to be implicitly converted to a string.
    }

    public static PersonFormalName Create(IEnumerable<string> parts, string title = null, string suffix = null)
    {
        return Create(parts?.ToArray(), title, suffix);
    }

    public static PersonFormalName Create(string[] parts, string title = null, string suffix = null)
    {
        return new PersonFormalName(parts, title, suffix);
    }

    public override string ToString()
    {
        var fullName = string.Join(" ", this.Parts);

        if (!string.IsNullOrEmpty(this.Title))
        {
            fullName = $"{this.Title} {fullName}";
        }

        if (!string.IsNullOrEmpty(this.Suffix))
        {
            fullName = $"{fullName}, {this.Suffix}";
        }

        return fullName.Trim().Trim(',');
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Title;

        foreach (var part in this.Parts)
        {
            yield return part;
        }

        yield return this.Suffix;
    }

    private static void Validate(string[] parts, string title, string suffix)
    {
        if (!parts.SafeAny())
        {
            throw new ArgumentException("PersonFormalName parts cannot be empty.");
        }

        foreach (var part in parts)
        {
            ValidateNamePart(part);
        }

        ValidateTitleSuffix(title, nameof(Title));
        ValidateTitleSuffix(suffix, nameof(Suffix));
    }

    private static void ValidateNamePart(string namePart)
    {
        if (string.IsNullOrWhiteSpace(namePart))
        {
            throw new ArgumentException("PersonFormalName part cannot be empty.");
        }

        if (!NamePartRegex.IsMatch(namePart))
        {
            throw new ArgumentException("PersonFormalName part contains invalid characters.");
        }
    }

    private static void ValidateTitleSuffix(string value, string propertyName)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        if (!TitleSuffixRegex.IsMatch(value))
        {
            throw new ArgumentException($"PersonFormalName {propertyName.ToLower()} contains invalid characters.");
        }
    }

    public static partial class Regexes
    {
        // Update the regular expression pattern in the PersonFormalName class

        [GeneratedRegex(@"^[\p{L}\p{M}.]+([\p{L}\p{M}'-]*[\p{L}\p{M}])?$", RegexOptions.Compiled)]
        public static partial Regex NamePartRegex();

        [GeneratedRegex(@"^[\p{L}\p{M}\.\-'\s]+$", RegexOptions.Compiled)]
        public static partial Regex TitleSuffixRegex();
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Domain\Model\PhoneNumber.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using System.Text.RegularExpressions;

[DebuggerDisplay("CountryCode={CountryCode}, Number={Number}")]
public class PhoneNumber : ValueObject
{
    // This regex pattern allows for 1 to 3 digit country codes, followed by the rest of the number
    private static readonly Regex PhoneRegex = new(@"^\+?(\d{1,3})([0-9\s\-\(\)\.]{1,20})$", RegexOptions.Compiled);

    // Source: ITU-T Recommendation E.164 (as of 2021)
    // Note: This list should be periodically reviewed and updated
    private static readonly HashSet<string> CountryCodes =
    [
        "1",
        // Two-digit country codes
        "20", "27", "30", "31", "32", "33", "34", "36", "39", "40", "41", "43", "44", "45", "46", "47", "48", "49",
        "51", "52", "53", "54", "55", "56", "57", "58", "60", "61", "62", "63", "64", "65", "66", "81", "82", "84",
        "86", "90", "91", "92", "93", "94", "95", "98",
        // Three-digit country codes
        "210", "211", "212", "213", "216", "218", "220", "221", "222", "223", "224", "225", "226", "227", "228", "229",
        "230", "231", "232", "233", "234", "235", "236", "237", "238", "239", "240", "241", "242", "243", "244", "245",
        "246", "247", "248", "249", "250", "251", "252", "253", "254", "255", "256", "257", "258", "260", "261", "262",
        "263", "264", "265", "266", "267", "268", "269", "290", "291", "297", "298", "299", "350", "351", "352", "353",
        "354", "355", "356", "357", "358", "359", "370", "371", "372", "373", "374", "375", "376", "377", "378", "379",
        "380", "381", "382", "383", "385", "386", "387", "389", "420", "421", "423", "500", "501", "502", "503", "504",
        "505", "506", "507", "508", "509", "590", "591", "592", "593", "594", "595", "596", "597", "598", "599", "670",
        "672", "673", "674", "675", "676", "677", "678", "679", "680", "681", "682", "683", "685", "686", "687", "688",
        "689", "690", "691", "692", "800", "808", "850", "852", "853", "855", "856", "870", "878", "880", "881", "882",
        "883", "886", "888", "960", "961", "962", "963", "964", "965", "966", "967", "968", "970", "971", "972", "973",
        "974", "975", "976", "977", "979", "992", "993", "994", "995", "996", "998"
    ];

    private PhoneNumber(string countryCode, string number)
    {
        this.CountryCode = countryCode;
        this.Number = number;
    }

    public string CountryCode { get; }

    public string Number { get; }

    public static implicit operator string(PhoneNumber phoneNumber)
    {
        return phoneNumber?.ToString();
    }

    public static implicit operator PhoneNumber(string value)
    {
        return Create(value);
    }

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        var cleanNumber = CleanNumber(value);
        if (!IsValid(cleanNumber))
        {
            throw new DomainRuleException("Invalid phone number format.");
        }

        var countryCode = ExtractCountryCode(cleanNumber);
        var number = cleanNumber.Substring(countryCode.Length).TrimStart('0');

        return new PhoneNumber(countryCode, number);
    }

    public override string ToString()
    {
        return $"+{this.CountryCode} {this.Number}";
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.CountryCode;
        yield return this.Number;
    }

    private static bool IsValid(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && PhoneRegex.IsMatch(value);
    }

    private static string CleanNumber(string value)
    {
        return new string(value.Where(c => char.IsDigit(c) && c != '+').ToArray()).TrimStart('0');
    }

    private static string ExtractCountryCode(string value)
    {
        foreach (var length in new[]
                 {
                     3,
                     2,
                     1
                 })
        {
            if (value.Length >= length)
            {
                var potentialCode = value.Substring(0, length);
                if (CountryCodes.Contains(potentialCode))
                {
                    return potentialCode;
                }
            }
        }

        throw new DomainRuleException("Invalid or unsupported country code.");
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Domain\Model\ProductSku.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using System.Text.RegularExpressions;

[DebuggerDisplay("Value={Value}")]
public partial class ProductSku : ValueObject
{
    private static readonly Regex SkuRegex = GeneratedRegexes.SkuRegex();

    private ProductSku() { }

    private ProductSku(string value)
    {
        this.Validate(value);
        this.Value = value;
    }

    public string Value { get; private set; }

    public static implicit operator ProductSku(string value)
    {
        return Create(value);
    }

    public static implicit operator string(ProductSku sku)
    {
        return sku.Value;
    }

    public static ProductSku Create(string value)
    {
        value = value?.Trim().ToUpperInvariant() ?? string.Empty;
        return new ProductSku(value);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }

    private void Validate(string sku)
    {
        if (string.IsNullOrEmpty(sku))
        {
            throw new ArgumentException("SKU cannot be empty.");
        }

        if (!SkuRegex.IsMatch(sku))
        {
            throw new ArgumentException("SKU must be 8-12 digits long.");
        }
    }

    public static partial class GeneratedRegexes
    {
        [GeneratedRegex(@"^\d{8,12}$", RegexOptions.Compiled)]
        public static partial Regex SkuRegex();
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Domain\Model\Rating.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

[DebuggerDisplay("Value={Value}")]
public class Rating : ValueObject
{
    private Rating() { }

    private Rating(int value)
    {
        this.Value = value;
    }

    public int Value { get; private set; }

    public static Rating Poor()
    {
        return new Rating(1);
    }

    public static Rating Fair()
    {
        return new Rating(2);
    }

    public static Rating Good()
    {
        return new Rating(3);
    }

    public static Rating VeryGood()
    {
        return new Rating(4);
    }

    public static Rating Excellent()
    {
        return new Rating(5);
    }

    public static Rating Create(int value)
    {
        DomainRules.Apply([RatingRules.ShouldBeInRange(value)]);

        return new Rating(value);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Domain\Model\Tag.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

[DebuggerDisplay("Id={Id}, Name={Name}")]
[TypedEntityId<Guid>]
public class Tag : Entity<TagId>, IConcurrent
{
    private Tag() { } // Private constructor required by EF Core

    private Tag(TenantId tenantId, string name, string category = null)
    {
        this.TenantId = tenantId;
        this.SetName(name);
        this.SetCategory(category);
    }

    public TenantId TenantId { get; private set; }

    public string Name { get; private set; }

    public string Category { get; private set; }

    /// <summary>
    ///     Gets or sets the concurrency token to handle optimistic concurrency.
    /// </summary>
    public Guid Version { get; set; }

    public static implicit operator string(Tag tag)
    {
        return tag?.Name;
        // allows a Tag value to be implicitly converted to a string.
    }

    public static Tag Create(TenantId tenantId, string name, string category)
    {
        _ = tenantId ?? throw new ArgumentException("TenantId cannot be empty.");

        return new Tag(tenantId, name, category);
    }

    public Tag SetName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new DomainRuleException("Tag name cannot be empty.");
        }

        this.Name = value;

        return this;
    }

    public Tag SetCategory(string value)
    {
        this.Category = value;

        return this;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Domain\Model\TagId.cs
// ----------------------------------------
//// MIT-License
//// Copyright BridgingIT GmbH - All Rights Reserved
//// Use of this source code is governed by an MIT-style license that can be
//// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

//namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

//public class TagId : EntityId<Guid> // TODO: move to SharedKernel
//{
//    private TagId()
//    {
//    }

//    private TagId(Guid guid)
//    {
//        this.Value = guid;
//    }

//    public override Guid Value { get; protected set; }

//    public bool IsEmpty => this.Value == Guid.Empty;

//    //public static implicit operator Guid(TagId id) => id?.Value ?? default; // allows a TagId value to be implicitly converted to a Guid.
//    //public static implicit operator TagId(Guid id) => id; // allows a Guid value to be implicitly converted to a TagId object.

//    public static TagId Create()
//    {
//        return new TagId(Guid.NewGuid());
//    }

//    public static TagId Create(Guid id)
//    {
//        return new TagId(id);
//    }

//    public static TagId Create(string id)
//    {
//        if (string.IsNullOrWhiteSpace(id))
//        {
//            throw new ArgumentException("Id cannot be null or whitespace.");
//        }

//        return new TagId(Guid.Parse(id));
//    }

//    protected override IEnumerable<object> GetAtomicValues()
//    {
//        yield return this.Value;
//    }
//}


// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Domain\Model\TenantId.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

[TypedEntityId<Guid>]
public partial class TenantId { }
//public class TenantId : AggregateRootId<Guid> // cannot be source generated while AggregateRoot in different project
//{
//    private TenantId()
//    {
//    }

//    private TenantId(Guid guid)
//    {
//        this.Value = guid;
//    }

//    public override Guid Value { get; protected set; }

//    public bool IsEmpty => this.Value == Guid.Empty;

//    public static implicit operator Guid(TenantId id) => id?.Value ?? default; // allows a TenantId value to be implicitly converted to a Guid.
//    public static implicit operator string(TenantId id) => id?.Value.ToString(); // allows a TenantId value to be implicitly converted to a string.
//    public static implicit operator TenantId(Guid id) => id; // allows a Guid value to be implicitly converted to a TenantId object.

//    public static TenantId Create()
//    {
//        return new TenantId(Guid.NewGuid());
//    }

//    public static TenantId Create(Guid id)
//    {
//        return new TenantId(id);
//    }

//    public static TenantId Create(string id)
//    {
//        if (string.IsNullOrEmpty(id))
//        {
//            throw new ArgumentException("Id cannot be null or empty.", nameof(id));
//        }

//        return new TenantId(Guid.Parse(id));
//    }

//    protected override IEnumerable<object> GetAtomicValues()
//    {
//        yield return this.Value;
//    }
//}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Domain\Model\TenantIdFactory.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

public static class TenantIdFactory
{
    public static TenantId CreateForName(string name)
    {
        return TenantId.Create(GuidGenerator.Create($"Tenant_{name}"));
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Domain\Model\Url.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using System.Text.RegularExpressions;

[DebuggerDisplay("Value={Value}")]
public partial class Url : ValueObject
{
    private static readonly Regex AbsoluteUrlRegex = AbsoluteRegex();
    private static readonly Regex RelativeUrlRegex = RelativeRegex();
    private static readonly Regex LocalUrlRegex = LocalRegex();

    private Url() { } // Private constructor required by EF Core

    private Url(string url)
    {
        this.Value = url;
    }

    public string Value { get; private set; }

    public UrlType Type
        => DetermineType(this.Value);

    public static implicit operator string(Url url)
    {
        return url.Value;
    }

    public static implicit operator Url(string value)
    {
        return Create(value);
    }

    public static Url Create(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null; //throw new DomainRuleException("Url cannot be empty.");
        }

        var normalizedUrl = Normalize(value);
        if (!IsValid(normalizedUrl))
        {
            throw new DomainRuleException($"Invalid URL format: {value}");
        }

        return new Url(normalizedUrl);
    }

    public bool IsAbsolute()
    {
        return this.Type == UrlType.Absolute;
    }

    public bool IsRelative()
    {
        return this.Type == UrlType.Relative;
    }

    public bool IsLocal()
    {
        return this.Type == UrlType.Local;
    }

    public string ToAbsolute(string value)
    {
        if (this.IsAbsolute())
        {
            return this.Value;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Base URL is required for converting relative or local URLs to absolute.",
                nameof(value));
        }

        var normalizedBaseUrl = Normalize(value);

        return this.IsRelative() ? $"{normalizedBaseUrl}{this.Value}" : $"{normalizedBaseUrl}/{this.Value}";
    }

    public override string ToString()
    {
        return this.Value;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
        yield return this.Type;
    }

    private static string Normalize(string value)
    {
        return value?.Trim()?.TrimEnd('/');
    }

    private static UrlType DetermineType(string value)
    {
        if (AbsoluteUrlRegex.IsMatch(value))
        {
            return UrlType.Absolute;
        }

        if (RelativeUrlRegex.IsMatch(value))
        {
            return UrlType.Relative;
        }

        if (LocalUrlRegex.IsMatch(value))
        {
            return UrlType.Local;
        }

        return UrlType.Invalid;
    }

    private static bool IsValid(string value)
    {
        return IsValid(value, DetermineType(value));
    }

    private static bool IsValid(string value, UrlType type)
    {
        return type switch
        {
            UrlType.Absolute => AbsoluteUrlRegex.IsMatch(value),
            UrlType.Relative => RelativeUrlRegex.IsMatch(value),
            UrlType.Local => LocalUrlRegex.IsMatch(value),
            _ => false
        };
    }

    [GeneratedRegex(
        @"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        "en-US")]
    private static partial Regex AbsoluteRegex();

    [GeneratedRegex(@"^(\/|\.\.?\/)([\w\.-]+\/?)*$", RegexOptions.Compiled)]
    private static partial Regex RelativeRegex();

    [GeneratedRegex(@"^[\w\.-]+(\/[\w\.-]+)*\/?$", RegexOptions.Compiled)]
    private static partial Regex LocalRegex();
}

public enum UrlType
{
    Absolute,
    Relative,
    Local,
    Invalid
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Domain\Model\VatNumber.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using System.Text.RegularExpressions;

[DebuggerDisplay("CountryCode={CountryCode}, Number={Number}")]
public class VatNumber : ValueObject
{
    private static readonly Regex GeneralVatFormat = new(@"^[A-Z]{2}[0-9A-Z]+$", RegexOptions.Compiled);

    private static readonly Dictionary<string, Regex> CountryVatFormats = new()
    {
        ["DE"] = new Regex(@"^DE[0-9]{9}$", RegexOptions.Compiled),
        ["GB"] = new Regex(@"^GB([0-9]{9}([0-9]{3})?|[A-Z]{2}[0-9]{3})$", RegexOptions.Compiled),
        ["FR"] = new Regex(@"^FR[A-Z0-9]{2}[0-9]{9}$", RegexOptions.Compiled),
        ["US"] = new Regex(@"^US[0-9]{2}-[0-9]{7}$", RegexOptions.Compiled)
        // Add more countries as needed
    };

    private VatNumber() { } // Private constructor required by EF Core

    private VatNumber(string countryCode, string number)
    {
        this.CountryCode = countryCode;
        this.Number = number;
    }

    public string CountryCode { get; }

    public string Number { get; }

    public static implicit operator string(VatNumber vatNumber)
    {
        return vatNumber.ToString();
    }

    public static implicit operator VatNumber(string value)
    {
        return Create(value);
    }

    public static VatNumber Create(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null; //throw new DomainRuleException("VatNumber number cannot be empty.");
        }

        value = Normalize(value);
        var countryCode = value[..2];
        var number = value[2..];

        if (CountryVatFormats.TryGetValue(countryCode, out var regex))
        {
            if (!regex.IsMatch(value))
            {
                throw new DomainRuleException($"Invalid VAT/EIN number ({value}) format for country {countryCode}.");
            }

            return new VatNumber(countryCode, number);
        }

        if (GeneralVatFormat.IsMatch(value))
        {
            return new VatNumber(countryCode, number);
        }

        throw new DomainRuleException($"Invalid VAT number  ({value}) format.");
    }

    public static bool TryParse(string value, out VatNumber result)
    {
        try
        {
            result = Create(value);

            return true;
        }
        catch (DomainRuleException)
        {
            result = null;

            return false;
        }
    }

    public override string ToString()
    {
        if (this.CountryCode == "US")
        {
            return $"{this.CountryCode}{this.Number[..2]}-{this.Number[2..]}".Replace("--", "-");
        }

        return $"{this.CountryCode}{this.Number}".Replace("--", "-");
    }

    public bool IsValid()
    {
        // Here you could implement more complex validation logic,
        // such as checksum validation for certain country codes
        return true;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.CountryCode;
        yield return this.Number;
    }

    private static string Normalize(string value)
    {
        return value?.ToUpperInvariant().Replace(" ", string.Empty).Replace("--", "-");
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Domain\Model\Website.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using System.Text.RegularExpressions;

[DebuggerDisplay("Value={Value}")]
public partial class Website : ValueObject
{
    private Website() { } // Private constructor required by EF Core

    private Website(string website)
    {
        this.Value = website;
    }

    public string Value { get; private set; }

    public static implicit operator string(Website website)
    {
        return website?.Value;
        // allows a Website value to be implicitly converted to a string.
    }

    public static implicit operator Website(string value)
    {
        return Create(value);
    }

    public static Website Create(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null; //throw new DomainRuleException("Website cannot be empty.");
        }

        value = Normalize(value);
        if (!IsValidRegex().IsMatch(value))
        {
            throw new DomainRuleException("Invalid website");
        }

        return new Website(value);
    }

    public override string ToString()
    {
        return this.Value;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }

    private static string Normalize(string value)
    {
        value = value?.Trim().ToLowerInvariant() ?? string.Empty;
        if (value?.StartsWith("http://") != false || value.StartsWith("https://"))
        {
            return value;
        }

        return "https://" + value;
    }

    [GeneratedRegex(
        @"^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$",
        RegexOptions.IgnoreCase,
        "en-US")]
    private static partial Regex IsValidRegex();
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Commands\BookCreateCommand.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class BookCreateCommand(string tenantId, BookModel model)
    : CommandRequestBase<Result<Book>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public BookModel Model { get; } = model;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<BookCreateCommand>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must not be empty or invalid.");
            this.RuleFor(c => c.TenantId)
                .Must((command, tenantId) => tenantId == command.Model.TenantId)
                .WithMessage("Must be equal to Model.TenantId.");
            this.RuleFor(c => c.Model).SetValidator(new ModelValidator());
        }

        private class ModelValidator : AbstractValidator<BookModel>
        {
            public ModelValidator()
            {
                this.RuleFor(m => m).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.Id).MustBeDefaultOrEmptyGuid().WithMessage("Must be empty.");
                this.RuleFor(m => m.Title).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.Sku).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.Isbn).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.Publisher).NotNull().WithMessage("Must not be empty.");
                this.RuleFor(m => m.Publisher.Id).MustNotBeDefaultOrEmptyGuid().WithMessage("Must not be empty.");
                this.RuleFor(m => m.PublishedDate).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.Authors)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage("Must not be empty.")
                    .ForEach(c => c.SetValidator(new BookAuthorValidator()));
                this.RuleFor(m => m.Categories)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage("Must not be empty.")
                    .ForEach(c => c.SetValidator(new BookCategoryValidator()));
            }
        }

        private class BookAuthorValidator : AbstractValidator<BookAuthorModel>
        {
            public BookAuthorValidator()
            {
                this.RuleFor(c => c.Id).MustNotBeDefaultOrEmptyGuid().WithMessage("Must not be empty or invalid.");
            }
        }

        private class BookCategoryValidator : AbstractValidator<BookCategoryModel>
        {
            public BookCategoryValidator()
            {
                this.RuleFor(c => c.Id).MustNotBeDefaultOrEmptyGuid().WithMessage("Must not be empty or invalid.");
            }
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Commands\BookCreateCommandHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using Money = BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain.Money;

public class BookCreateCommandHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Book> bookRepository,
    IGenericRepository<Author> authorRepository,
    IGenericRepository<Publisher> publisherRepository)
    : CommandHandlerBase<BookCreateCommand, Result<Book>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Book>>> Process(
        BookCreateCommand command,
        CancellationToken cancellationToken)
    {
        var publisher = await publisherRepository.FindOneAsync(
                PublisherId.Create(command.Model.Publisher.Id),
                cancellationToken: cancellationToken)
            .AnyContext(); // TODO: Check if publisher exists

        var book = Book.Create(
            TenantId.Create(command.TenantId),
            command.Model.Title,
            command.Model.Edition,
            command.Model.Description,
            ProductSku.Create(command.Model.Sku),
            BookIsbn.Create(command.Model.Isbn),
            Money.Create(command.Model.Price),
            publisher,
            command.Model.PublishedDate);

        foreach (var bookAuthorModel in command.Model.Authors)
        {
            var author = await authorRepository.FindOneAsync(
                    AuthorId.Create(bookAuthorModel.Id),
                    cancellationToken: cancellationToken)
                .AnyContext(); // TODO: Check if author exists

            book.AssignAuthor(author, bookAuthorModel.Position);
        }

        await DomainRules.ApplyAsync([
                BookRules.IsbnMustBeUnique(bookRepository, book)
            ],
            cancellationToken);

        await bookRepository.InsertAsync(book, cancellationToken).AnyContext();

        return CommandResponse.Success(book);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Commands\CustomerCreateCommand.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class CustomerCreateCommand(string tenantId, CustomerModel model)
    : CommandRequestBase<Result<Customer>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public CustomerModel Model { get; } = model;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<CustomerCreateCommand>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must not be empty or invalid.");
            this.RuleFor(c => c.TenantId)
                .Must((command, tenantId) => tenantId == command.Model.TenantId)
                .WithMessage("Must be equal to Model.TenantId.");
            this.RuleFor(c => c.Model).SetValidator(new ModelValidator());
        }

        private class ModelValidator : AbstractValidator<CustomerModel>
        {
            public ModelValidator()
            {
                this.RuleFor(m => m).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.Id).MustBeDefaultOrEmptyGuid().WithMessage("Must be empty.");
                this.RuleFor(m => m.PersonName).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.PersonName.Parts).NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.Email).NotNull().NotEmpty().WithMessage("Must not be empty.");
            }
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Commands\CustomerCreateCommandHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class CustomerCreateCommandHandler(ILoggerFactory loggerFactory, IGenericRepository<Customer> repository)
    : CommandHandlerBase<CustomerCreateCommand, Result<Customer>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Customer>>> Process(
        CustomerCreateCommand command,
        CancellationToken cancellationToken)
    {
        var customer = Customer.Create(
            TenantId.Create(command.TenantId),
            PersonFormalName.Create(
                command.Model.PersonName.Parts,
                command.Model.PersonName.Title,
                command.Model.PersonName.Suffix),
            EmailAddress.Create(command.Model.Email),
            Address.Create(
                command.Model.Address.Name,
                command.Model.Address.Line1,
                command.Model.Address.Line2,
                command.Model.Address.PostalCode,
                command.Model.Address.City,
                command.Model.Address.Country));

        await DomainRules.ApplyAsync([
                CustomerRules.EmailMustBeUnique(repository, customer)
            ],
            cancellationToken);

        await repository.InsertAsync(customer, cancellationToken).AnyContext();

        return CommandResponse.Success(customer);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Commands\CustomerDeleteCommand.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class CustomerDeleteCommand(string tenantId, string id) : CommandRequestBase<Result<Customer>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public string Id { get; } = id;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<CustomerDeleteCommand>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
            this.RuleFor(c => c.Id).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Commands\CustomerDeleteCommandHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class CustomerDeleteCommandHandler(ILoggerFactory loggerFactory, IGenericRepository<Customer> repository)
    : CommandHandlerBase<CustomerDeleteCommand, Result<Customer>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Customer>>> Process(
        CustomerDeleteCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId =
            TenantId.Create(command.TenantId); // TODO: use in findone query or check later > notfoundexception
        var customerResult = await repository.FindOneResultAsync(
            CustomerId.Create(command.Id),
            cancellationToken: cancellationToken);

        if (customerResult.IsFailure)
        {
            return CommandResponse.For(customerResult);
        }

        await DomainRules.ApplyAsync([], cancellationToken);

        await repository.DeleteAsync(customerResult.Value, cancellationToken).AnyContext();

        return CommandResponse.Success(customerResult.Value);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Commands\CustomerUpdateCommand.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class CustomerUpdateCommand(string tenantId, CustomerModel model) : CommandRequestBase<Result<Customer>>,
    ITenantAware
{
    public string TenantId { get; } = tenantId;

    public CustomerModel Model { get; } = model;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<CustomerUpdateCommand>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must not be empty or invalid.");
            this.RuleFor(c => c.TenantId)
                .Must((command, tenantId) => tenantId == command.Model.TenantId)
                .WithMessage("Must be equal to Model.TenantId.");
            this.RuleFor(c => c.Model).SetValidator(new ModelValidator());
        }

        private class ModelValidator : AbstractValidator<CustomerModel>
        {
            public ModelValidator()
            {
                this.RuleFor(m => m).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.Id).MustNotBeDefaultOrEmptyGuid().WithMessage("Must not be empty.");
                this.RuleFor(m => m.PersonName).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.PersonName.Parts).NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.Email).NotNull().NotEmpty().WithMessage("Must not be empty.");
            }
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Commands\CustomerUpdateCommandHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class CustomerUpdateCommandHandler(ILoggerFactory loggerFactory, IGenericRepository<Customer> repository)
    : CommandHandlerBase<CustomerUpdateCommand, Result<Customer>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Customer>>> Process(
        CustomerUpdateCommand command,
        CancellationToken cancellationToken)
    {
        var customerResult = await repository.FindOneResultAsync(
            CustomerId.Create(command.Model.Id),
            cancellationToken: cancellationToken);

        if (customerResult.IsFailure)
        {
            return CommandResponse.For(customerResult);
        }

        await DomainRules.ApplyAsync([], cancellationToken);

        customerResult.Value.SetName(
            PersonFormalName.Create(
                command.Model.PersonName.Parts,
                command.Model.PersonName.Title,
                command.Model.PersonName.Suffix));
        customerResult.Value.SetAddress(
            Address.Create(
                command.Model.Address.Name,
                command.Model.Address.Line1,
                command.Model.Address.Line2,
                command.Model.Address.PostalCode,
                command.Model.Address.City,
                command.Model.Address.Country));

        await repository.UpsertAsync(customerResult.Value, cancellationToken).AnyContext();

        return CommandResponse.Success(customerResult.Value);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Messages\StockCreatedMessageHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application.Messages;

using BridgingIT.DevKit.Application.Messaging;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockCreatedMessageHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Book> repository)
    : MessageHandlerBase<StockCreatedMessage>(loggerFactory)
{
    public override async Task Handle(StockCreatedMessage message, CancellationToken cancellationToken)
    {
        var book = (await repository.FindAllResultAsync(
            new Specification<Book>(e => e.TenantId == message.TenantId && e.Sku == message.Sku),
            cancellationToken: cancellationToken)).Value.FirstOrDefault();

        if (book == null)
        {
            // TODO: log book not found by sku
            return;
        }

        book.SetStock(message.QuantityOnHand, message.QuantityReserved);
        await repository.UpdateAsync(book, cancellationToken);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Messages\StockUpdatedMessageHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application.Messages;

using BridgingIT.DevKit.Application.Messaging;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockUpdatedMessageHandler(ILoggerFactory loggerFactory, IGenericRepository<Book> repository)
    : MessageHandlerBase<StockUpdatedMessage>(loggerFactory)
{
    //private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

    public override async Task Handle(StockUpdatedMessage message, CancellationToken cancellationToken)
    {
        // await Semaphore.WaitAsync(cancellationToken);
        //
        // try
        // {
            var book = (await repository.FindAllResultAsync(
                new Specification<Book>(e => e.TenantId == message.TenantId && e.Sku == message.Sku),
                cancellationToken: cancellationToken)).Value.FirstOrDefault();
            if (book == null)
            {
                // TODO: log book not found by sku
                return;
            }

            book.SetStock(message.QuantityOnHand, message.QuantityReserved);
            await repository.UpdateAsync(book, cancellationToken);
        // }
        // finally
        // {
        //     Semaphore.Release();
        // }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\AuthorFindAllQuery.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class AuthorFindAllQuery(string tenantId) : QueryRequestBase<Result<IEnumerable<Author>>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public class Validator : AbstractValidator<AuthorFindAllQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\AuthorFindAllQueryHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using BridgingIT.DevKit.Domain;

public class AuthorFindAllQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Author> repository)
    : QueryHandlerBase<AuthorFindAllQuery, Result<IEnumerable<Author>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Author>>>> Process(
        AuthorFindAllQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(query.TenantId);

        return QueryResponse.For(
            await repository.FindAllResultAsync(
                [new Specification<Author>(e => e.TenantId == tenantId)],
                cancellationToken: cancellationToken).AnyContext());
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\AuthorFindOneQuery.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class AuthorFindOneQuery(string tenantId, string bookId) : QueryRequestBase<Result<Author>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public string AuthorId { get; } = bookId;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<AuthorFindOneQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
            this.RuleFor(c => c.AuthorId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\AuthorFindOneQueryHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class AuthorFindOneQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Author> repository)
    : QueryHandlerBase<AuthorFindOneQuery, Result<Author>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<Author>>> Process(
        AuthorFindOneQuery query,
        CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await repository.FindOneResultAsync(AuthorId.Create(query.AuthorId), cancellationToken: cancellationToken)
                .AnyContext());
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\BookFindAllForCategoryQuery.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class BookFindAllForCategoryQuery(string tenantId, string categoryId)
    : QueryRequestBase<Result<IEnumerable<Book>>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public string CategoryId { get; } = categoryId;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<BookFindAllForCategoryQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.CategoryId).NotNull().NotEmpty().WithMessage("Must not be empty.");
            this.RuleFor(c => c.CategoryId)
                .MustNotBeDefaultOrEmptyGuid()
                .WithMessage("Must be valid and not be empty.");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\BookFindAllForCategoryQueryHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using BridgingIT.DevKit.Domain;

public class BookFindAllForCategoryQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Book> repository)
    : QueryHandlerBase<BookFindAllForCategoryQuery, Result<IEnumerable<Book>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Book>>>> Process(
        BookFindAllForCategoryQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(query.TenantId);
        var categoryId = CategoryId.Create(query.CategoryId);

        return QueryResponse.For(
            await repository.FindAllResultAsync(
                    new Specification<Book>(e => e.Categories.Any(c => c.TenantId == tenantId && c.Id == categoryId)),
                    cancellationToken: cancellationToken)
                .AnyContext());
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\BookFindAllForPublisherQuery.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class BookFindAllForPublisherQuery(string tenantId, string publisherId)
    : QueryRequestBase<Result<IEnumerable<Book>>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public string PublisherId { get; } = publisherId;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<BookFindAllForPublisherQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.PublisherId).NotNull().NotEmpty().WithMessage("Must not be empty.");
            this.RuleFor(c => c.PublisherId)
                .MustNotBeDefaultOrEmptyGuid()
                .WithMessage("Must be valid and not be empty.");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\BookFindAllForPublisherQueryHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using BridgingIT.DevKit.Domain;

public class BookFindAllForPublisherQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Book> repository)
    : QueryHandlerBase<BookFindAllForPublisherQuery, Result<IEnumerable<Book>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Book>>>> Process(
        BookFindAllForPublisherQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(query.TenantId);
        var publisherId = PublisherId.Create(query.PublisherId);

        var result = await repository.FindAllResultAsync(
                    new Specification<Book>(e => e.TenantId == tenantId && e.Publisher.PublisherId == publisherId),
                    cancellationToken: cancellationToken)
                .AnyContext() ??
            throw new EntityNotFoundException();

        return QueryResponse.For(result);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\BookFindAllQuery.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class BookFindAllQuery(string tenantId) : QueryRequestBase<Result<IEnumerable<Book>>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public class Validator : AbstractValidator<BookFindAllQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\BookFindAllQueryHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using BridgingIT.DevKit.Domain;

public class BookFindAllQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Book> repository)
    : QueryHandlerBase<BookFindAllQuery, Result<IEnumerable<Book>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Book>>>> Process(
        BookFindAllQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(query.TenantId);

        return QueryResponse.For(
            await repository.FindAllResultAsync(
                    [new Specification<Book>(e => e.TenantId == tenantId)],
                    new FindOptions<Book> { Order = new OrderOption<Book>(e => e.Title) },
                    cancellationToken)
                .AnyContext());
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\BookFindAllRelatedQuery.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class BookFindAllRelatedQuery(string tenantId, string bookId) : QueryRequestBase<Result<IEnumerable<Book>>>,
    ITenantAware
{
    public string TenantId { get; } = tenantId;

    public string BookId { get; } = bookId;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<BookFindAllRelatedQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.BookId).NotNull().NotEmpty().WithMessage("Must not be empty.");
            this.RuleFor(c => c.BookId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\BookFindAllRelatedQueryHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class BookFindAllRelatedQueryHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Book> repository,
    ICatalogQueryService queryService)
    : QueryHandlerBase<BookFindAllRelatedQuery, Result<IEnumerable<Book>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Book>>>> Process(
        BookFindAllRelatedQuery query,
        CancellationToken cancellationToken)
    {
        var bookId = BookId.Create(query.BookId);
        var result = await repository.FindOneResultAsync(bookId, cancellationToken: cancellationToken).AnyContext() ??
            throw new EntityNotFoundException();

        return result.IsSuccess
            ? QueryResponse.For(await queryService.BookFindAllRelatedAsync(result.Value))
            : QueryResponse.For<IEnumerable<Book>>(result);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\BookFindOneQuery.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class BookFindOneQuery(string tenantId, string bookId) : QueryRequestBase<Result<Book>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public string BookId { get; } = bookId;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<BookFindOneQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
            this.RuleFor(c => c.BookId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\BookFindOneQueryHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class BookFindOneQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Book> repository)
    : QueryHandlerBase<BookFindOneQuery, Result<Book>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<Book>>> Process(
        BookFindOneQuery query,
        CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await repository.FindOneResultAsync(BookId.Create(query.BookId), cancellationToken: cancellationToken)
                .AnyContext());
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\CategoryFindAllQuery.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class CategoryFindAllQuery(string tenantId) : QueryRequestBase<Result<IEnumerable<Category>>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public bool Flatten { get; set; } = true;

    public class Validator : AbstractValidator<CategoryFindAllQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\CategoryFindAllQueryHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using BridgingIT.DevKit.Domain;

public class CategoryFindAllQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Category> repository)
    : QueryHandlerBase<CategoryFindAllQuery, Result<IEnumerable<Category>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Category>>>> Process(
        CategoryFindAllQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(query.TenantId);
        var categories = await repository.FindAllResultAsync(
            [new Specification<Category>(e => e.TenantId == tenantId)],
            cancellationToken: cancellationToken).AnyContext();

        this.PrintCategories(categories.Value.SafeNull().Where(c => c.Parent == null).OrderBy(e => e.Order));

        if (query.Flatten)
        {
            categories.Value = this.FlattenCategories(categories.Value);

            return QueryResponse.Success(categories.Value.SafeNull().AsEnumerable());
        }

        return QueryResponse.Success(
            categories.Value.SafeNull().Where(c => c.Parent == null).OrderBy(e => e.Order).AsEnumerable());
    }

    private IEnumerable<Category> FlattenCategories(IEnumerable<Category> categories)
    {
        return categories.SafeAny()
            ? categories.SelectMany(
                    c => new[]
                    {
                        c
                    }.Concat(c.Children))
                .ToList()
                .DistinctBy(c => c.Id)
            : [];
    }

    private void PrintCategories(IEnumerable<Category> categories, int level = 0)
    {
        foreach (var category in categories)
        {
            Console.WriteLine($"{new string(' ', level * 4)}[{category.Order}] {category.Title}");

            if (category.Children.SafeAny())
            {
                this.PrintCategories(category.Children.OrderBy(e => e.Title), level + 1);
            }
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\CategoryFindOneQuery.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class CategoryFindOneQuery(string tenantId, string bookId) : QueryRequestBase<Result<Category>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public string CategoryId { get; } = bookId;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<CategoryFindOneQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
            this.RuleFor(c => c.CategoryId)
                .MustNotBeDefaultOrEmptyGuid()
                .WithMessage("Must be valid and not be empty.");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\CategoryFindOneQueryHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class CategoryFindOneQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Category> repository)
    : QueryHandlerBase<CategoryFindOneQuery, Result<Category>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<Category>>> Process(
        CategoryFindOneQuery query,
        CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await repository.FindOneResultAsync(
                    CategoryId.Create(query.CategoryId),
                    cancellationToken: cancellationToken)
                .AnyContext());
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\CustomerFindAllQuery.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class CustomerFindAllQuery(string tenantId) : QueryRequestBase<Result<IEnumerable<Customer>>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public class Validator : AbstractValidator<CustomerFindAllQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\CustomerFindAllQueryHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using BridgingIT.DevKit.Domain;

public class CustomerFindAllQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Customer> repository)
    : QueryHandlerBase<CustomerFindAllQuery, Result<IEnumerable<Customer>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Customer>>>> Process(
        CustomerFindAllQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(query.TenantId);

        return QueryResponse.For(
            await repository.FindAllResultAsync(
                [new Specification<Customer>(e => e.TenantId == tenantId)],
                new FindOptions<Customer> { Order = new OrderOption<Customer>(e => e.Email) },
                cancellationToken: cancellationToken).AnyContext());
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\CustomerFindOneQuery.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class CustomerFindOneQuery(string tenantId, string customerId) : QueryRequestBase<Result<Customer>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public string CustomerId { get; } = customerId;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<CustomerFindOneQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
            this.RuleFor(c => c.CustomerId)
                .MustNotBeDefaultOrEmptyGuid()
                .WithMessage("Must be valid and not be empty.");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\CustomerFindOneQueryHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class CustomerFindOneQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Customer> repository)
    : QueryHandlerBase<CustomerFindOneQuery, Result<Customer>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<Customer>>> Process(
        CustomerFindOneQuery query,
        CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await repository.FindOneResultAsync(
                    CustomerId.Create(query.CustomerId),
                    cancellationToken: cancellationToken)
                .AnyContext());
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\PublisherFindAllQuery.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class PublisherFindAllQuery(string tenantId) : QueryRequestBase<Result<IEnumerable<Publisher>>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public class Validator : AbstractValidator<PublisherFindAllQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\PublisherFindAllQueryHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using BridgingIT.DevKit.Domain;

public class PublisherFindAllQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Publisher> repository)
    : QueryHandlerBase<PublisherFindAllQuery, Result<IEnumerable<Publisher>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Publisher>>>> Process(
        PublisherFindAllQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(query.TenantId);

        return QueryResponse.For(
            await repository.FindAllResultAsync(
                    [new Specification<Publisher>(e => e.TenantId == tenantId)],
                    new FindOptions<Publisher> { Order = new OrderOption<Publisher>(e => e.Name) },
                    cancellationToken)
                .AnyContext());
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\PublisherFindOneQuery.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class PublisherFindOneQuery(string tenantId, string bookId) : QueryRequestBase<Result<Publisher>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public string PublisherId { get; } = bookId;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<PublisherFindOneQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
            this.RuleFor(c => c.PublisherId)
                .MustNotBeDefaultOrEmptyGuid()
                .WithMessage("Must be valid and not be empty.");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Queries\PublisherFindOneQueryHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class PublisherFindOneQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Publisher> repository)
    : QueryHandlerBase<PublisherFindOneQuery, Result<Publisher>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<Publisher>>> Process(
        PublisherFindOneQuery query,
        CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await repository.FindOneResultAsync(
                    PublisherId.Create(query.PublisherId),
                    cancellationToken: cancellationToken)
                .AnyContext());
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Rules\BookIsbnMustBeUniqueRule.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class BookIsbnMustBeUniqueRule(IGenericRepository<Book> repository, Book book) : DomainRuleBase
{
    public override string Message
        => "Book ISBN should be unique";

    public override async Task<bool> ApplyAsync(CancellationToken cancellationToken = default)
    {
        return !(await repository.FindAllAsync(
            BookSpecifications.ForIsbn(book.TenantId, book.Isbn),
            cancellationToken: cancellationToken)).SafeAny();
    }
}

public static class BookRules
{
    public static IDomainRule IsbnMustBeUnique(IGenericRepository<Book> repository, Book book)
    {
        return new BookIsbnMustBeUniqueRule(repository, book);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Rules\CustomerEmailMustBeUniqueRule.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class CustomerEmailMustBeUniqueRule(IGenericRepository<Customer> repository, Customer customer) : DomainRuleBase
{
    public override string Message
        => "Customer email should be unique";

    public override async Task<bool> ApplyAsync(CancellationToken cancellationToken = default)
    {
        return !(await repository.FindAllAsync(
            CustomerSpecifications.ForEmail(customer.TenantId, customer.Email),
            cancellationToken: cancellationToken)).SafeAny();
    }
}

public static class CustomerRules
{
    public static IDomainRule EmailMustBeUnique(IGenericRepository<Customer> repository, Customer customer)
    {
        return new CustomerEmailMustBeUniqueRule(repository, customer);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Services\ICatalogQueryService.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public interface ICatalogQueryService
{
    /// <summary>
    ///     Retrieves a collection of related books based on the provided book and limit.
    /// </summary>
    /// <param name="book">The book to find related books for.</param>
    /// <param name="limit">The maximum number of related books to retrieve (default is 5).</param>
    Task<Result<IEnumerable<Book>>> BookFindAllRelatedAsync(Book book, int limit = 5);
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application\Tasks\CatalogDomainSeederTask.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using Microsoft.Extensions.Logging.Abstractions;

public class CatalogDomainSeederTask(
    ILoggerFactory loggerFactory,
    IGenericRepository<Tag> tagRepository,
    IGenericRepository<Customer> customerRepository,
    IGenericRepository<Author> authorRepository,
    IGenericRepository<Publisher> publisherRepository,
    IGenericRepository<Category> categoryRepository,
    IGenericRepository<Book> bookRepository) : IStartupTask
{
    private readonly ILogger<CatalogDomainSeederTask> logger =
        loggerFactory?.CreateLogger<CatalogDomainSeederTask>() ??
        NullLoggerFactory.Instance.CreateLogger<CatalogDomainSeederTask>();

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("{LogKey} seed catalog (task={StartupTaskType})", "IFR", this.GetType().PrettyName());

        TenantId[] tenantIds =
        [
            TenantIdFactory.CreateForName("Tenant_AcmeBooks"), TenantIdFactory.CreateForName("Tenant_TechBooks")
        ];
        var tags = await this.SeedTags(tagRepository, tenantIds);
        var customers = await this.SeedCustomers(customerRepository, tenantIds);
        var authors = await this.SeedAuthors(authorRepository, tenantIds, tags);
        var publishers = await this.SeedPublishers(publisherRepository, tenantIds);
        var categories = await this.SeedCategories(categoryRepository, tenantIds);
        var books = await this.SeedBooks(bookRepository, tenantIds, tags, categories, publishers, authors);
    }

    private async Task<Tag[]> SeedTags(IGenericRepository<Tag> repository, TenantId[] tenantIds)
    {
        this.logger.LogInformation("{LogKey} seed tags (task={StartupTaskType})", "IFR", this.GetType().PrettyName());

        var entities = CatalogSeedEntities.Tags.Create(tenantIds);

        foreach (var entity in entities)
        {
            if (!await repository.ExistsAsync(entity.Id))
            {
                await repository.InsertAsync(entity);
            }
            else
            {
                return entities;
            }
        }

        return entities;
    }

    private async Task<Customer[]> SeedCustomers(IGenericRepository<Customer> repository, TenantId[] tenantIds)
    {
        this.logger.LogInformation("{LogKey} seed customers (task={StartupTaskType})", "IFR", this.GetType().PrettyName());

        var entities = CatalogSeedEntities.Customers.Create(tenantIds);

        foreach (var entity in entities)
        {
            if (!await repository.ExistsAsync(entity.Id))
            {
                entity.AuditState.SetCreated("seed", nameof(CatalogDomainSeederTask));
                await repository.InsertAsync(entity);
            }
            else
            {
                return entities;
            }
        }

        return entities;
    }

    private async Task<Author[]> SeedAuthors(IGenericRepository<Author> repository, TenantId[] tenantIds, Tag[] tags)
    {
        this.logger.LogInformation("{LogKey} seed authors (task={StartupTaskType})", "IFR", this.GetType().PrettyName());

        var entities = CatalogSeedEntities.Authors.Create(tenantIds, tags);

        foreach (var entity in entities)
        {
            if (!await repository.ExistsAsync(entity.Id))
            {
                entity.AuditState.SetCreated("seed", nameof(CatalogDomainSeederTask));
                await repository.InsertAsync(entity);
            }
            else
            {
                return entities;
            }
        }

        return entities;
    }

    private async Task<Publisher[]> SeedPublishers(IGenericRepository<Publisher> repository, TenantId[] tenantIds)
    {
        this.logger.LogInformation("{LogKey} seed publishers (task={StartupTaskType})", "IFR", this.GetType().PrettyName());

        var entities = CatalogSeedEntities.Publishers.Create(tenantIds);

        foreach (var entity in entities)
        {
            if (!await repository.ExistsAsync(entity.Id))
            {
                entity.AuditState.SetCreated("seed", nameof(CatalogDomainSeederTask));
                await repository.InsertAsync(entity);
            }
            else
            {
                return entities;
            }
        }

        return entities;
    }

    private async Task<Category[]> SeedCategories(IGenericRepository<Category> repository, TenantId[] tenantIds)
    {
        this.logger.LogInformation("{LogKey} seed categories (task={StartupTaskType})", "IFR", this.GetType().PrettyName());

        var entities = CatalogSeedEntities.Categories.Create(tenantIds);

        foreach (var entity in entities)
        {
            if (!await repository.ExistsAsync(entity.Id))
            {
                entity.AuditState.SetCreated("seed", nameof(CatalogDomainSeederTask));
                await repository.InsertAsync(entity);
            }
            else
            {
                return entities;
            }
        }

        return entities;
    }

    private async Task<Book[]> SeedBooks(
        IGenericRepository<Book> repository,
        TenantId[] tenantIds,
        Tag[] tags,
        Category[] categories,
        Publisher[] publishers,
        Author[] authors)
    {
        this.logger.LogInformation("{LogKey} seed books (task={StartupTaskType})", "IFR", this.GetType().PrettyName());

        var entities = CatalogSeedEntities.Books.Create(tenantIds, tags, categories, publishers, authors);

        foreach (var entity in entities)
        {
            if (!await repository.ExistsAsync(entity.Id))
            {
                entity.AuditState.SetCreated("seed", nameof(CatalogDomainSeederTask));
                await repository.InsertAsync(entity);
            }
            else
            {
                return entities;
            }
        }

        return entities;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application.Contracts\Messages\BookCreatedMessage.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class BookCreatedMessage { }

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application.Contracts\Models\AuthorModel.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class AuthorModel
{
    public string Id { get; set; }

    public string TenantId { get; set; }

    public string PersonName { get; set; }

    public string Biography { get; set; }

    public IEnumerable<AuthorBookModel> Books { get; set; }

    public TagModel[] Tags { get; set; }

    public string Version { get; set; }
}

public class AuthorBookModel
{
    public string Id { get; set; }

    public string Title { get; set; }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application.Contracts\Models\BookModel.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class BookModel
{
    public string Id { get; set; }

    public string TenantId { get; set; }

    public string Title { get; set; }

    public string Edition { get; set; }

    public string Description { get; set; }

    public string Sku { get; set; }

    public string Isbn { get; set; }

    public decimal Price { get; set; }

    public BookPublisherModel Publisher { get; set; }

    public DateOnly PublishedDate { get; set; }

    public IEnumerable<string> Keywords { get; set; }

    public IEnumerable<BookAuthorModel> Authors { get; set; }

    public BookCategoryModel[] Categories { get; set; }

    public BookChapterModel[] Chapters { get; set; }

    public TagModel[] Tags { get; set; }

    public string Version { get; set; }
}

public class BookChapterModel
{
    public string Id { get; set; }

    public string Title { get; set; }

    public int Number { get; set; }

    public string Content { get; set; }
}

public class BookPublisherModel
{
    public string Id { get; set; }

    public string Name { get; set; }
}

public class BookAuthorModel
{
    public string Id { get; set; }

    public string Name { get; set; }

    public int Position { get; set; }
}

public class BookCategoryModel
{
    public string Id { get; set; }

    public string Title { get; set; }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application.Contracts\Models\CategoryModel.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class CategoryModel
{
    public string Id { get; set; }

    public string TenantId { get; set; }

    public int Order { get; set; }

    public string Title { get; set; }

    public string ParentId { get; set; }

    public CategoryModel[] Children { get; set; }

    public string Version { get; set; }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application.Contracts\Models\CustomerModel.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Application;

public class CustomerModel
{
    public string Id { get; set; }

    public string TenantId { get; set; }

    public PersonFormalNameModel PersonName { get; set; }

    public AddressModel Address { get; set; }

    public string Email { get; set; }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application.Contracts\Models\PublisherModel.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class PublisherModel
{
    public string Id { get; set; }

    public string TenantId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Version { get; set; }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Application.Contracts\Models\TagModel.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class TagModel
{
    public string Id { get; set; }

    public string TenantId { get; set; }

    public string Name { get; set; }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Infrastructure\EntityFramework\CatalogDbContext.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Infrastructure;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using BridgingIT.DevKit.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options)
    : ModuleDbContextBase(options), IDocumentStoreContext, IOutboxDomainEventContext, IOutboxMessageContext
{
    public DbSet<Book> Books { get; set; }

    public DbSet<Category> Categories { get; set; }

    public DbSet<Author> Authors { get; set; }

    public DbSet<Customer> Customers { get; set; }

    public DbSet<StorageDocument> StorageDocuments { get; set; }

    public DbSet<OutboxDomainEvent> OutboxDomainEvents { get; set; }

    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //    modelBuilder.ApplyConfigurationsFromAssembly(typeof(TagEntityTypeConfiguration).Assembly);

    //    base.OnModelCreating(modelBuilder); // applies the other EntityTypeConfigurations
    //}
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Infrastructure\EntityFramework\CatalogQueryService.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Infrastructure;

using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using Microsoft.EntityFrameworkCore;

public class CatalogQueryService(IGenericRepository<Book> bookRepository, CatalogDbContext dbContext)
    : ICatalogQueryService
{
    /// <summary>
    ///     Retrieves a collection of related books based on the provided book.
    /// </summary>
    /// <param name="book">The book to find related books for.</param>
    /// <param name="limit">The maximum number of related books to retrieve (default is 5).</param>
    public async Task<Result<IEnumerable<Book>>> BookFindAllRelatedAsync(Book book, int limit = 5)
    {
        if (book == null)
        {
            return Result<IEnumerable<Book>>.Failure();
        }

        var bookKeywords = book.Keywords.SafeNull().Select(k => k.Text).ToList();
        var relatedBookIds = await dbContext.Books.SelectMany(e => e.Keywords)
            .Where(ki => bookKeywords.Contains(ki.Text) && ki.BookId != book.Id)
            .GroupBy(ki => ki.BookId)
            .OrderByDescending(g => g.Count())
            .Take(limit)
            .Select(g => g.Key)
            .ToListAsync();

        return await bookRepository.FindAllResultAsync(
            new Specification<Book>(e => e.TenantId == book.TenantId && relatedBookIds.Contains(e.Id)));
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Infrastructure\EntityFramework\CatalogRepository.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Infrastructure;

using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using BridgingIT.DevKit.Infrastructure.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class BookRepository : EntityFrameworkReadOnlyGenericRepository<Book>
{
    public BookRepository(EntityFrameworkRepositoryOptions options)
        : base(options) { }

    public BookRepository(
        Builder<EntityFrameworkRepositoryOptionsBuilder, EntityFrameworkRepositoryOptions> optionsBuilder)
        : base(optionsBuilder) { }

    public BookRepository(ILoggerFactory loggerFactory, DbContext context)
        : base(loggerFactory, context) { }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application\Commands\StockCreateCommand.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockCreateCommand(string tenantId, StockModel model) : CommandRequestBase<Result<Stock>>,
    ITenantAware
{
    public string TenantId { get; } = tenantId;

    public StockModel Model { get; } = model;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<StockCreateCommand>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must not be empty or invalid.");
            this.RuleFor(c => c.TenantId)
                .Must((command, tenantId) => tenantId == command.Model.TenantId)
                .WithMessage("Must be equal to Model.TenantId.");
            this.RuleFor(c => c.Model).SetValidator(new ModelValidator());
        }

        private class ModelValidator : AbstractValidator<StockModel>
        {
            public ModelValidator()
            {
                this.RuleFor(m => m).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.Id).MustBeDefaultOrEmptyGuid().WithMessage("Must be empty.");
            }
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application\Commands\StockCreateCommandHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;
using Money = BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain.Money;

public class StockCreateCommandHandler(ILoggerFactory loggerFactory, IGenericRepository<Stock> repository)
    : CommandHandlerBase<StockCreateCommand, Result<Stock>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Stock>>> Process(
        StockCreateCommand command,
        CancellationToken cancellationToken)
    {
        var stock = Stock.Create(
            TenantId.Create(command.TenantId),
            ProductSku.Create(command.Model.Sku),
            command.Model.QuantityOnHand,
            command.Model.ReorderThreshold,
            command.Model.ReorderQuantity,
            Money.Create(command.Model.UnitCost),
            StorageLocation.Create("A", "1", "1"));
        // -> register StockCreatedDomainEvent -> Handler -> publish StockCreatedMessage

        await DomainRules.ApplyAsync(
            [
                StockRules.SkuMustBeUnique(repository, stock)
            ],
            cancellationToken);

        await repository.InsertAsync(stock, cancellationToken).AnyContext(); // -> dispatch DomainEvents

        return CommandResponse.Success(stock);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application\Commands\StockMovementApplyCommand.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockMovementApplyCommand(string tenantId, string stockId, StockMovementModel model)
    : CommandRequestBase<Result<Stock>>,
    ITenantAware
{
    public string TenantId { get; } = tenantId;

    public string StockId { get; } = stockId;

    public StockMovementModel Model { get; } = model;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<StockMovementApplyCommand>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must not be empty or invalid.");
            this.RuleFor(c => c.Model).SetValidator(new ModelValidator());
        }

        private class ModelValidator : AbstractValidator<StockMovementModel>
        {
            public ModelValidator()
            {
                this.RuleFor(m => m).NotNull().NotEmpty().WithMessage("Must not be empty.");
            }
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application\Commands\StockMovementApplyCommandHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockMovementApplyCommandHandler(ILoggerFactory loggerFactory, IGenericRepository<Stock> repository)
    : CommandHandlerBase<StockMovementApplyCommand, Result<Stock>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Stock>>> Process(
        StockMovementApplyCommand command,
        CancellationToken cancellationToken)
    {
        var stockResult = await repository.FindOneResultAsync(
            StockId.Create(command.StockId),
            cancellationToken: cancellationToken);

        if (stockResult.IsFailure)
        {
            return CommandResponse.For(stockResult);
        }

        if (command.Model.Type == StockMovementType.Addition.Id)
        {
            stockResult.Value.AddStock(command.Model.Quantity);
            // -> register StockUpdatedDomainEvent -> Handler -> publish StockUpdatedMessage
        }
        else if (command.Model.Type == StockMovementType.Removal.Id)
        {
            stockResult.Value.RemoveStock(command.Model.Quantity);
            // -> register StockUpdatedDomainEvent -> Handler -> publish StockUpdatedMessage
        }
        else
        {
            throw new DomainRuleException("Stock movement type not supported");
        }

        await DomainRules.ApplyAsync([], cancellationToken);

        await repository.UpdateAsync(stockResult.Value, cancellationToken).AnyContext(); // -> dispatch DomainEvents

        return CommandResponse.Success(stockResult.Value);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application\Commands\StockSnapshotCreateCommand.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockSnapshotCreateCommand(string tenantId, string stockId)
    : CommandRequestBase<Result<StockSnapshot>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public string StockId { get; } = stockId;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<StockSnapshotCreateCommand>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must not be empty or invalid.");
            this.RuleFor(c => c.StockId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must not be empty or invalid.");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application\Commands\StockSnapshotCreateCommandHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockSnapshotCreateCommandHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Stock> stockRepository,
    IGenericRepository<StockSnapshot> stockSnapshotRepository)
    : CommandHandlerBase<StockSnapshotCreateCommand, Result<StockSnapshot>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<StockSnapshot>>> Process(
        StockSnapshotCreateCommand command,
        CancellationToken cancellationToken)
    {
        var stockResult = await stockRepository.FindOneResultAsync(
            StockId.Create(command.StockId),
            cancellationToken: cancellationToken);

        if (stockResult.IsFailure)
        {
            return CommandResponse.For<StockSnapshot>(stockResult);
        }

        await DomainRules.ApplyAsync([], cancellationToken);

        var stockSnapshot = StockSnapshot.Create(
            stockResult.Value.TenantId,
            stockResult.Value.Id,
            stockResult.Value.Sku,
            stockResult.Value.QuantityOnHand,
            stockResult.Value.QuantityReserved,
            stockResult.Value.UnitCost,
            stockResult.Value.Location,
            DateTimeOffset.UtcNow);

        await stockSnapshotRepository.InsertAsync(stockSnapshot, cancellationToken).AnyContext();

        return CommandResponse.Success(stockSnapshot);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application\Events\StockCreatedDomainEventMessagePublisher.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application.Events;

using BridgingIT.DevKit.Application.Messaging;

public class StockCreatedDomainEventMessagePublisher(
    ILoggerFactory loggerFactory,
    IMessageBroker messageBroker)
    : DomainEventHandlerBase<StockCreatedDomainEvent>(loggerFactory)
{
    public override bool CanHandle(StockCreatedDomainEvent @event)
    {
        return true;
    }

    public override async Task Process(StockCreatedDomainEvent @event, CancellationToken cancellationToken)
    {
        await messageBroker.Publish(
            new StockCreatedMessage
            {
                TenantId = @event.TenantId,
                StockId = @event.StockId,
                Sku = @event.Sku,
                QuantityOnHand = @event.QuantityOnHand,
                QuantityReserved = @event.QuantityReserved,
                UnitCost = @event.UnitCost
            },
            cancellationToken);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application\Events\StockUpdatedDomainEventMessagePublisher.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application.Events;

using BridgingIT.DevKit.Application.Messaging;

public class StockUpdatedDomainEventMessagePublisher(
    ILoggerFactory loggerFactory,
    IMessageBroker messageBroker)
    : DomainEventHandlerBase<StockUpdatedDomainEvent>(loggerFactory)
{
    public override bool CanHandle(StockUpdatedDomainEvent @event)
    {
        return true;
    }

    public override async Task Process(StockUpdatedDomainEvent @event, CancellationToken cancellationToken)
    {
        await messageBroker.Publish(
            new StockUpdatedMessage
            {
                TenantId = @event.TenantId,
                StockId = @event.StockId,
                Sku = @event.Sku,
                QuantityOnHand = @event.QuantityOnHand,
                QuantityReserved = @event.QuantityReserved,
                UnitCost = @event.UnitCost
            },
            cancellationToken);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application\Queries\StockFindAllQuery.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockFindAllQuery(string tenantId)
    : QueryRequestBase<Result<IEnumerable<Stock>>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public class Validator : AbstractValidator<StockFindAllQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application\Queries\StockFindAllQueryHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

using BridgingIT.DevKit.Domain;

public class StockFindAllQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Stock> repository)
    : QueryHandlerBase<StockFindAllQuery, Result<IEnumerable<Stock>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Stock>>>> Process(
        StockFindAllQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(query.TenantId);

        return QueryResponse.For(
            await repository.FindAllResultAsync(
                    [new Specification<Stock>(e => e.TenantId == tenantId)],
                    new FindOptions<Stock> { Order = new OrderOption<Stock>(e => e.Sku) },
                    cancellationToken)
                .AnyContext());
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application\Queries\StockFindOneQuery.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockFindOneQuery(string tenantId, string stockId)
    : QueryRequestBase<Result<Stock>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public string StockId { get; } = stockId;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<StockFindOneQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
            this.RuleFor(c => c.StockId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application\Queries\StockFindOneQueryHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockFindOneQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Stock> repository)
    : QueryHandlerBase<StockFindOneQuery, Result<Stock>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<Stock>>> Process(
        StockFindOneQuery query,
        CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await repository.FindOneResultAsync(
                    StockId.Create(query.StockId),
                    cancellationToken: cancellationToken)
                .AnyContext());
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application\Queries\StockFindTopQuery.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockFindTopQuery(string tenantId) : QueryRequestBase<Result<IEnumerable<Stock>>>,
    ITenantAware
{
    public string TenantId { get; } = tenantId;

    public DateTimeOffset Start { get; set; }

    public DateTimeOffset End { get; set; }

    public int Limit { get; set; }

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<StockFindTopQuery>
    {
        public Validator()
        {
            // this.RuleFor(c => c.BookId).NotNull().NotEmpty().WithMessage("Must not be empty.");
            // this.RuleFor(c => c.BookId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application\Queries\StockFindTopQueryHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockFindTopQueryHandler(
    ILoggerFactory loggerFactory,
    IInventoryQueryService queryService)
    : QueryHandlerBase<StockFindTopQuery, Result<IEnumerable<Stock>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Stock>>>> Process(
        StockFindTopQuery query,
        CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await queryService.StockFindTopAsync(query.Start, query.End, query.Limit));
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application\Queries\StockSnapshotFindAllQuery.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockSnapshotFindAllQuery(string tenantId, string stockId)
    : QueryRequestBase<Result<IEnumerable<StockSnapshot>>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public string StockId { get; } = stockId;

    public class Validator : AbstractValidator<StockSnapshotFindAllQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
            this.RuleFor(c => c.StockId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application\Queries\StockSnapshotFindAllQueryHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

using BridgingIT.DevKit.Domain;

public class StockSnapshotFindAllQueryHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<StockSnapshot> stocksnapshotRepository,
    IGenericRepository<Stock> stockRepository)
    : QueryHandlerBase<StockSnapshotFindAllQuery, Result<IEnumerable<StockSnapshot>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<StockSnapshot>>>> Process(
        StockSnapshotFindAllQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(query.TenantId);
        var stockResult = await stockRepository.FindOneResultAsync(
            StockId.Create(query.StockId),
            cancellationToken: cancellationToken);

        if (stockResult.IsFailure)
        {
            return QueryResponse.For<IEnumerable<StockSnapshot>>(stockResult);
        }

        return QueryResponse.For(
            await stocksnapshotRepository.FindAllResultAsync(
                    [new Specification<StockSnapshot>(e => e.TenantId == tenantId && e.StockId == query.StockId)],
                    new FindOptions<StockSnapshot> { Order = new OrderOption<StockSnapshot>(e => e.Timestamp) },
                    cancellationToken)
                .AnyContext());
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application\Queries\StockSnapshotFindOneQuery.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockSnapshotFindOneQuery(string tenantId, string stockId, string stockSnapshotId)
    : QueryRequestBase<Result<StockSnapshot>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public string StockId { get; } = stockId;

    public string StockSnapshotId { get; } = stockSnapshotId;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<StockSnapshotFindOneQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
            this.RuleFor(c => c.StockId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
            this.RuleFor(c => c.StockSnapshotId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application\Queries\StockSnapshotFindOneQueryHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockSnapshotFindOneQueryHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<StockSnapshot> stocksnapshotRepository,
    IGenericRepository<Stock> stockRepository)
    : QueryHandlerBase<StockSnapshotFindOneQuery, Result<StockSnapshot>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<StockSnapshot>>> Process(
        StockSnapshotFindOneQuery query,
        CancellationToken cancellationToken)
    {
        var stockResult = await stockRepository.FindOneResultAsync(
            StockId.Create(query.StockId),
            cancellationToken: cancellationToken);

        if (stockResult.IsFailure)
        {
            return QueryResponse.For<StockSnapshot>(stockResult);
        }

        return QueryResponse.For(
            await stocksnapshotRepository.FindOneResultAsync(
                    StockSnapshotId.Create(query.StockSnapshotId),
                    cancellationToken: cancellationToken)
                .AnyContext());
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application\Rules\SkuMustBeUniqueRule.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class SkuMustBeUniqueRule(IGenericRepository<Stock> repository, Stock stock) : DomainRuleBase
{
    public override string Message => "Stock Sku should be unique";

    public override async Task<bool> ApplyAsync(CancellationToken cancellationToken = default)
    {
        return !(await repository.FindAllAsync(
            StockSpecifications.ForSku(stock.TenantId, stock.Sku),
            cancellationToken: cancellationToken)).SafeAny();
    }
}

public static class StockRules
{
    public static IDomainRule SkuMustBeUnique(IGenericRepository<Stock> repository, Stock stock)
    {
        return new SkuMustBeUniqueRule(repository, stock);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application\Services\IInventoryQueryService.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public interface IInventoryQueryService
{
    /// <summary>
    ///     Retrieves the top stocks based on total movement quantity within a specified time period.
    /// </summary>
    /// <param name="start">The start of the time period.</param>
    /// <param name="end">The end of the time period.</param>
    /// <param name="limit">The maximum number of stocks to retrieve (default is 5).</param>
    Task<Result<IEnumerable<Stock>>> StockFindTopAsync(DateTimeOffset start, DateTimeOffset end, int limit = 5);
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application\Tasks\InventoryDomainSeederTask.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

using Microsoft.Extensions.Logging.Abstractions;

public class InventoryDomainSeederTask(
    ILoggerFactory loggerFactory,
    IGenericRepository<Stock> stockRepository,
    IGenericRepository<StockSnapshot> stockSnapshotRepository)
        : IStartupTask
{
    private readonly ILogger<InventoryDomainSeederTask> logger =
        loggerFactory?.CreateLogger<InventoryDomainSeederTask>() ??
        NullLoggerFactory.Instance.CreateLogger<InventoryDomainSeederTask>();

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("{LogKey} seed inventory (task={StartupTaskType})", "IFR", this.GetType().PrettyName());

        TenantId[] tenantIds =
        [
            TenantIdFactory.CreateForName("Tenant_AcmeBooks"), TenantIdFactory.CreateForName("Tenant_TechBooks")
        ];
        var stocks = await this.SeedStocks(stockRepository, tenantIds);
        var stockSnapshots = await this.SeedStocksSnapshots(stockSnapshotRepository, stocks, tenantIds);
    }

    private async Task<Stock[]> SeedStocks(IGenericRepository<Stock> repository, TenantId[] tenantIds)
    {
        this.logger.LogInformation("{LogKey} seed stocks (task={StartupTaskType})", "IFR", this.GetType().PrettyName());

        var entities = InventorySeedEntities.Stocks.Create(tenantIds);

        foreach (var entity in entities)
        {
            if (!await repository.ExistsAsync(entity.Id))
            {
                entity.AuditState.SetCreated("seed", nameof(InventoryDomainSeederTask));
                await repository.InsertAsync(entity);
            }
            else
            {
                return entities;
            }
        }

        return entities;
    }

    private async Task<StockSnapshot[]> SeedStocksSnapshots(IGenericRepository<StockSnapshot> repository, Stock[] stocks, TenantId[] tenantIds)
    {
        this.logger.LogInformation("{LogKey} seed stocksnapshots (task={StartupTaskType})", "IFR", this.GetType().PrettyName());

        var entities = InventorySeedEntities.StockSnapshots.Create(tenantIds, stocks);

        foreach (var entity in entities)
        {
            if (!await repository.ExistsAsync(entity.Id))
            {
                entity.AuditState.SetCreated("seed", nameof(InventoryDomainSeederTask));
                await repository.InsertAsync(entity);
            }
            else
            {
                return entities;
            }
        }

        return entities;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application.Contracts\Messages\StockCreatedMessage.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

using BridgingIT.DevKit.Application.Messaging;

public class StockCreatedMessage : MessageBase
{
    public string TenantId { get; set; }

    public string StockId { get; set; }

    public string Sku { get; set; }

    public int QuantityOnHand { get; set; }

    public int QuantityReserved { get; set; }

    public decimal UnitCost { get; set; }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application.Contracts\Messages\StockSnapshotCreatedMessage.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockSnapshotCreatedMessage
{
    public StockSnapshotModel Model { get; set; }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application.Contracts\Messages\StockUpdatedMessage.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

using BridgingIT.DevKit.Application.Messaging;

public class StockUpdatedMessage : MessageBase
{
    public string TenantId { get; set; }

    public string StockId { get; set; }

    public string Sku { get; set; }

    public int QuantityOnHand { get; set; }

    public int QuantityReserved { get; set; }

    public decimal UnitCost { get; set; }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application.Contracts\Models\StockModel.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockModel
{
    public string Id { get; set; }

    public string TenantId { get; set; }

    public string Sku { get; set; }

    public int QuantityOnHand { get; set; }

    public int QuantityReserved { get; set; }

    public int ReorderThreshold { get; set; }

    public int ReorderQuantity { get; set; }

    public decimal UnitCost { get; set; }

    public string Location { get; set; }

    public DateTimeOffset? LastRestockedAt { get; set; }

    public StockMovementModel[] Movements { get; set; }

    public StockAdjustmentModel[] Adjustments { get; set; }
}

public class StockMovementModel
{
    public int Quantity { get; set; }

    public int Type { get; set; }

    public string Reason { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}

public class StockAdjustmentModel
{
    public string Id { get; set; }

    public int? QuantityChange { get; set; }

    public decimal OldUnitCost { get; set; }

    public decimal NewUnitCost { get; set; }

    public string Reason { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Application.Contracts\Models\StockSnapshotModel.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockSnapshotModel
{
    public string Id { get; set; }

    public string TenantId { get; set; }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Infrastructure\EntityFramework\InventoryDbContext.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Infrastructure;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;
using BridgingIT.DevKit.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options)
    : ModuleDbContextBase(options), IDocumentStoreContext, IOutboxDomainEventContext
{
    public DbSet<Stock> Stocks { get; set; }

    public DbSet<StockSnapshot> StockSnapshots { get; set; }

    public DbSet<StorageDocument> StorageDocuments { get; set; }

    public DbSet<OutboxDomainEvent> OutboxDomainEvents { get; set; }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Infrastructure\EntityFramework\InventoryQueryService.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Infrastructure;

using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq;

public class InventoryQueryService(IGenericRepository<Stock> stockRepository, InventoryDbContext dbContext)
    : IInventoryQueryService
{
    /// <summary>
    ///     Retrieves the top stocks based on total movement quantity within a specified time period.
    /// </summary>
    /// <param name="start">The start of the time period.</param>
    /// <param name="end">The end of the time period.</param>
    /// <param name="limit">The maximum number of stocks to retrieve (default is 5).</param>
    public async Task<Result<IEnumerable<Stock>>> StockFindTopAsync(DateTimeOffset start, DateTimeOffset end, int limit = 5)
    {
        var topStockIds = await dbContext.Stocks
            .SelectMany(s => s.Movements)
            .Where(m => m.Timestamp >= start && m.Timestamp <= end)
            .GroupBy(m => m.StockId)
            .Select(g => new
            {
                StockId = g.Key,
                TotalMovement = Math.Abs(g.Sum(m => m.Quantity))
            })
            .OrderByDescending(x => x.TotalMovement)
            .Take(limit)
            .Select(x => x.StockId)
            .ToListAsync();

        var topStocks = await dbContext.Stocks
            .Where(s => topStockIds.Contains(s.Id))
            .Include(s => s.Movements.Where(m => m.Timestamp >= start && m.Timestamp <= end))
            .ToListAsync();

        // Order the results to match the order of topStockIds
        var orderedTopStocks = topStockIds
            .Select(id => topStocks.First(s => s.Id == id))
            .ToList();

        var result = new List<Stock>();
        foreach (var stock in orderedTopStocks)
        {
            result.Add(await stockRepository.FindOneAsync(stock.Id));
        }

        return Result<IEnumerable<Stock>>.Success(result);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\Commands\CompanyCreateCommand.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class CompanyCreateCommand(CompanyModel model) : CommandRequestBase<Result<Company>>
{
    public CompanyModel Model { get; } = model;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<CompanyCreateCommand>
    {
        public Validator()
        {
            this.RuleFor(c => c.Model).SetValidator(new ModelValidator());
        }

        private class ModelValidator : AbstractValidator<CompanyModel>
        {
            public ModelValidator()
            {
                this.RuleFor(m => m).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.Id).MustBeDefaultOrEmptyGuid().WithMessage("Must be empty.");
                this.RuleFor(m => m.Name).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.RegistrationNumber).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.ContactEmail).NotNull().NotEmpty().WithMessage("Must not be empty.");
            }
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\Commands\CompanyCreateCommandHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class CompanyCreateCommandHandler(
    ILoggerFactory loggerFactory,
    IMapper mapper,
    IGenericRepository<Company> repository)
    : CommandHandlerBase<CompanyCreateCommand, Result<Company>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Company>>> Process(
        CompanyCreateCommand command,
        CancellationToken cancellationToken)
    {
        //var company = CompanyModelMapper.Map(command.Model);
        var company = mapper.Map<CompanyModel, Company>(command.Model);

        await DomainRules.ApplyAsync([CompanyRules.NameMustBeUnique(repository, company)], cancellationToken);

        await repository.InsertAsync(company, cancellationToken).AnyContext();

        return CommandResponse.Success(company);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\Commands\CompanyDeleteCommand.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class CompanyDeleteCommand(string id) : CommandRequestBase<Result<Company>>
{
    public string Id { get; } = id;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<CompanyDeleteCommand>
    {
        public Validator()
        {
            this.RuleFor(c => c.Id).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\Commands\CompanyDeleteCommandHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class CompanyDeleteCommandHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Company> companyRepository,
    IGenericRepository<Tenant> tenantRepository)
    : CommandHandlerBase<CompanyDeleteCommand, Result<Company>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Company>>> Process(
        CompanyDeleteCommand command,
        CancellationToken cancellationToken)
    {
        var companyResult = await companyRepository.FindOneResultAsync(
            CompanyId.Create(command.Id),
            cancellationToken: cancellationToken);

        if (companyResult.IsFailure)
        {
            return CommandResponse.For(companyResult);
        }

        DomainRules.Apply([CompanyRules.MustHaveNoTenants(tenantRepository, companyResult.Value)]);

        await companyRepository.DeleteAsync(companyResult.Value, cancellationToken).AnyContext();

        return CommandResponse.Success(companyResult.Value);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\Commands\CompanyModelMapper.cs
// ----------------------------------------
//namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;
//using System;
//using System.Collections.Generic;
//using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;
//using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Application;
//using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

//public static class CompanyModelMapper
//{
//    private static readonly List<Action<CompanyModel, Company>> PropertyMappers = [];

//    public static Company Map(CompanyModel source, Company destination = null)
//    {
//        destination ??= Company.Create(
//            source.Name,
//            source.RegistrationNumber,
//            EmailAddress.Create(source.ContactEmail),
//            MapAddress(source.Address));

//        if (destination.Id != null)
//        {
//            destination.SetName(source.Name)
//               .SetRegistrationNumber(source.RegistrationNumber)
//               .SetContactEmail(EmailAddress.Create(source.ContactEmail))
//               .SetAddress(MapAddress(source.Address));
//        }

//        destination.SetContactPhone(PhoneNumber.Create(source.ContactPhone))
//            .SetWebsite(Url.Create(source.Website))
//            .SetVatNumber(VatNumber.Create(source.VatNumber));

//        return destination;
//    }

//    private static Address MapAddress(AddressModel source)
//    {
//        return Address.Create(
//            source?.Name,
//            source?.Line1,
//            source?.Line2,
//            source?.PostalCode,
//            source?.City,
//            source?.Country);
//    }
//}


// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\Commands\CompanyUpdateCommand.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class CompanyUpdateCommand(CompanyModel model) : CommandRequestBase<Result<Company>>
{
    public CompanyModel Model { get; } = model;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<CompanyUpdateCommand>
    {
        public Validator()
        {
            this.RuleFor(c => c.Model).SetValidator(new ModelValidator());
        }

        private class ModelValidator : AbstractValidator<CompanyModel>
        {
            public ModelValidator()
            {
                this.RuleFor(m => m).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.Id).MustNotBeDefaultOrEmptyGuid().WithMessage("Must not be empty.");
                this.RuleFor(m => m.Name).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.RegistrationNumber).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.ContactEmail).NotNull().NotEmpty().WithMessage("Must not be empty.");
            }
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\Commands\CompanyUpdateCommandHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class CompanyUpdateCommandHandler(
    ILoggerFactory loggerFactory,
    IMapper mapper,
    IGenericRepository<Company> repository)
    : CommandHandlerBase<CompanyUpdateCommand, Result<Company>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Company>>> Process(
        CompanyUpdateCommand command,
        CancellationToken cancellationToken)
    {
        var companyResult = await repository.FindOneResultAsync(
            CompanyId.Create(command.Model.Id),
            cancellationToken: cancellationToken);

        if (companyResult.IsFailure)
        {
            return CommandResponse.For(companyResult);
        }

        //var company = CompanyModelMapper.Map(command.Model, companyResult.Value);
        var company = mapper.Map(command.Model, companyResult.Value);

        await DomainRules.ApplyAsync([CompanyRules.NameMustBeUnique(repository, company)], cancellationToken);

        await repository.UpdateAsync(company, cancellationToken).AnyContext();

        return CommandResponse.Success(company);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\Commands\TenantCreateCommand.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class TenantCreateCommand(TenantModel model) : CommandRequestBase<Result<Tenant>>
{
    public TenantModel Model { get; } = model;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<TenantCreateCommand>
    {
        public Validator()
        {
            this.RuleFor(c => c.Model).SetValidator(new ModelValidator());
        }

        private class ModelValidator : AbstractValidator<TenantModel>
        {
            public ModelValidator()
            {
                this.RuleFor(m => m).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.Id).MustBeDefaultOrEmptyGuid().WithMessage("Must be empty.");
                this.RuleFor(m => m.CompanyId)
                    .MustNotBeDefaultOrEmptyGuid()
                    .WithMessage("Must be valid and not be empty.");
                this.RuleFor(m => m.Name).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.ContactEmail).NotNull().NotEmpty().WithMessage("Must not be empty.");
            }
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\Commands\TenantCreateCommandHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class TenantCreateCommandHandler(
    ILoggerFactory loggerFactory,
    IMapper mapper,
    IGenericRepository<Tenant> tenantRepository,
    IGenericRepository<Company> companyRepository)
    : CommandHandlerBase<TenantCreateCommand, Result<Tenant>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Tenant>>> Process(
        TenantCreateCommand command,
        CancellationToken cancellationToken)
    {
        var companyResult = await companyRepository.FindOneResultAsync(
            CompanyId.Create(command.Model.CompanyId),
            cancellationToken: cancellationToken);

        if (companyResult.IsFailure)
        {
            return CommandResponse.For<Tenant>(companyResult);
        }

        //var tenant = TenantModelMapper.Map(command.Model);
        var tenant = mapper.Map<TenantModel, Tenant>(command.Model);

        await DomainRules.ApplyAsync([TenantRules.NameMustBeUnique(tenantRepository, tenant)], cancellationToken);

        await tenantRepository.InsertAsync(tenant, cancellationToken).AnyContext();

        return CommandResponse.Success(tenant);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\Commands\TenantModelMapper.cs
// ----------------------------------------
//// MIT-License
//// Copyright BridgingIT GmbH - All Rights Reserved
//// Use of this source code is governed by an MIT-style license that can be
//// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

//namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using BridgingIT.DevKit.Domain.Model;
//using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;
//using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

//public static class TenantModelMapper
//{
//    public static Tenant Map(TenantModel source, Tenant destination = null)
//    {
//        destination ??= Tenant.Create(
//            source.CompanyId,
//            source.Name,
//            EmailAddress.Create(source.ContactEmail));

//        if (destination.Id != null)
//        {
//            destination.SetCompany(source.CompanyId)
//                       .SetName(source.Name)
//                       .SetContactEmail(EmailAddress.Create(source.ContactEmail));
//        }

//        destination.SetDescription(source.Description);
//        MapSubscriptions(source, destination);
//        MapBranding(source, destination);

//        return destination;
//    }

//    private static void MapSubscriptions(TenantModel source, Tenant destination)
//    {
//        var sourceSubscriptionModels = (source.Subscriptions ?? []).ToDictionary(s => s.Id, s => s);
//        var destinationSubscriptions = destination.Subscriptions.ToDictionary(s => s.Id.Value.ToString(), s => s);

//        foreach (var existingId in destinationSubscriptions.Keys.Except(sourceSubscriptionModels.Keys))
//        {
//            destination.RemoveSubscription(destinationSubscriptions[existingId]);
//        }

//        foreach (var (id, sourceSubscriptionModel) in sourceSubscriptionModels)
//        {
//            if (destinationSubscriptions.TryGetValue(id, out var destinationSubscription))
//            {
//                destinationSubscription
//                    .SetPlanType(Enumeration.FromId<TenantSubscriptionPlanType>(sourceSubscriptionModel.PlanType))
//                    .SetStatus(Enumeration.FromId<TenantSubscriptionStatus>(sourceSubscriptionModel.Status))
//                    .SetSchedule(DateSchedule.Create(sourceSubscriptionModel.Schedule.StartDate, sourceSubscriptionModel.Schedule.EndDate))
//                    .SetBillingCycle(Enumeration.FromId<TenantSubscriptionBillingCycle>(sourceSubscriptionModel.BillingCycle));
//            }
//            else
//            {
//                destination.AddSubscription(
//                    CreateSubscription(sourceSubscriptionModel, destination));
//            }
//        }
//    }

//    private static TenantSubscription CreateSubscription(TenantSubscriptionModel source, Tenant destination)
//    {
//        return TenantSubscription.Create(
//                destination,
//                Enumeration.FromId<TenantSubscriptionPlanType>(source.PlanType),
//                DateSchedule.Create(source.Schedule.StartDate, source.Schedule.EndDate))
//            .SetStatus(Enumeration.FromId<TenantSubscriptionStatus>(source.Status))
//            .SetBillingCycle(Enumeration.FromId<TenantSubscriptionBillingCycle>(source.BillingCycle));
//    }

//    private static void MapBranding(TenantModel source, Tenant destination)
//    {
//        var brandingModel = source.Branding;
//        if (brandingModel == null)
//        {
//            destination.SetBranding(null);
//            return;
//        }

//        var branding = destination.Branding ?? TenantBranding.Create();
//        destination.SetBranding(branding);

//        branding.SetPrimaryColor(HexColor.Create(brandingModel.PrimaryColor))
//            .SetSecondaryColor(HexColor.Create(brandingModel.SecondaryColor))
//            .SetLogoUrl(Url.Create(brandingModel.LogoUrl))
//            .SetFaviconUrl(Url.Create(brandingModel.FaviconUrl))
//            .SetCustomCss(brandingModel.CustomCss);
//    }
//}


// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\Commands\TenantUpdateCommand.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class TenantUpdateCommand(TenantModel model) : CommandRequestBase<Result<Tenant>>
{
    public TenantModel Model { get; } = model;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<TenantUpdateCommand>
    {
        public Validator()
        {
            this.RuleFor(c => c.Model).SetValidator(new ModelValidator());
        }

        private class ModelValidator : AbstractValidator<TenantModel>
        {
            public ModelValidator()
            {
                this.RuleFor(m => m).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.Id).MustNotBeDefaultOrEmptyGuid().WithMessage("Must not be empty.");
                this.RuleFor(m => m.CompanyId)
                    .MustNotBeDefaultOrEmptyGuid()
                    .WithMessage("Must be valid and not be empty.");
                this.RuleFor(m => m.Name).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.ContactEmail).NotNull().NotEmpty().WithMessage("Must not be empty.");
            }
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\Commands\TenantUpdateCommandHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class TenantUpdateCommandHandler(
    ILoggerFactory loggerFactory,
    IMapper mapper,
    IGenericRepository<Tenant> tenantRepository,
    IGenericRepository<Company> companyRepository)
    : CommandHandlerBase<TenantUpdateCommand, Result<Tenant>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Tenant>>> Process(
        TenantUpdateCommand command,
        CancellationToken cancellationToken)
    {
        var companyResult = await companyRepository.FindOneResultAsync(
            CompanyId.Create(command.Model.CompanyId),
            cancellationToken: cancellationToken);

        if (companyResult.IsFailure)
        {
            return CommandResponse.For<Tenant>(companyResult);
        }

        var tenantResult = await tenantRepository.FindOneResultAsync(
            TenantId.Create(command.Model.Id),
            cancellationToken: cancellationToken);

        if (tenantResult.IsFailure)
        {
            return CommandResponse.For(tenantResult);
        }

        //var tenant = TenantModelMapper.Map(command.Model, tenantResult.Value);
        var tenant = mapper.Map(command.Model, tenantResult.Value);

        await DomainRules.ApplyAsync([TenantRules.NameMustBeUnique(tenantRepository, tenant)], cancellationToken);

        await tenantRepository.UpdateAsync(tenant, cancellationToken).AnyContext();

        return CommandResponse.Success(tenant);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\Queries\CompanyFindAllQuery.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class CompanyFindAllQuery : QueryRequestBase<Result<IEnumerable<Company>>>
{
    public class Validator : AbstractValidator<CompanyFindAllQuery> { }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\Queries\CompanyFindAllQueryHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class CompanyFindAllQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Company> repository)
    : QueryHandlerBase<CompanyFindAllQuery, Result<IEnumerable<Company>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Company>>>> Process(
        CompanyFindAllQuery query,
        CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await repository.FindAllResultAsync(
                    new FindOptions<Company> { Order = new OrderOption<Company>(e => e.Name) },
                    cancellationToken)
                .AnyContext());
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\Queries\CompanyFindOneQuery.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class CompanyFindOneQuery(string companyId) : QueryRequestBase<Result<Company>>
{
    public string CompanyId { get; } = companyId;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<CompanyFindOneQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.CompanyId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\Queries\CompanyFindOneQueryHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class CompanyFindOneQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Company> repository)
    : QueryHandlerBase<CompanyFindOneQuery, Result<Company>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<Company>>> Process(
        CompanyFindOneQuery query,
        CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await repository.FindOneResultAsync(CompanyId.Create(query.CompanyId), cancellationToken: cancellationToken)
                .AnyContext());
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\Queries\TenantFindAllQuery.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class TenantFindAllQuery : QueryRequestBase<Result<IEnumerable<Tenant>>>
{
    public string CompanyId { get; set; }

    public class Validator : AbstractValidator<TenantFindAllQuery> { }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\Queries\TenantFindAllQueryHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

using BridgingIT.DevKit.Domain;

public class TenantFindAllQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Tenant> repository)
    : QueryHandlerBase<TenantFindAllQuery, Result<IEnumerable<Tenant>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Tenant>>>> Process(
        TenantFindAllQuery query,
        CancellationToken cancellationToken)
    {
        var specifications = new List<ISpecification<Tenant>>();

        if (!query.CompanyId.IsNullOrEmpty())
        {
            specifications.Add(TenantSpecifications.ForCompany(CompanyId.Create(query.CompanyId)));
        }

        return QueryResponse.For(
            await repository.FindAllResultAsync(
                    specifications,
                    new FindOptions<Tenant> { Order = new OrderOption<Tenant>(e => e.Name) },
                    cancellationToken)
                .AnyContext());
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\Queries\TenantFindOneQuery.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class TenantFindOneQuery(string tenantId) : QueryRequestBase<Result<Tenant>>
{
    public string TenantId { get; } = tenantId;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<TenantFindOneQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\Queries\TenantFindOneQueryHandler.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class TenantFindOneQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Tenant> repository)
    : QueryHandlerBase<TenantFindOneQuery, Result<Tenant>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<Tenant>>> Process(
        TenantFindOneQuery query,
        CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await repository.FindOneResultAsync(TenantId.Create(query.TenantId), cancellationToken: cancellationToken)
                .AnyContext());
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\Tasks\OrganizationDomainSeederTask.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

using Microsoft.Extensions.Logging.Abstractions;

public class OrganizationDomainSeederTask(
    ILoggerFactory loggerFactory,
    IGenericRepository<Company> companyRepository,
    IGenericRepository<Tenant> tenantRepository) : IStartupTask
{
    private readonly ILogger<OrganizationDomainSeederTask> logger =
        loggerFactory?.CreateLogger<OrganizationDomainSeederTask>() ??
        NullLoggerFactory.Instance.CreateLogger<OrganizationDomainSeederTask>();

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("{LogKey} seed organization (task={StartupTaskType})", "IFR", this.GetType().PrettyName());

        var companies = await this.SeedCompanies(companyRepository);
        var tenants = await this.SeedTenants(tenantRepository, companies);
    }

    private async Task<Company[]> SeedCompanies(IGenericRepository<Company> repository)
    {
        this.logger.LogInformation("{LogKey} seed companies (task={StartupTaskType})", "IFR", this.GetType().PrettyName());

        var entities = OrganizationSeedEntities.Companies.Create();

        foreach (var entity in entities)
        {
            if (!await repository.ExistsAsync(entity.Id))
            {
                entity.AuditState.SetCreated("seed", nameof(OrganizationDomainSeederTask));
                await repository.InsertAsync(entity);
            }
            else
            {
                return entities;
            }
        }

        return entities;
    }

    private async Task<Tenant[]> SeedTenants(IGenericRepository<Tenant> repository, Company[] companies)
    {
        this.logger.LogInformation("{LogKey} seed tenants (task={StartupTaskType})", "IFR", this.GetType().PrettyName());

        var entities = OrganizationSeedEntities.Tenants.Create(companies);

        foreach (var entity in entities)
        {
            if (!await repository.ExistsAsync(entity.Id))
            {
                entity.AuditState.SetCreated("seed", nameof(OrganizationDomainSeederTask));
                await repository.InsertAsync(entity);
            }
            else
            {
                return entities;
            }
        }

        return entities;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application.Contracts\Models\CompanyModel.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Application;

public class CompanyModel
{
    public string Id { get; set; }

    public string Name { get; set; }

    public AddressModel Address { get; set; }

    public string RegistrationNumber { get; set; }

    public string ContactEmail { get; set; }

    public string ContactPhone { get; set; }

    public string Website { get; set; }

    public string VatNumber { get; set; }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application.Contracts\Models\TenantBrandingModel.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class TenantBrandingModel
{
    public string Id { get; set; }

    public string TenantId { get; set; }

    public string PrimaryColor { get; set; }

    public string SecondaryColor { get; set; }

    public string LogoUrl { get; set; }

    public string FaviconUrl { get; set; }

    public string CustomCss { get; set; }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application.Contracts\Models\TenantModel.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class TenantModel
{
    public string Id { get; set; }

    public string CompanyId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string ContactEmail { get; set; }

    public bool IsActive { get; set; }

    public TenantBrandingModel Branding { get; set; }

    public TenantSubscriptionModel[] Subscriptions { get; set; }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application.Contracts\Models\TenantSubscriptionModel.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Application;

public class TenantSubscriptionModel
{
    public string Id { get; set; }

    public int PlanType { get; set; }

    public int Status { get; set; }

    public DateScheduleModel Schedule { get; set; }

    public int BillingCycle { get; set; }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Domain\CompanyAggregate\Company.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

[DebuggerDisplay("Id={Id}, Name={Name}")]
[TypedEntityId<Guid>]
public class Company : AuditableAggregateRoot<CompanyId>, IConcurrent
{
    private readonly List<TenantId> tenantIds = [];

    private Company() { } // Private constructor required by EF Core

    private Company(string name, string registrationNumber, EmailAddress contactEmail, Address address = null)
    {
        this.SetName(name);
        this.SetRegistrationNumber(registrationNumber);
        this.SetContactEmail(contactEmail);
        this.SetAddress(address);
    }

    public string Name { get; private set; }

    public Address Address { get; private set; }

    public string RegistrationNumber { get; private set; }

    public EmailAddress ContactEmail { get; private set; }

    public PhoneNumber ContactPhone { get; private set; }

    public Url Website { get; private set; }

    public VatNumber VatNumber { get; private set; }

    //public IReadOnlyCollection<TenantId> TenantIds => this.tenantIds.AsReadOnly(); // TODO

    /// <summary>
    ///     Gets or sets the concurrency token to handle optimistic concurrency.
    /// </summary>
    public Guid Version { get; set; }

    public static Company Create(
        string name,
        string registrationNumber,
        EmailAddress contactEmail,
        Address address = null)
    {
        var company = new Company(name, registrationNumber, contactEmail, address);

        company.DomainEvents.Register(new CompanyCreatedDomainEvent(company), true);

        return company;
    }

    public Company SetName(string name)
    {
        _ = name ?? throw new ArgumentException("Company Name cannot be empty.");

        if (name != this.Name)
        {
            this.Name = name;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(new CompanyUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Company SetAddress(Address address)
    {
        if (address != this.Address)
        {
            this.Address = address;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(new CompanyUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Company SetRegistrationNumber(string registrationNumber)
    {
        _ = registrationNumber ?? throw new ArgumentException("Company RegistrationNumber cannot be empty.");

        if (registrationNumber != this.RegistrationNumber)
        {
            this.RegistrationNumber = registrationNumber;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(new CompanyUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Company SetContactEmail(EmailAddress emailAddress)
    {
        _ = emailAddress ?? throw new ArgumentException("Company EmailAddress cannot be empty.");

        if (emailAddress != this.ContactEmail)
        {
            this.ContactEmail = emailAddress;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(new CompanyUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Company SetContactPhone(PhoneNumber phoneNumber)
    {
        if (phoneNumber != this.ContactPhone)
        {
            this.ContactPhone = phoneNumber;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(new CompanyUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Company SetWebsite(Url website)
    {
        if (website != this.Website)
        {
            this.Website = website;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(new CompanyUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Company SetVatNumber(VatNumber vatNumber)
    {
        if (vatNumber != this.VatNumber)
        {
            this.VatNumber = vatNumber;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(new CompanyUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }

    //public Company AddTenant(TenantId tenantId)
    //{
    //    if (!this.tenantIds.Contains(tenantId))
    //    {
    //        this.tenantIds.Add(tenantId);

    //        if (this.Id?.IsEmpty == false)
    //        {
    //            this.DomainEvents.Register(
    //            new CompanyUpdatedDomainEvent(this), true);
    //        }
    //    }

    //    return this;
    //}

    //public Company RemoveTenant(TenantId tenantId)
    //{
    //    if (this.tenantIds.Remove(tenantId))
    //    {
    //        if (this.Id?.IsEmpty == false)
    //        {
    //            this.DomainEvents.Register(
    //            new CompanyUpdatedDomainEvent(this), true);
    //        }
    //    }

    //    return this;
    //}
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Domain\TenantAggregate\Tenant.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

/// <summary>
///     Represents the client organization or individual using the shop platform.
/// </summary>
[DebuggerDisplay("Id={Id}, Name={Name}")]
public class Tenant : AuditableAggregateRoot<TenantId>, IConcurrent
{
    private readonly List<TenantSubscription> subscriptions = [];

    private Tenant() { } // Private constructor required by EF Core

    private Tenant(CompanyId companyId, string name, EmailAddress contactEmail)
    {
        this.SetCompany(companyId);
        this.SetName(name);
        this.SetContactEmail(contactEmail);
        this.Activate();
    }

    public CompanyId CompanyId { get; private set; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public bool Activated { get; private set; }

    public EmailAddress ContactEmail { get; private set; }

    public IEnumerable<TenantSubscription> Subscriptions
        => this.subscriptions.OrderBy(e => e.Schedule);

    public TenantBranding Branding { get; private set; }

    /// <summary>
    ///     Gets or sets the concurrency token to handle optimistic concurrency.
    /// </summary>
    public Guid Version { get; set; }

    public static Tenant Create(CompanyId companyId, string name, EmailAddress contactEmail)
    {
        _ = companyId ?? throw new ArgumentException("Tenant CompanyId cannot be empty.");

        var tenant = new Tenant(companyId, name, contactEmail);

        tenant.DomainEvents.Register(new TenantCreatedDomainEvent(tenant), true);

        return tenant;
    }

    public bool IsActive()
    {
        return this.IsActive(DateOnly.FromDateTime(DateTime.Now));
    }

    public bool IsActive(DateOnly date)
    {
        return this.Activated && this.subscriptions.Any(e => e.IsActive(date));
    }

    public Tenant SetCompany(CompanyId companyId)
    {
        _ = companyId ?? throw new ArgumentException("Tenant CompanyId cannot be empty.");

        if (companyId != this.CompanyId)
        {
            this.CompanyId = companyId;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(new TenantUpdatedDomainEvent(this), true);
                this.DomainEvents.Register(new TenantReassignedCompanyDomainEvent(this), true);
            }
        }

        return this;
    }

    public Tenant SetName(string name)
    {
        _ = name ?? throw new ArgumentException("Tenant Name cannot be empty.");

        if (name != this.Name)
        {
            this.Name = name;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(new TenantUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Tenant SetDescription(string description)
    {
        if (description != this.Name)
        {
            this.Description = description;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(new TenantUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Tenant Deactivate()
    {
        if (this.Activated)
        {
            this.Activated = false;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(new TenantDeactivatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Tenant Activate()
    {
        if (!this.Activated)
        {
            this.Activated = true;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(new TenantActivatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Tenant SetContactEmail(EmailAddress email)
    {
        _ = email ?? throw new ArgumentException("Tenant ContactEmail cannot be empty.");

        if (email != this.ContactEmail)
        {
            this.ContactEmail = email;

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(new TenantUpdatedDomainEvent(this), true);
            }
        }

        return this;
    }

    public Tenant SetBranding(TenantBranding branding)
    {
        _ = branding ?? throw new ArgumentException("Tenant Branding cannot be empty.");

        if (branding.TenantId != null && branding.TenantId != this.Id)
        {
            throw new InvalidOperationException("Branding does not belong to this tenant.");
        }

        this.Branding = branding;

        if (this.Id?.IsEmpty == false)
        {
            this.DomainEvents.Register(new TenantUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public TenantSubscription AddSubscription()
    {
        var subscription = TenantSubscription.Create(
            this,
            TenantSubscriptionPlanType.Free,
            DateSchedule.Create(DateOnly.FromDateTime(DateTime.Now)));

        this.subscriptions.Add(subscription);

        if (this.Id?.IsEmpty == false)
        {
            this.DomainEvents.Register(new TenantUpdatedDomainEvent(this), true);
        }

        return subscription;
    }

    public Tenant AddSubscription(TenantSubscription subscription)
    {
        if (subscription.Tenant != null && subscription.Tenant != this)
        {
            throw new InvalidOperationException("Subscription does not belong to this tenant.");
        }

        this.subscriptions.Add(subscription);

        if (this.Id?.IsEmpty == false)
        {
            this.DomainEvents.Register(new TenantUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public Tenant RemoveSubscription(TenantSubscription subscription)
    {
        if (subscription.Tenant != this)
        {
            throw new InvalidOperationException("Subscription does not belong to this tenant.");
        }

        if (!this.subscriptions.Contains(subscription))
        {
            this.subscriptions.Remove(subscription);

            if (this.Id?.IsEmpty == false)
            {
                this.DomainEvents.Register(new TenantUpdatedDomainEvent(this), true)
                    .Register(new TenantSubscriptionRemovedDomainEvent(subscription), true);
            }
        }

        return this;
    }

    public Tenant RemoveSubscription(TenantSubscriptionId id)
    {
        var subscription = this.subscriptions.Find(c => c.Id == id);
        if (subscription == null)
        {
            return this;
        }

        this.subscriptions.Remove(subscription);

        if (this.Id?.IsEmpty == false)
        {
            this.DomainEvents.Register(new TenantUpdatedDomainEvent(this), true)
                .Register(new TenantSubscriptionRemovedDomainEvent(subscription), true);
        }

        return this;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Domain\TenantAggregate\TenantBranding.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

[DebuggerDisplay("Id={Id}, TenantId={TenantId}")]
[TypedEntityId<Guid>]
public class TenantBranding : Entity<TenantBrandingId>
{
    private TenantBranding() { } // Private constructor required by EF Core

    private TenantBranding(
        HexColor primaryColor = null,
        HexColor secondaryColor = null,
        Url logoUrl = null,
        Url faviconUrl = null)
    {
        this.SetPrimaryColor(primaryColor);
        this.SetSecondaryColor(secondaryColor);
        this.SetLogoUrl(logoUrl);
        this.SetFaviconUrl(faviconUrl);
    }

    public TenantId TenantId { get; private set; }

    public HexColor PrimaryColor { get; private set; }

    public HexColor SecondaryColor { get; private set; }

    public Url LogoUrl { get; private set; }

    public Url FaviconUrl { get; private set; }

    public string CustomCss { get; private set; }

    public static TenantBranding Create(
        HexColor primaryColor = null,
        HexColor secondaryColor = null,
        Url logoUrl = null,
        Url faviconUrl = null)
    {
        return new TenantBranding(primaryColor, secondaryColor, logoUrl, faviconUrl);
    }

    public TenantBranding SetPrimaryColor(HexColor color)
    {
        this.PrimaryColor = color;

        return this;
    }

    public TenantBranding SetSecondaryColor(HexColor color)
    {
        this.SecondaryColor = color;

        return this;
    }

    public TenantBranding SetLogoUrl(Url url)
    {
        this.LogoUrl = url;

        return this;
    }

    public TenantBranding SetFaviconUrl(Url url)
    {
        this.FaviconUrl = url;

        return this;
    }

    public TenantBranding SetCustomCss(string customCss)
    {
        this.CustomCss = customCss; // TODO: check if valid css (xss)

        return this;
    }

    private bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Domain\TenantAggregate\TenantSubscription.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

/// <summary>
///     Represents the commercial agreements for the tenant.
/// </summary>
[DebuggerDisplay("Id={Id}, TenantId={Tenant?.Id}, Status={Status}")]
[TypedEntityId<Guid>]
public class TenantSubscription : Entity<TenantSubscriptionId>, IConcurrent
{
    private TenantSubscription() { } // Private constructor required by EF Core

    private TenantSubscription(Tenant tenant, TenantSubscriptionPlanType planType, DateSchedule schedule)
    {
        this.Tenant = tenant;
        this.SetPlanType(planType);
        this.SetStatus(TenantSubscriptionStatus.Pending);
        this.SetSchedule(schedule);
    }

    public Tenant Tenant { get; }

    public TenantSubscriptionPlanType PlanType { get; private set; }

    public TenantSubscriptionStatus Status { get; private set; }

    public DateSchedule Schedule { get; private set; }

    public TenantSubscriptionBillingCycle BillingCycle { get; private set; }

    /// <summary>
    ///     Gets or sets the concurrency token to handle optimistic concurrency.
    /// </summary>
    public Guid Version { get; set; }

    public static TenantSubscription Create(Tenant tenant, TenantSubscriptionPlanType planType, DateSchedule schedule)
    {
        var subscription = new TenantSubscription(tenant, planType, schedule);

        tenant.DomainEvents.Register(new TenantSubscriptionCreatedDomainEvent(subscription), true);

        return subscription;
    }

    public bool IsActive(DateOnly date)
    {
        return Equals(this.Status, TenantSubscriptionStatus.Approved) && this.Schedule.IsActive(date);
    }

    public TenantSubscription SetPlanType(TenantSubscriptionPlanType planType)
    {
        if (planType == null)
        {
            throw new DomainRuleException("Plan type cannot be null.");
        }

        if (planType != this.PlanType)
        {
            this.PlanType = planType;

            // Set default billing cycle for free plans
            var plan = Enumeration.FromId<TenantSubscriptionPlanType>(this.PlanType.Id);
            if (!planType.IsPaid)
            {
                this.SetBillingCycle(TenantSubscriptionBillingCycle.Never);
            }

            // Set default billing cycle for paid plans
            if (planType.IsPaid && this.BillingCycle == TenantSubscriptionBillingCycle.Never)
            {
                this.SetBillingCycle(TenantSubscriptionBillingCycle.Monthly);
            }

            this.Tenant.DomainEvents.Register(new TenantSubscriptionUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public TenantSubscription SetSchedule(DateSchedule schedule)
    {
        if (schedule == null)
        {
            throw new DomainRuleException("Schedule cannot be null.");
        }

        if (schedule != this.Schedule)
        {
            this.Schedule = schedule;

            this.Tenant.DomainEvents.Register(new TenantSubscriptionUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public TenantSubscription SetBillingCycle(TenantSubscriptionBillingCycle billingCycle)
    {
        if (billingCycle != this.BillingCycle)
        {
            if (this.PlanType.IsPaid && billingCycle == TenantSubscriptionBillingCycle.Never)
            {
                throw new DomainRuleException("Subscription billing cycle should not be 'never' for paid plans.");
            }

            this.BillingCycle = billingCycle ?? TenantSubscriptionBillingCycle.Monthly;

            this.Tenant.DomainEvents.Register(new TenantSubscriptionUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public TenantSubscription SetStatus(TenantSubscriptionStatus status)
    {
        // Validate name
        if (status != this.Status)
        {
            // TODO: check valid transitions
            this.Status = status;

            this.Tenant.DomainEvents.Register(new TenantSubscriptionUpdatedDomainEvent(this), true);
        }

        return this;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Domain\TenantAggregate\TenantSubscriptionBillingCycle.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

[DebuggerDisplay("Id={Id}, Value={Value}")]
public class TenantSubscriptionBillingCycle(int id, string value, string description, bool autoRenew)
    : Enumeration(id, value)
{
    public static TenantSubscriptionBillingCycle Never = new(0, nameof(Never), "Lorem Ipsum", false);

    public static TenantSubscriptionBillingCycle Monthly = new(1, nameof(Monthly), "Lorem Ipsum", true);

    public static TenantSubscriptionBillingCycle Yearly = new(2, nameof(Yearly), "Lorem Ipsum", true);

    public string Description { get; } = description;

    public bool AutoRenew { get; } = autoRenew;

    public static IEnumerable<TenantSubscriptionBillingCycle> GetAll()
    {
        return GetAll<TenantSubscriptionBillingCycle>();
    }

    // public override bool Equals(object obj)
    // {
    //     if (obj is TenantSubscriptionBillingCycle other)
    //     {
    //         return this.Id == other.Id;
    //     }
    //
    //     return false;
    // }
    //
    // public override int GetHashCode() => this.Id.GetHashCode();
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Domain\TenantAggregate\TenantSubscriptionPlanType.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

[DebuggerDisplay("Id={Id}, Value={Value}")]
public class TenantSubscriptionPlanType(int id, string value, string code, string description, bool isPaid)
    : Enumeration(id, value)
{
    public static TenantSubscriptionPlanType Free = new(0, nameof(Free), "FRE", "Lorem Ipsum", false);

    public static TenantSubscriptionPlanType Basic = new(1, nameof(Basic), "BAS", "Lorem Ipsum", true);

    public static TenantSubscriptionPlanType Premium = new(2, nameof(Premium), "PRM", "Lorem Ipsum", true);

    public string Code { get; } = code;

    public string Description { get; } = description;

    public bool IsPaid { get; } = isPaid;

    public static IEnumerable<TenantSubscriptionPlanType> GetAll()
    {
        return GetAll<TenantSubscriptionPlanType>();
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Domain\TenantAggregate\TenantSubscriptionStatus.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

[DebuggerDisplay("Id={Id}, Value={Value}")]
public class TenantSubscriptionStatus(int id, string value, string description)
    : Enumeration(id, value)
{
    public static TenantSubscriptionStatus Pending = new(1, nameof(Pending), "Lorem Ipsum");
    public static TenantSubscriptionStatus Approved = new(2, nameof(Approved), "Lorem Ipsum");
    public static TenantSubscriptionStatus Cancelled = new(3, nameof(Cancelled), "Lorem Ipsum");
    public static TenantSubscriptionStatus Ended = new(4, nameof(Ended), "Lorem Ipsum");

    public string Description { get; } = description;

    public static IEnumerable<TenantSubscriptionStatus> GetAll()
    {
        return GetAll<TenantSubscriptionStatus>();
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Infrastructure\EntityFramework\OrganizationDbContext.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Infrastructure;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;
using BridgingIT.DevKit.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

public class OrganizationDbContext(DbContextOptions<OrganizationDbContext> options)
    : ModuleDbContextBase(options), IDocumentStoreContext, IOutboxDomainEventContext, IOutboxMessageContext
{
    public DbSet<Tenant> Tenants { get; set; }

    public DbSet<Company> Companies { get; set; }

    public DbSet<StorageDocument> StorageDocuments { get; set; }

    public DbSet<OutboxDomainEvent> OutboxDomainEvents { get; set; }

    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //    modelBuilder.ApplyConfigurationsFromAssembly(typeof(TagEntityTypeConfiguration).Assembly);

    //    base.OnModelCreating(modelBuilder); // applies the other EntityTypeConfigurations
    //}
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Domain\Model\Rules\RatingShouldBeInRangeRule.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

public class RatingShouldBeInRangeRule(int value) : DomainRuleBase
{
    private readonly double? value = value;

    public override string Message
        => "Rating should be between 1 and 5";

    public override Task<bool> ApplyAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(this.value >= 1 && this.value <= 5);
    }
}

public static class RatingRules
{
    public static IDomainRule ShouldBeInRange(int value)
    {
        return new RatingShouldBeInRangeRule(value);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Domain\Model\Rules\TagMustBelongToTenantRule.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

public class TagMustBelongToTenantRule(Tag tag, TenantId tenantId) : DomainRuleBase
{
    public override string Message
        => $"Tag should belong to tenant {tenantId}";

    public override Task<bool> ApplyAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(tag.TenantId == tenantId);
    }
}

public static class TagRules
{
    public static IDomainRule TagMustBelongToTenant(Tag tag, TenantId tenantId)
    {
        return new TagMustBelongToTenantRule(tag, tenantId);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Infrastructure\EntityFramework\Configurations\TagEntityTypeConfiguration.cs
// ----------------------------------------
//// MIT-License
//// Copyright BridgingIT GmbH - All Rights Reserved
//// Use of this source code is governed by an MIT-style license that can be
//// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

//namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Infrastructure;

//using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;

//public class TagEntityTypeConfiguration : TenantAwareEntityTypeConfiguration<Tag>
//{
//    public override void Configure(EntityTypeBuilder<Tag> builder)
//    {
//        base.Configure(builder);

//        builder.ToTable("Tags")
//            .HasKey(e => e.Id)
//            .IsClustered(false);

//        builder.Property(e => e.Id)
//            .ValueGeneratedOnAdd()
//            .HasConversion(
//                id => id.Value,
//                value => TagId.Create(value));

//        //builder.HasOne<Tenant>() // one-to-many with no navigations https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many#one-to-many-with-no-navigations
//        //    .WithMany()
//        //    .HasForeignKey(e => e.TenantId).IsRequired();
//        builder.Property(e => e.TenantId)
//            .HasConversion(
//                id => id.Value,
//                value => TenantId.Create(value))
//            .IsRequired();
//        //builder.HasIndex(e => e.TenantId);
//        //builder.HasOne("organization.Tenants")
//        //    .WithMany()
//        //    .HasForeignKey(nameof(TenantId))
//        //    .IsRequired();

//        builder.Property(e => e.Name)
//            .IsRequired().HasMaxLength(128);

//        builder.Property(e => e.Category)
//            .IsRequired(false).HasMaxLength(128);

//        builder.HasIndex(nameof(Tag.Name));
//        builder.HasIndex(nameof(Tag.Category));
//        builder.HasIndex(nameof(Tag.Name), nameof(Tag.Category))
//             .IsUnique();
//    }
//}


// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel.Infrastructure\EntityFramework\Configurations\TenantAwareEntityTypeConfiguration.cs
// ----------------------------------------
//// MIT-License
//// Copyright BridgingIT GmbH - All Rights Reserved
//// Use of this source code is governed by an MIT-style license that can be
//// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

//namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Infrastructure;

//using BridgingIT.DevKit.Domain.Model;
//using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;

//public abstract class TenantAwareEntityTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
//    where TEntity : class, IEntity
//{
//    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
//    {
//        builder.Property<TenantId>("TenantId")
//            .HasConversion(
//                id => id.Value,
//                value => TenantId.Create(value))
//            .IsRequired();

//        builder.HasIndex("TenantId");

//        builder.HasOne<TenantReference>()
//            .WithMany()
//            .HasForeignKey("TenantId")
//            .IsRequired()
//            .OnDelete(DeleteBehavior.NoAction);
//    }
//}

//public class TenantReferenceEntityTypeConfiguration : IEntityTypeConfiguration<TenantReference>
//{
//    public void Configure(EntityTypeBuilder<TenantReference> builder)
//    {
//        builder.ToTable("Tenants", "organization")
//            .HasKey(e => e.Id)
//            .IsClustered(false);

//        builder.Property(e => e.Id)
//            .ValueGeneratedOnAdd()
//            .HasConversion(
//                id => id.Value,
//                value => TenantId.Create(value));

//        builder.ToTable(tb => tb.ExcludeFromMigrations());
//    }
//}

//public class TenantReference
//{
//    public TenantId Id { get; set; }
//}


// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\AuthorAggregate\Author.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

[TypedEntityId<Guid>]
public class Author : AuditableAggregateRoot<AuthorId>, IConcurrent
{
    private readonly List<AuthorBook> books = [];
    private readonly List<Tag> tags = [];

    private Author() { } // Private constructor required by EF Core

    private Author(TenantId tenantId, PersonFormalName name, string biography = null)
    {
        this.TenantId = tenantId;
        this.SetName(name);
        this.SetBiography(biography);
    }

    public TenantId TenantId { get; }

    public PersonFormalName PersonName { get; private set; }

    public string Biography { get; private set; }

    public IEnumerable<AuthorBook> Books
        => this.books;

    public IEnumerable<Tag> Tags
        => this.tags.OrderBy(e => e.Name);

    /// <summary>
    ///     Gets or sets the concurrency token to handle optimistic concurrency.
    /// </summary>
    public Guid Version { get; set; }

    public static Author Create(TenantId tenantId, PersonFormalName name, string biography = null)
    {
        _ = tenantId ?? throw new ArgumentException("TenantId cannot be empty.");

        var author = new Author(tenantId, name, biography);

        author.DomainEvents.Register(new AuthorCreatedDomainEvent(tenantId, author));

        return author;
    }

    public Author SetName(PersonFormalName name)
    {
        _ = name ?? throw new ArgumentException("Author Name cannot be empty.");

        if (this.PersonName == name)
        {
            return this;
        }

        this.PersonName = name;

        if (this.Id?.IsEmpty == false)
        {
            this.DomainEvents.Register(new AuthorUpdatedDomainEvent(this.TenantId, this), true);
        }

        return this;
    }

    public Author SetBiography(string biography)
    {
        if (this.Biography == biography)
        {
            return this;
        }

        this.Biography = biography;

        this.DomainEvents.Register(new AuthorUpdatedDomainEvent(this.TenantId, this), true);

        return this;
    }

    public Author AssignBook(Book book)
    {
        if (this.books.Any(e => e.BookId == book.Id))
        {
            return this;
        }

        this.books.Add(AuthorBook.Create(book));

        this.DomainEvents.Register(new AuthorBookAssignedDomainEvent(this, book));

        return this;
    }

    //public Author RemoveBook(BookId bookId)
    //{
    //    var bookAuthor = this.bookIds.FirstOrDefault(ba => ba.BookId == bookId);
    //    if (bookAuthor != null)
    //    {
    //        this.bookIds.Remove(bookAuthor);
    //        // Reorder remaining books
    //        for (var i = 0; i < this.bookIds.Count; i++)
    //        {
    //            this.bookIds[i] = new BookAuthor(this.bookIds[i].BookId, this.Id.Value, i);
    //        }
    //    }

    //    return this;
    //}

    public Author AddTag(Tag tag)
    {
        if (this.tags.Contains(tag))
        {
            return this;
        }

        DomainRules.Apply([new TagMustBelongToTenantRule(tag, this.TenantId)]);

        this.tags.Add(tag);

        return this;
    }

    public Author RemoveTag(TagId tagId)
    {
        this.tags.RemoveAll(t => t.Id == tagId);

        return this;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\AuthorAggregate\AuthorBook.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

[DebuggerDisplay("BookId={BookId}, Title={Title")]
public class AuthorBook : ValueObject
{
    private AuthorBook() { }

#pragma warning disable SA1202 // Elements should be ordered by access
    private AuthorBook(BookId bookId, string title)
#pragma warning restore SA1202 // Elements should be ordered by access
    {
        this.BookId = bookId;
        this.Title = title;
    }

    public BookId BookId { get; }

    public string Title { get; }

    public static AuthorBook Create(Book book)
    {
        _ = book ?? throw new ArgumentException("AuthorBook Book cannot be empty.");

        return new AuthorBook(book.Id, book.Title);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.BookId;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\BookAggregate\Book.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

[DebuggerDisplay("Id={Id}, Isbn={Isbn}, Sku={Sku}, Title={Title}")]
[TypedEntityId<Guid>]
public class Book : AuditableAggregateRoot<BookId>, IConcurrent
{
    private readonly List<BookAuthor> authors = [];
    private readonly List<Category> categories = [];
    private readonly List<Tag> tags = [];
    private readonly List<BookKeyword> keywords = [];
    private List<BookChapter> chapters = [];

    private Book() { } // Private constructor required by EF Core

    private Book(
        TenantId tenantId,
        string title,
        string edition,
        string description,
        ProductSku sku,
        BookIsbn isbn,
        Money price,
        Publisher publisher,
        DateOnly? publishedDate = null)
    {
        this.TenantId = tenantId;
        this.SetTitle(title);
        this.SetEdition(edition);
        this.SetDescription(description);
        this.SetSku(sku);
        this.SetIsbn(isbn);
        this.SetPrice(price);
        this.SetPublisher(publisher);
        this.SetPublishedDate(publishedDate);
        this.AverageRating = AverageRating.Create();
    }

    public TenantId TenantId { get; }

    public string Title { get; private set; }

    public string Edition { get; private set; }

    public string Description { get; private set; }

    public ProductSku Sku { get; private set; }

    public BookIsbn Isbn { get; private set; }

    public Money Price { get; private set; }

    public BookPublisher Publisher { get; private set; }

    public DateOnly? PublishedDate { get; private set; }

    public AverageRating AverageRating { get; }

    public IEnumerable<BookKeyword> Keywords
        => this.keywords;

    public IEnumerable<BookAuthor> Authors
        => this.authors;

    public IEnumerable<Category> Categories
        => this.categories.OrderBy(e => e.Order);

    public IEnumerable<BookChapter> Chapters
        => this.chapters.OrderBy(e => e.Number);

    public IEnumerable<Tag> Tags
        => this.tags.OrderBy(e => e.Name);

    public int StockQuantityOnHand { get; private set; }

    public int StockQuantityReserved { get; private set; }

    /// <summary>
    ///     Gets or sets the concurrency token to handle optimistic concurrency.
    /// </summary>
    public Guid Version { get; set; }

    public static Book Create(
        TenantId tenantId,
        string title,
        string edition,
        string description,
        ProductSku sku,
        BookIsbn isbn,
        Money price,
        Publisher publisher,
        DateOnly? publishedDate = null)
    {
        _ = tenantId ?? throw new ArgumentException("TenantId cannot be empty.");

        var book = new Book(tenantId, title, edition, description, sku, isbn, price, publisher, publishedDate);

        book.DomainEvents.Register(new BookCreatedDomainEvent(tenantId, book), true);

        return book;
    }

    public Book SetTitle(string title)
    {
        _ = title ?? throw new ArgumentException("Book Title cannot be empty.");

        if (this.Title == title)
        {
            return this;
        }

        this.Title = title;
        this.ReindexKeywords();

        this.DomainEvents.Register(new BookUpdatedDomainEvent(this.TenantId, this), true);

        return this;
    }

    public Book SetEdition(string edition)
    {
        if (this.Edition == edition)
        {
            return this;
        }

        this.Edition = edition;
        // this.ReindexKeywords();

        this.DomainEvents.Register(new BookUpdatedDomainEvent(this.TenantId, this), true);

        return this;
    }

    public Book SetDescription(string description)
    {
        if (this.Description == description)
        {
            return this;
        }

        this.ReindexKeywords();

        this.DomainEvents.Register(new BookUpdatedDomainEvent(this.TenantId, this), true);

        return this;
    }

    public Book SetSku(ProductSku sku)
    {
        _ = sku ?? throw new ArgumentException("Book Sku cannot be empty.");

        if (this.Sku == sku)
        {
            return this;
        }

        this.Sku = sku;

        this.DomainEvents.Register(new BookUpdatedDomainEvent(this.TenantId, this), true);

        return this;
    }

    public Book SetIsbn(BookIsbn isbn)
    {
        _ = isbn ?? throw new ArgumentException("Book Isbn cannot be empty.");

        if (this.Isbn == isbn)
        {
            return this;
        }

        this.Isbn = isbn;

        this.DomainEvents.Register(new BookUpdatedDomainEvent(this.TenantId, this), true);

        return this;
    }

    public Book SetPrice(Money price)
    {
        _ = price ?? throw new ArgumentException("Book Price cannot be empty.");

        if (this.Price == price)
        {
            return this;
        }

        // TODO: Validate price is > 0
        this.Price = price;

        this.DomainEvents.Register(new BookUpdatedDomainEvent(this.TenantId, this), true);

        return this;
    }

    public Book SetPublisher(Publisher publisher)
    {
        _ = publisher ?? throw new ArgumentException("Book Publisher cannot be empty.");

        var bookPublisher = BookPublisher.Create(publisher);
        if (this.Publisher == bookPublisher)
        {
            return this;
        }

        this.Publisher = bookPublisher;

        this.DomainEvents.Register(new BookUpdatedDomainEvent(this.TenantId, this), true);

        return this;
    }

    public Book SetPublishedDate(DateOnly? publishedDate)
    {
        if (this.PublishedDate == publishedDate)
        {
            return this;
        }

        this.PublishedDate = publishedDate;

        this.DomainEvents.Register(new BookUpdatedDomainEvent(this.TenantId, this), true);

        return this;
    }

    public Book SetStock(int quantityOnHand, int quantityReserved)
    {
        if (this.StockQuantityOnHand == quantityOnHand && this.StockQuantityReserved == quantityReserved)
        {
            return this;
        }

        this.StockQuantityOnHand = quantityOnHand;
        this.StockQuantityReserved = quantityReserved;

        this.DomainEvents.Register(new BookUpdatedDomainEvent(this.TenantId, this), true);

        return this;
    }

    public Book AddRating(Rating rating)
    {
        _ = rating ?? throw new ArgumentException("Book Rating cannot be empty.");

        this.AverageRating.Add(rating);

        return this;
    }

    public Book AssignAuthor(Author author, int position = 0)
    {
        _ = author ?? throw new ArgumentException("Book Author cannot be empty.");

        if (this.authors.Any(e => e.AuthorId == author.Id))
        {
            return this;
        }

        this.authors.Add(BookAuthor.Create(author, position == 0 ? this.authors.Count + 1 : 0));
        this.ReindexKeywords();

        this.DomainEvents.Register(new BookAuthorAssignedDomainEvent(this, author));

        return this;
    }

    //public Book RemoveAuthor(AuthorId authorId)
    //{
    //    var bookAuthor = this.bookAuthors.FirstOrDefault(ba => ba.AuthorId == authorId);
    //    if (bookAuthor != null)
    //    {
    //        this.bookAuthors.Remove(bookAuthor);
    //        // Reorder remaining authors
    //        for (var i = 0; i < this.bookAuthors.Count; i++)
    //        {
    //            this.bookAuthors[i] = new BookAuthor(this.Id.Value, this.bookAuthors[i].AuthorId, i);
    //        }
    //    }

    //    return this;
    //}

    public Book AddChapter(string title, string content = null)
    {
        return this.AddChapter(title, this.chapters.LastOrDefault()?.Number + 1 ?? 1, content);
    }

    public Book AddChapter(string title, int number, string content = null)
    {
        // Validate title
        var index = this.chapters.FindIndex(c => c.Number == number);
        if (index < 0)
        {
            this.chapters.Add(BookChapter.Create(number, title, content));
        }
        else
        {
            this.chapters.Insert(index, BookChapter.Create(number, title, content));
        }

        this.chapters = this.ReindexChapters(this.chapters);
        this.ReindexKeywords();

        return this;
    }

    public Book UpdateChapter(BookChapter chapter)
    {
        return this.UpdateChapter(chapter.Id, chapter.Title, chapter.Number, chapter.Content);
    }

    public Book UpdateChapter(BookChapterId id, string title, int number, string content = null)
    {
        var chapter = this.chapters.SingleOrDefault(c => c.Id == id);
        if (chapter == null)
        {
            return this;
        }

        chapter.SetTitle(title);
        chapter.SetNumber(number);
        chapter.SetContent(content);

        this.chapters = this.ReindexChapters(this.chapters);
        this.ReindexKeywords();

        return this;
    }

    public Book RemoveChapter(BookChapter chapter)
    {
        return this.RemoveChapter(chapter.Id);
    }

    public Book RemoveChapter(int number)
    {
        var chapter = this.chapters.SingleOrDefault(c => c.Number == number);
        if (chapter == null)
        {
            return this;
        }

        this.RemoveChapter(chapter.Id);
        this.ReindexKeywords();

        return this;
    }

    public Book RemoveChapter(BookChapterId id)
    {
        this.chapters.RemoveAll(c => c.Id == id);
        this.chapters = this.ReindexChapters(this.chapters);
        this.ReindexKeywords();

        return this;
    }

    public Book AddCategory(Category category)
    {
        if (this.categories.Contains(category))
        {
            return this;
        }

        this.categories.Add(category);
        this.ReindexKeywords();

        return this;
    }

    public Book RemoveCategory(Category category)
    {
        this.categories.Remove(category);
        this.ReindexKeywords();

        return this;
    }

    public Book AddTag(Tag tag)
    {
        if (this.tags.Contains(tag))
        {
            return this;
        }

        DomainRules.Apply([new TagMustBelongToTenantRule(tag, this.TenantId)]);

        this.tags.Add(tag);
        this.ReindexKeywords();

        return this;
    }

    public Book RemoveTag(Tag tag)
    {
        if (this.tags.Contains(tag))
        {
            return this;
        }

        this.tags.Remove(tag);
        this.ReindexKeywords();

        return this;
    }

    private List<BookChapter> ReindexChapters(IEnumerable<BookChapter> chapters)
    {
        // First, sort the chapters by their number to ensure they are in order.
        var sortedChapters = chapters.OrderBy(c => c.Number).ToList();

        // Use a HashSet to keep track of used numbers to easily identify gaps.
        var usedNumbers = new HashSet<int>();

        for (var i = 0; i < sortedChapters.Count; i++)
        {
            var currentChapter = sortedChapters[i];
            var expectedNumber = i + 1;

            // If the current chapter's number is already used or it's less than the expected number,
            // and the expected number hasn't been used, then set the chapter number to the expected number.
            if ((usedNumbers.Contains(currentChapter.Number) || currentChapter.Number < expectedNumber) &&
                !usedNumbers.Contains(expectedNumber))
            {
                currentChapter.SetNumber(expectedNumber);
                usedNumbers.Add(expectedNumber);
            }
            else if (!usedNumbers.Contains(currentChapter.Number))
            {
                // If the current chapter's number is not used, just add it to the set of used numbers.
                usedNumbers.Add(currentChapter.Number);
            }
            else
            {
                // Find the next available number that isn't used.
                while (usedNumbers.Contains(expectedNumber))
                {
                    expectedNumber++;
                }

                currentChapter.SetNumber(expectedNumber);
                usedNumbers.Add(expectedNumber);
            }
        }

        // After reindexing, the chapters list might be out of order, so sort it again.
        return [.. sortedChapters.OrderBy(c => c.Number)];
    }

    private List<string> ReindexKeywords()
    {
        var keywords = new HashSet<string>();
        keywords.UnionWith(this.Title.SafeNull().ToLower().Split(' ').Where(word => word.Length > 3));
        //keywords.UnionWith(this.Edition.SafeNull().ToLower().Split(' ').Where(word => word.Length > 3));
        keywords.UnionWith(this.Description.SafeNull().ToLower().Split(' ').Where(word => word.Length > 3));
        keywords.UnionWith(
            this.authors.SafeNull().SelectMany(a => a.Name.ToLower().Split(' ').Where(word => word.Length > 3)));
        //newKeywords.UnionWith(this.categories.SafeNull().SelectMany(c => c.Name.ToLower().Split(' ').Where(word => word.Length > 3)));
        keywords.UnionWith(this.tags.SafeNull().Select(t => t.Name.ToLower()));
        keywords.UnionWith(
            this.chapters.SafeNull().SelectMany(c => c.Title.ToLower().Split(' ').Where(word => word.Length > 3)));

        UpdateKeywords(keywords);

        return [.. keywords]; // TODO: order by weight?

        void UpdateKeywords(HashSet<string> newKeywords)
        {
            var existingKeywords = this.keywords.ToDictionary(ki => ki.Text);

            // Remove keywords that are no longer present
            foreach (var keyword in existingKeywords.Keys.Except(newKeywords).ToList())
            {
                this.keywords.Remove(existingKeywords[keyword]);
            }

            // Add new keywords
            foreach (var keyword in newKeywords.Except(existingKeywords.Keys))
            {
                this.keywords.Add(new BookKeyword { BookId = this.Id, Text = keyword });
            }
        }
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\BookAggregate\BookAuthor.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

[DebuggerDisplay("AuthorId={AuthorId}, Name={Name}")]
public class BookAuthor : ValueObject
{
    private BookAuthor() { }

#pragma warning disable SA1202 // Elements should be ordered by access
    public BookAuthor(AuthorId authorId, PersonFormalName name, int position)
#pragma warning restore SA1202 // Elements should be ordered by access
    {
        this.AuthorId = authorId;
        this.Name = name;
        this.Position = position;
    }

    public AuthorId AuthorId { get; private set; }

    public string Name { get; private set; }

    public int Position { get; private set; }

    public static BookAuthor Create(Author author, int position)
    {
        _ = author ?? throw new ArgumentException("BookAuthor Author cannot be empty.");

        return new BookAuthor(author.Id, author.PersonName, position);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.AuthorId;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\BookAggregate\BookChapter.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

[DebuggerDisplay("Id={Id}, Number={Number}, Title={Title}")]
[TypedEntityId<Guid>]
public class BookChapter : Entity<BookChapterId>
{
    private BookChapter() { } // Private constructor required by EF Core

    private BookChapter(int number, string title, string content)
    {
        this.Number = number;
        this.SetTitle(title);
        this.SetContent(content);
    }

    public string Title { get; private set; }

    public int Number { get; private set; }

    public string Content { get; private set; }

    public static BookChapter Create(int number, string title, string content)
    {
        return new BookChapter(number, title, content);
    }

    public BookChapter SetTitle(string title)
    {
        _ = title ?? throw new ArgumentException("BookChapter Title cannot be empty.");

        this.Title = title;

        return this;
    }

    public BookChapter SetNumber(int number)
    {
        // Validate number
        this.Number = number;

        return this;
    }

    public BookChapter SetContent(string content)
    {
        // Validate content
        this.Content = content;

        return this;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\BookAggregate\BookIsbn.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

using System.Text.RegularExpressions;

[DebuggerDisplay("Value={Value}, Type={Type}")]
public partial class BookIsbn : ValueObject
{
    private static readonly Regex IsbnRegex = Regexes.IsbnRegex();
    private static readonly Regex Isbn10Regex = Regexes.Isbn10Regex();
    private static readonly Regex Isbn13Regex = Regexes.Isbn13Regex();

    private BookIsbn() { }

    private BookIsbn(string value)
    {
        Validate(value);

        this.Value = value;

        // Determine ISBN type
        if (Isbn13Regex.IsMatch(value))
        {
            this.Type = "ISBN-13";
        }
        else if (Isbn10Regex.IsMatch(value))
        {
            this.Type = "ISBN-10";
        }
    }

    public string Value { get; private set; }

    public string Type { get; private set; }

    public static implicit operator BookIsbn(string value)
    {
        return Create(value);
        // allows a String value to be implicitly converted to a BookIsbn object.
    }

    public static implicit operator string(BookIsbn isbn)
    {
        return isbn.Value;
        // allows a BookIsbn value to be implicitly converted to a String.
    }

    public static BookIsbn Create(string value)
    {
        value = value?.ToUpperInvariant()
                ?.Replace("ISBN-10", string.Empty)
                ?.Replace("ISBN-13", string.Empty)
                ?.Replace("ISBN", string.Empty)
                ?.Trim() ??
            string.Empty;

        return new BookIsbn(value);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }

    private static void Validate(string isbn)
    {
        if (string.IsNullOrEmpty(isbn))
        {
            throw new ArgumentException("ISBN cannot be empty.");
        }

        if (!IsbnRegex.IsMatch(isbn))
        {
            throw new ArgumentException("ISBN is not a valid ISBN-10 or ISBN-13.");
        }
    }

    public static partial class Regexes
    {
        [GeneratedRegex(
            @"^(?:ISBN(?:-1[03])?:?\s*)?(?=[-0-9X ]{10,17}$)(?:97[89][ -]?)?\d{1,5}[ -]?\d{1,7}[ -]?\d{1,7}[ -]?[0-9X]$",
            RegexOptions.Compiled)]
        public static partial Regex IsbnRegex();

        [GeneratedRegex(@"^\d{1,5}[\s-]?\d{1,7}[\s-]?\d{1,7}[\s-]?[0-9X]$", RegexOptions.Compiled)]
        public static partial Regex Isbn10Regex();

        [GeneratedRegex(@"^(97[89])[\s-]?\d{1,5}[\s-]?\d{1,7}[\s-]?\d{1,7}[\s-]?\d$", RegexOptions.Compiled)]
        public static partial Regex Isbn13Regex();
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\BookAggregate\BookKeyword.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

[DebuggerDisplay("BookId={BookId}, Text={Text")]
[TypedEntityId<Guid>]
public class BookKeyword : Entity<BookKeywordId>
{
    public BookId BookId { get; set; }

    public string Text { get; set; }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\BookAggregate\BookPublisher.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

[DebuggerDisplay("PublisherId={PublisherId}, Name={Name}")]
public class BookPublisher : ValueObject
{
    private BookPublisher() { }

    private BookPublisher(PublisherId publisherId, string name)
    {
        this.PublisherId = publisherId;
        this.Name = name;
    }

    public PublisherId PublisherId { get; private set; }

    public string Name { get; private set; }

    public static BookPublisher Create(Publisher publisher)
    {
        _ = publisher ?? throw new ArgumentException("BookPublisher Publisher cannot be empty.");

        return new BookPublisher(publisher.Id, publisher.Name);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.PublisherId;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\CategoryAggregate\Category.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

[DebuggerDisplay("Id={Id}, Title={Title}, Order={Order}, ParentId={Parent?.Id}")]
[TypedEntityId<Guid>]
public class Category : AuditableEntity<CategoryId>, IConcurrent // TODO: make this an aggregate root?
{
    private readonly List<Book> books = [];
    private readonly List<Category> children = [];

    private Category() { } // Private constructor required by EF Core

    private Category(TenantId tenantId, string title, string description = null, int order = 0, Category parent = null)
    {
        this.TenantId = tenantId;
        this.SetTitle(title);
        this.SetDescription(description);
        this.SetParent(parent);
        this.Order = order;
    }

    public TenantId TenantId { get; private set; }

    public string Title { get; private set; }

    public int Order { get; private set; }

    public string Description { get; private set; }

    public Category Parent { get; private set; }

    public IEnumerable<Book> Books
        => this.books.OrderBy(e => e.Title);

    public IEnumerable<Category> Children
        => this.children.OrderBy(e => e.Order).ThenBy(e => e.Title);

    /// <summary>
    ///     Gets or sets the concurrency token to handle optimistic concurrency.
    /// </summary>
    public Guid Version { get; set; }

    public static Category Create(
        TenantId tenantId,
        string title,
        string description = null,
        int order = 0,
        Category parent = null)
    {
        _ = tenantId ?? throw new ArgumentException("TenantId cannot be empty.");

        var category = new Category(tenantId, title, description, order, parent);

        // category.DomainEvents.Register(
        //     new CategoryCreatedDomainEvent(category));

        return category;
    }

    public Category SetTitle(string title)
    {
        _ = title ?? throw new ArgumentException("Category Title cannot be empty.");

        if (this.Title == title)
        {
            return this;
        }

        this.Title = title;

        // if (this.Id?.IsEmpty == false)
        // {
        //     this.DomainEvents.Register(
        //         new CategoryUpdatedDomainEvent(this), true);
        // }

        return this;
    }

    public Category SetDescription(string description)
    {
        if (this.Description == description)
        {
            return this;
        }

        this.Description = description;

        // if (this.Id?.IsEmpty == false)
        // {
        //     this.DomainEvents.Register(
        //         new CategoryUpdatedDomainEvent(this), true);
        // }

        return this;
    }

    public Category AddBook(Book book)
    {
        _ = book ?? throw new ArgumentException("Category Book cannot be empty.");

        if (this.books.Contains(book))
        {
            return this;
        }

        this.books.Add(book);

        return this;
    }

    public Category RemoveBook(Book book)
    {
        _ = book ?? throw new ArgumentException("Category Book cannot be empty.");

        this.books.Remove(book);

        return this;
    }

    public Category AddChild(Category category)
    {
        _ = category ?? throw new ArgumentException("Category cannot be empty.");

        if (this.children.Contains(category))
        {
            return this;
        }

        this.children.Add(category);
        category.SetParent(this);

        return this;
    }

    public Category RemoveChild(Category category)
    {
        _ = category ?? throw new ArgumentException("Category cannot be empty.");

        if (!this.children.Contains(category))
        {
            return this;
        }

        this.children.Remove(category);
        category.RemoveParent();

        return this;
    }

    private void SetParent(Category parent)
    {
        this.Parent = parent;

        if (parent != null)
        {
            //this.ParentId = CategoryId.Create(parent.Id.Value);
        }
    }

    private void RemoveParent()
    {
        this.Parent = null;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\CustomerAggregate\Customer.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

[TypedEntityId<Guid>]
public class Customer : AuditableAggregateRoot<CustomerId>, IConcurrent
{
    private Customer() { }

    private Customer(TenantId tenantId, PersonFormalName name, EmailAddress email, Address address = null)
    {
        this.TenantId = tenantId;
        this.SetName(name);
        this.SetEmail(email);
        this.SetAddress(address);
    }

    public TenantId TenantId { get; private set; }

    public PersonFormalName PersonName { get; private set; }

    public Address Address { get; private set; }

    public EmailAddress Email { get; private set; }

    /// <summary>
    ///     Gets or sets the concurrency token to handle optimistic concurrency.
    /// </summary>
    public Guid Version { get; set; }

    public static Customer Create(TenantId tenantId, PersonFormalName name, EmailAddress email, Address address = null)
    {
        _ = tenantId ?? throw new ArgumentException("TenantId cannot be empty.");

        var customer = new Customer(tenantId, name, email, address);

        customer.DomainEvents.Register(new CustomerCreatedDomainEvent(customer));

        return customer;
    }

    public Customer SetName(PersonFormalName name)
    {
        _ = name ?? throw new ArgumentException("Customer Name cannot be empty.");

        if (this.PersonName == name)
        {
            return this;
        }

        this.PersonName = name;

        if (this.Id?.IsEmpty == false)
        {
            this.DomainEvents.Register(new CustomerUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public Customer SetEmail(EmailAddress email)
    {
        _ = email ?? throw new ArgumentException("Customer Email cannot be empty.");

        if (email == this.Email)
        {
            return this;
        }

        this.Email = email;

        if (this.Id?.IsEmpty == false)
        {
            this.DomainEvents.Register(new CustomerUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public Customer SetAddress(Address address)
    {
        if (address == this.Address)
        {
            return this;
        }

        this.Address = address;

        if (this.Id?.IsEmpty == false)
        {
            this.DomainEvents.Register(new CustomerUpdatedDomainEvent(this), true);
        }

        return this;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\PublisherAggregate\Publisher.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

[DebuggerDisplay("Id={Id}, Name={Name}")]
[TypedEntityId<Guid>]
public class Publisher : AuditableAggregateRoot<PublisherId>, IConcurrent
{
    //private readonly List<PublisherBook> book = [];

    private Publisher() { } // Private constructor required by EF Core

    private Publisher(
        TenantId tenantId,
        string name,
        string description = null,
        EmailAddress contactEmail = null,
        Address address = null,
        Website website = null)
    {
        this.TenantId = tenantId;
        this.SetName(name);
        this.SetDescription(description);
        this.SetContactEmail(contactEmail);
        this.SetAddress(address);
        this.SetDescription(website);
    }

    public TenantId TenantId { get; private set; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public Address Address { get; private set; }

    public EmailAddress ContactEmail { get; private set; }

    public Website Website { get; private set; }

    /// <summary>
    ///     Gets or sets the concurrency token to handle optimistic concurrency.
    /// </summary>
    public Guid Version { get; set; }

    //public IEnumerable<PublisherBook> Books => this.books;

    public static Publisher Create(
        TenantId tenantId,
        string name,
        string description = null,
        EmailAddress contactEmail = null,
        Address address = null,
        Website website = null)
    {
        _ = tenantId ?? throw new ArgumentException("TenantId cannot be empty.");

        var publisher = new Publisher(tenantId, name, description, contactEmail, address, website);

        publisher.DomainEvents.Register(new PublisherCreatedDomainEvent(publisher), true);

        return publisher;
    }

    public Publisher SetName(string name)
    {
        _ = name ?? throw new ArgumentException("Publisher Name cannot be empty.");

        // Validate name
        if (this.Name == name)
        {
            return this;
        }

        this.Name = name;

        if (this.Id?.IsEmpty == false)
        {
            this.DomainEvents.Register(new PublisherUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public Publisher SetDescription(string description)
    {
        if (this.Description == description)
        {
            return this;
        }

        this.Description = description;

        if (this.Id?.IsEmpty == false)
        {
            this.DomainEvents.Register(new PublisherUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public Publisher SetContactEmail(EmailAddress email)
    {
        if (this.ContactEmail == email)
        {
            return this;
        }

        this.ContactEmail = email;

        if (this.Id?.IsEmpty == false)
        {
            this.DomainEvents.Register(new PublisherUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public Publisher SetAddress(Address address)
    {
        // Validate address
        if (this.Address == address)
        {
            return this;
        }

        this.Address = address;

        if (this.Id?.IsEmpty == false)
        {
            this.DomainEvents.Register(new PublisherUpdatedDomainEvent(this), true);
        }

        return this;
    }

    public Publisher SetWebsite(Website website)
    {
        // Validate website
        if (this.Website == website)
        {
            return this;
        }

        this.Website = website;

        if (this.Id?.IsEmpty == false)
        {
            this.DomainEvents.Register(new PublisherUpdatedDomainEvent(this), true);
        }

        return this;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Infrastructure\EntityFramework\Configurations\AuthorEntityTypeConfiguration.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Infrastructure;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using BridgingIT.DevKit.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AuthorEntityTypeConfiguration : TenantAwareEntityTypeConfiguration<Author>
{
    public override void Configure(EntityTypeBuilder<Author> builder)
    {
        base.Configure(builder);

        ConfigureAuthors(builder);
        ConfigureAuthorBooks(builder);
    }

    private static void ConfigureAuthors(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable("Authors").HasKey(e => e.Id).IsClustered(false);

        builder.Navigation(e => e.Tags).AutoInclude();

        builder.Property(e => e.Version).IsConcurrencyToken();

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(id => id.Value, value => AuthorId.Create(value));

        //builder.HasOne<Tenant>() // one-to-many with no navigations https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many#one-to-many-with-no-navigations
        //    .WithMany()
        //    .HasForeignKey(e => e.TenantId).IsRequired();
        builder.Property(e => e.TenantId).HasConversion(id => id.Value, value => TenantId.Create(value)).IsRequired();

        //builder.HasIndex(e => e.TenantId);
        //builder.HasOne("organization.Tenants") // The navigation 'organization.Tenants' cannot be added to the entity type 'Author' because there is no corresponding CLR property on the underlying type and navigations properties cannot be added in shadow state.
        //    .WithMany()
        //    .HasForeignKey(nameof(TenantId))
        //    .IsRequired()
        //    .OnDelete(DeleteBehavior.Cascade);

        builder.Property(a => a.Biography).IsRequired(false).HasMaxLength(4096);

        builder.OwnsOne(
            e => e.PersonName,
            b =>
            {
                b.Property(e => e.Title)
                    .HasColumnName("PersonNameTitle")
                    .IsRequired(false)
                    .HasMaxLength(64);
                b.Property(e => e.Parts)
                    .HasColumnName("PersonNameParts")
                    .IsRequired()
                    .HasMaxLength(1024)
                    .HasConversion(
                        parts => string.Join("|", parts),
                        value => value.Split("|", StringSplitOptions.RemoveEmptyEntries),
                        new ValueComparer<IEnumerable<string>>(
                            (c1, c2) => c1.SequenceEqual(c2),
                            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => c.AsEnumerable()));
                b.Property(e => e.Suffix).HasColumnName("PersonNameSuffix").IsRequired(false).HasMaxLength(64);
                b.Property(e => e.Full).HasColumnName("PersonNameFull").IsRequired().HasMaxLength(2048);
                b.HasIndex(e => e.Full);
            });

        builder
            .HasMany(e => e.Tags) // unidirectional many-to-many relationship https://learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many#unidirectional-many-to-many
            .WithMany()
            .UsingEntity(b => b.ToTable("AuthorTags"));

        builder.OwnsOneAuditState(); // TODO: use ToJson variant
        //builder.OwnsOne(e => e.AuditState, b => b.ToJson());

        // Configure relationships
        // Assuming a many-to-many relationship is managed through BookEntityTypeConfiguration

        builder.Metadata.FindNavigation(nameof(Author.Books)).SetPropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureAuthorBooks(EntityTypeBuilder<Author> builder)
    {
        builder.OwnsMany(
            e => e.Books,
            b =>
            {
                b.ToTable("AuthorBooks").HasKey("AuthorId", "BookId");
                b.HasIndex("AuthorId", "BookId");

                b.WithOwner().HasForeignKey("AuthorId");

                b.Property(r => r.BookId).IsRequired().HasConversion(id => id.Value, value => BookId.Create(value));
                b.HasOne(typeof(Book)).WithMany().HasForeignKey(nameof(BookId)); // FK -> Book.Id

                b.Property(r => r.Title).IsRequired().HasMaxLength(512);
            });
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Infrastructure\EntityFramework\Configurations\BookEntityTypeConfiguration.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Infrastructure;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using BridgingIT.DevKit.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class BookEntityTypeConfiguration
    : TenantAwareEntityTypeConfiguration<Book>, IEntityTypeConfiguration<BookKeyword>
{
    public override void Configure(EntityTypeBuilder<Book> builder)
    {
        base.Configure(builder);

        ConfigureBooks(builder);
        ConfigureBookAuthors(builder);
        ConfigureBookCategories(builder);
        ConfigureBookChapters(builder);
        ConfigureBookPublisher(builder);
    }

    public void Configure(EntityTypeBuilder<BookKeyword> builder)
    {
        ConfigureBookKeywords(builder);
    }

    private static void ConfigureBooks(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books").HasKey(e => e.Id).IsClustered(false);

        //builder.Navigation(e => e.BookAuthors).AutoInclude();
        builder.Navigation(e => e.Categories).AutoInclude();
        builder.Navigation(e => e.Chapters).AutoInclude();
        builder.Navigation(e => e.Tags).AutoInclude();
        builder.Navigation(e => e.Keywords).AutoInclude();

        builder.Property(e => e.Version).IsConcurrencyToken();

        builder.Property(e => e.Id).ValueGeneratedOnAdd()
            .HasConversion(id => id.Value, value => BookId.Create(value));

        //builder.HasOne<Tenant>() // one-to-many with no navigations https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many#one-to-many-with-no-navigations
        //    .WithMany()
        //    .HasForeignKey(e => e.TenantId).IsRequired();
        builder.Property(e => e.TenantId)
            .HasConversion(id => id.Value, value => TenantId.Create(value)).IsRequired();

        //builder.HasIndex(e => e.TenantId);
        //builder.HasOne("organization.Tenants")
        //    .WithMany()
        //    .HasForeignKey(nameof(TenantId))
        //    .IsRequired();

        builder.Property(e => e.Title).IsRequired().HasMaxLength(512);

        builder.Property(e => e.Edition).IsRequired(false).HasMaxLength(256);

        builder.Property(e => e.Description).IsRequired(false);

        builder.Property(e => e.PublishedDate).IsRequired(false);

        builder.Property(e => e.Sku)
            .HasConversion(sku => sku.Value, value => ProductSku.Create(value))
            .IsRequired()
            .HasMaxLength(12);
        builder.HasIndex(nameof(Book.TenantId), nameof(Book.Sku)).IsUnique();

        builder.Property(e => e.Isbn)
            .HasConversion(isbn => isbn.Value, value => BookIsbn.Create(value))
            .IsRequired()
            .HasMaxLength(32);
        builder.HasIndex(nameof(Book.TenantId), nameof(Book.Isbn)).IsUnique();

        // builder.OwnsOne(
        //     e => e.Isbn,
        //     b =>
        //     {
        //         b.Property(e => e.Value).HasColumnName("Isbn").IsRequired().HasMaxLength(32);
        //         b.Property(e => e.Type).HasColumnName("IsbnType").IsRequired(false).HasMaxLength(32);
        //         b.HasIndex(nameof(Book.TenantId), nameof(BookIsbn.Value)).IsUnique();
        //     });
        // builder.Navigation(e => e.Isbn).IsRequired();

        builder.OwnsOne(
            e => e.AverageRating,
            b =>
            {
                b.Property(e => e.Value)
                    .HasColumnName("AverageRating")
                    // .HasDefaultValue(0m)
                    .IsRequired(false)
                    .HasColumnType("decimal(5,2)");

                b.Property(e => e.Amount).HasColumnName("AverageRatingAmount").HasDefaultValue(0).IsRequired();
            });
        builder.Navigation(e => e.AverageRating).IsRequired();

        builder.OwnsOne(
            e => e.Price,
            b =>
            {
                b.Property(e => e.Amount)
                    .HasColumnName("Price")
                    .HasDefaultValue(0)
                    .IsRequired()
                    .HasColumnType("decimal(5,2)");

                b.OwnsOne(
                    e => e.Currency,
                    b =>
                    {
                        b.Property(e => e.Code)
                            .HasColumnName("PriceCurrency")
                            .HasDefaultValue("USD")
                            .IsRequired()
                            .HasMaxLength(8);
                    });
            });

        builder
            .HasMany(e => e.Tags) // unidirectional many-to-many relationship https://learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many#unidirectional-many-to-many
            .WithMany()
            .UsingEntity(b => b.ToTable("BookTags"));

        builder.HasMany(b => b.Keywords)
            .WithOne()
            .HasForeignKey(ki => ki.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOneAuditState(); // TODO: use ToJson variant
        //builder.OwnsOne(e => e.AuditState, b => b.ToJson());

        builder.Metadata.FindNavigation(nameof(Book.Authors)).SetPropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureBookAuthors(EntityTypeBuilder<Book> builder)
    {
        builder.OwnsMany(
            e => e.Authors,
            b =>
            {
                b.ToTable("BookAuthors").HasKey("BookId", "AuthorId");
                b.HasIndex("BookId", "AuthorId");

                b.WithOwner().HasForeignKey("BookId");

                b.Property(r => r.AuthorId).IsRequired().HasConversion(id => id.Value, value => AuthorId.Create(value));
                b.HasOne(typeof(Author)).WithMany().HasForeignKey(nameof(AuthorId)); // FK -> Author.Id

                b.Property(r => r.Name).IsRequired().HasMaxLength(2048);

                b.Property(r => r.Position).IsRequired().HasDefaultValue(0);
            });
    }

    private static void ConfigureBookCategories(EntityTypeBuilder<Book> builder)
    {
        // https://learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many#many-to-many-with-join-table-foreign-key-names
        builder.HasMany(e => e.Categories)
            .WithMany(e => e.Books)
            .UsingEntity(
                "BookCategories",
                l => l.HasOne(typeof(Category)).WithMany().HasForeignKey(nameof(CategoryId)),
                r => r.HasOne(typeof(Book)).WithMany().HasForeignKey(nameof(BookId)));
    }

    private static void ConfigureBookChapters(EntityTypeBuilder<Book> builder)
    {
        builder.OwnsMany(
            e => e.Chapters,
            b =>
            {
                b.ToTable("BookChapters");
                b.WithOwner().HasForeignKey("BookId");
                b.HasKey("Id", "BookId");

                b.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .HasConversion(id => id.Value, value => BookChapterId.Create(value));

                b.Property("Title").IsRequired().HasMaxLength(256);
                b.Property("Content").IsRequired(false);
            });
    }

    private static void ConfigureBookPublisher(EntityTypeBuilder<Book> builder)
    {
        builder.OwnsOne(
            e => e.Publisher,
            b =>
            {
                b.Property(r => r.PublisherId)
                    .HasColumnName("PublisherId")
                    .IsRequired()
                    .HasConversion(id => id.Value, value => PublisherId.Create(value));
                b.HasOne(typeof(Publisher)).WithMany().HasForeignKey(nameof(PublisherId)); // FK -> Publisher.Id

                b.Property(e => e.Name).HasColumnName("PublisherName").IsRequired().HasMaxLength(512);
            });
    }

    private static void ConfigureBookKeywords(EntityTypeBuilder<BookKeyword> builder)
    {
        builder.ToTable("BookKeywords").HasKey(e => e.Id).IsClustered(false);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(id => id.Value, value => BookKeywordId.Create(value));

        builder.Property(e => e.Text).IsRequired().HasMaxLength(128);

        builder.HasIndex(e => e.Text);

        builder.HasIndex(e => new { e.BookId, e.Text }).IsUnique();

        builder.HasOne<Book>().WithMany(e => e.Keywords).HasForeignKey(e => e.BookId).OnDelete(DeleteBehavior.Cascade);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Infrastructure\EntityFramework\Configurations\CategoryEntityTypeConfiguration.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Infrastructure;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using BridgingIT.DevKit.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CategoryEntityTypeConfiguration : TenantAwareEntityTypeConfiguration<Category>
{
    public override void Configure(EntityTypeBuilder<Category> builder)
    {
        base.Configure(builder);

        builder.ToTable("Categories").HasKey(e => e.Id).IsClustered(false);

        builder.Navigation(e => e.Parent).AutoInclude(false);
        //builder.Navigation(e => e.Children).AutoInclude();

        builder.Property(e => e.Version).IsConcurrencyToken();

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(id => id.Value, value => CategoryId.Create(value));

        //builder.HasOne<Tenant>() // one-to-many with no navigations https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many#one-to-many-with-no-navigations
        //    .WithMany()
        //    .HasForeignKey(e => e.TenantId).IsRequired();
        builder.Property(e => e.TenantId).HasConversion(id => id.Value, value => TenantId.Create(value)).IsRequired();
        //builder.HasIndex(e => e.TenantId);
        //builder.HasOne("organization.Tenants")
        //    .WithMany()
        //    .HasForeignKey(nameof(TenantId))
        //    .IsRequired();

        builder.Property(e => e.Title).IsRequired().HasMaxLength(512);

        builder.Property(e => e.Description).IsRequired(false);

        builder.Property(e => e.Order).IsRequired().HasDefaultValue(0);

        builder.HasMany(c => c.Children).WithOne(c => c.Parent).OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOneAuditState(); // TODO: use ToJson variant
        //builder.OwnsOne(e => e.AuditState, b => b.ToJson());
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Infrastructure\EntityFramework\Configurations\CustomerEntityTypeConfiguration.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Infrastructure;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using BridgingIT.DevKit.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CustomerEntityTypeConfiguration : TenantAwareEntityTypeConfiguration<Customer>
{
    public override void Configure(EntityTypeBuilder<Customer> builder)
    {
        base.Configure(builder);

        ConfigureCustomersTable(builder);
    }

    private static void ConfigureCustomersTable(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers").HasKey(d => d.Id).IsClustered(false);

        builder.Property(e => e.Version).IsConcurrencyToken();

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(id => id.Value, value => CustomerId.Create(value));

        //builder.HasOne<Tenant>() // one-to-many with no navigations https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many#one-to-many-with-no-navigations
        //    .WithMany()
        //    .HasForeignKey(e => e.TenantId).IsRequired();
        builder.Property(e => e.TenantId).HasConversion(id => id.Value, value => TenantId.Create(value)).IsRequired();
        //builder.HasIndex(e => e.TenantId);
        //builder.HasOne("organization.Tenants")
        //    .WithMany()
        //    .HasForeignKey(nameof(TenantId))
        //    .IsRequired();

        builder.OwnsOne(
            e => e.PersonName,
            b =>
            {
                b.Property(e => e.Title)
                    .HasColumnName("PersonNameTitle").IsRequired(false).HasMaxLength(64);
                b.Property(e => e.Parts)
                    .HasColumnName("PersonNameParts")
                    .IsRequired()
                    .HasMaxLength(1024)
                    .HasConversion(
                        parts => string.Join("|", parts),
                        value => value.Split("|", StringSplitOptions.RemoveEmptyEntries),
                        new ValueComparer<IEnumerable<string>>(
                            (c1, c2) => c1.SequenceEqual(c2),
                            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => c.AsEnumerable()));
                b.Property(e => e.Suffix).HasColumnName("PersonNameSuffix").IsRequired(false).HasMaxLength(64);
                b.Property(e => e.Full).HasColumnName("PersonNameFull").IsRequired().HasMaxLength(2048);
            });

        builder.OwnsOne(
            e => e.Address,
            b =>
            {
                b.Property(e => e.Name).HasColumnName("AddressName").HasMaxLength(512).IsRequired();

                b.Property(e => e.Line1).HasColumnName("AddressLine1").HasMaxLength(256).IsRequired();

                b.Property(e => e.Line2).HasColumnName("AddressLine2").HasMaxLength(256);

                b.Property(e => e.City).HasColumnName("AddressCity").HasMaxLength(128).IsRequired();

                b.Property(e => e.PostalCode).HasColumnName("AddressPostalCode").HasMaxLength(32).IsRequired();

                b.Property(e => e.Country).HasColumnName("AddressCountry").HasMaxLength(128).IsRequired();
            });

        builder.Property(e => e.Email)
            .HasConversion(email => email.Value, value => EmailAddress.Create(value))
            .IsRequired()
            .HasMaxLength(256);
        builder.HasIndex(nameof(Customer.TenantId), nameof(Customer.Email)).IsUnique();

        builder.OwnsOneAuditState(); // TODO: use ToJson variant
        //builder.OwnsOne(e => e.AuditState, b => b.ToJson());
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Infrastructure\EntityFramework\Configurations\PublisherEntityTypeConfiguration.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Infrastructure;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using BridgingIT.DevKit.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PublisherEntityTypeConfiguration : TenantAwareEntityTypeConfiguration<Publisher>
{
    public override void Configure(EntityTypeBuilder<Publisher> builder)
    {
        base.Configure(builder);

        ConfigurePublishersTable(builder);
    }

    private static void ConfigurePublishersTable(EntityTypeBuilder<Publisher> builder)
    {
        builder.ToTable("Publishers").HasKey(d => d.Id).IsClustered(false);

        builder.Property(e => e.Version).IsConcurrencyToken();

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(id => id.Value, value => PublisherId.Create(value));

        //builder.HasOne<Tenant>() // one-to-many with no navigations https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many#one-to-many-with-no-navigations
        //    .WithMany()
        //    .HasForeignKey(e => e.TenantId).IsRequired();
        builder.Property(e => e.TenantId).HasConversion(id => id.Value, value => TenantId.Create(value)).IsRequired();
        //builder.HasIndex(e => e.TenantId);
        //builder.HasOne("organization.Tenants")
        //    .WithMany()
        //    .HasForeignKey(nameof(TenantId))
        //    .IsRequired();

        builder.Property(e => e.Name).IsRequired().HasMaxLength(512);

        builder.OwnsOne(
            e => e.Address,
            b =>
            {
                b.Property(e => e.Name).HasColumnName("AddressName").HasMaxLength(512).IsRequired();

                b.Property(e => e.Line1).HasColumnName("AddressLine1").HasMaxLength(256).IsRequired();

                b.Property(e => e.Line2).HasColumnName("AddressLine2").HasMaxLength(256);

                b.Property(e => e.City).HasColumnName("AddressCity").HasMaxLength(128).IsRequired();

                b.Property(e => e.PostalCode).HasColumnName("AddressPostalCode").HasMaxLength(32).IsRequired();

                b.Property(e => e.Country).HasColumnName("AddressCountry").HasMaxLength(128).IsRequired();
            });

        builder.Property(e => e.ContactEmail)
            .HasConversion(email => email.Value, value => EmailAddress.Create(value))
            .IsRequired(false)
            .HasMaxLength(256);
        builder.HasIndex(nameof(Publisher.TenantId), nameof(Publisher.ContactEmail)).IsUnique();

        builder.OwnsOne(
            e => e.Website,
            b =>
            {
                b.Property(e => e.Value).HasColumnName(nameof(Publisher.Website)).IsRequired(false).HasMaxLength(512);
            });
        builder.Navigation(e => e.Website).IsRequired();

        builder.OwnsOneAuditState(); // TODO: use ToJson variant
        //builder.OwnsOne(e => e.AuditState, b => b.ToJson());
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Infrastructure\EntityFramework\Configurations\TagEntityTypeConfiguration.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Infrastructure;

using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TagEntityTypeConfiguration : TenantAwareEntityTypeConfiguration<Tag>
{
    public override void Configure(EntityTypeBuilder<Tag> builder)
    {
        base.Configure(builder);

        builder.ToTable("Tags").HasKey(e => e.Id).IsClustered(false);

        builder.Property(e => e.Id).ValueGeneratedOnAdd().HasConversion(id => id.Value, value => TagId.Create(value));

        //builder.HasOne<Tenant>() // one-to-many with no navigations https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many#one-to-many-with-no-navigations
        //    .WithMany()
        //    .HasForeignKey(e => e.TenantId).IsRequired();
        builder.Property(e => e.TenantId)
            .HasConversion(id => id.Value, value => TenantId.Create(value)).IsRequired();
        //builder.HasIndex(e => e.TenantId);
        //builder.HasOne("organization.Tenants")
        //    .WithMany()
        //    .HasForeignKey(nameof(TenantId))
        //    .IsRequired();

        builder.Property(e => e.Name).IsRequired().HasMaxLength(128);

        builder.Property(e => e.Category).IsRequired(false).HasMaxLength(128);

        builder.HasIndex(nameof(Tag.TenantId));
        builder.HasIndex(nameof(Tag.Name));
        builder.HasIndex(nameof(Tag.Category));
        builder.HasIndex(nameof(Tag.TenantId), nameof(Tag.Name), nameof(Tag.Category)).IsUnique();
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Infrastructure\EntityFramework\Configurations\TenantAwareEntityTypeConfiguration.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Infrastructure;

using BridgingIT.DevKit.Domain.Model;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public abstract class TenantAwareEntityTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : class, IEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property<TenantId>("TenantId")
            .HasConversion(id => id.Value, value => TenantId.Create(value))
            .IsRequired();

        builder.HasIndex("TenantId");

        builder.HasOne<TenantReference>()
            .WithMany()
            .HasForeignKey("TenantId")
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class TenantReferenceEntityTypeConfiguration : IEntityTypeConfiguration<TenantReference>
{
    public void Configure(EntityTypeBuilder<TenantReference> builder)
    {
        builder.ToTable("Tenants", "organization").HasKey(e => e.Id).IsClustered(false);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(id => id.Value, value => TenantId.Create(value));

        builder.ToTable(tb => tb.ExcludeFromMigrations());
    }
}

public class TenantReference
{
    public TenantId Id { get; set; }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Presentation\Web\Controllers\CustomersController.cs
// ----------------------------------------
// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license
//
// namespace BridgingIT.DevKit.Examples.BookFiesta.Presentation.Web.Controllers;
//
// using System.Threading;
// using BridgingIT.DevKit.Common;
// using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;
// using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
// using BridgingIT.DevKit.Presentation.Web;
// using MediatR;
// using Microsoft.AspNetCore.Mvc;
//
// [ApiController]
// [Route("api/tenants/{tenantId}/[controller]")]
// public class CustomersController(IMapper mapper, IMediator mediator) : ControllerBase // TODO: use the new IEndpoints from bitdevkit, see Maps below
// {
//     private readonly IMediator mediator = mediator;
//     private readonly IMapper mapper = mapper;
//
//     [HttpGet("{id}", Name = nameof(Get))]
//     public async Task<ActionResult<CustomerModel>> Get(string tenantId, string id, CancellationToken cancellationToken)
//     {
//         var result = (await this.mediator.Send(
//             new CustomerFindOneQuery(tenantId, id), cancellationToken)).Result;
//         return result.ToOkActionResult<Customer, CustomerModel>(this.mapper);
//     }
//
//     [HttpGet]
//     public async Task<ActionResult<ICollection<CustomerModel>>> GetAll(string tenantId, CancellationToken cancellationToken)
//     {
//         var result = (await this.mediator.Send(
//             new CustomerFindAllQuery(tenantId), cancellationToken)).Result;
//         return result.ToOkActionResult<Customer, CustomerModel>(this.mapper);
//     }
//
//     [HttpPost]
//     public async Task<ActionResult<CustomerModel>> PostAsync(string tenantId, [FromBody] CustomerModel model, CancellationToken cancellationToken)
//     {
//         if (tenantId != model.TenantId)
//         {
//             return new BadRequestObjectResult(null);
//         }
//
//         var result = (await this.mediator.Send(
//             this.mapper.Map<CustomerModel, CustomerCreateCommand>(model), cancellationToken)).Result;
//         return result.ToCreatedActionResult<Customer, CustomerModel>(this.mapper, nameof(this.Get), new { id = result.Value?.Id });
//     }
//
//     [HttpPut("{id}")]
//     public async Task<ActionResult<CustomerModel>> PutAsync(string tenantId, string id, [FromBody] CustomerModel model, CancellationToken cancellationToken)
//     {
//         if (tenantId != model.TenantId || id != model.Id)
//         {
//             return new BadRequestObjectResult(null);
//         }
//
//         var result = (await this.mediator.Send(
//             this.mapper.Map<CustomerModel, CustomerUpdateCommand>(model), cancellationToken)).Result;
//         return result.ToUpdatedActionResult<Customer, CustomerModel>(this.mapper, nameof(this.Get), new { id = result.Value?.Id });
//     }
//
//     [HttpDelete("{id}")]
//     public async Task<ActionResult<CustomerModel>> DeleteAsync(string tenantId, string id, CancellationToken cancellationToken)
//     {
//         var result = (await this.mediator.Send(new CustomerDeleteCommand(tenantId, id), cancellationToken)).Result;
//         return result.ToDeletedActionResult<CustomerModel>(); // TODO: remove generic CustomerModel
//     }
// }
//
// //app.MapGet("/api/customers/{id}", async(string id, IMediator mediator, IMapper mapper, CancellationToken cancellationToken) =>
// //{
// //    var result = (await mediator.Send(new CustomerFindOneQuery(id), cancellationToken)).Result;
// //    return result.ToOkActionResult<Customer, CustomerModel>(mapper);
// //}).WithName("GetCustomer");
//
// //// Endpoint for GetAll action
// //app.MapGet("/api/customers", async (IMediator mediator, IMapper mapper, CancellationToken cancellationToken) =>
// //{
// //    var result = (await mediator.Send(new CustomerFindAllQuery(), cancellationToken)).Result;
// //    return result.ToOkActionResult<Customer, CustomerModel>(mapper);
// //});
//
// //// Endpoint for PostAsync action
// //app.MapPost("/api/customers", async (CustomerModel model, IMediator mediator, IMapper mapper, CancellationToken cancellationToken) =>
// //{
// //    var result = (await mediator.Send(mapper.Map<CustomerModel, CustomerCreateCommand>(model), cancellationToken)).Result;
// //    return result.ToCreatedActionResult<Customer, CustomerModel>(mapper, "GetCustomer", new { id = result.Value?.Id });
// //});
//
// //// Endpoint for PutAsync action
// //app.MapPut("/api/customers/{id}", async (string id, CustomerModel model, IMediator mediator, IMapper mapper, CancellationToken cancellationToken) =>
// //{
// //    var result = (await mediator.Send(mapper.Map<CustomerModel, CustomerUpdateCommand>(model), cancellationToken)).Result;
// //    return result.ToUpdatedActionResult<Customer, CustomerModel>(mapper, "GetCustomer", new { id = result.Value?.Id });
// //});
//
// //// Endpoint for DeleteAsync action
// //app.MapDelete("/api/customers/{id}", async (string id, IMediator mediator, CancellationToken cancellationToken) =>
// //{
// //    var result = (await mediator.Send(new CustomerDeleteCommand { Id = id }, cancellationToken)).Result;
// //    return result.ToDeletedActionResult<CustomerModel>();
// //});


// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Presentation\Web\Endpoints\CatalogAuthorEndpoints.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.AuthorFiesta.Modules.Catalog.Presentation.Web;

using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using BridgingIT.DevKit.Presentation.Web;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

public class CatalogAuthorEndpoints : EndpointsBase
{
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/tenants/{tenantId}/catalog/authors")
            .WithGroupName("Catalog/Authors")
            .WithTags("Catalog");

        group.MapGet("/{id}", GetAuthor)
            .WithName("GetCatalogAuthor")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet("/", GetAuthors)
            .WithName("GetCatalogAuthors")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapPost("/", CreateAuthor)
            .WithName("CreateCatalogAuthor")
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);
    }

    private static async Task<Results<Ok<AuthorModel>, NotFound, ProblemHttpResult>> GetAuthor(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new AuthorFindOneQuery(tenantId, id))).Result;

        return result.IsFailure && result.HasError<NotFoundResultError>()
            ? TypedResults.NotFound()
            : result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Author, AuthorModel>(result.Value))
                : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<AuthorModel>>, ProblemHttpResult>> GetAuthors(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId)
    {
        var result = (await mediator.Send(new AuthorFindAllQuery(tenantId))).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Author>, IEnumerable<AuthorModel>>(result.Value))
            : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static Task<Results<Created<AuthorModel>, ProblemHttpResult>> CreateAuthor(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromBody] AuthorModel model)
    {
        throw new NotImplementedException();
        // var result = (await mediator.Send(new AuthorCreateCommand(tenantId, model))).Result;
        //
        // return result.IsSuccess
        //     ? TypedResults.Created(
        //         $"api/tenants/{tenantId}/catalog/books/{result.Value.Id}",
        //         mapper.Map<Author, AuthorModel>(result.Value))
        //     : TypedResults.Problem(result.ToString(), statusCode: 400);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Presentation\Web\Endpoints\CatalogBookEndpoints.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Presentation.Web;

using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using BridgingIT.DevKit.Presentation.Web;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

public class CatalogBookEndpoints : EndpointsBase
{
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/tenants/{tenantId}/catalog/books")
            .WithGroupName("Catalog/Books")
            .WithTags("Catalog");

        group.MapGet("/{id}", GetBook)
            .WithName("GetCatalogBook")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet("/", GetBooks)
            .WithName("GetCatalogBooks")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapPost("/", CreateBook)
            .WithName("CreateCatalogBook")
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);
    }

    private static async Task<Results<Ok<BookModel>, NotFound, ProblemHttpResult>> GetBook(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new BookFindOneQuery(tenantId, id))).Result;

        return result.IsFailure && result.HasError<NotFoundResultError>()
            ? TypedResults.NotFound()
            : result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Book, BookModel>(result.Value))
                : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<BookModel>>, ProblemHttpResult>> GetBooks(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId)
    {
        var result = (await mediator.Send(new BookFindAllQuery(tenantId))).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Book>, IEnumerable<BookModel>>(result.Value))
            : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Created<BookModel>, ProblemHttpResult>> CreateBook(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromBody] BookModel model)
    {
        var result = (await mediator.Send(new BookCreateCommand(tenantId, model))).Result;

        return result.IsSuccess
            ? TypedResults.Created(
                $"api/tenants/{tenantId}/catalog/books/{result.Value.Id}",
                mapper.Map<Book, BookModel>(result.Value))
            : TypedResults.Problem(result.ToString(), statusCode: 400);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Presentation\Web\Endpoints\CatalogCategoryEndpoints.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Presentation.Web;

using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using BridgingIT.DevKit.Presentation.Web;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

public class CatalogCategoryEndpoints : EndpointsBase
{
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/tenants/{tenantId}/catalog/categories")
            .WithGroupName("Catalog/Categories")
            .WithTags("Catalog");

        group.MapGet("/{id}", GetCategory)
            .WithName("GetCatalogCategory")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet("/{id}/books", GetCategoryBooks)
            .WithName("GetCatalogCategoryBooks")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet(string.Empty, GetCategories)
            .WithName("GetCatalogCategories")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);
    }

    private static async Task<Results<Ok<CategoryModel>, NotFound, ProblemHttpResult>> GetCategory(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new CategoryFindOneQuery(tenantId, id))).Result;

        return result.IsFailure && result.HasError<NotFoundResultError>()
            ? TypedResults.NotFound()
            : result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Category, CategoryModel>(result.Value))
                : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<BookModel>>, NotFound, ProblemHttpResult>> GetCategoryBooks(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new BookFindAllForCategoryQuery(tenantId, id))).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Book>, IEnumerable<BookModel>>(result.Value))
            : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<CategoryModel>>, ProblemHttpResult>> GetCategories(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId)
    {
        var result = (await mediator.Send(new CategoryFindAllQuery(tenantId))).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Category>, IEnumerable<CategoryModel>>(result.Value))
            : TypedResults.Problem(result.ToString(), statusCode: 400);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Presentation\Web\Endpoints\CatalogCustomerEndpoints.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Presentation.Web;

using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using BridgingIT.DevKit.Presentation.Web;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

public class CatalogCustomerEndpoints : EndpointsBase
{
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/tenants/{tenantId}/catalog/customers")
            .WithGroupName("Catalog/Customers")
            .WithTags("Catalog");

        group.MapGet("/{id}", GetCustomer)
            .WithName("GetCatalogCustomer")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet(string.Empty, GetCustomers)
            .WithName("GetCatalogCustomers")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapPost(string.Empty, CreateCustomer)
            .WithName("CreateCatalogCustomer")
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapPut("/{id}", UpdateCustomer)
            .WithName("UpdateCatalogCustomer")
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapDelete("/{id}", DeleteCustomer)
            .WithName("DeleteCatalogCustomer")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);
    }

    private static async Task<Results<Ok<CustomerModel>, NotFound, ProblemHttpResult>> GetCustomer(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new CustomerFindOneQuery(tenantId, id))).Result;

        return result.IsFailure && result.HasError<NotFoundResultError>()
            ? TypedResults.NotFound()
            : result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Customer, CustomerModel>(result.Value))
                : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<CustomerModel>>, ProblemHttpResult>> GetCustomers(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId)
    {
        var result = (await mediator.Send(new CustomerFindAllQuery(tenantId))).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Customer>, IEnumerable<CustomerModel>>(result.Value))
            : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Created<CustomerModel>, ProblemHttpResult>> CreateCustomer(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromBody] CustomerModel model)
    {
        var result = (await mediator.Send(new CustomerCreateCommand(tenantId, model))).Result;

        return result.IsSuccess
            ? TypedResults.Created(
                $"api/tenants/{tenantId}/catalog/customers/{result.Value.Id}",
                mapper.Map<Customer, CustomerModel>(result.Value))
            : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Ok<CustomerModel>, NotFound, ProblemHttpResult>> UpdateCustomer(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string id,
        [FromBody] CustomerModel model)
    {
        var result = (await mediator.Send(new CustomerUpdateCommand(tenantId, model))).Result;

        return result.IsFailure && result.HasError<NotFoundResultError>()
            ? TypedResults.NotFound()
            : result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Customer, CustomerModel>(result.Value))
                : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Ok, NotFound, ProblemHttpResult>> DeleteCustomer(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new CustomerDeleteCommand(tenantId, id))).Result;

        return result.IsFailure && result.HasError<NotFoundResultError>()
            ? TypedResults.NotFound()
            : result.IsSuccess
                ? TypedResults.Ok()
                : TypedResults.Problem(result.ToString(), statusCode: 400);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Presentation\Web\Endpoints\CatalogPublisherEndpoints.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Presentation.Web;

using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using BridgingIT.DevKit.Presentation.Web;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

public class CatalogPublisherEndpoints : EndpointsBase
{
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/tenants/{tenantId}/catalog/publishers")
            .WithGroupName("Catalog/Publishers")
            .WithTags("Catalog");

        group.MapGet("/{id}", GetPublisher)
            .WithName("GetCatalogPublisher")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet("/{id}/books", GetPublisherBooks)
            .WithName("GetCatalogPublisherBooks")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet(string.Empty, GetPublishers)
            .WithName("GetCatalogPublishers")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);
    }

    private static async Task<Results<Ok<PublisherModel>, NotFound, ProblemHttpResult>> GetPublisher(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new PublisherFindOneQuery(tenantId, id))).Result;

        return result.IsFailure && result.HasError<NotFoundResultError>()
            ? TypedResults.NotFound()
            : result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Publisher, PublisherModel>(result.Value))
                : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<BookModel>>, NotFound, ProblemHttpResult>> GetPublisherBooks(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new BookFindAllForPublisherQuery(tenantId, id))).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Book>, IEnumerable<BookModel>>(result.Value))
            : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<PublisherModel>>, ProblemHttpResult>> GetPublishers(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId)
    {
        var result = (await mediator.Send(new PublisherFindAllQuery(tenantId))).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Publisher>, IEnumerable<PublisherModel>>(result.Value))
            : TypedResults.Problem(result.ToString(), statusCode: 400);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Domain\Model\StockAggregate\Stock.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

[DebuggerDisplay("Id={Id}, Sku={Sku}, QuantityOnHand={QuantityOnHand}")]
[TypedEntityId<Guid>]
public class Stock : AuditableAggregateRoot<StockId>, IConcurrent
{
    private readonly List<StockMovement> movements = [];
    private readonly List<StockAdjustment> adjustments = [];

    private Stock() { } // Private constructor required by EF Core

    private Stock(
        TenantId tenantId,
        ProductSku sku,
        int quantityOnHand,
        int reorderThreshold,
        int reorderQuantity,
        Money unitCost,
        StorageLocation location)
    {
        this.TenantId = tenantId;
        this.Sku = sku;
        this.QuantityOnHand = quantityOnHand;
        this.QuantityReserved = 0;
        this.ReorderThreshold = reorderThreshold;
        this.ReorderQuantity = reorderQuantity;
        this.UnitCost = unitCost;
        this.Location = location;
        this.LastRestockedAt = DateTime.UtcNow;
    }

    public TenantId TenantId { get; }

    public ProductSku Sku { get; private set; }

    public int QuantityOnHand { get; private set; }

    public int QuantityReserved { get; private set; }

    public int ReorderThreshold { get; private set; }

    public int ReorderQuantity { get; private set; }

    public Money UnitCost { get; private set; }

    public StorageLocation Location { get; private set; }

    public DateTimeOffset? LastRestockedAt { get; private set; }

    public IEnumerable<StockMovement> Movements =>
        this.movements.OrderBy(m => m.Timestamp);

    public IEnumerable<StockAdjustment> Adjustments =>
        this.adjustments.OrderBy(m => m.Timestamp);

    public Guid Version { get; set; }

    public static Stock Create(
        TenantId tenantId,
        ProductSku sku,
        int quantityOnHand,
        int reorderThreshold,
        int reorderQuantity,
        Money unitCost,
        StorageLocation storageLocation)
    {
        _ = tenantId ?? throw new ArgumentException("TenantId cannot be empty.");
        _ = sku ?? throw new ArgumentException("ProductSku cannot be empty.");
        _ = unitCost ?? throw new ArgumentException("UnitCost cannot be empty.");
        _ = storageLocation ?? throw new ArgumentException("StorageLocation cannot be empty.");

        var stock = new Stock(tenantId, sku, quantityOnHand, reorderThreshold, reorderQuantity, unitCost, storageLocation);

        stock.DomainEvents.Register(new StockCreatedDomainEvent(stock));

        return stock;
    }

    public Stock AdjustQuantity(int quantityChange, string reason)
    {
        if (this.QuantityOnHand + quantityChange < 0)
        {
            throw new DomainRuleException("Stock adjustment would result in negative quantity.");
        }

        var oldQuantity = this.QuantityOnHand;
        this.QuantityOnHand += quantityChange;

        this.adjustments.Add(
            StockAdjustment.CreateQuantityAdjustment(this.Id, quantityChange, reason));

        this.DomainEvents.Register(
            new StockQuantityAdjustedDomainEvent(this, oldQuantity, this.QuantityOnHand, quantityChange, reason));

        return this;
    }

    public Stock AdjustUnitCost(Money newUnitCost, string reason)
    {
        if (newUnitCost == null || newUnitCost.Amount <= 0)
        {
            throw new DomainRuleException("New unit cost must be a positive value.");
        }

        if (this.UnitCost == newUnitCost)
        {
            return this;
        }

        var oldUnitCost = this.UnitCost;
        this.UnitCost = newUnitCost;

        this.adjustments.Add(
            StockAdjustment.CreateUnitCostAdjustment(this.Id, oldUnitCost, newUnitCost, reason));

        this.DomainEvents.Register(
            new StockUnitCostAdjustedDomainEvent(this, oldUnitCost, newUnitCost, reason));

        return this;
    }

    public Stock AddStock(int quantity, string reason = null)
    {
        if (quantity <= 0)
        {
            throw new DomainRuleException("Quantity to add must be positive.");
        }

        this.QuantityOnHand += quantity;
        this.LastRestockedAt = DateTime.UtcNow;

        this.movements.Add(
            StockMovement.Create(this.Id, quantity, StockMovementType.Addition, reason ?? "Stock addition"));

        this.DomainEvents.Register(new StockUpdatedDomainEvent(this));

        return this;
    }

    public Stock RemoveStock(int quantity, string reason = null)
    {
        if (quantity <= 0)
        {
            throw new DomainRuleException("Quantity to remove must be positive.");
        }

        if (quantity > this.QuantityOnHand - this.QuantityReserved)
        {
            throw new DomainRuleException("Not enough available stock to remove.");
        }

        this.QuantityOnHand -= quantity;

        this.movements.Add(
            StockMovement.Create(this.Id, -quantity, StockMovementType.Removal, reason ?? "Stock removal"));

        this.DomainEvents.Register(new StockUpdatedDomainEvent(this));

        return this;
    }

    public Stock ReserveStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainRuleException("Quantity to reserve must be positive.");
        }

        if (quantity > this.QuantityOnHand - this.QuantityReserved)
        {
            throw new DomainRuleException("Not enough available stock to reserve.");
        }

        this.QuantityReserved += quantity;

        this.DomainEvents.Register(new StockReservedDomainEvent(this, quantity));

        return this;
    }

    public Stock ReleaseReservedStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainRuleException("Quantity to release must be positive.");
        }

        if (quantity > this.QuantityReserved)
        {
            throw new DomainRuleException("Not enough reserved stock to release.");
        }

        this.QuantityReserved -= quantity;

        this.DomainEvents.Register(new StockReservedReleasedDomainEvent(this, quantity));

        return this;
    }

    public Stock UpdateReorderInfo(int threshold, int quantity)
    {
        if (threshold < 0)
        {
            throw new DomainRuleException("Reorder threshold must be non-negative.");
        }

        if (quantity <= 0)
        {
            throw new DomainRuleException("Reorder quantity must be positive.");
        }

        this.ReorderThreshold = threshold;
        this.ReorderQuantity = quantity;

        this.DomainEvents.Register(new StockUpdatedDomainEvent(this));

        return this;
    }

    public Stock MoveToLocation(StorageLocation newLocation)
    {
        _ = newLocation ?? throw new ArgumentException("New location cannot be empty.");

        if (this.Location == newLocation)
        {
            return this;
        }

        this.Location = newLocation;

        this.DomainEvents.Register(new StockLocationChangedDomainEvent(this, newLocation));

        return this;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Domain\Model\StockAggregate\StockAdjustment.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

[DebuggerDisplay("Id={Id}, StockId={StockId}, QuantityChange={QuantityChange}, OldUnitCost={OldUnitCost}, NewUnitCost={NewUnitCost}")]
[TypedEntityId<Guid>]
public class StockAdjustment : Entity<StockAdjustmentId>
{
    private StockAdjustment() { } // Private constructor required by EF Core

    private StockAdjustment(StockId stockId, int? quantityChange, string reason, DateTimeOffset timestamp)
    {
        this.StockId = stockId;
        this.QuantityChange = quantityChange;
        this.Reason = reason;
        this.Timestamp = timestamp;
    }

    private StockAdjustment(StockId stockId, Money oldUnitCost, Money newUnitCost, string reason, DateTimeOffset timestamp)
    {
        this.StockId = stockId;
        this.OldUnitCost = oldUnitCost;
        this.NewUnitCost = newUnitCost;
        this.Reason = reason;
        this.Timestamp = timestamp;
    }

    public StockId StockId { get; private set; }

    public int? QuantityChange { get; private set; }

    public Money OldUnitCost { get; private set; }

    public Money NewUnitCost { get; private set; }

    public string Reason { get; private set; }

    public DateTimeOffset Timestamp { get; private set; }

    public static StockAdjustment CreateQuantityAdjustment(StockId stockId, int quantityChange, string reason, DateTimeOffset? timestamp = null)
    {
        _ = stockId ?? throw new ArgumentException("StockId cannot be empty.");
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainRuleException("Reason for adjustment cannot be empty.");
        }

        return new StockAdjustment(stockId, quantityChange, reason, timestamp ?? DateTimeOffset.UtcNow);
    }

    public static StockAdjustment CreateUnitCostAdjustment(StockId stockId, Money oldUnitCost, Money newUnitCost, string reason, DateTimeOffset? timestamp = null)
    {
        _ = stockId ?? throw new ArgumentException("StockId cannot be empty.");
        _ = oldUnitCost ?? throw new ArgumentException("Old unit cost cannot be empty.");
        _ = newUnitCost ?? throw new ArgumentException("New unit cost cannot be empty.");
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainRuleException("Reason for adjustment cannot be empty.");
        }

        return new StockAdjustment(stockId, oldUnitCost, newUnitCost, reason, timestamp ?? DateTimeOffset.UtcNow);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Domain\Model\StockAggregate\StockMovement.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

[DebuggerDisplay("Id={Id}, StockId={StockId}, Quantity={Quantity}")]
[TypedEntityId<Guid>]
public class StockMovement : Entity<StockMovementId>
{
    private StockMovement() { } // Private constructor required by EF Core

    private StockMovement(StockId stockId, int quantity, StockMovementType type, string reason, DateTimeOffset timestamp)
    {
        this.StockId = stockId;
        this.Quantity = quantity;
        this.Type = type;
        this.Reason = reason;
        this.Timestamp = timestamp;
    }

    public StockId StockId { get; private set; }

    public int Quantity { get; private set; }

    public StockMovementType Type { get; private set; }

    public string Reason { get; private set; }

    public DateTimeOffset Timestamp { get; private set; }

    public static StockMovement Create(StockId stockId, int quantity, StockMovementType type, string reason, DateTimeOffset? timestamp = null)
    {
        _ = stockId ?? throw new ArgumentException("StockId cannot be empty.");
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainRuleException("Reason for movement cannot be empty.");
        }

        return new StockMovement(stockId, quantity, type, reason, timestamp ?? DateTimeOffset.UtcNow);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Domain\Model\StockAggregate\StockMovementType.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

[DebuggerDisplay("Id={Id}, Value={Value}")]
public class StockMovementType(int id, string value, string code)
    : Enumeration(id, value)
{
    public static StockMovementType Addition = new(0, nameof(Addition), "ADD");

    public static StockMovementType Removal = new(1, nameof(Removal), "REM");

    public string Code { get; } = code;

    public static IEnumerable<StockMovementType> GetAll()
    {
        return GetAll<StockMovementType>();
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Domain\Model\StockAggregate\StorageLocation.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

public class StorageLocation : ValueObject
{
    private StorageLocation() { } // Private constructor required by EF Core

    private StorageLocation(string aisle, string shelf, string bin)
    {
        this.Aisle = aisle;
        this.Shelf = shelf;
        this.Bin = bin;
    }

    public string Aisle { get; }

    public string Shelf { get; }

    public string Bin { get; }

    public string Full
    {
        get => this.ToString();
        set // needs to be private
            => _ = value;
    }

    public static implicit operator string(StorageLocation location) => location.ToString();

    public static implicit operator StorageLocation(string value)
    {
        var parts = value.Split('|');
        if (parts.Length != 3)
        {
            throw new DomainRuleException("Invalid location format. Expected 'Aisle|Shelf|Bin'.");
        }

        return Create(parts[0], parts[1], parts[2]);
    }

    public static StorageLocation Create(string aisle, string shelf, string bin)
    {
        if (string.IsNullOrWhiteSpace(aisle))
        {
            throw new DomainRuleException("Aisle cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(shelf))
        {
            throw new DomainRuleException("Shelf cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(bin))
        {
            throw new DomainRuleException("Bin cannot be empty.");
        }

        return new StorageLocation(aisle, shelf, bin);
    }

    public override string ToString()
    {
        return $"{this.Aisle}|{this.Shelf}|{this.Bin}";
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Aisle;
        yield return this.Shelf;
        yield return this.Bin;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Domain\Model\StockSnapshotAggregate\StockSnapshot.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

[DebuggerDisplay("Id={Id}, Sku={Sku}, Timestamp={Timestamp}")]
[TypedEntityId<Guid>]
public class StockSnapshot : AuditableAggregateRoot<StockSnapshotId>, IConcurrent
{
    private StockSnapshot() { } // Private constructor required by EF Core

    private StockSnapshot(
        TenantId tenantId,
        StockId stockId,
        ProductSku sku,
        int quantityOnHand,
        int quantityReserved,
        Money unitCost,
        StorageLocation location,
        DateTimeOffset timestamp)
    {
        this.TenantId = tenantId;
        this.StockId = stockId;
        this.Sku = sku;
        this.QuantityOnHand = quantityOnHand;
        this.QuantityReserved = quantityReserved;
        this.UnitCost = unitCost;
        this.Location = location;
        this.Timestamp = timestamp;
    }

    public TenantId TenantId { get; private set; }

    public StockId StockId { get; private set; }

    public ProductSku Sku { get; private set; }

    public int QuantityOnHand { get; private set; }

    public int QuantityReserved { get; private set; }

    public Money UnitCost { get; private set; }

    public StorageLocation Location { get; private set; }

    public DateTimeOffset Timestamp { get; private set; }

    public Guid Version { get; set; }

    public static StockSnapshot Create(
        TenantId tenantId,
        StockId stockId,
        ProductSku sku,
        int quantityOnHand,
        int quantityReserved,
        Money unitCost,
        StorageLocation location,
        DateTimeOffset? timestamp = null)
    {
        _ = tenantId ?? throw new ArgumentException("TenantId cannot be empty.");
        _ = stockId ?? throw new ArgumentException("StockId cannot be empty.");
        _ = sku ?? throw new ArgumentException("ProductSku cannot be empty.");
        _ = unitCost ?? throw new ArgumentException("UnitCost cannot be empty.");
        _ = location ?? throw new ArgumentException("Location cannot be empty.");

        var snapshot = new StockSnapshot(tenantId, stockId, sku, quantityOnHand, quantityReserved, unitCost, location, timestamp ?? DateTimeOffset.UtcNow);

        snapshot.DomainEvents.Register(new StockSnapshotCreatedDomainEvent(snapshot));

        return snapshot;
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Infrastructure\EntityFramework\Configurations\StockEntityTypeConfiguration.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Infrastructure;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using BridgingIT.DevKit.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StockEntityTypeConfiguration
    : TenantAwareEntityTypeConfiguration<Stock>
{
    public override void Configure(EntityTypeBuilder<Stock> builder)
    {
        base.Configure(builder);

        ConfigureStocks(builder);
        ConfigureStockMovements(builder);
        ConfigureStockAdjustments(builder);
    }

    private static void ConfigureStocks(EntityTypeBuilder<Stock> builder)
    {
        builder.ToTable("Stocks").HasKey(e => e.Id).IsClustered(false);

        builder.Navigation(e => e.Movements).AutoInclude();
        builder.Navigation(e => e.Adjustments).AutoInclude();

        builder.Property(e => e.Version).IsConcurrencyToken();

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(id => id.Value, value => StockId.Create(value));

        builder.Property(e => e.TenantId)
            .HasConversion(id => id.Value, value => TenantId.Create(value)).IsRequired();

        builder.Property(e => e.Sku)
            .HasConversion(sku => sku.Value, value => ProductSku.Create(value))
            .IsRequired()
            .HasMaxLength(12);
        builder.HasIndex(nameof(Stock.TenantId), nameof(Stock.Sku)).IsUnique();

        builder.Property(e => e.QuantityOnHand).HasDefaultValue(0).IsRequired();

        builder.Property(e => e.QuantityReserved).HasDefaultValue(0).IsRequired();

        builder.Property(e => e.ReorderThreshold).HasDefaultValue(0).IsRequired();

        builder.Property(e => e.ReorderQuantity).HasDefaultValue(0).IsRequired();

        builder.OwnsOne(
            e => e.UnitCost,
            b =>
            {
                b.Property(e => e.Amount)
                    .HasColumnName("UnitCost")
                    .HasDefaultValue(0)
                    .IsRequired()
                    .HasColumnType("decimal(5,2)");

                b.OwnsOne(
                    e => e.Currency,
                    b =>
                    {
                        b.Property(e => e.Code)
                            .HasColumnName("UnitCostCurrency")
                            .HasDefaultValue("USD")
                            .IsRequired()
                            .HasMaxLength(8);
                    });
            });

        builder.OwnsOne(e => e.Location, b =>
        {
            b.Property(l => l.Aisle).HasColumnName("LocationAisle").IsRequired().HasMaxLength(32);
            b.Property(l => l.Shelf).HasColumnName("LocationShelf").IsRequired().HasMaxLength(32);
            b.Property(l => l.Bin).HasColumnName("LocationBin").IsRequired().HasMaxLength(32);
            b.Property(e => e.Full).HasColumnName("LocationFull").IsRequired().HasMaxLength(128);
            b.HasIndex(e => e.Full);
        });

        builder.Property(e => e.LastRestockedAt).IsRequired(false);

        builder.OwnsOneAuditState();
        //builder.OwnsOne(e => e.AuditState, b => b.ToJson());

        builder.Metadata.FindNavigation(nameof(Stock.Movements)).SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(Stock.Adjustments)).SetPropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureStockMovements(EntityTypeBuilder<Stock> builder)
    {
        builder.OwnsMany(
            e => e.Movements,
            b =>
            {
                b.ToTable("StockMovements");
                b.WithOwner().HasForeignKey("StockId");
                // b.HasKey("Id", "StockId");
                b.HasKey("Id");

                b.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .HasConversion(id => id.Value, value => StockMovementId.Create(value));

                b.Property(e => e.Quantity)
                    .IsRequired();

                b.Property(e => e.Type)
                    .HasConversion(new EnumerationConverter<StockMovementType>())
                    .IsRequired();

                b.Property(e => e.Reason)
                    .IsRequired(false)
                    .HasMaxLength(1024);

                b.Property(e => e.Timestamp)
                    .IsRequired();
            });
    }

    private static void ConfigureStockAdjustments(EntityTypeBuilder<Stock> builder)
    {
        builder.OwnsMany(
            e => e.Adjustments,
            b =>
            {
                b.ToTable("StockAdjustments");
                b.WithOwner().HasForeignKey("StockId");
                //b.HasKey("Id", "StockId");
                b.HasKey("Id");

                b.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .HasConversion(id => id.Value, value => StockAdjustmentId.Create(value));

                b.Property(e => e.QuantityChange).HasDefaultValue(0).IsRequired(false);

                b.OwnsOne(
                    e => e.OldUnitCost,
                    b =>
                    {
                        b.Property(e => e.Amount)
                            .HasColumnName("OldUnitCost")
                            .HasDefaultValue(0)
                            .IsRequired()
                            .HasColumnType("decimal(5,2)");

                        b.OwnsOne(
                            e => e.Currency,
                            b =>
                            {
                                b.Property(e => e.Code)
                                    .HasColumnName("OldUnitCostCurrency")
                                    .HasDefaultValue("USD")
                                    .IsRequired()
                                    .HasMaxLength(8);
                            });
                    });

                b.OwnsOne(
                    e => e.NewUnitCost,
                    b =>
                    {
                        b.Property(e => e.Amount)
                            .HasColumnName("NewUnitCost")
                            .HasDefaultValue(0)
                            .IsRequired()
                            .HasColumnType("decimal(5,2)");

                        b.OwnsOne(
                            e => e.Currency,
                            b =>
                            {
                                b.Property(e => e.Code)
                                    .HasColumnName("NewUnitCostCurrency")
                                    .HasDefaultValue("USD")
                                    .IsRequired()
                                    .HasMaxLength(8);
                            });
                    });

                b.Property(e => e.Reason)
                    .IsRequired()
                    .HasMaxLength(1024);

                b.Property(e => e.Timestamp).IsRequired();
            });
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Infrastructure\EntityFramework\Configurations\StockSnapshotEntityTypeConfiguration.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Infrastructure;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using BridgingIT.DevKit.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StockSnapshotEntityTypeConfiguration : TenantAwareEntityTypeConfiguration<StockSnapshot>
{
    public override void Configure(EntityTypeBuilder<StockSnapshot> builder)
    {
        base.Configure(builder);

        builder.ToTable("StockSnapshots").HasKey(e => e.Id).IsClustered(false);

        builder.Property(e => e.Version).IsConcurrencyToken();

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(id => id.Value, value => StockSnapshotId.Create(value));

        builder.Property(e => e.TenantId)
            .HasConversion(id => id.Value, value => TenantId.Create(value))
            .IsRequired();

        builder.Property(e => e.StockId)
            .HasConversion(id => id.Value, value => StockId.Create(value))
            .IsRequired();

        builder.HasOne<Stock>()
            .WithMany()
            .HasForeignKey(e => e.StockId)
            .IsRequired();

        builder.Property(e => e.Sku)
            .HasConversion(sku => sku.Value, value => ProductSku.Create(value))
            .IsRequired()
            .HasMaxLength(12);
        builder.HasIndex(nameof(StockSnapshot.TenantId), nameof(StockSnapshot.Sku));

        builder.Property(e => e.QuantityOnHand)
            .IsRequired();

        builder.Property(e => e.QuantityReserved)
            .IsRequired();

        builder.OwnsOne(
            e => e.UnitCost,
            b =>
            {
                b.Property(e => e.Amount)
                    .HasColumnName("UnitCost")
                    .HasDefaultValue(0)
                    .IsRequired()
                    .HasColumnType("decimal(5,2)");

                b.OwnsOne(
                    e => e.Currency,
                    b =>
                    {
                        b.Property(e => e.Code)
                            .HasColumnName("UnitCostCurrency")
                            .HasDefaultValue("USD")
                            .IsRequired()
                            .HasMaxLength(8);
                    });
            });

        builder.OwnsOne(e => e.Location,
            locationBuilder =>
            {
                locationBuilder.Property(l => l.Aisle).HasColumnName("LocationAisle").IsRequired().HasMaxLength(50);
                locationBuilder.Property(l => l.Shelf).HasColumnName("LocationShelf").IsRequired().HasMaxLength(50);
                locationBuilder.Property(l => l.Bin).HasColumnName("LocationBin").IsRequired().HasMaxLength(50);
            });

        builder.Property(e => e.Timestamp)
            .IsRequired();

        builder.OwnsOneAuditState();
        //builder.OwnsOne(e => e.AuditState, b => b.ToJson());
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Infrastructure\EntityFramework\Configurations\TenantAwareEntityTypeConfiguration.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Infrastructure;

using BridgingIT.DevKit.Domain.Model;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public abstract class TenantAwareEntityTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : class, IEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property<TenantId>("TenantId")
            .HasConversion(id => id.Value, value => TenantId.Create(value))
            .IsRequired();

        builder.HasIndex("TenantId");

        builder.HasOne<TenantReference>()
            .WithMany()
            .HasForeignKey("TenantId")
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class TenantReferenceEntityTypeConfiguration : IEntityTypeConfiguration<TenantReference>
{
    public void Configure(EntityTypeBuilder<TenantReference> builder)
    {
        builder.ToTable("Tenants", "organization").HasKey(e => e.Id).IsClustered(false);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(id => id.Value, value => TenantId.Create(value));

        builder.ToTable(tb => tb.ExcludeFromMigrations());
    }
}

public class TenantReference
{
    public TenantId Id { get; set; }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Presentation\Web\Endpoints\InventoryStockEndpoints.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Presentation.Web;

using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;
using BridgingIT.DevKit.Presentation.Web;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

public class InventoryStockEndpoints : EndpointsBase
{
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/tenants/{tenantId}/inventory/stocks")
            .WithTags("Inventory");

        group.MapGet("/{id}", GetStock)
            .WithName("GetInventoryStock")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet("/", GetStocks)
            .WithName("GetInventoryStocks")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapPost("/", CreateStock)
            .WithName("CreateInventoryStock")
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapPost("/{id}/movements", CreateStockMovement)
            .WithName("CreateInventoryStockMovement")
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);
    }

    private static async Task<Results<Ok<StockModel>, NotFound, ProblemHttpResult>> GetStock(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new StockFindOneQuery(tenantId, id))).Result;

        return result.IsFailure && result.HasError<NotFoundResultError>()
            ? TypedResults.NotFound()
            : result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Stock, StockModel>(result.Value))
                : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<StockModel>>, ProblemHttpResult>> GetStocks(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId)
    {
        var result = (await mediator.Send(new StockFindAllQuery(tenantId))).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Stock>, IEnumerable<StockModel>>(result.Value))
            : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Created<StockModel>, ProblemHttpResult>> CreateStock(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromBody] StockModel model)
    {
        var result = (await mediator.Send(new StockCreateCommand(tenantId, model))).Result;

        return result.IsSuccess
            ? TypedResults.Created(
                $"api/tenants/{tenantId}/inventory/stocks/{result.Value.Id}",
                mapper.Map<Stock, StockModel>(result.Value))
            : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Created<StockModel>, NotFound, ProblemHttpResult>> CreateStockMovement(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string id,
        [FromBody] StockMovementModel model)
    {
        var result = (await mediator.Send(new StockMovementApplyCommand(tenantId, id, model))).Result;

        return result.IsFailure && result.HasError<NotFoundResultError>() ? TypedResults.NotFound()
            : result.IsSuccess ? TypedResults.Created(
                $"api/tenants/{tenantId}/inventory/stocks/{result.Value.Id}",
                mapper.Map<Stock, StockModel>(result.Value))
            : TypedResults.Problem(result.ToString(), statusCode: 400);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Presentation\Web\Endpoints\InventoryStockSnapshotEndpoints.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Presentation.Web;

using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;
using BridgingIT.DevKit.Presentation.Web;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

public class InventoryStockSnapshotEndpoints : EndpointsBase
{
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/tenants/{tenantId}/inventory/stocks/{stockId}/stocksnapshots")
            .WithTags("Inventory");

        group.MapGet("/{id}", GetStockSnapshot)
            .WithName("GetInventoryStockSnapshot")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet("/", GetStockSnapshots)
            .WithName("GetInventoryStockSnapshots")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapPost("/", CreateStockSnapshot)
            .WithName("CreateInventoryStockSnapshot")
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);
    }

    private static async Task<Results<Ok<StockSnapshotModel>, NotFound, ProblemHttpResult>> GetStockSnapshot(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string stockId,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(
            new StockSnapshotFindOneQuery(tenantId, stockId, id))).Result;

        return result.Value == null ? TypedResults.NotFound() :
            result.IsSuccess ? TypedResults.Ok(mapper.Map<StockSnapshot, StockSnapshotModel>(result.Value)) :
            TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<StockSnapshotModel>>, ProblemHttpResult>> GetStockSnapshots(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string stockId)
    {
        var result = (await mediator.Send(
            new StockSnapshotFindAllQuery(tenantId, stockId))).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<StockSnapshot>, IEnumerable<StockSnapshotModel>>(result.Value))
            : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Created<StockSnapshotModel>, ProblemHttpResult>> CreateStockSnapshot(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string stockId)
    {
        var result = (await mediator.Send(
            new StockSnapshotCreateCommand(tenantId, stockId))).Result;

        return result.IsSuccess
            ? TypedResults.Created(
                $"api/tenants/{tenantId}/inventory/stocks/{stockId}/stocksnapshots/{result.Value.Id}",
                mapper.Map<StockSnapshot, StockSnapshotModel>(result.Value))
            : TypedResults.Problem(result.ToString(), statusCode: 400);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\Queries\Rules\CompanyNameMustBeUniqueRule.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class CompanyNameMustBeUniqueRule(IGenericRepository<Company> repository, Company company) : DomainRuleBase
{
    public override string Message
        => "Company name must be unique";

    public override async Task<bool> ApplyAsync(CancellationToken cancellationToken = default)
    {
        return !(await repository.FindAllAsync(
            CompanySpecifications.ForName(company.Name),
            cancellationToken: cancellationToken)).SafeAny(c => c.Id != company.Id);
    }
}

public class CompanyMustHaveNoTenantsRule(IGenericRepository<Tenant> repository, Company company) : DomainRuleBase
{
    public override string Message
        => "Company must have no tenants assigned";

    public override async Task<bool> ApplyAsync(CancellationToken cancellationToken = default)
    {
        return !(await repository.FindAllAsync(
            TenantSpecifications.ForCompany(company.Id),
            cancellationToken: cancellationToken)).SafeAny();
    }
}

public static class CompanyRules
{
    public static IDomainRule NameMustBeUnique(IGenericRepository<Company> repository, Company company)
    {
        return new CompanyNameMustBeUniqueRule(repository, company);
    }

    public static IDomainRule MustHaveNoTenants(IGenericRepository<Tenant> repository, Company company)
    {
        return new CompanyMustHaveNoTenantsRule(repository, company);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Application\Queries\Rules\TenantNameMustBeUniqueRule.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class TenantNameMustBeUniqueRule(IGenericRepository<Tenant> repository, Tenant tenant) : DomainRuleBase
{
    public override string Message
        => "Tenant name should be unique";

    public override async Task<bool> ApplyAsync(CancellationToken cancellationToken = default)
    {
        return !(await repository.FindAllAsync(
            TenantSpecifications.ForName(tenant.Name),
            cancellationToken: cancellationToken)).SafeAny();
    }
}

public static class TenantRules
{
    public static IDomainRule NameMustBeUnique(IGenericRepository<Tenant> repository, Tenant tenant)
    {
        return new TenantNameMustBeUniqueRule(repository, tenant);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Domain\CompanyAggregate\Events\CompanyCreatedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

public class CompanyCreatedDomainEvent(Company company) : DomainEventBase
{
    //public TenantId TenantId { get; } = company.TenantIds;

    public CompanyId CompanyId { get; } = company.Id;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Domain\CompanyAggregate\Events\CompanyUpdatedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

public class CompanyUpdatedDomainEvent(Company company) : DomainEventBase
{
    //public TenantId TenantId { get; } = company.TenantIds;

    public CompanyId CompanyId { get; } = company.Id;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Domain\CompanyAggregate\Specifications\CompanySpecifications.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

public class CompanyForNameSpecification(string name) : Specification<Company>
{
    public override Expression<Func<Company, bool>> ToExpression()
    {
        return e => e.Name == name;
    }
}

//public class CompanyForTenantSpecification(TenantId tenantId)
//    : Specification<Company>
//{
//    public override Expression<Func<Company, bool>> ToExpression()
//    {
//        return e => e.TenantIds.Contains(tenantId);
//    }
//}

public static class CompanySpecifications
{
    public static Specification<Company> ForName(string name)
    {
        return new CompanyForNameSpecification(name);
    }

    public static Specification<Company> ForName2(string name) // INFO: short version to define a specification
    {
        return new Specification<Company>(e => e.Name == name);
    }

    //public static Specification<Company> ForTenant(TenantId tenantId)
    //    => new CompanyForTenantSpecification(tenantId);

    //public static Specification<Company> ForTenant2(TenantId tenantId) // INFO: short version to define a specification
    //    => new(e => e.TenantIds.Contains(tenantId));
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Domain\TenantAggregate\Events\TenantActivatedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

public class TenantActivatedDomainEvent(Tenant tenant) : DomainEventBase
{
    public TenantId TenantId { get; } = tenant.Id;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Domain\TenantAggregate\Events\TenantCreatedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

public class TenantCreatedDomainEvent(Tenant tenant) : DomainEventBase
{
    public TenantId TenantId { get; } = tenant.Id;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Domain\TenantAggregate\Events\TenantDeactivatedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

public class TenantDeactivatedDomainEvent(Tenant tenant) : DomainEventBase
{
    public TenantId TenantId { get; } = tenant.Id;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Domain\TenantAggregate\Events\TenantReassignedCompanyDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

public class TenantReassignedCompanyDomainEvent(Tenant tenant) : DomainEventBase
{
    public TenantId TenantId { get; } = tenant.Id;

    public CompanyId CompanyId { get; } = tenant.CompanyId;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Domain\TenantAggregate\Events\TenantSubscriptionCreatedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

public class TenantSubscriptionCreatedDomainEvent(TenantSubscription subscription) : DomainEventBase
{
    public TenantId TenantId { get; } = subscription.Tenant.Id;

    public TenantSubscriptionId SubscriptionId { get; } = subscription.Id;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Domain\TenantAggregate\Events\TenantSubscriptionRemovedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

public class TenantSubscriptionRemovedDomainEvent(TenantSubscription subscription) : DomainEventBase
{
    public TenantId TenantId { get; } = subscription.Tenant.Id;

    public TenantSubscriptionId SubscriptionId { get; } = subscription.Id;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Domain\TenantAggregate\Events\TenantSubscriptionUpdatedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

public class TenantSubscriptionUpdatedDomainEvent(TenantSubscription subscription) : DomainEventBase
{
    public TenantId TenantId { get; } = subscription.Tenant.Id;

    public TenantSubscriptionId SubscriptionId { get; } = subscription.Id;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Domain\TenantAggregate\Events\TenantUpdatedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

public class TenantUpdatedDomainEvent(Tenant tenant) : DomainEventBase
{
    public TenantId TenantId { get; } = tenant.Id;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Domain\TenantAggregate\Specifications\TenantSpecifications.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

public class TenantForNameSpecification(string name) : Specification<Tenant>
{
    public override Expression<Func<Tenant, bool>> ToExpression()
    {
        return e => e.Name == name;
    }
}

public class TenantForCompanySpecification(CompanyId companyId) : Specification<Tenant>
{
    public override Expression<Func<Tenant, bool>> ToExpression()
    {
        return e => e.CompanyId == companyId;
    }
}

public static class TenantSpecifications
{
    public static Specification<Tenant> ForName(string name)
    {
        return new TenantForNameSpecification(name);
    }

    public static Specification<Tenant> ForName2(string name) // INFO: short version to define a specification
    {
        return new Specification<Tenant>(e => e.Name == name);
    }

    public static Specification<Tenant> ForCompany(CompanyId companyId)
    {
        return new TenantForCompanySpecification(companyId);
    }

    public static Specification<Tenant>
        ForCompany2(CompanyId companyId) // INFO: short version to define a specification
    {
        return new Specification<Tenant>(e => e.CompanyId == companyId);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Infrastructure\EntityFramework\Configurations\CompanyEntityTypeConfiguration.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Infrastructure;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;
using BridgingIT.DevKit.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CompanyEntityTypeConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        ConfigureCompanies(builder);
    }

    private static void ConfigureCompanies(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies").HasKey(d => d.Id).IsClustered(false);

        builder.Property(e => e.Version).IsConcurrencyToken();

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(id => id.Value, value => CompanyId.Create(value));

        builder.Property(e => e.Name).IsRequired().HasMaxLength(512);

        builder.OwnsOne(
            e => e.Address,
            b =>
            {
                b.Property(e => e.Name).HasMaxLength(512).IsRequired();

                b.Property(e => e.Line1).HasMaxLength(256).IsRequired();

                b.Property(e => e.Line2).HasMaxLength(256);

                b.Property(e => e.City).HasMaxLength(128).IsRequired();

                b.Property(e => e.PostalCode).HasMaxLength(32).IsRequired();

                b.Property(e => e.Country).HasMaxLength(128).IsRequired();
            });
        builder.Navigation(e => e.Address).IsRequired();

        builder.Property(e => e.RegistrationNumber).IsRequired().HasMaxLength(128);

        builder.OwnsOne(
            e => e.ContactEmail,
            b =>
            {
                b.Property(e => e.Value).HasColumnName(nameof(Company.ContactEmail)).IsRequired().HasMaxLength(256);
            });
        builder.Navigation(e => e.ContactEmail).IsRequired();

        builder.OwnsOne(
            e => e.ContactPhone,
            b =>
            {
                b.Property(e => e.CountryCode).HasMaxLength(8).IsRequired(false);

                b.Property(e => e.Number).HasMaxLength(32).IsRequired(false);
            });
        builder.Navigation(e => e.ContactPhone).IsRequired();

        builder.OwnsOne(
            e => e.Website,
            b =>
            {
                b.Property(e => e.Value).HasColumnName(nameof(Company.Website)).IsRequired(false).HasMaxLength(512);
            });
        builder.Navigation(e => e.Website).IsRequired();

        builder.OwnsOne(
            e => e.VatNumber,
            b =>
            {
                b.Property(e => e.CountryCode).IsRequired(false).HasMaxLength(16);

                b.Property(e => e.Number).IsRequired(false).HasMaxLength(128);
            });
        builder.Navigation(e => e.VatNumber).IsRequired();

        builder.OwnsOneAuditState(); // TODO: use ToJson variant
        //builder.OwnsOne(e => e.AuditState, b => b.ToJson());
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Infrastructure\EntityFramework\Configurations\TenantEntityTypeConfiguration.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Infrastructure;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using BridgingIT.DevKit.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TenantEntityTypeConfiguration : IEntityTypeConfiguration<Tenant>, IEntityTypeConfiguration<TenantBranding>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        ConfigureTenants(builder);
        ConfigureTenantSubscriptions(builder);
    }

    public void Configure(EntityTypeBuilder<TenantBranding> builder)
    {
        ConfigureTenantBranding(builder);
    }

    private static void ConfigureTenants(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants").HasKey(d => d.Id).IsClustered(false);

        builder.Property(e => e.Version).IsConcurrencyToken();

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(id => id.Value, value => TenantId.Create(value));

        builder.Property(e => e.Name).IsRequired().HasMaxLength(512);

        builder.Property(e => e.Description).IsRequired(false);

        builder.OwnsOne(
            e => e.ContactEmail,
            b =>
            {
                b.Property(e => e.Value).HasColumnName(nameof(Tenant.ContactEmail)).IsRequired().HasMaxLength(256);
            });
        builder.Navigation(e => e.ContactEmail).IsRequired();

        builder
            .HasOne<
                Company>() // one-to-many with no navigations https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many#one-to-many-with-no-navigations
            .WithMany()
            .HasForeignKey(e => e.CompanyId)
            .IsRequired();

        //builder.OwnsOneAuditState(); // TODO: use ToJson variant
        builder.OwnsOne(e => e.AuditState, b => b.ToJson());
    }

    private static void ConfigureTenantSubscriptions(EntityTypeBuilder<Tenant> builder)
    {
        builder.OwnsMany(
            e => e.Subscriptions,
            b =>
            {
                b.ToTable("TenantSubscriptions").HasKey(e => e.Id).IsClustered(false);
                b.WithOwner(e => e.Tenant);

                b.Property(e => e.Version).IsConcurrencyToken();

                b.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .HasConversion(id => id.Value, id => TenantSubscriptionId.Create(id));

                b.Property(e => e.PlanType)
                    //.HasConversion(
                    //    status => status.Id,
                    //    id => Enumeration.FromId<TenantSubscriptionPlanType>(id))
                    .HasConversion(new EnumerationConverter<TenantSubscriptionPlanType>())
                    .IsRequired();

                b.Property(e => e.Status)
                    //.HasConversion(
                    //    status => status.Id,
                    //    id => Enumeration.FromId<TenantSubscriptionStatus>(id))
                    .HasConversion(new EnumerationConverter<TenantSubscriptionStatus>())
                    .IsRequired();

                b.Property(e => e.BillingCycle)
                    //.HasConversion(
                    //    status => status.Id,
                    //    id => Enumeration.FromId<TenantSubscriptionBillingCycle>(id))
                    .HasConversion( //.HasConversion(
                        new EnumerationConverter<TenantSubscriptionBillingCycle>())
                    .IsRequired();

                b.OwnsOne(
                    e => e.Schedule,
                    b =>
                    {
                        b.Property(e => e.StartDate).IsRequired();

                        b.Property(e => e.EndDate).IsRequired(false);
                    });
                b.Navigation(e => e.Schedule).IsRequired();
            });
    }

    private static void ConfigureTenantBranding(EntityTypeBuilder<TenantBranding> builder)
    {
        builder.ToTable("TenantBrandings").HasKey(e => e.Id).IsClustered(false);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(id => id.Value, value => TenantBrandingId.Create(value));

        builder.Property(e => e.TenantId).HasConversion(id => id.Value, value => TenantId.Create(value));

        builder.HasOne<Tenant>()
            .WithOne(e => e.Branding)
            .IsRequired()
            .HasForeignKey<TenantBranding>(e => e.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(
            e => e.PrimaryColor,
            b =>
            {
                b.Property(e => e.Value)
                    .HasColumnName(nameof(TenantBranding.PrimaryColor))
                    .IsRequired(false)
                    .HasMaxLength(16);
            });
        builder.Navigation(e => e.PrimaryColor).IsRequired();

        builder.OwnsOne(
            e => e.SecondaryColor,
            b =>
            {
                b.Property(e => e.Value)
                    .HasColumnName(nameof(TenantBranding.SecondaryColor))
                    .IsRequired(false)
                    .HasMaxLength(16);
            });
        builder.Navigation(e => e.SecondaryColor).IsRequired();

        builder.OwnsOne(
            e => e.LogoUrl,
            b =>
            {
                b.Property(e => e.Value)
                    .HasColumnName(nameof(TenantBranding.LogoUrl))
                    .IsRequired(false)
                    .HasMaxLength(512);
            });
        builder.Navigation(e => e.LogoUrl).IsRequired();

        builder.OwnsOne(
            e => e.FaviconUrl,
            b =>
            {
                b.Property(e => e.Value)
                    .HasColumnName(nameof(TenantBranding.FaviconUrl))
                    .IsRequired(false)
                    .HasMaxLength(512);
            });
        builder.Navigation(e => e.FaviconUrl).IsRequired();
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Presentation\Web\Endpoints\OrganizationCompanyEndpoint.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Presentation.Web;

using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;
using BridgingIT.DevKit.Presentation.Web;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

/// <summary>
///     Specifies the external API for this module that will be exposed for the outside boundary
/// </summary>
public class OrganizationCompanyEndpoint : EndpointsBase
{
    /// <summary>
    ///     Maps the endpoints for the Organization Company to the specified route builder.
    /// </summary>
    /// <param name="app">The IEndpointRouteBuilder instance used to define routes.</param>
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/organization/companies")
            .WithTags("Organization");

        group.MapGet("/{id}", CompanyFindOne)
            .WithName("GetOrganizationCompany")
            .ProducesValidationProblem()
            .ProducesProblem(500);
        //.Produces<ProblemDetails>(400)
        //.Produces<ProblemDetails>(500);

        group.MapGet("/", CompanyFindAll)
            .WithName("GetOrganizationCompanies")
            .ProducesValidationProblem()
            .ProducesProblem(500);
        //.Produces<ProblemDetails>(400)
        //.Produces<ProblemDetails>(500);

        group.MapGet("/{id}/tenants", CompanyFindAllTenants)
            .WithName("GetOrganizationCompanyTenants")
            .ProducesValidationProblem()
            .ProducesProblem(500);
        //.Produces<ProblemDetails>(400)
        //.Produces<ProblemDetails>(500);

        group.MapPost("/", CompanyCreate)
            .WithName("CreateOrganizationCompany")
            .ProducesValidationProblem()
            .ProducesProblem(500);
        //.Produces<ProblemDetails>(400)
        //.Produces<ProblemDetails>(500);

        group.MapPut("/{id}", CompanyUpdate)
            .WithName("UpdateOrganizationCompany")
            .ProducesValidationProblem()
            .ProducesProblem(500);
        //.Produces<ProblemDetails>(400)
        //.Produces<ProblemDetails>(500);

        group.MapDelete("/{id}", CompanyDelete)
            .WithName("DeleteCatalogCompany")
            .ProducesValidationProblem()
            .ProducesProblem(500);
        //.Produces<ProblemDetails>(400)
        //.Produces<ProblemDetails>(500);
    }

    private static async Task<Results<Ok<CompanyModel>, NotFound, ProblemHttpResult>> CompanyFindOne(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new CompanyFindOneQuery(id))).Result;

        return result.IsFailure && result.HasError<NotFoundResultError>() ? TypedResults.NotFound() :
            result.IsSuccess ? TypedResults.Ok(mapper.Map<Company, CompanyModel>(result.Value)) :
            TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<TenantModel>>, NotFound, ProblemHttpResult>> CompanyFindAllTenants(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new TenantFindAllQuery { CompanyId = id })).Result;

        return result.IsFailure && result.HasError<NotFoundResultError>()
            ? TypedResults.NotFound()
            : result.IsSuccess
                ? TypedResults.Ok(mapper.Map<IEnumerable<Tenant>, IEnumerable<TenantModel>>(result.Value))
                : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<CompanyModel>>, ProblemHttpResult>> CompanyFindAll(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper)
    {
        var result = (await mediator.Send(new CompanyFindAllQuery())).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Company>, IEnumerable<CompanyModel>>(result.Value))
            : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Created<CompanyModel>, ProblemHttpResult>> CompanyCreate(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromBody] CompanyModel model)
    {
        var result = (await mediator.Send(new CompanyCreateCommand(model))).Result;

        return result.IsSuccess
            ? TypedResults.Created($"api/companies/{result.Value.Id}", mapper.Map<Company, CompanyModel>(result.Value))
            : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Ok<CompanyModel>, NotFound, ProblemHttpResult>> CompanyUpdate(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string id,
        [FromBody] CompanyModel model)
    {
        var result = (await mediator.Send(new CompanyUpdateCommand(model))).Result;

        return result.IsFailure && result.HasError<NotFoundResultError>()
            ? TypedResults.NotFound()
            : result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Company, CompanyModel>(result.Value))
                : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Ok, NotFound, ProblemHttpResult>> CompanyDelete(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new CompanyDeleteCommand(id))).Result;

        return result.IsFailure && result.HasError<NotFoundResultError>()
            ? TypedResults.NotFound()
            : result.IsSuccess
                ? TypedResults.Ok()
                : TypedResults.Problem(result.ToString(), statusCode: 400);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Presentation\Web\Endpoints\OrganizationTenantEndpoints.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Presentation.Web;

using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;
using BridgingIT.DevKit.Presentation.Web;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

/// <summary>
///     Specifies the external API for this module that will be exposed for the outside boundary
/// </summary>
public class OrganizationTenantEndpoints : EndpointsBase
{
    /// <summary>
    ///     Maps the endpoints for the Organization Tenant to the specified route builder.
    /// </summary>
    /// <param name="app">The IEndpointRouteBuilder instance used to define routes.</param>
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/organization/tenants")
            .WithTags("Organization");

        group.MapGet("/{id}", TenantFindOne)
            .WithName("GetOrganizationTenant")
            .ProducesValidationProblem()
            .ProducesProblem(500);
        //.Produces<ProblemDetails>(400)
        //.Produces<ProblemDetails>(500);

        group.MapGet(string.Empty, TenantFindAll)
            .WithName("GetOrganizationTenants")
            .ProducesValidationProblem()
            .ProducesProblem(500);
        //.Produces<ProblemDetails>(400)
        //.Produces<ProblemDetails>(500);

        group.MapPost(string.Empty, TenantCreate)
            .WithName("CreateOrganizationTenant")
            .ProducesValidationProblem()
            .ProducesProblem(500);
        //.Produces<ProblemDetails>(400)
        //.Produces<ProblemDetails>(500);

        // TODO: update/delete tenant
    }

    private static async Task<Results<Ok<TenantModel>, NotFound, ProblemHttpResult>> TenantFindOne(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new TenantFindOneQuery(id))).Result;

        return result.IsFailure && result.HasError<NotFoundResultError>()
            ? TypedResults.NotFound()
            : result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Tenant, TenantModel>(result.Value))
                : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<TenantModel>>, ProblemHttpResult>> TenantFindAll(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper)
    {
        var result = (await mediator.Send(new TenantFindAllQuery())).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Tenant>, IEnumerable<TenantModel>>(result.Value))
            : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Created<TenantModel>, ProblemHttpResult>> TenantCreate(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromBody] TenantModel model)
    {
        var result = (await mediator.Send(new TenantCreateCommand(model))).Result;

        return result.IsSuccess
            ? TypedResults.Created($"api/tenants/{result.Value.Id}", mapper.Map<Tenant, TenantModel>(result.Value))
            : TypedResults.Problem(result.ToString(), statusCode: 400);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\AuthorAggregate\Events\AuthorBookAssignedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

public class AuthorBookAssignedDomainEvent(Author author, Book book) : DomainEventBase
{
    public TenantId TenantId { get; } = author.TenantId;

    public AuthorId AuthorId { get; } = author.Id;

    public string AuthorName { get; } = author.PersonName;

    public BookId BookId { get; } = book.Id;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\AuthorAggregate\Events\AuthorCreatedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

public class AuthorCreatedDomainEvent(TenantId tenantId, Author author) : DomainEventBase
{
    public TenantId TenantId { get; } = tenantId;

    public AuthorId AuthorId { get; } = author.Id;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\AuthorAggregate\Events\AuthorUpdatedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

public class AuthorUpdatedDomainEvent(TenantId tenantId, Author author) : DomainEventBase
{
    public TenantId TenantId { get; } = tenantId;

    public AuthorId AuthorId { get; } = author.Id;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\BookAggregate\Events\BookAuthorAssignedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

public class BookAuthorAssignedDomainEvent(Book book, Author author) : DomainEventBase
{
    public TenantId TenantId { get; } = book.TenantId;

    public BookId BookId { get; } = book.Id;

    public string BookTitle { get; } = book.Title;

    public AuthorId AuthorId { get; } = author.Id;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\BookAggregate\Events\BookCreatedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

public class BookCreatedDomainEvent(TenantId tenantId, Book book) : DomainEventBase
{
    public TenantId TenantId { get; } = tenantId;

    public BookId BookId { get; } = book.Id;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\BookAggregate\Events\BookUpdatedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

public class BookUpdatedDomainEvent(TenantId tenantId, Book book) : DomainEventBase
{
    public TenantId TenantId { get; } = tenantId;

    public BookId BookId { get; } = book.Id;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\BookAggregate\Specifications\BookForIsbnSpecification.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

public class BookForIsbnSpecification(TenantId tenantId, BookIsbn isbn) : Specification<Book>
{
    public override Expression<Func<Book, bool>> ToExpression()
    {
        return e => e.TenantId == tenantId && e.Isbn == isbn;
    }
}

public static class BookSpecifications
{
    public static Specification<Book> ForIsbn(TenantId tenantId, BookIsbn isbn)
    {
        return new BookForIsbnSpecification(tenantId, isbn);
    }

    public static Specification<Book> ForIsbn2(TenantId tenantId, BookIsbn isbn) // INFO: short version to define a specification
    {
        return new Specification<Book>(e => e.TenantId == tenantId && e.Isbn == isbn);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\CategoryAggregate\Events\CategoryCreatedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

public class CategoryCreatedDomainEvent(Category category) : DomainEventBase
{
    public CategoryId CategoryId { get; } = category.Id;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\CategoryAggregate\Events\CategoryUpdatedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

public class CategoryUpdatedDomainEvent(Category category) : DomainEventBase
{
    public CategoryId CategoryId { get; } = category.Id;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\CustomerAggregate\Events\CustomerAddressUpdatedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

public class CustomerAddressUpdatedDomainEvent(Customer customer) : DomainEventBase
{
    public CustomerId CustomerId { get; } = customer.Id;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\CustomerAggregate\Events\CustomerCreatedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

public class CustomerCreatedDomainEvent(Customer customer) : DomainEventBase
{
    public TenantId TenantId { get; } = customer.TenantId;
    public CustomerId CustomerId { get; } = customer.Id;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\CustomerAggregate\Events\CustomerUpdatedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

public class CustomerUpdatedDomainEvent(Customer customer) : DomainEventBase
{
    public CustomerId CustomerId { get; } = customer.Id;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\CustomerAggregate\Specifications\CustomerForEmailSpecification.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

public class CustomerForEmailSpecification(TenantId tenantId, EmailAddress email) : Specification<Customer>
{
    public override Expression<Func<Customer, bool>> ToExpression()
    {
        return e => e.TenantId == tenantId && e.Email == email;
    }
}

public static class CustomerSpecifications
{
    public static Specification<Customer> ForEmail(TenantId tenantId, EmailAddress email)
    {
        return new CustomerForEmailSpecification(tenantId, email);
    }

    public static Specification<Customer>
        ForEmail2(TenantId tenantId, EmailAddress email) // INFO: short version to define a specification
    {
        return new Specification<Customer>(e => e.TenantId == tenantId && e.Email == email);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\PublisherAggregate\Events\PublisherCreatedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

public class PublisherCreatedDomainEvent(Publisher publisher) : DomainEventBase
{
    public PublisherId PublisherId { get; } = publisher.Id;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Domain\Model\PublisherAggregate\Events\PublisherUpdatedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

public class PublisherUpdatedDomainEvent(Publisher publisher) : DomainEventBase
{
    public PublisherId PublisherId { get; } = publisher.Id;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Domain\Model\StockAggregate\Events\StockAdjustedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

public class StockAdjustedDomainEvent(Stock stock, int oldQuantity, int newQuantity, int quantityChange, string reason)
    : DomainEventBase
{
    public TenantId TenantId { get; } = stock.TenantId;
    public StockId StockId { get; } = stock.Id;
    public ProductSku Sku { get; } = stock.Sku;
    public int OldQuantity { get; } = oldQuantity;
    public int NewQuantity { get; } = newQuantity;
    public int QuantityChange { get; } = quantityChange;
    public string Reason { get; } = reason;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Domain\Model\StockAggregate\Events\StockCreatedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

public class StockCreatedDomainEvent(Stock stock) : DomainEventBase
{
    public TenantId TenantId { get; } = stock.TenantId;
    public StockId StockId { get; } = stock.Id;
    public ProductSku Sku { get; } = stock.Sku;
    public int QuantityOnHand { get; } = stock.QuantityOnHand;
    public int QuantityReserved { get; } = stock.QuantityReserved;
    public Money UnitCost { get; } = stock.UnitCost;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Domain\Model\StockAggregate\Events\StockLocationChangedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

public class StockLocationChangedDomainEvent(Stock stock, StorageLocation newLocation) : DomainEventBase
{
    public TenantId TenantId { get; } = stock.TenantId;
    public StockId StockId { get; } = stock.Id;
    public ProductSku Sku { get; } = stock.Sku;
    public StorageLocation NewLocation { get; } = newLocation;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Domain\Model\StockAggregate\Events\StockQuantityAdjustedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

public class StockQuantityAdjustedDomainEvent(Stock stock, int oldQuantity, int newQuantity, int quantityChange, string reason)
    : DomainEventBase
{
    public TenantId TenantId { get; } = stock.TenantId;
    public StockId StockId { get; } = stock.Id;
    public ProductSku Sku { get; } = stock.Sku;
    public int OldQuantity { get; } = oldQuantity;
    public int NewQuantity { get; } = newQuantity;
    public int QuantityChange { get; } = quantityChange;
    public string Reason { get; } = reason;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Domain\Model\StockAggregate\Events\StockReservedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

public class StockReservedDomainEvent(Stock stock, int quantityReserved) : DomainEventBase
{
    public TenantId TenantId { get; } = stock.TenantId;
    public StockId StockId { get; } = stock.Id;
    public ProductSku Sku { get; } = stock.Sku;
    public int QuantityReserved { get; } = quantityReserved;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Domain\Model\StockAggregate\Events\StockReservedReleasedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

public class StockReservedReleasedDomainEvent(Stock stock, int quantityReleased) : DomainEventBase
{
    public TenantId TenantId { get; } = stock.TenantId;
    public StockId StockId { get; } = stock.Id;
    public ProductSku Sku { get; } = stock.Sku;
    public int QuantityReleased { get; } = quantityReleased;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Domain\Model\StockAggregate\Events\StockUnitCostAdjustedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

public class StockUnitCostAdjustedDomainEvent(Stock stock, Money oldUnitCost, Money newUnitCost, string reason)
    : DomainEventBase
{
    public TenantId TenantId { get; } = stock.TenantId;
    public StockId StockId { get; } = stock.Id;
    public ProductSku Sku { get; } = stock.Sku;
    public Money OldUnitCost { get; } = oldUnitCost;
    public Money NewUnitCost { get; } = newUnitCost;
    public string Reason { get; } = reason;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Domain\Model\StockAggregate\Events\StockUnitCostChangedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

public class StockUnitCostChangedDomainEvent(Stock stock, Money oldUnitCost, Money newUnitCost) : DomainEventBase
{
    public TenantId TenantId { get; } = stock.TenantId;
    public StockId StockId { get; } = stock.Id;
    public ProductSku Sku { get; } = stock.Sku;
    public Money OldUnitCost { get; } = oldUnitCost;
    public Money NewUnitCost { get; } = newUnitCost;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Domain\Model\StockAggregate\Events\StockUpdatedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

public class StockUpdatedDomainEvent(Stock stock) : DomainEventBase
{
    public TenantId TenantId { get; } = stock.TenantId;
    public StockId StockId { get; } = stock.Id;
    public ProductSku Sku { get; } = stock.Sku;
    public int QuantityOnHand { get; } = stock.QuantityOnHand;
    public int QuantityReserved { get; } = stock.QuantityReserved;
    public Money UnitCost { get; } = stock.UnitCost;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Domain\Model\StockAggregate\Specifications\StockForSkuSpecification.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

public class StockForSkuSpecification(TenantId tenantId, ProductSku sku) : Specification<Stock>
{
    public override Expression<Func<Stock, bool>> ToExpression()
    {
        return e => e.TenantId == tenantId && e.Sku.Value == sku.Value;
    }
}

public static class StockSpecifications
{
    public static Specification<Stock> ForSku(TenantId tenantId, ProductSku sku)
    {
        return new StockForSkuSpecification(tenantId, sku);
    }

    public static Specification<Stock> ForSku2(TenantId tenantId, ProductSku sku) // INFO: short version to define a specification
    {
        return new Specification<Stock>(e => e.TenantId == tenantId && e.Sku.Value == sku.Value);
    }
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Domain\Model\StockSnapshotAggregate\Events\StockSnapshotCreatedDomainEvent.cs
// ----------------------------------------
namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

public class StockSnapshotCreatedDomainEvent(StockSnapshot snapshot) : DomainEventBase
{
    public StockSnapshotId Id { get; } = snapshot.Id;
    public TenantId TenantId { get; } = snapshot.TenantId;
    public StockId StockId { get; } = snapshot.StockId;
    public ProductSku Sku { get; } = snapshot.Sku;
    public int QuantityOnHand { get; } = snapshot.QuantityOnHand;
    public int QuantityReserved { get; } = snapshot.QuantityReserved;
    public Money UnitCost { get; } = snapshot.UnitCost;
    public StorageLocation Location { get; } = snapshot.Location;
    public DateTimeOffset SnapshotTimestamp { get; } = snapshot.Timestamp;
}

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\CODE_OF_CONDUCT.md
// ----------------------------------------
```
# Contributor Code of Conduct

As contributors and maintainers of this project, and in the interest of fostering an open and welcoming community, we pledge to respect all people who contribute through reporting issues, posting feature requests, updating documentation, submitting pull requests or patches, and other activities.

We are committed to making participation in this project a harassment-free experience for everyone, regardless of level of experience, gender, gender identity and expression, sexual orientation, disability, personal appearance, body size, race, ethnicity, age, religion, or nationality.

Examples of unacceptable behavior by participants include:

* The use of sexualized language or imagery
* Personal attacks
* Trolling or insulting/derogatory comments
* Public or private harassment
* Publishing other's private information, such as physical or electronic addresses, without explicit permission
* Other unethical or unprofessional conduct.

Project maintainers have the right and responsibility to remove, edit, or reject comments, commits, code, wiki edits, issues, and other contributions that are not aligned to this Code of Conduct. By adopting this Code of Conduct, project maintainers commit themselves to fairly and consistently applying these principles to every aspect of managing this project. Project maintainers who do not follow or enforce the Code of Conduct may be permanently removed from the project team.

This code of conduct applies both within project spaces and in public spaces when an individual is representing the project or its community.

Instances of abusive, harassing, or otherwise unacceptable behavior may be reported by opening an issue or contacting one or more of the project maintainers.

This Code of Conduct is adapted from the [Contributor Covenant](https://contributor-covenant.org), version 1.2.0, available at [http://contributor-covenant.org/version/1/2/0/](https://contributor-covenant.org/version/1/2/0/)
```

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\README.md
// ----------------------------------------
![bITDevKit](https://github.com/BridgingIT-GmbH/bITdevKit.Examples.BookFiesta/blob/main/bITDevKit_Logo.png?raw=true)

![bITDevKit](https://raw.githubusercontent.com/BridgingIT-GmbH/bITdevKit.Examples.BookFiesta/refs/heads/main/bITDevKit_BookFiesta_Banner.png)
=====================================

> An application built using .NET 8 and following a Domain-Driven Design approach by using the
> BridgingIT DevKit.

<!-- TOC -->
  * [Features](#features)
  * [Key Technologies and Frameworks](#key-technologies-and-frameworks)
  * [Architecture Overview](#architecture-overview)
    * [Patterns & Principles](#patterns--principles)
    * [Layers](#layers)
      * [1. Domain Layer](#1-domain-layer)
      * [2. Application Layer](#2-application-layer)
      * [3. Infrastructure Layer](#3-infrastructure-layer)
      * [4. Presentation Layer](#4-presentation-layer)
    * [Request Processing Flow](#request-processing-flow)
    * [Architecture Decision Records (ADR)](#architecture-decision-records-adr)
    * [Cross-cutting Concerns](#cross-cutting-concerns)
  * [Data Storage](#data-storage)
  * [External API Layer](#external-api-layer)
  * [Application Testing](#application-testing)
  * [Modular Structure](#modular-structure)
    * [Organization Module](#organization-module)
    * [Catalog Module](#catalog-module)
    * [Inventory Module](#inventory-module)
  * [Getting Started](#getting-started)
    * [Prerequisites](#prerequisites)
    * [Running the Application](#running-the-application)
    * [Solution Structure](#solution-structure)
    * [Testing the API](#testing-the-api)
      * [Swagger UI](#swagger-ui)
      * [Unit Tests](#unit-tests)
      * [Integration Tests](#integration-tests)
      * [Http Tests](#http-tests)
<!-- TOC -->

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

## Key Technologies and Frameworks

- [.NET 8](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8/overview)
- [C#](https://learn.microsoft.com/en-us/dotnet/csharp/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [ASP.NET Core](https://dotnet.microsoft.com/en-us/apps/aspnet)
- [Serilog](https://serilog.net/)
- MediatR
- FluentValidation
- Mapster
- OpenTelemetry
- [xUnit.net](https://xunit.net/), [NSubstitute](https://nsubstitute.github.io/), [Shouldly](https://docs.shouldly.org/)

## Architecture Overview

The application is designed as a modular monolith, adhering to Clean Architecture
principles and Domain-Driven Design (DDD) concepts. Built using .NET Core, it's organized into
several distinct modules, each focusing on a specific business domain.

### Patterns & Principles

This architecture provides a scalable, maintainable, and modular structure for the application,
allowing for easy extension and modification of individual modules while maintaining a
cohesive overall system.
The modular approach combined with Clean Architecture and DDD principles ensures clear separation of
concerns and high cohesion within each module.

1. **Domain-Driven Design (DDD)**: Evident in the use of aggregates, entities, value objects, and
   domain events.
2. **Modularization**: The application is divided into distinct, loosely-coupled modules, each with
   its own domain logic and infrastructure.
3. **Screaming Architecture**: The folder structure and namespaces reflect the business domains
   rather than technical concerns.
4. **The Dependency Rule**: Dependencies can only point inwards. Nothing in an inner circle can know
   anything at all about something in an outer circle (See Layers).
5. **Dependency Injection (DI)**: Widely used throughout the application for loose coupling and
   better testability.
6. **CQS (Command Query Separation)**: Commands and queries are separated, but using the same
   persistence store.
7. **Use Cases**: Application logic is encapsulated in commands and queries, each with a single
   responsibility.
8. **Mediator Pattern**: MediatR is used for handling commands and queries.
9. **Repository Pattern**: Used for data access and persistence.
10. **Specification Pattern**: Used for defining query criteria.
11. **Outbox Pattern**: Used for reliable event and message publishing.

### Layers

The solution is structured following the Clean Architecture layers and the references
between them:

- **Domain**: Contains the core business logic and domain model.
- **Application**: Handles application-specific logic, including commands and queries.
- **Infrastructure**: Manages data access, external services, and other infrastructure concerns.
- **Presentation**: Provides the user interface and API endpoints.
- **SharedKernel**: Contains shared concepts, such as value objects, rules, and interfaces.

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

#### 1. Domain Layer

The Domain layer is the core of the application, containing the business logic and rules.

Key components:

- Entities (e.g., `Book`, `Author`, `Stock`)
- Value Objects (e.g., `Money`, `BookIsbn`, `EmailAddress`)
- Aggregates (e.g., `Book` aggregate, `Stock` aggregate)
- Domain Events (e.g., `BookCreatedDomainEvent`, `StockUpdatedDomainEvent`)
- Specifications (e.g., `BookForIsbnSpecification`)
- Policies
- Rules

Characteristics:

- No dependencies on other layers or external libraries (except for some .NET base classes)
- Contains the core business logic and rules
- Defines interfaces for repositories (e.g., `IBookRepository`)

Example from the application:

```csharp
public class Book : AuditableAggregateRoot<BookId>, IConcurrent
{
    public TenantId TenantId { get; }
    public string Title { get; private set; }
    public BookIsbn Isbn { get; private set; }
    public Money Price { get; private set; }
    // ... other properties and methods
}
```

#### 2. Application Layer

The Application layer orchestrates the flow of data to and from the Domain layer, and coordinates
application logic.

Key components:

- Command/Query Handlers (e.g., `BookCreateCommandHandler`, `BookFindOneQueryHandler`)
- Commands and Queries (e.g., `BookCreateCommand`, `BookFindOneQuery`)
- Interfaces for infrastructure concerns (e.g., `IBookRepository`)
- Application Services (e.g., `ICatalogQueryService`)
- Application Models (Contracts)

Characteristics:

- Depends on the Domain layer
- Contains no business logic
- Orchestrates the execution of business logic defined in the Domain layer
- Uses the Mediator pattern (via MediatR) for handling commands and queries

Example from the application:

```csharp

public class BookCreateCommand(string tenantId, BookModel model)
  : CommandRequestBase<Result<Book>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public BookModel Model { get; } = model;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<BookCreateCommand>
    {
        public Validator()
        {
          // Add validation rules (RuleFor)
        }
    }
}

public class BookCreateCommandHandler
  : CommandHandlerBase<BookCreateCommand, Result<Book>>
{
    private readonly IGenericRepository<Book> repository;

    public BookCreateCommandHandler(ILoggerFactory loggerFactory, IGenericRepository<Book> repository)
        : base(loggerFactory)
    {
        this.repository = repository;
    }

    public override async Task<CommandResponse<Result<Book>>> Process(BookCreateCommand command, CancellationToken cancellationToken)
    {
        // Create book entity, apply business rules, save to repository
    }
}
```

#### 3. Infrastructure Layer

The Infrastructure layer provides implementations for database access, external services, and other
infrastructure concerns.

Key components:

- Repository/QueryService Implementations
- Database Contexts (e.g., `CatalogDbContext`, `InventoryDbContext`)
- Entity Configurations (e.g., `BookEntityTypeConfiguration`)
- External Service Clients
- Infrastructure-specific Models

Characteristics:

- Depends on the Application and Domain layers
- Implements interfaces defined in the Application layer
- Contains database-specific logic and ORM configurations
- Handles data persistence and retrieval

Example from the application:

```csharp
public class CatalogDbContext : ModuleDbContextBase, IDocumentStoreContext, IOutboxDomainEventContext
{
    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }
    // ... other DbSets and configuration
}
```

#### 4. Presentation Layer

The Presentation layer is responsible for handling HTTP requests and presenting data to the client.

Key components:

- API Controllers or Minimal API Endpoints
- Middleware

Characteristics:

- Depends on the Application layer
- Handles HTTP requests and responses
- Maps between Application models and Application layer commands/queries
- Implements API-specific concerns (authentication, authorization, etc.)

Example from the application:

```csharp
public class CatalogBookEndpoints : EndpointsBase
{
    public override void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/books/{id}", GetBook);
        app.MapPost("/api/books", CreateBook);
        // ... other endpoint mappings
    }

    private static async Task<IResult> GetBook(IMediator mediator, string id)
    {
        var result = await mediator.Send(new BookFindOneQuery(id));
        return result.Match(
            success => Results.Ok(success),
            failure => Results.NotFound());
    }
}
```

### Request Processing Flow

The following sequence diagram illustrates the flow of a request through the layers of the
application:

```mermaid
sequenceDiagram
  participant C as Client
  participant EA as External API
  participant CM as Command
  participant CV as CommandValidator
  participant CH as CommandHandler
  participant M as Mapper
  participant R as Repository

  C ->> EA: 1. Request
  EA ->> CM: 2. Create Command (with Model)
  CM ->> CV: 3. Validate
  CV -->> CM: 4. Validation Result
  CM ->> CH: 5. Process
  CH ->> R: 6. FindOneResultAsync(EntityId)
  R -->> CH: 7. Result
  CH ->> M: 8. Map Model to existing Domain Entity
  M -->> CH: 9. Updated Domain Entity
  CH ->> CH: 10. Apply Domain Rules
  CH ->> R: 11. UpdateAsync(Domain Entity)
  R -->> CH: 12. Updated Domain Entity
  CH -->> EA: 13. Result
  EA ->> M: 14. Map Domain Entity to Model
  M -->> EA: 15. Model
  EA ->> EA: 16. Format Response
  EA -->> C: 17. Response
```

1. The process begins when a Client sends a Web Request to the External API.
2. The External API creates a Command object, which includes a Model.
3. The Command is then validated by a CommandValidator, which returns a Validation Result.
4. If validation passes, the Command is processed by a CommandHandler.
5. The CommandHandler interacts with a Repository to find an existing entity using FindOneResultAsync(EntityId).
6. Once the existing entity is retrieved, a Mapper is used to update the Domain Entity with the new data from the Model.
7. The CommandHandler then applies any necessary Domain Rules to the updated entity.
8. The updated Domain Entity is then saved back to the Repository using UpdateAsync(Domain Entity).
9. The Repository returns the updated Domain Entity to the CommandHandler.
10. The CommandHandler returns a Result to the External API.
11. The External API uses the Mapper again to convert the Domain Entity back into a Model.
12. Finally, the External API formats the response and sends the Web Response back to the Client.

### Architecture Decision Records (ADR)

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

### Cross-cutting Concerns

While not a specific layer, cross-cutting concerns are handled across all layers:

- Logging (using Serilog)
- Validation (using FluentValidation)
- Authentication and Authorization
- Error Handling
- Caching
- Multitenancy

These concerns are often implemented using middleware, attributes, or by integrating into the
request pipeline.

This layered architecture ensures a clear separation of concerns, with each layer having a distinct
responsibility. The dependencies flow inwards, with the Domain layer at the core having no external
dependencies. This structure allows for easier testing, maintenance, and evolution of the system
over time.

## Data Storage

- SQL Server is used as the primary database.
- Each module has its own DbContext for database operations.
- Each module has its own schema and migrations in the database.

## External API Layer

- RESTful APIs are implemented using minimal API syntax in .NET Core.
- OpenAPI (Swagger) is used for API documentation.

## Application Testing

- The architecture supports unit testing, integration testing, and end-to-end testing.
- Integration testing depends on docker containers.

## Modular Structure

The application is divided into the following main modules:

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

> Manages tenants, companies, and subscriptions.

[see](./src/Modules/Organization/Organization-README.md)

### Catalog Module

> Manages books, authors, and categories.

[see](./src/Modules/Catalog/Catalog-README.md)

### Inventory Module

> Handles stock management and inventory tracking.

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

### Solution Structure

<img src="./assets/image-20240426112716841.png" alt="image-20240426112716841" style="zoom:50%;" />

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

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\docs\adrs\adr-001-modular-monolith.md
// ----------------------------------------
# ADR 001: Modular Monolith vs Microservices

## Status

`Accepted`

## Context

We need to decide on the overall architecture for the application. The main options are a modular monolith and a microservices architecture.

## Decision

We will implement the application as a modular monolith.

## Consequences

### Positive

- Simplified deployment and operations
- Easier development and testing
- Lower initial complexity
- Better performance for inter-module communication
- Easier refactoring and code sharing

### Negative

- Limited independent scalability
- Technology lock-in
- Potential for decreased development velocity in the long term

### Neutral

- Future migration path to microservices if needed
- Team organization flexibility

## References

- [Modular Monolith: A Primer](https://www.kamilgrzybek.com/design/modular-monolith-primer/)
- [MonolithFirst by Martin Fowler](https://martinfowler.com/bliki/MonolithFirst.html)

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\docs\adrs\adr-002-http-api.md
// ----------------------------------------
# ADR 002: HTTP API vs gRPC

## Status

`Accepted`

## Context

We need to decide on the communication protocol for the application it's external API.

## Decision

We will implement an HTTP API (REST) for the application.

## Consequences

### Positive

- Broad client support
- Human-readable payloads
- Can leverage existing web infrastructure
- Familiar to most developers
- Easier testing and debugging

### Negative

- Less e

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\docs\adrs\adr-003-sync-module-clients.md
// ----------------------------------------
# ADR 003: Synchronous Inter-Module Communication

## Status

`Accepted`

## Context

We need a way for modules in our modular monolith to communicate synchronously.

## Decision

We will use ModuleClients for synchronous inter-module communication.

## Consequences

### Positive
- Clear module boundaries
- Compile-time type safety
- Easier refactoring and testing

### Negative
- Additional code for client interfaces
- Small performance overhead

## Example

```csharp
public interface IInventoryModuleClient
{
    Task<Result<StockModel>> StockFindOne(string tenantId, string id);
    Task<Result<IEnumerable<StockModel>>> StockFindAll(string tenantId);
    // ...
}
```

Another module can use this client to interact with the module synchronously.

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\docs\adrs\adr-004-async-messaging.md
// ----------------------------------------
# ADR 004: Asynchronous Inter-Module Communication

## Status

`Accepted`

## Context

We need a way for modules to communicate asynchronously for eventually consistent operations.

## Decision

We will use a message bus for asynchronous inter-module communication.

## Consequences

### Positive
- Decouples modules
- Supports eventual consistency
- Improves scalability

### Negative
- Increased complexity
- Potential for message failures

## Example

```csharp
public class OrderCreatedMessage : IMessage
{
    public string OrderId { get; set; }
    public string ProductId { get; set; }
    public int Quantity { get; set; }
}

// In the Order module
await _messageBroker.PublishAsync(new OrderCreatedMessage { ... });

// In the Inventory module
public class OrderCreatedMessageHandler : IMessageHandler<OrderCreatedMessage>
{
    public Task Handle(OrderCreatedMessage message, CancellationToken cancellationToken)
    {
        // Update inventory
    }
}
```

This allows for asynchronous communication between the distinct modules.

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\docs\adrs\adr-005-rich-domain-models.md
// ----------------------------------------
# ADR 005: Rich Domain Models vs Anemic CRUD Entities

## Status

`Accepted`

## Context

We need to decide how to structure our domain models and where to place business logic within our application architecture.

## Decision

We will use rich domain models that encapsulate business logic rather than anemic CRUD entities.

## Consequences

### Positive
- Business logic is centralized in the domain layer
- Improved encapsulation and data integrity
- Better alignment with Domain-Driven Design principles
- Easier to maintain and extend as the application grows

### Negative
- Steeper learning curve for developers used to anemic models
- Potential for increased complexity in simple CRUD operations

## Example

```csharp
public class Book : AggregateRoot<BookId>
{
    public string Title { get; private set; }
    public Money Price { get; private set; }
    public List<AuthorId> AuthorIds { get; private set; }

    public void UpdatePrice(Money newPrice)
    {
        if (newPrice.Amount < 0)
            throw new InvalidOperationException("Price cannot be negative");

        Price = newPrice;
        AddDomainEvent(new BookPriceUpdatedEvent(Id, newPrice));
    }

    public void AddAuthor(AuthorId authorId)
    {
        if (AuthorIds.Count >= 5)
            throw new InvalidOperationException("A book cannot have more than 5 authors");

        AuthorIds.Add(authorId);
    }
}
```

This rich domain model encapsulates business rules (e.g., price validation, author limit) and raises domain events, unlike a simple CRUD entity.

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\docs\adrs\adr-006-database-choice.md
// ----------------------------------------
# ADR 006: Database Choice - SQL Server

## Status

`Accepted`

## Context

We need a reliable, scalable database solution for the application that can handle complex relationships and support ACID transactions.

## Decision

We will use SQL Server as our primary database.

## Consequences

### Positive
- Strong consistency and ACID compliance
- Powerful querying capabilities with T-SQL
- Good integration with Entity Framework Core
- Robust security features

### Negative
- Licensing costs for commercial use
- May be overkill for simple data storage needs

## Example

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
}
```

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\docs\adrs\adr-007-logging-monitoring.md
// ----------------------------------------
# ADR 007: Logging and Monitoring Approach

## Status

`Accepted`

## Context

We need a comprehensive logging and monitoring solution to track application performance, errors, and user activities across all modules.

## Decision

We will use Serilog for logging and integrate with Seq for log aggregation and analysis.

## Consequences

### Positive
- Structured logging with Serilog
- Centralized log management and analysis with Seq
- Easy to query and visualize logs

### Negative
- Additional setup and maintenance for Seq
- Potential cost for Seq licenses in production

## Example

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

Log.Information("Application {AppName} started", "BookFiesta");
```

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\docs\adrs\adr-008-modularization-strategy.md
// ----------------------------------------
# ADR 008: Comprehensive Modularization Strategy

## Status

`Accepted`~~~~

## Context

The application needs a clear structure that promotes maintainability, scalability, and separation of concerns. We need to decide on an approach that will guide the organization of our codebase, from the domain model through the application layer to the data model.

## Decision

We will implement a comprehensive modularization strategy that extends from the domain model to the data layer. Each module will encapsulate highly cohesive functionality and will have the ability to communicate with other modules when necessary.

## Consequences

### Positive
- Clear boundaries between different parts of the system
- Improved maintainability and easier to understand codebase
- Facilitates parallel development by different teams
- Easier to test individual modules
- Flexibility to evolve or replace modules independently

### Negative
- Increased initial complexity in setting up module boundaries
- Potential for overengineering if modularization is taken to extremes
- Need for careful design of inter-module communication

## Implementation Details

1. Domain Layer: Each module will have its own set of domain entities, value objects, and domain services.

2. Application Layer: Modules will have their own application services, commands, and queries.

3. Infrastructure Layer: Each module can have its own repositories and external service integrations.

4. Presentation Layer: API endpoints will be organized by module.

5. Data Model: Database schemas will be aligned with modules to maintain separation.

6. Inter-module Communication: Modules can communicate through well-defined interfaces (ModuleClients) and message-based integration events.

## Example

```csharp
// Catalog Module
namespace BookFiesta.Catalog.Domain
{
    public class Book : AggregateRoot<BookId>
    {
        // Book properties and methods
    }
}

namespace BookFiesta.Catalog.Application
{
    public class CreateBookCommand : IRequest<Result<BookDto>>
    {
        // Command properties
    }
}

// Inventory Module
namespace BookFiesta.Inventory.Domain
{
    public class StockItem : AggregateRoot<StockItemId>
    {
        // StockItem properties and methods
    }
}

// Inter-module communication
namespace BookFiesta.Catalog.Infrastructure
{
    public class CatalogService
    {
        private readonly IInventoryModuleClient _inventoryClient;

        public async Task<bool> IsBookInStock(BookId bookId)
        {
            return await _inventoryClient.CheckStockAvailability(bookId);
        }
    }
}
```

This structure demonstrates how different modules (Catalog and Inventory) are separated but can still communicate when needed.

## References

- [Modular Monolith: A Primer](https://www.kamilgrzybek.com/design/modular-monolith-primer/)
- [Domain-Driven Design](https://domainlanguage.com/ddd/)

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Presentation.Web.Server\README.md
// ----------------------------------------
# BookFiesta

## Create and apply a new Database Migration

## Update the generated ApiClient

### Prerequisites

- `dotnet new tool-manifest`
- `dotnet tool install NSwag.ConsoleCore`

The tools manifest can be found [here](../../../.config/dotnet-tools.json)

### Install the dotnet tools

- `dotnet tool restore`

### Start the web project

- `dotnet run --project '.\src\Presentation.Web.Server\Presentation.Web.Server.csproj'`

### Update the swagger file

- `dotnet nswag run '.\src\Presentation.Web.Server\nswag.json'`

Rebuild the solution and the ApiClient should be updated.
For details see the `OpenApiReference` target in the Client project.

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\SharedKernel\SharedKernel-README.md
// ----------------------------------------
SharedKernel![bITDevKit](https://raw.githubusercontent.com/bridgingIT/bITdevKit.Examples.BookFiesta/main/bITDevKit_Logo.png)
=====================================

# SharedKernel Component Overview

> The SharedKernel is a crucial component in our Domain-Driven Design (DDD) architecture. It
> contains common elements, concepts, and utilities that are shared across multiple bounded contexts
> or modules in the system. These shared elements can come from any layer of the application
> architecture, including the domain layer, application layer, infrastructure layer, and
> presentation
> layer.

## Domain Layer

The following value objects are part of the domain layer within the SharedKernel:

### 1. Address

- Represents a general physical address
- Contains fields for name, line1, line2, postal code, city, and country

```mermaid
classDiagram
    class Address {
        +string Name
        +string Line1
        +string Line2
        +string PostalCode
        +string City
        +string Country
        +Create(name, line1, line2, postalCode, city, country)
    }
```

### 2. AverageRating

- Represents an average rating with the total number of ratings
- Provides methods to add or remove individual ratings

```mermaid
classDiagram
    class AverageRating {
        +double? Value
        +int Amount
        +Create(value, numRatings)
        +Add(Rating)
        +Remove(Rating)
    }
```

### 3. Currency

- Represents a monetary currency
- Provides a list of world currencies with their symbols

```mermaid
classDiagram
    class Currency {
        +string Code
        +string Symbol
        +Create(code)
        +static Currency USDollar
        +static Currency Euro
        +static Currency GBPound
    }
```

### 4. EmailAddress

- Represents a valid email address
- Validates the email format using a regular expression

```mermaid
classDiagram
    class EmailAddress {
        +string Value
        +Create(email)
    }
```

### 5. HexColor

- Represents a color in hexadecimal format
- Provides methods to create from string or RGB values

```mermaid
classDiagram
    class HexColor {
        +string Value
        +Create(color)
        +Create(r, g, b)
        +ToRGB()
    }
```

### 6. Money

- Represents a monetary amount with a specific currency
- Provides arithmetic operations (addition, subtraction)

```mermaid
classDiagram
    class Money {
        +decimal Amount
        +Currency Currency
        +Create(amount, currency)
        +operator +(Money, Money)
        +operator -(Money, Money)
    }
```

### 7. PersonFormalName

- Represents a person's formal name
- Stores name parts, title, and suffix separately

```mermaid
classDiagram
    class PersonFormalName {
        +string Title
        +string[] Parts
        +string Suffix
        +string Full
        +Create(parts, title, suffix)
    }
```

### 8. PhoneNumber

- Represents a phone number with country code
- Validates phone number format

```mermaid
classDiagram
    class PhoneNumber {
        +string CountryCode
        +string Number
        +Create(phoneNumber)
    }
```

### 9. Rating

- Represents a single rating value
- Provides static methods for common ratings (Poor, Fair, Good, etc.)

```mermaid
classDiagram
    class Rating {
        +int Value
        +Create(value)
        +static Rating Poor()
        +static Rating Fair()
        +static Rating Good()
        +static Rating VeryGood()
        +static Rating Excellent()
    }
```

### 10. Schedule

- Represents a time period with a start date and an optional end date
- Supports open-ended schedules

```mermaid
classDiagram
    class Schedule {
        +DateOnly StartDate
        +DateOnly? EndDate
        +bool IsOpenEnded
        +Create(startDate, endDate)
        +IsActive(date)
        +OverlapsWith(Schedule)
    }
```

### 11. TenantId

- Represents a unique identifier for a tenant
- Implements the AggregateRootId<Guid> class

```mermaid
classDiagram
    class TenantId {
        +Guid Value
        +bool IsEmpty
        +Create()
        +Create(Guid)
        +Create(string)
    }
```

### 12. Url

- Represents a URL (Uniform Resource Locator)
- Supports absolute, relative, and local URLs

```mermaid
classDiagram
    class Url {
        +string Value
        +UrlType Type
        +Create(url)
        +IsAbsolute()
        +IsRelative()
        +IsLocal()
        +ToAbsolute(baseUrl)
    }
```

### 13. VatNumber

- Represents a VAT (Value Added Tax) or EIN (Employer Identification Number)
- Supports country-specific formatting

```mermaid
classDiagram
    class VatNumber {
        +string CountryCode
        +string Number
        +Create(vatNumber)
        +IsValid()
    }
```

### 14. Website

- Represents a website address
- Normalizes and validates website URLs

```mermaid
classDiagram
    class Website {
        +string Value
        +Create(website)
    }
```

## Usage

These domain layer components are designed to be immutable and self-validating, ensuring that domain
logic is consistently applied across the entire system.

Example usage:

```csharp
var address = Address.Create("John Doe", "123 Main St", null, "12345", "Anytown", "USA");
var averageRating = AverageRating.Create(4.5, 10);
var currency = Currency.USDollar;
var email = EmailAddress.Create("user@example.com");
var color = HexColor.Create("#FF5733");
var money = Money.Create(100.50m, Currency.Euro);
var name = PersonFormalName.Create(new[] { "John", "Doe" }, "Mr.", "Jr.");
var phone = PhoneNumber.Create("+1234567890");
var rating = Rating.Create(4);
var schedule = Schedule.Create(new DateOnly(2023, 1, 1), new DateOnly(2023, 12, 31));
var url = Url.Create("https://example.com");
var vatNumber = VatNumber.Create("DE123456789");
var website = Website.Create("www.example.com");
```

### 14. ProductSku

- Represents a product stock keeping unit (SKU)
- Provides methods to create and validate SKU values

```mermaid
classDiagram
  class ProductSku {
    +string Value
    +Create(value)
    +Validate(sku)
  }
```

Example usage:

```csharp
var sku = ProductSku.Create("12345678");
```

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog-README.md
// ----------------------------------------
![bITDevKit](https://github.com/BridgingIT-GmbH/bITdevKit.Examples.BookFiesta/blob/main/bITDevKit_Logo.png?raw=true)

![bITDevKit](https://raw.githubusercontent.com/BridgingIT-GmbH/bITdevKit.Examples.BookFiesta/refs/heads/main/bITDevKit_BookFiesta_Banner.png)
=====================================

<!-- TOC -->
* [Catalog Module Overview](#catalog-module-overview)
  * [Domain Model](#domain-model)
    * [Book Aggregate](#book-aggregate)
    * [Author Aggregate](#author-aggregate)
    * [Supporting Entities and Value Objects](#supporting-entities-and-value-objects)
    * [Key Relationships](#key-relationships)
<!-- TOC -->

# Catalog Module Overview

> The Catalog Module is responsible for managing the book catalog.

## Domain Model

The domain model provides a robust foundation for the module, capturing the essential entities and
their relationships while adhering to Domain-Driven Design principles. It allows for complex
operations such as managing books with multiple authors, hierarchical categorization, and flexible
tagging, while maintaining clear boundaries between aggregates.

The domain model consists of two main aggregates: Book and Author. These aggregates, along with
supporting entities and value objects, form the core of the domain model. A summary of each
aggregate and their relationships:

```mermaid
classDiagram
    class Book {a
        +BookId Id
        +TenantId TenantId
        -List~AuthorId~ authorIds
        -List~Category~ categories
        -List~Tag~ tags
        -List~BookChapter~ chapters
        +Title: string
        +Description: string
        +Isbn: BookIsbn
        +Price: Money
        +Version: Guid
    }

    class Author {
        +AuthorId Id
        +TenantId TenantId
        -List~BookId~ bookIds
        -List~Tag~ tags
        +PersonName: PersonFormalName
        +Biography: string
        +Version: Guid
    }

    class BookChapter {
        +BookChapterId Id
        +Title: string
        +Number: int
        +Content: string
    }

    class Category {
        +CategoryId Id
        +TenantId TenantId
        -List~Book~ books
        -List~Category~ children
        +Title: string
        +Order: int
        +Description: string
        +Parent: Category
        +Version: Guid
    }

    class BookIsbn {
        +Value: string
        +Type: string
    }

    class Tag {
        +Name: string
    }

    Book "1" *-- "0..*" BookChapter : contains
    Book "1" *-- "1" BookIsbn : has
    Book "0..*" -- "0..*" Category : belongs to
    Book "1" *-- "0..*" Tag : tagged with
    Author "1" *-- "0..*" Tag : tagged with
    Category "1" *-- "0..*" Category : has subcategories
    Book "0..*" -- "0..*" Author : written by
```

### Book Aggregate

The Book aggregate is the central entity in the catalog module.

Components:

- Book (Aggregate Root): Represents a book in the catalog.
- BookChapter: Represents individual chapters within a book.
- BookIsbn: A value object representing the book's ISBN.
- Tag: A value object for categorizing books.

Relationships:

- A Book contains multiple BookChapters.
- Each Book has one BookIsbn.
- A Book can be tagged with multiple Tags.
- Books have a many-to-many relationship with Categories.
- Books have a many-to-many relationship with Authors, referenced by AuthorIds.

### Author Aggregate

The Author aggregate represents book authors in the module.

Components:

- Author (Aggregate Root): Represents an author.
- Tag: A value object for categorizing authors.

Relationships:

- An Author can be tagged with multiple Tags.
- Authors have a many-to-many relationship with Books, referenced by BookIds.

### Supporting Entities and Value Objects

1. Category:

- Represents book categories.
- Has a hierarchical structure (parent-child relationships).
- Has a many-to-many relationship with Books.

2. Tag:

- A value object used by both Book and Author for categorization.

### Key Relationships

1. Book-Author:

- Many-to-many relationship.
- Managed through typed IDs (BookId in Author, AuthorId in Book).
- Allows for books with multiple authors and authors with multiple books.

2. Book-Category:

- Many-to-many relationship.
- Allows for flexible categorization of books.

3. Category Hierarchy:

- Self-referential relationship in Category.
- Enables the creation of a category tree structure.

4. Tagging:

- Both Books and Authors can be tagged.
- Provides a flexible way to add metadata and improve searchability.

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory-README.md
// ----------------------------------------
![bITDevKit](https://github.com/BridgingIT-GmbH/bITdevKit.Examples.BookFiesta/blob/main/bITDevKit_Logo.png?raw=true)

![bITDevKit](https://raw.githubusercontent.com/BridgingIT-GmbH/bITdevKit.Examples.BookFiesta/refs/heads/main/bITDevKit_BookFiesta_Banner.png)
=====================================

<!-- TOC -->
* [Inventory Module Overview](#inventory-module-overview)
  * [Domain Model](#domain-model)
    * [Stock Aggregate](#stock-aggregate)
    * [StockSnapshot Aggregate](#stocksnapshot-aggregate)
    * [Supporting Entities and Value Objects](#supporting-entities-and-value-objects)
    * [Key Relationships](#key-relationships)
  * [Key Concepts and Operations](#key-concepts-and-operations)
  * [External API Endpoints](#external-api-endpoints)
  * [Public API Client](#public-api-client)
<!-- TOC -->

# Inventory Module Overview

> The Inventory Module is responsible for managing the book inventories, tracking stock levels, and
> handling stock-related operations.

## Domain Model

The domain model captures the essential entities and their relationships within the Inventory
module. It provides a foundation for managing stock levels, tracking stock movements, and creating
historical snapshots, while maintaining clear boundaries between aggregates.

The domain model consists of two main aggregates: Stock and StockSnapshot. These aggregates, along
with supporting entities and value objects, form the core of the domain model. A summary of each
aggregate and their relationships:

```mermaid
classDiagram
  class Stock {
    +StockId Id
    +TenantId TenantId
    +ProductSku Sku
    +int QuantityOnHand
    +int QuantityReserved
    +int ReorderThreshold
    +int ReorderQuantity
    +Money UnitCost
    +Location StorageLocation
    +DateTime LastRestockedAt
    +Version Version
    +AddStock(quantity)
    +RemoveStock(quantity)
    +ReserveStock(quantity)
    +ReleaseReservedStock(quantity)
    +UpdateReorderInfo(threshold, quantity)
    +MoveToLocation(newLocation)
  }

  class StockSnapshot {
    +StockSnapshotId Id
    +TenantId TenantId
    +ProductSku Sku
    +int QuantityOnHand
    +int QuantityReserved
    +DateTime Timestamp
    +Create(tenantId, sku, quantityOnHand, quantityReserved)
  }

  class StockMovement {
    +StockMovementId Id
    +StockId StockId
    +int Quantity
    +MovementType Type
    +string Reason
    +DateTime Timestamp
    +Create(stockId, quantity, type, reason)
  }

  class StockAdjustment {
    +StockAdjustmentId Id
    +StockId StockId
    +int QuantityChange
    +string Reason
    +DateTime Timestamp
    +Create(stockId, quantityChange, reason)
  }

  class Location {
    +string Aisle
    +string Shelf
    +string Bin
    +Create(aisle, shelf, bin)
    +ToString()
  }

  Stock "1" *-- "1" Location
  Stock "1" -- "0..*" StockMovement
  Stock "1" -- "0..*" StockAdjustment
```

### Stock Aggregate

The Stock aggregate is the central entity in the inventory module.

[Stock.cs](Inventory.Domain%2FModel%2FStockAggregate%2FStock.cs)

Components:

- Stock (Aggregate Root): Represents the current inventory state for a specific product.
- Location: A value object representing the storage location of the stock.
- StockMovement: An entity representing individual stock movements.
- StockAdjustment: An entity representing manual adjustments to stock levels.

Relationships:

- A Stock is identified by a unique ProductSku.
- Each Stock has one Location.
- A Stock can have multiple StockMovements.
- A Stock can have multiple StockAdjustments.

### StockSnapshot Aggregate

The StockSnapshot aggregate represents historical snapshots of stock levels.

[StockSnapshot.cs](Inventory.Domain%2FModel%2FStockSnapshotAggregate%2FStockSnapshot.cs)

Components:

- StockSnapshot (Aggregate Root): Represents a point-in-time record of stock levels.

Relationships:

- A StockSnapshot is associated with a specific ProductSku.
- StockSnapshots are created based on the current state of Stock entities but are managed
  independently.

### Supporting Entities and Value Objects

1. Location:

- Represents the physical storage location of stock within a warehouse or store.

1. StockMovement:

- Represents individual movements of stock (additions, removals, transfers).

1. StockAdjustment:

- Represents manual adjustments made to stock quantities.

### Key Relationships

1. Stock-ProductSku:

- One-to-one relationship.
- Each Stock is uniquely identified by a ProductSku.

2. Stock-Location:

- One-to-one relationship.
- Each Stock is associated with a specific storage Location.

3. Stock-StockMovement:

- One-to-many relationship.
- A Stock can have multiple StockMovements, tracking its history of quantity changes.

4. Stock-StockAdjustment:

- One-to-many relationship.
- A Stock can have multiple StockAdjustments, recording manual corrections to its quantity.

5. StockSnapshot-ProductSku:

- One-to-one relationship.
- Each StockSnapshot is associated with a specific ProductSku, allowing for historical tracking of
  stock levels for each product.

## Key Concepts and Operations

1. Stock Management:

- Adding and removing stock
- Reserving and releasing stock
- Updating reorder information
- Moving stock to different locations

2. Stock Movements:

- Tracking individual stock movements for auditing purposes
- Different types of movements (e.g., restocking, sales, transfers)

3. Stock Adjustments:

- Manual adjustments to stock levels
- Recording reasons for adjustments

4. Stock Snapshots:

- Creating point-in-time records of stock levels
- Historical tracking and analysis of stock levels

5. Inventory Queries:

- Checking current stock levels
- Reviewing stock movement history
- Analyzing historical stock levels through snapshots

## External API Endpoints

The following table describes the public HTTP API for this module:

- [InventoryStockEndpoints.cs](Inventory.Presentation%2FWeb%2FEndpoints%2FInventoryStockEndpoints.cs)
- [InventoryStockSnapshotEndpoints.cs](Inventory.Presentation%2FWeb%2FEndpoints%2FInventoryStockSnapshotEndpoints.cs)

| Endpoint                                                                 | HTTP Method | Description                                                   |
|--------------------------------------------------------------------------|-------------|---------------------------------------------------------------|
| `/api/tenants/{tenantId}/inventory/stocks/{id}`                          | GET         | Retrieves a specific stock by ID.                             |
| `/api/tenants/{tenantId}/inventory/stocks`                               | GET         | Retrieves a list of all stocks.                               |
| `/api/tenants/{tenantId}/inventory/stocks`                               | POST        | Creates a new stock.                                          |
| `/api/tenants/{tenantId}/inventory/stocks/{id}/adjust`                   | POST        | Creates a new stock movement for a specific stock.            |
| `/api/tenants/{tenantId}/inventory/stocks/{stockId}/stocksnapshots/{id}` | GET         | Retrieves a specific stock snapshot by ID.                    |
| `/api/tenants/{tenantId}/inventory/stocks/{stockId}/stocksnapshots`      | GET         | Retrieves a list of all stock snapshots for a specific stock. |
| `/api/tenants/{tenantId}/inventory/stocks/{stockId}/stocksnapshots`      | POST        | Creates a new stock snapshot for a specific stock.            |

## Public API Client

This module provides a public API client that other modules can use to communicate with it.
The client exposes various methods to interact with the inventory module.

- [IInventoryModuleClient.cs](Inventory.Application.Contracts%2FIInventoryModuleClient.cs)

| Method                                                                                              | Description                                        |
|-----------------------------------------------------------------------------------------------------|----------------------------------------------------|
| `Task<Result<StockModel>> StockFindOne(string tenantId, string id)`                                 | Retrieves the details of a specific stock by ID.   |
| `Task<Result<IEnumerable<StockModel>>> StockFindAll(string tenantId)`                               | Retrieves a list of all stocks for a tenant.       |
| `Task<Result<StockModel>> StockCreate(string tenantId, StockModel model)`                           | Creates a new stock entry.                         |
| `Task<Result<StockModel>> StockMovementApply(string tenantId, string id, StockMovementModel model)` | Creates a new stock movement for a specific stock. |

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization-README.md
// ----------------------------------------
![bITDevKit](https://github.com/BridgingIT-GmbH/bITdevKit.Examples.BookFiesta/blob/main/bITDevKit_Logo.png?raw=true)

![bITDevKit](https://raw.githubusercontent.com/BridgingIT-GmbH/bITdevKit.Examples.BookFiesta/refs/heads/main/bITDevKit_BookFiesta_Banner.png)
=====================================

<!-- TOC -->
* [Organization Module Overview](#organization-module-overview)
  * [Domain Model](#domain-model)
    * [Tenant Aggregate](#tenant-aggregate)
    * [Company Aggregate](#company-aggregate)
    * [Supporting Entities and Value Objects](#supporting-entities-and-value-objects)
    * [Key Relationships](#key-relationships)
<!-- TOC -->

# Organization Module Overview

> The Organization Module is responsible for managing tenants, companies, and their associated
> subscriptions and branding.

## Domain Model

The domain model provides a robust foundation for the module, capturing the essential entities and
their relationships while adhering to Domain-Driven Design principles. It allows for complex
operations such as managing tenants, companies, subscriptions, and branding, while maintaining clear
boundaries between aggregates.

The domain model consists of two main aggregates: Tenant and Company. These aggregates, along with
supporting entities and value objects, form the core of the domain model. A summary of each
aggregate and their relationships:

```mermaid
classDiagram
    class Tenant {
        <<Aggregate Root>>
        +TenantId Id
        +CompanyId CompanyId
        +string Name
        +string Description
        +bool Activated
        +EmailAddress ContactEmail
        +List~TenantSubscription~ Subscriptions
        +TenantBranding Branding
    }

    class Company {
        <<Aggregate Root>>
        +CompanyId Id
        +string Name
        +Address Address
        +string RegistrationNumber
        +EmailAddress ContactEmail
        +PhoneNumber ContactPhone
        +Url Website
        +VatNumber VatNumber
        +List~TenantId~ TenantIds
    }

    class TenantSubscription {
        <<Entity>>
        +TenantSubscriptionId Id
        +TenantSubscriptionPlanType PlanType
        +TenantSubscriptionStatus Status
        +Schedule Schedule
        +TenantSubscriptionBillingCycle BillingCycle
    }

    class TenantBranding {
        <<Entity>>
        +TenantBrandingId Id
        +HexColor PrimaryColor
        +HexColor SecondaryColor
        +Url LogoUrl
        +Url FaviconUrl
        +string CustomCss
    }

    Tenant "1" *-- "0..*" TenantSubscription : contains
    Tenant "1" *-- "1" TenantBranding : has
    Tenant "0..*" -- "1" Company : belongs to
```

### Tenant Aggregate

The Tenant aggregate is the central entity in the organization module.

Components:

- Tenant (Aggregate Root): Represents a client organization or individual using the shop platform.
- TenantSubscription (Entity): Represents the commercial agreements for the tenant.
- TenantBranding (Entity): Represents the branding information for the tenant.

Relationships:

- A Tenant belongs to one Company.
- A Tenant can have multiple TenantSubscriptions.
- A Tenant has one TenantBranding.

### Company Aggregate

The Company aggregate represents the parent organization of tenants.

Components:

- Company (Aggregate Root): Represents a company that can have multiple tenants.

Relationships:

- A Company can have multiple Tenants (referenced by TenantIds).

### Supporting Entities and Value Objects

1. TenantSubscription (Entity):

- Represents a subscription plan for a tenant.
- Contains information about the plan type, status, schedule, and billing cycle.

2. TenantBranding (Entity):

- Represents the branding information for a tenant.
- Contains colors, logo URLs, and custom CSS.

3. Value Objects:

- TenantSubscriptionPlanType: Enumeration of subscription plan types (Free, Basic, Premium).
- TenantSubscriptionStatus: Enumeration of subscription statuses (Pending, Approved, Cancelled,
  Ended).
- TenantSubscriptionBillingCycle: Enumeration of billing cycles (Never, Monthly, Yearly).
- EmailAddress, PhoneNumber, Url, VatNumber: Represent specific data types with their own
  validation rules.

### Key Relationships

1. Tenant-Company:

- Many-to-one relationship.
- A Tenant belongs to one Company, but a Company can have multiple Tenants.

2. Tenant-TenantSubscription:

- One-to-many relationship within the Tenant aggregate.
- A Tenant can have multiple TenantSubscriptions.

3. Tenant-TenantBranding:

- One-to-one relationship within the Tenant aggregate.
- Each Tenant has its own TenantBranding.

This domain model provides a flexible structure for managing organizations, tenants, and their
associated data in the system. It allows for complex operations while maintaining clear boundaries
between aggregates and ensuring data integrity through the use of specific value objects and
entities.

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Catalog\Catalog.Infrastructure\EntityFramework\README.md
// ----------------------------------------
# BookFiesta - Catalog Module

## Create and apply a new Database Migration

These database commands should be executed from the solution root folder.

### new migration:

-

`dotnet ef migrations add Initial --context CatalogDbContext --output-dir .\EntityFramework\Migrations --project .\src\Modules\Catalog\Catalog.Infrastructure\Catalog.Infrastructure.csproj --startup-project .\src\Presentation.Web.Server\Presentation.Web.Server.csproj`

### update database:

-

`dotnet ef database update --context CatalogDbContext --project .\src\Modules\Catalog\Catalog.Infrastructure\Catalog.Infrastructure.csproj --startup-project .\src\Presentation.Web.Server\Presentation.Web.Server.csproj`

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Inventory\Inventory.Infrastructure\EntityFramework\README.md
// ----------------------------------------
# BookFiesta - Inventory Module

## Create and apply a new Database Migration

These database commands should be executed from the solution root folder.

### new migration:

-

`dotnet ef migrations add Initial --context InventoryDbContext --output-dir .\EntityFramework\Migrations --project .\src\Modules\Inventory\Inventory.Infrastructure\Inventory.Infrastructure.csproj --startup-project .\src\Presentation.Web.Server\Presentation.Web.Server.csproj`

### update database:

-

`dotnet ef database update --context InventoryDbContext --project .\src\Modules\Inventory\Inventory.Infrastructure\Inventory.Infrastructure.csproj --startup-project .\src\Presentation.Web.Server\Presentation.Web.Server.csproj`

// File: F:\projects\bit\bITdevKit.Examples.BookFiesta\src\Modules\Organization\Organization.Infrastructure\EntityFramework\README.md
// ----------------------------------------
# BookFiesta - Organization Module

## Create and apply a new Database Migration

These database commands should be executed from the solution root folder.

### new migration:

-

`dotnet ef migrations add Initial --context OrganizationDbContext --output-dir .\EntityFramework\Migrations --project .\src\Modules\Organization\Organization.Infrastructure\Organization.Infrastructure.csproj --startup-project .\src\Presentation.Web.Server\Presentation.Web.Server.csproj`

### update database:

-

`dotnet ef database update --context OrganizationDbContext --project .\src\Modules\Organization\Organization.Infrastructure\Organization.Infrastructure.csproj --startup-project .\src\Presentation.Web.Server\Presentation.Web.Server.csproj`

