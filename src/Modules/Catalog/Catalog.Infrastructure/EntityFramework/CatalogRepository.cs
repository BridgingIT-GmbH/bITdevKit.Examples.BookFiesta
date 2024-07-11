namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Infrastructure;

using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;
using BridgingIT.DevKit.Infrastructure.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class BookRepository : EntityFrameworkReadOnlyGenericRepository<Book>
{
    public BookRepository(EntityFrameworkRepositoryOptions options)
        : base(options)
    {
    }

    public BookRepository(Builder<EntityFrameworkRepositoryOptionsBuilder, EntityFrameworkRepositoryOptions> optionsBuilder)
        : base(optionsBuilder)
    {
    }

    public BookRepository(ILoggerFactory loggerFactory, DbContext context)
        : base(loggerFactory, context)
    {
    }
}