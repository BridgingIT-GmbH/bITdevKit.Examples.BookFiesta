namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

using BridgingIT.DevKit.Domain.Specifications;
using System.Linq.Expressions;

public class CustomerForEmailSpecification(EmailAddress email) : Specification<Customer>
{
    public override Expression<Func<Customer, bool>> ToExpression()
    {
        return e => e.Email.Value == email.Value;
    }
}

public static partial class CustomerSpecifications
{
    public static Specification<Customer> ForEmail(EmailAddress email)
        => new CustomerForEmailSpecification(email);

    public static Specification<Customer> ForEmail2(EmailAddress email) // INFO: short version to define a specification
        => new(e => e.Email.Value == email.Value);
}