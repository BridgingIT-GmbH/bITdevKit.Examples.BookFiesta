﻿namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

public class CustomerForEmailSpecification(TenantId tenantId, EmailAddress email) : Specification<Customer>
{
    public override Expression<Func<Customer, bool>> ToExpression()
    {
        return e => e.TenantId == tenantId && e.Email == email;
    }
}

public static class CustomerSpecifications
{
    public static Specification<Customer> ForEmail(TenantId tenantId, EmailAddress email)
    {
        return new CustomerForEmailSpecification(tenantId, email);
    }

    public static Specification<Customer>
        ForEmail2(TenantId tenantId, EmailAddress email) // INFO: short version to define a specification
    {
        return new Specification<Customer>(e => e.TenantId == tenantId && e.Email == email);
    }
}