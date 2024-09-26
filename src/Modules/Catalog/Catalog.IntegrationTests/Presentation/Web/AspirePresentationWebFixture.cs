// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Catalog.IntegrationTests.Presentation;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

public class AspirePresentationWebFixture
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly IHost app;
    private string sqlServerConnectionString;

    public AspirePresentationWebFixture()
    {
        var options = new DistributedApplicationOptions
        {
            AssemblyName = typeof(AspirePresentationWebFixture).Assembly.FullName,
            DisableDashboard = true
        };

        var appBuilder = DistributedApplication.CreateBuilder(options);

        // setup the aspire resources
        this.Sql = appBuilder.AddSqlServer("sql", port: 14320)
            .WithImageTag("latest")
            //.WithDataVolume() // requires persistent password https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/persist-data-volumes
            //.WithHealthCheck()
            .AddDatabase("sqldata");

        this.app = appBuilder.Build();
    }

    public IResourceBuilder<SqlServerDatabaseResource> Sql { get; }

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

    public async Task InitializeAsync()
    {
        await this.app.StartAsync();

        this.sqlServerConnectionString = await this.Sql.Resource.Parent.GetConnectionStringAsync() + ";Database=sqldata;";
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                { $"ConnectionStrings:sqldata", this.sqlServerConnectionString },
            });
        });

        return base.CreateHost(builder);
    }
}