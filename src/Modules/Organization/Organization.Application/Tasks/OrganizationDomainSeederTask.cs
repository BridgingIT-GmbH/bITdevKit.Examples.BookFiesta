// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Organization.Application;

using System.Threading;
using System.Threading.Tasks;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookFiesta.Organization.Domain;

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
        var companies = OrganizationSeedModels.Companies.Create();

        foreach (var company in companies)
        {
            if (!await repository.ExistsAsync(company.Id))
            {
                await repository.InsertAsync(company);
            }
        }

        return companies;
    }

    private async Task<Tenant[]> SeedTenants(IGenericRepository<Tenant> repository, Company[] companies)
    {
        var tenants = OrganizationSeedModels.Tenants.Create(companies);

        foreach (var tenant in tenants)
        {
            if (!await repository.ExistsAsync(tenant.Id))
            {
                await repository.InsertAsync(tenant);
            }
        }

        return tenants;
    }
}