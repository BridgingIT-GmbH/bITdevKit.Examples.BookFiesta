// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Catalog.IntegrationTests.Presentation;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

public class PresentationWebAspireFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly IHost app;
    private string sqlServerConnectionString;

    public PresentationWebAspireFixture()
    {
        var options = new DistributedApplicationOptions { AssemblyName = typeof(PresentationWebAspireFixture).Assembly.FullName, DisableDashboard = true };
        var appBuilder = DistributedApplication.CreateBuilder(options);
        this.SqlServer = appBuilder.AddSqlServer("sql")
            .WithDataVolume().WithImageTag("latest");
        this.SqlServer.AddDatabase("sqldata");

        this.app = appBuilder.Build();
    }

    public IResourceBuilder<SqlServerServerResource> SqlServer { get; }

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

        this.sqlServerConnectionString = await this.SqlServer.Resource.GetConnectionStringAsync();
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