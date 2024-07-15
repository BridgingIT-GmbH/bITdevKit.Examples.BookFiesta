// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Infrastructure;

using System.Collections.Generic;
using System.Threading.Tasks;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Domain.Specifications;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Application;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;
using BridgingIT.DevKit.Examples.BookStore.Infrastructure;
using Microsoft.EntityFrameworkCore;

public class CatalogQueryService(
    IGenericRepository<Book> bookRepository, CatalogDbContext dbContext)
    : ICatalogQueryService
{
    /// <summary>
    /// Retrieves a collection of related books based on the provided book.
    /// </summary>
    /// <param name="book">The book to find related books for.</param>
    /// <param name="limit">The maximum number of related books to retrieve (default is 5).</param>
    public async Task<Result<IEnumerable<Book>>> BookFindAllRelatedAsync(Book book, int limit = 5)
    {
        if (book is null)
        {
            return Result<IEnumerable<Book>>.Failure();
        }

        var bookKeywords = book.Keywords.SafeNull().Select(k => k.Text).ToList();
        var relatedBookIds = await dbContext.Books.SelectMany(e => e.Keywords)
            .Where(ki => bookKeywords.Contains(ki.Text) && ki.BookId != book.Id)
            .GroupBy(ki => ki.BookId)
            .OrderByDescending(g => g.Count())
            .Take(limit)
            .Select(g => g.Key)
            .ToListAsync();

        return await bookRepository.FindAllResultAsync(
            new Specification<Book>(e => relatedBookIds.Contains(e.Id)));
    }}