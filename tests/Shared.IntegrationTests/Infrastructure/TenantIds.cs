// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Shared.IntegrationTests.Infrastructure;

using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

public static class TenantHelper
{
    public static readonly TenantId[] Ids =
    [
        TenantIdFactory.CreateForName("Tenant_AcmeBooks"),
        TenantIdFactory.CreateForName("Tenant_TechBooks")
    ];
}