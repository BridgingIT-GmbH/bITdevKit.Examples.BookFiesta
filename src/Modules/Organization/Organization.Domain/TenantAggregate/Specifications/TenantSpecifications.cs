// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Organization.Domain;

using BridgingIT.DevKit.Domain.Specifications;
using System.Linq.Expressions;

public class TenantForNameSpecification(string name) : Specification<Tenant>
{
    public override Expression<Func<Tenant, bool>> ToExpression()
    {
        return e => e.Name == name;
    }
}

public static partial class TenantSpecifications
{
    public static Specification<Tenant> ForName(string name)
        => new TenantForNameSpecification(name);
}