// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using System.Threading.Tasks;
using BridgingIT.DevKit.Domain.Repositories;

public class TagMustBelongToTenantRule(Tag tag, TenantId tenantId) : DomainRuleBase
{
    public override string Message => $"Tag should belong to tenant {tenantId}";

    public override Task<bool> ApplyAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(tag.TenantId == tenantId);
    }
}

public static partial class TagRules
{
    public static IDomainRule TagMustBelongToTenant(Tag tag, TenantId tenantId)
    {
        return new TagMustBelongToTenantRule(tag, tenantId);
    }
}