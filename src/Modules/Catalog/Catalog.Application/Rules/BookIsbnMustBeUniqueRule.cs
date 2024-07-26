// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Application;

using System.Threading.Tasks;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

public class BookIsbnMustBeUniqueRule(
    IGenericRepository<Book> repository,
    Book book) : IBusinessRule
{
    public string Message => "Book ISBN should be unique";

    public async Task<bool> IsSatisfiedAsync(CancellationToken cancellationToken = default)
    {
        return !(await repository.FindAllAsync(
            BookSpecifications.ForIsbn(book.Isbn), cancellationToken: cancellationToken)).SafeAny();
    }
}

public static partial class BookRules
{
    public static IBusinessRule IsbnMustBeUnique(
        IGenericRepository<Book> repository,
        Book book) => new BookIsbnMustBeUniqueRule(repository, book);
}