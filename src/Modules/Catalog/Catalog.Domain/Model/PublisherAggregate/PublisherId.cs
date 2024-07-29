// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Catalog.Domain;

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

    public bool IsEmpty => this.Value == Guid.Empty;

    //public static implicit operator Guid(PublisherId id) => id?.Value ?? default; // allows a PublisherId value to be implicitly converted to a Guid.
    //public static implicit operator PublisherId(Guid id) => id; // allows a Guid value to be implicitly converted to a PublisherId object.

    public static PublisherId Create()
    {
        return new PublisherId(Guid.NewGuid());
    }

    public static PublisherId Create(Guid id)
    {
        return new PublisherId(id);
    }

    public static PublisherId Create(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Id cannot be null or whitespace.");
        }

        return new PublisherId(Guid.Parse(id));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }
}