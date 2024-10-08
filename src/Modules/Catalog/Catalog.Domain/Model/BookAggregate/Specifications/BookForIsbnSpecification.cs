namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

using System.Linq.Expressions;
using BridgingIT.DevKit.Domain.Specifications;

public class BookForIsbnSpecification(TenantId tenantId, BookIsbn isbn) : Specification<Book>
{
    public override Expression<Func<Book, bool>> ToExpression()
    {
        return e => e.TenantId == tenantId && e.Isbn == isbn;
    }
}

public static class BookSpecifications
{
    public static Specification<Book> ForIsbn(TenantId tenantId, BookIsbn isbn)
    {
        return new BookForIsbnSpecification(tenantId, isbn);
    }

    public static Specification<Book> ForIsbn2(TenantId tenantId, BookIsbn isbn) // INFO: short version to define a specification
    {
        return new Specification<Book>(e => e.TenantId == tenantId && e.Isbn == isbn);
    }
}