// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

using BridgingIT.DevKit.Domain;

public class BookUpdatedDomainEvent(Book book) : DomainEventBase
{
    public BookId BookId { get; } = book.Id;
}
