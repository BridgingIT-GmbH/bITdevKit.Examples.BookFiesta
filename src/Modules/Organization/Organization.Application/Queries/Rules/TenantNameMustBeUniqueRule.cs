// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class TenantNameMustBeUniqueRule(
    IGenericRepository<Tenant> repository,
    Tenant tenant)
    : DomainRuleBase
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