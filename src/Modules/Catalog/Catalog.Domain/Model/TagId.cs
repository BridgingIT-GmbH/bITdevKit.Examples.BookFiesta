﻿// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

public class TagId : EntityId<Guid>
{
    private TagId()
    {
    }

    private TagId(Guid guid)
    {
        this.Value = guid;
    }

    public override Guid Value { get; protected set; }

    public bool IsEmpty => this.Value == Guid.Empty;

    //public static implicit operator Guid(TagId id) => id?.Value ?? default; // allows a TagId value to be implicitly converted to a Guid.
    //public static implicit operator TagId(Guid id) => id; // allows a Guid value to be implicitly converted to a TagId object.

    public static TagId Create()
    {
        return new TagId(Guid.NewGuid());
    }

    public static TagId Create(Guid id)
    {
        return new TagId(id);
    }

    public static TagId Create(string id)
    {
        EnsureArg.IsNotNullOrWhiteSpace(id, nameof(id));
        return new TagId(Guid.Parse(id));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }
}