// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

public class PublisherCreatedDomainEvent(Publisher publisher) : DomainEventBase
{
    public PublisherId PublisherId { get; } = publisher.Id;
}
