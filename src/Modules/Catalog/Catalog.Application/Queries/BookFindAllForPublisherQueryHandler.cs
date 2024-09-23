// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using DevKit.Domain.Specifications;

public class BookFindAllForPublisherQueryHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Book> repository)
    : QueryHandlerBase<BookFindAllForPublisherQuery, Result<IEnumerable<Book>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Book>>>> Process(
        BookFindAllForPublisherQuery query,
        CancellationToken cancellationToken)
    {
        var publisherId = PublisherId.Create(query.PublisherId);

        var result = await repository.FindAllResultAsync(
                    new Specification<Book>(e => e.Publisher.PublisherId == publisherId),
                    cancellationToken: cancellationToken)
                .AnyContext() ??
            throw new EntityNotFoundException();

        return QueryResponse.For(result);
    }
}