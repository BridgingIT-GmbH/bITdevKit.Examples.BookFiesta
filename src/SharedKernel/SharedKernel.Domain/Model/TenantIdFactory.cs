// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using BridgingIT.DevKit.Common;

public static class TenantIdFactory
{
    public static TenantId CreateForName(string name)
    {
        return TenantId.Create(GuidGenerator.Create($"Tenant_{name}"));
    }
}