// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

[DebuggerDisplay("BookId={BookId}, Title={Title")]
public class AuthorBook : ValueObject
{
    private AuthorBook() { }

#pragma warning disable SA1202 // Elements should be ordered by access
    private AuthorBook(BookId bookId, string title)
#pragma warning restore SA1202 // Elements should be ordered by access
    {
        this.BookId = bookId;
        this.Title = title;
    }

    public BookId BookId { get; }

    public string Title { get; }

    public static AuthorBook Create(Book book)
    {
        _ = book ?? throw new DomainRuleException("AuthorBook Book cannot be empty.");

        return new AuthorBook(book.Id, book.Title);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.BookId;
    }
}