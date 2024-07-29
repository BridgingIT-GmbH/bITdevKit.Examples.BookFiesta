// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Organization.Domain;

public class TenantCompanyReassignedDomainEvent(Tenant tenant, Company company) : DomainEventBase
{
    public TenantId TenantId { get; } = tenant.Id;

    public CompanyId CompanyId { get; } = company.Id;
}