﻿// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

using System.Collections.Generic;
using BridgingIT.DevKit.Domain.Model;

public class BookPublisher : ValueObject
{
    private BookPublisher() { }

    private BookPublisher(PublisherId publisherId, string name)
    {
        this.PublisherId = publisherId;
        this.Name = name;
    }

    public PublisherId PublisherId { get; private set; }

    public string Name { get; private set; }

    public static BookPublisher Create(Publisher publisher) => new(publisher.Id, publisher.Name);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.PublisherId;
    }
}