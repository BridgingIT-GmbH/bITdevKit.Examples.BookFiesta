// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Application;

using System.Collections.Generic;
using System.Threading.Tasks;
using BridgingIT.DevKit.Application.Queries;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Domain.Specifications;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;
using Microsoft.Extensions.Logging;
using Polly;

public class BookFindAllQueryHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Book> repository)
        : QueryHandlerBase<BookFindAllQuery, Result<IEnumerable<Book>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Book>>>> Process(
        BookFindAllQuery query, CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await repository.FindAllResultAsync(
                new FindOptions<Book>() { Order = new OrderOption<Book>(e => e.Title) }, cancellationToken: cancellationToken).AnyContext());
    }
}

public class BookFindAllRelatedQueryHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Book> repository)
        : QueryHandlerBase<BookFindAllRelatedQuery, Result<IEnumerable<Book>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Book>>>> Process(
        BookFindAllRelatedQuery query, CancellationToken cancellationToken)
    {
        var bookId = BookId.Create(query.BookId);
        var result = await repository.FindOneResultAsync(
            bookId, cancellationToken: cancellationToken).AnyContext();

        throw new NotImplementedException();
        //var targetBookKeywords = result.Value.Keywords.SafeNull();

        //if (!result.IsSuccess)
        //{
        //    var keywords = await repository.ProjectAllAsync(
        //        new Specification<Book>(e => e.Id != bookId), e => e.Keywords, cancellationToken: cancellationToken).AnyContext();

        //    var relatedBookIds = await _context.KeywordIndices
        //        .Where(ki => targetBookKeywords.Contains(ki.Keyword) && ki.BookId != targetBookId)
        //        .GroupBy(ki => ki.BookId)
        //        .OrderByDescending(g => g.Count())
        //        .Take(maxRecommendations)
        //        .Select(g => g.Key)
        //        .ToListAsync();
        //}

        //return QueryResponse.For(
        //    await repository.FindAllResultAsync(
        //        new FindOptions<Book>() { Order = new OrderOption<Book>(e => e.Title) }, cancellationToken: cancellationToken).AnyContext());
    }
}