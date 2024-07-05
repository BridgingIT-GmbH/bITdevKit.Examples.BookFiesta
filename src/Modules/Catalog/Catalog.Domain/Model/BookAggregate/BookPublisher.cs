// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

using BridgingIT.DevKit.Domain.Model;

public class BookPublisher : Entity<Guid>
{
    private BookPublisher() { } // EF Core requires a parameterless constructor

#pragma warning disable SA1202 // Elements should be ordered by access
    public BookPublisher(PublisherId publisherId, string name)
#pragma warning restore SA1202 // Elements should be ordered by access
    {
        this.PublisherId = publisherId;
        this.Name = name;
    }

    public PublisherId PublisherId { get; private set; }

    public string Name { get; private set; }
}