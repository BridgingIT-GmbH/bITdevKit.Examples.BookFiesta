// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using System.Threading.Tasks;
using BridgingIT.DevKit.Domain.Repositories;

public class TagMustBelongToTenantRule(
    Tag tag,
    TenantId tenantId) : DomainRuleBase
{
    private readonly Tag tag = tag;
    private readonly TenantId tenantId = tenantId;

    public override string Message => $"Tag should belong to tenant {this.tenantId}";

    public override Task<bool> ApplyAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(this.tag.TenantId == this.tenantId);
    }
}

public static partial class TagRules
{
    public static IDomainRule TagMustBelongToTenant(
        Tag tag,
        TenantId tenantId) => new TagMustBelongToTenantRule(tag, tenantId);
}