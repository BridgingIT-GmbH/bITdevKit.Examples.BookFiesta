// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

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
                entity.AuditState.SetCreated();
                entity.AuditState.SetCreated("seed", nameof(OrganizationDomainSeederTask));
                await repository.InsertAsync(entity);
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
        }

        return entities;
    }
}