namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Infrastructure;

using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
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