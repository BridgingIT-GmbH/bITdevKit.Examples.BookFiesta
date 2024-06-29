// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

using BridgingIT.DevKit.Domain.Model;

public class BookId : AggregateRootId<Guid>
{
    private BookId()
    {
    }

    private BookId(Guid guid)
    {
        this.Value = guid;
    }

    public override Guid Value { get; protected set; }

    public static implicit operator Guid(BookId value) => value?.Value ?? default; // allows a BookId value to be implicitly converted to a Guid.
    public static implicit operator BookId(Guid value) => value; // allows a Guid value to be implicitly converted to a BookId object.
    //public static implicit operator BookId(AggregateRootId<Guid> value) => value.Value; // allows a AggregateRootId value to be implicitly converted to a BookId object.

    public static BookId CreateUnique()
    {
        return new BookId(Guid.NewGuid());
    }

    public static BookId Create(Guid value)
    {
        return new BookId(value);
    }

    public static BookId Create(string value)
    {
        return new BookId(Guid.Parse(value));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }
}