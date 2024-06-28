// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

using System;
using BridgingIT.DevKit.Domain.Model;

public class AuthorBook : Entity<Guid>
{
    private AuthorBook() { } // EF Core requires a parameterless constructor

#pragma warning disable SA1202 // Elements should be ordered by access
    public AuthorBook(BookId bookId)
#pragma warning restore SA1202 // Elements should be ordered by access
    {
        this.BookId = bookId;
    }

    public BookId BookId { get; private set; }
}