// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.IntegrationTests.Infrastructure;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;
using Microsoft.EntityFrameworkCore;
using Organization.Infrastructure;
using Testcontainers.MsSql;

public class TestEnvironmentFixture : IAsyncLifetime
{
    private IServiceProvider serviceProvider;

    public TestEnvironmentFixture()
    {
        this.Services.AddLogging(c => c.AddProvider(new XunitLoggerProvider(this.Output)));

        this.Network = new NetworkBuilder()
            .WithName(this.NetworkName)
            .Build();

        this.SqlContainer = new MsSqlBuilder()
            .WithNetworkAliases(this.NetworkName)
            .WithExposedPort(1433)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
            .Build();

        //this.RabbitMQContainer = new RabbitMqBuilder()
        //    .WithNetworkAliases(this.NetworkName)
        //    .Build();
    }

    public IServiceCollection Services { get; set; } = new ServiceCollection();

    public ITestOutputHelper Output { get; private set; }

    public string NetworkName
        => HashHelper.Compute(DateTime.UtcNow.Ticks);

    public string SqlConnectionString
        => this.SqlContainer.GetConnectionString();

    // public string RabbitMQConnectionString => this.RabbitMQContainer.GetConnectionString();

    public INetwork Network { get; }

    public MsSqlContainer SqlContainer { get; }

    // public RabbitMqContainer RabbitMQContainer { get; }

    public IServiceProvider ServiceProvider
    {
        get
        {
            this.serviceProvider ??= this.Services.BuildServiceProvider();
            return this.serviceProvider;
        }
    }

    public OrganizationDbContext StubContext { get; private set; }

    private static bool IsCiEnvironment
        => Environment.GetEnvironmentVariable(
            "AGENT_NAME") is not null; // check if running on Microsoft's CI environment

    public TestEnvironmentFixture WithOutput(ITestOutputHelper output)
    {
        this.Output = output;
        return this;
    }

    public async Task InitializeAsync()
    {
        await this.Network.CreateAsync().AnyContext();

        await this.SqlContainer.StartAsync().AnyContext();

        //await this.RabbitMQContainer.StartAsync().AnyContext();

        this.StubContext = this.CreateSqlServerDbContext();
        await this.StubContext.Database.MigrateAsync(); // ensure migrations are applied
    }

    public async Task DisposeAsync()
    {
        this.StubContext?.Dispose();

        await this.SqlContainer.DisposeAsync().AnyContext();

        //await this.RabbitMQContainer.DisposeAsync().AnyContext();

        await this.Network.DeleteAsync().AnyContext();
    }

    public OrganizationDbContext CreateSqlServerDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrganizationDbContext>();

        if (this.Output is not null)
        {
            optionsBuilder = new DbContextOptionsBuilder<OrganizationDbContext>()
                .LogTo(this.Output.WriteLine);
        }

        optionsBuilder.UseSqlServer(this.SqlConnectionString);

        return new OrganizationDbContext(optionsBuilder.Options);
    }
}