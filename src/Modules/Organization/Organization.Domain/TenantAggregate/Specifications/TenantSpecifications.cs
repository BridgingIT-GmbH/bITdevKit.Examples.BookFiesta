// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

using System.Linq.Expressions;
using DevKit.Domain.Specifications;

public class TenantForNameSpecification(string name) : Specification<Tenant>
{
    public override Expression<Func<Tenant, bool>> ToExpression()
    {
        return e => e.Name == name;
    }
}

public class TenantForCompanySpecification(CompanyId companyId) : Specification<Tenant>
{
    public override Expression<Func<Tenant, bool>> ToExpression()
    {
        return e => e.CompanyId == companyId;
    }
}

public static class TenantSpecifications
{
    public static Specification<Tenant> ForName(string name)
    {
        return new TenantForNameSpecification(name);
    }

    public static Specification<Tenant> ForName2(string name) // INFO: short version to define a specification
    {
        return new Specification<Tenant>(e => e.Name == name);
    }

    public static Specification<Tenant> ForCompany(CompanyId companyId)
    {
        return new TenantForCompanySpecification(companyId);
    }

    public static Specification<Tenant>
        ForCompany2(CompanyId companyId) // INFO: short version to define a specification
    {
        return new Specification<Tenant>(e => e.CompanyId == companyId);
    }
}