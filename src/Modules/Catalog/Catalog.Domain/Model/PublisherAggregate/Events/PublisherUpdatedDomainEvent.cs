// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

public class PublisherUpdatedDomainEvent(Publisher publisher) : DomainEventBase
{
    public PublisherId PublisherId { get; } = publisher.Id;
}
