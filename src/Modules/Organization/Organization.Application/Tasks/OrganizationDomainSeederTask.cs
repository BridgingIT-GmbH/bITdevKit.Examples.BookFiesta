// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

using System.Threading;
using System.Threading.Tasks;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

public class OrganizationDomainSeederTask(
    IGenericRepository<Company> companyRepository,
    IGenericRepository<Tenant> tenantRepository) : IStartupTask
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var companies = await this.SeedCompanies(companyRepository);
        var tenants = await this.SeedTenants(tenantRepository, companies);
    }

    private async Task<Company[]> SeedCompanies(IGenericRepository<Company> repository)
    {
        var entities = OrganizationSeedEntities.Companies.Create();

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

    private async Task<Tenant[]> SeedTenants(IGenericRepository<Tenant> repository, Company[] companies)
    {
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