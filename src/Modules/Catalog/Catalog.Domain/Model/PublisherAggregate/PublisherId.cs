// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

using BridgingIT.DevKit.Domain.Model;

public class PublisherId : AggregateRootId<Guid>
{
    private PublisherId()
    {
    }

    private PublisherId(Guid guid)
    {
        this.Value = guid;
    }

    public override Guid Value { get; protected set; }

    //public static implicit operator Guid(PublisherId id) => id?.Value ?? default; // allows a PublisherId value to be implicitly converted to a Guid.
    //public static implicit operator PublisherId(Guid value) => value; // allows a Guid value to be implicitly converted to a PublisherId object.

    public static PublisherId CreateUnique()
    {
        return new PublisherId(Guid.NewGuid());
    }

    public static PublisherId Create(Guid value)
    {
        return new PublisherId(value);
    }

    public static PublisherId Create(string value)
    {
        return new PublisherId(Guid.Parse(value));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }
}