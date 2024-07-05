// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

using BridgingIT.DevKit.Domain;

public class AuthorBookAssignedDomainEvent(Author author, Book book) : DomainEventBase
{
    public AuthorId AuthorId { get; } = author.Id;

    public string AuthorName { get; } = author.PersonName;

    public BookId BookId { get; } = book.Id;
}