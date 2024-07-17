// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

public class BookAuthorAssignedDomainEvent(Book book, Author author) : DomainEventBase
{
    public TenantId TenantId { get; } = book.TenantId;

    public BookId BookId { get; } = book.Id;

    public string BookTitle { get; } = book.Title;

    public AuthorId AuthorId { get; } = author.Id;
}