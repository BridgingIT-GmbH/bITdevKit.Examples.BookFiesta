// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Organization.Domain;

using BridgingIT.DevKit.Domain.Specifications;
using System.Linq.Expressions;

public class CompanyForNameSpecification(string name)
    : Specification<Company>
{
    public override Expression<Func<Company, bool>> ToExpression()
    {
        return e => e.Name == name;
    }
}

//public class CompanyForTenantSpecification(TenantId tenantId)
//    : Specification<Company>
//{
//    public override Expression<Func<Company, bool>> ToExpression()
//    {
//        return e => e.TenantIds.Contains(tenantId);
//    }
//}

public static partial class CompanySpecifications
{
    public static Specification<Company> ForName(string name)
        => new CompanyForNameSpecification(name);

    public static Specification<Company> ForName2(string name) // INFO: short version to define a specification
        => new(e => e.Name == name);

    //public static Specification<Company> ForTenant(TenantId tenantId)
    //    => new CompanyForTenantSpecification(tenantId);

    //public static Specification<Company> ForTenant2(TenantId tenantId) // INFO: short version to define a specification
    //    => new(e => e.TenantIds.Contains(tenantId));
}