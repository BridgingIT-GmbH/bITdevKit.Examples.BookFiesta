﻿// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using System.Threading.Tasks;
using Common;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Repositories;
using Domain;

public class BookIsbnMustBeUniqueRule(IGenericRepository<Book> repository, Book book) : DomainRuleBase
{
    public override string Message => "Book ISBN should be unique";

    public override async Task<bool> ApplyAsync(CancellationToken cancellationToken = default)
    {
        return !(await repository.FindAllAsync(BookSpecifications.ForIsbn(book.Isbn), cancellationToken: cancellationToken)).SafeAny();
    }
}

public static partial class BookRules
{
    public static IDomainRule IsbnMustBeUnique(IGenericRepository<Book> repository, Book book)
    {
        return new BookIsbnMustBeUniqueRule(repository, book);
    }
}