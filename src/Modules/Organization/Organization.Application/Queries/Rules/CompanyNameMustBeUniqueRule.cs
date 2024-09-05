// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Organization.Application;

using System.Threading.Tasks;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookFiesta.Organization.Domain;

public class CompanyNameMustBeUniqueRule(
    IGenericRepository<Company> repository,
    Company company) : DomainRuleBase
{
    public override string Message => "Company name must be unique";

    public override async Task<bool> ApplyAsync(CancellationToken cancellationToken = default)
    {
        return !(await repository.FindAllAsync(
            CompanySpecifications.ForName(company.Name), cancellationToken: cancellationToken))
            .SafeAny(c => c.Id != company.Id);
    }
}

public class CompanyMustHaveNoTenantsRule(
    IGenericRepository<Tenant> repository,
    Company company) : DomainRuleBase
{
    public override string Message => "Company must have no tenants assigned";

    public override async Task<bool> ApplyAsync(CancellationToken cancellationToken = default)
    {
        return !(await repository.FindAllAsync(
            TenantSpecifications.ForCompany(company.Id), cancellationToken: cancellationToken))
            .SafeAny();
    }
}

public static partial class CompanyRules
{
    public static IDomainRule NameMustBeUnique(
        IGenericRepository<Company> repository,
        Company company) => new CompanyNameMustBeUniqueRule(repository, company);

    public static IDomainRule MustHaveNoTenants(
        IGenericRepository<Tenant> repository,
        Company company) => new CompanyMustHaveNoTenantsRule(repository, company);
}