// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Shared.IntegrationTests.Infrastructure;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Infrastructure;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Infrastructure;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Infrastructure;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using BridgingIT.DevKit.Infrastructure.EntityFramework.Repositories;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.MsSql;

public class TestEnvironmentFixture : IAsyncLifetime
{
    private IServiceProvider serviceProvider;

    public TestEnvironmentFixture()
    {
        this.Services.AddLogging(c => c.AddProvider(new XunitLoggerProvider(this.Output)));

        this.Network = new NetworkBuilder().WithName(this.NetworkName).Build();

        this.SqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
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

    public string SqlConnectionString => this.SqlContainer.GetConnectionString();

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

    public OrganizationDbContext OrganizationDbContext { get; private set; }

    public CatalogDbContext CatalogDbContext { get; private set; }

    public InventoryDbContext InventoryDbContext { get; private set; }

    private static bool IsCiEnvironment =>
        Environment.GetEnvironmentVariable("AGENT_NAME") is not null; // check if running on Microsoft's CI environment

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

        this.OrganizationDbContext = this.CreateOrganizationDbContext();
        this.CatalogDbContext = this.CreateCatalogDbContext();
        this.InventoryDbContext = this.CreateInventoryDbContext();

        // ensure migrations are applied
        await this.OrganizationDbContext.Database.MigrateAsync();
        await this.CatalogDbContext.Database.MigrateAsync();
        await this.InventoryDbContext.Database.MigrateAsync();

        // seed entities for all modules
        await new OrganizationDomainSeederTask(
            new NullLoggerFactory(),
            new EntityFrameworkGenericRepository<Company>(o => o.DbContext(this.OrganizationDbContext)),
            new EntityFrameworkGenericRepository<Tenant>(o => o.DbContext(this.OrganizationDbContext))
        ).ExecuteAsync(CancellationToken.None);

        await new CatalogDomainSeederTask(
            new NullLoggerFactory(),
            new EntityFrameworkGenericRepository<Tag>(o => o.DbContext(this.CatalogDbContext)),
            new EntityFrameworkGenericRepository<Customer>(o => o.DbContext(this.CatalogDbContext)),
            new EntityFrameworkGenericRepository<Author>(o => o.DbContext(this.CatalogDbContext)),
            new EntityFrameworkGenericRepository<Publisher>(o => o.DbContext(this.CatalogDbContext)),
            new EntityFrameworkGenericRepository<Category>(o => o.DbContext(this.CatalogDbContext)),
            new EntityFrameworkGenericRepository<Book>(o => o.DbContext(this.CatalogDbContext))
        ).ExecuteAsync(CancellationToken.None);

        await new InventoryDomainSeederTask(
            new NullLoggerFactory(),
            new EntityFrameworkGenericRepository<Stock>(o => o.DbContext(this.InventoryDbContext)),
            new EntityFrameworkGenericRepository<StockSnapshot>(o => o.DbContext(this.InventoryDbContext))
        ).ExecuteAsync(CancellationToken.None);
    }

    public async Task DisposeAsync()
    {
        this.InventoryDbContext?.Dispose();

        await this.SqlContainer.DisposeAsync().AnyContext();

        //await this.RabbitMQContainer.DisposeAsync().AnyContext();

        await this.Network.DeleteAsync().AnyContext();
    }

    public OrganizationDbContext CreateOrganizationDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrganizationDbContext>();

        if (this.Output is not null)
        {
            optionsBuilder = new DbContextOptionsBuilder<OrganizationDbContext>().LogTo(this.Output.WriteLine);
        }

        optionsBuilder.UseSqlServer(this.SqlConnectionString);

        return new OrganizationDbContext(optionsBuilder.Options);
    }

    public CatalogDbContext CreateCatalogDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();

        if (this.Output is not null)
        {
            optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>().LogTo(this.Output.WriteLine);
        }

        optionsBuilder.UseSqlServer(this.SqlConnectionString);

        return new CatalogDbContext(optionsBuilder.Options);
    }

    public InventoryDbContext CreateInventoryDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();

        if (this.Output is not null)
        {
            optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>().LogTo(this.Output.WriteLine);
        }

        optionsBuilder.UseSqlServer(this.SqlConnectionString);

        return new InventoryDbContext(optionsBuilder.Options);
    }
}