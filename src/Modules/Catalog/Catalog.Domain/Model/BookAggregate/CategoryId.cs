// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

using System;
using BridgingIT.DevKit.Domain.Model;

public class CategoryId : EntityId<Guid>
{
    private CategoryId()
    {
    }

    private CategoryId(Guid guid)
    {
        this.Value = guid;
    }

    public override Guid Value { get; protected set; }

    //public static implicit operator Guid(CategoryId id) => id?.Value ?? default; // allows a CategoryId value to be implicitly converted to a Guid.
    //public static implicit operator CategoryId(Guid value) => value; // allows a Guid value to be implicitly converted to a CategoryId object.

    public static CategoryId CreateUnique()
    {
        return new CategoryId(Guid.NewGuid());
    }

    public static CategoryId Create(Guid value)
    {
        return new CategoryId(value);
    }

    public static CategoryId Create(string value)
    {
        return new CategoryId(Guid.Parse(value));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }
}