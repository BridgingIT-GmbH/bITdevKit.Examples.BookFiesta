namespace BridgingIT.DevKit.Examples.BookFiesta.Catalog.Domain;

using BridgingIT.DevKit.Domain.Specifications;
using System.Linq.Expressions;

public class BookForIsbnSpecification(BookIsbn isbn)
    : Specification<Book>
{
    public override Expression<Func<Book, bool>> ToExpression()
    {
        return e => e.Isbn.Value == isbn.Value;
    }
}

public static partial class BookSpecifications
{
    public static Specification<Book> ForIsbn(BookIsbn isbn)
        => new BookForIsbnSpecification(isbn);

    public static Specification<Book> ForIsbn2(BookIsbn isbn) // INFO: short version to define a specification
        => new(e => e.Isbn.Value == isbn.Value);
}