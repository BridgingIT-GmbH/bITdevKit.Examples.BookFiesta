// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Common;

using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Xunit.Abstractions;

/// <summary>
/// AspireApphostFactory is a specialized WebApplicationFactory for
/// creating integration testing environments with customized configurations.
/// </summary>
/// <typeparam name="TEntryPoint">The entry point of the web application.</typeparam>
public class AspireApphostFactory<TEntryPoint> // https://xunit.net/docs/shared-context#class-fixture
    : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    private IHost app;
    private string environment = "Development";
    private bool fakeAuthenticationEnabled;
    private Action<IServiceCollection> services;

    public AspireApphostFactory()
    {
        // var options = new DistributedApplicationOptions
        // {
        //     //AssemblyName = typeof(ApiServiceFixture).Assembly.FullName,
        //     DisableDashboard = true
        // };
        //
        // var appBuilder = DistributedApplication.CreateBuilder(options);
        // this.app = appBuilder.Build();
    }

    public async Task InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<TEntryPoint>();
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });
        this.app = await appHost.BuildAsync();

        var resourceNotificationService = this.app.Services.GetRequiredService<ResourceNotificationService>();
        await resourceNotificationService.WaitForResourceAsync("webfrontend", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));

        await this.app.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await this.app.StopAsync();

        if (this.app is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        else
        {
            this.app.Dispose();
        }
    }

    /// <summary>
    /// Gets the <see cref="ITestOutputHelper"/> instance for output during integration tests.
    /// </summary>
    /// <value>
    /// The <see cref="ITestOutputHelper"/> used for capturing test output logs.
    /// </value>
    public ITestOutputHelper Output { get; private set; }

    /// <summary>
    /// Provides access to the service provider instance for resolving dependencies during integration testing.
    /// </summary>
    /// <value>
    /// An <see cref="IServiceProvider"/> instance used to resolve services within the testing scope.
    /// </value>
    public IServiceProvider ServiceProvider => this.Services.CreateScope().ServiceProvider;

    /// <summary>
    /// Sets the output helper for logging and diagnostics during integration testing.
    /// </summary>
    /// <param name="output">The ITestOutputHelper instance used for output during tests.</param>
    /// <returns>Returns the current instance of AspireApphostFactory for method chaining.</returns>
    public AspireApphostFactory<TEntryPoint> WithOutput(ITestOutputHelper output)
    {
        this.Output = output;

        return this;
    }

    /// <summary>
    /// Sets the environment for the WebApplicationFactoryFixture instance.
    /// </summary>
    /// <param name="environment">The environment name to set (e.g., "Development", "Staging", "Production").</param>
    /// <returns>Returns the current instance of AspireApphostFactory for method chaining.</returns>
    public AspireApphostFactory<TEntryPoint> WithEnvironment(string environment)
    {
        this.environment = environment;

        return this;
    }

    /// <summary>
    /// Enables or disables fake authentication for integration tests.
    /// </summary>
    /// <param name="enabled">A boolean value indicating whether fake authentication should be enabled.</param>
    /// <returns>The updated instance of AspireApphostFactory with the specified fake authentication setting.</returns>
    public AspireApphostFactory<TEntryPoint> WithFakeAuthentication(bool enabled)
    {
        this.fakeAuthenticationEnabled = enabled;

        return this;
    }

    /// <summary>
    /// Configures the services for the AspireApphostFactory.
    /// </summary>
    /// <param name="action">An action to configure the service collection.</param>
    /// <returns>The current instance of AspireApphostFactory for chaining.</returns>
    public AspireApphostFactory<TEntryPoint> WithServices(Action<IServiceCollection> action)
    {
        this.services = action;

        return this;
    }

    // /// <summary>
    // /// Creates and configures an instance of the <see cref="IHost"/> for integration testing.
    // /// </summary>
    // /// <param name="builder">The <see cref="IHostBuilder"/> to configure the host.</param>
    // /// <returns>An instance of <see cref="IHost"/> configured for integration testing.</returns>
    // protected override IHost CreateHost(IHostBuilder builder)
    // {
    //
    //     builder.UseEnvironment(this.environment);
    //     builder.ConfigureAppConfiguration((ctx, cnf) =>
    //     {
    //         cnf.SetBasePath(Directory.GetCurrentDirectory())
    //             .AddJsonFile("appsettings.json", false, true)
    //             .AddEnvironmentVariables();
    //     });
    //     builder.ConfigureLogging(ctx => ctx // TODO: webapp logs are not visible in test log anymore (serilog?)
    //         .Services.AddSingleton<ILoggerProvider>(sp => new XunitLoggerProvider(this.Output)));
    //
    //     builder.UseSerilog(); // comes before Program.cs > ConfigureLogging
    //     var loggerConfiguration = new LoggerConfiguration().MinimumLevel.Information();
    //     if (this.Output is not null)
    //     {
    //         loggerConfiguration.WriteTo.TestOutput(this.Output, LogEventLevel.Information);
    //     }
    //
    //     Log.Logger = loggerConfiguration.CreateLogger().ForContext<AspireApphostFactory<TEntryPoint>>();
    //
    //     builder.ConfigureServices(services =>
    //     {
    //         this.services?.Invoke(services);
    //
    //         if (this.fakeAuthenticationEnabled)
    //         {
    //             services.AddAuthentication(options => // add a fake authentication handler
    //                 {
    //                     options.DefaultAuthenticateScheme =
    //                         FakeAuthenticationHandler
    //                             .SchemeName; // use the fake handler instead of the jwt handler (Startup)
    //                     options.DefaultScheme = FakeAuthenticationHandler.SchemeName;
    //                 })
    //                 .AddScheme<AuthenticationSchemeOptions, FakeAuthenticationHandler>(
    //                     FakeAuthenticationHandler.SchemeName,
    //                     null);
    //         }
    //     });
    //
    //     return base.CreateHost(builder);
    // }
}