// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

public class BookCreatedDomainEvent(
    TenantId tenantId,
    Book book) : DomainEventBase
{
    public TenantId TenantId { get; } = tenantId;

    public BookId BookId { get; } = book.Id;
}