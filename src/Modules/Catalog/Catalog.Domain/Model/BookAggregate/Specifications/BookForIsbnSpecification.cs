namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

using BridgingIT.DevKit.Domain.Specifications;
using System.Linq.Expressions;

public class BookForIsbnSpecification(BookIsbn isbn) : Specification<Book>
{
    public override Expression<Func<Book, bool>> ToExpression()
    {
        return e => e.Isbn.Value == isbn.Value;
    }
}

public static partial class BookSpecifications
{
    public static Specification<Book> ForIsbn(BookIsbn isbn)
    {
        return new BookForIsbnSpecification(isbn);
    }

    public static Specification<Book> ForIsbn2(BookIsbn isbn) // INFO: short version to define a specification
    {
        return new Specification<Book>(e => e.Isbn.Value == isbn.Value);
    }
}