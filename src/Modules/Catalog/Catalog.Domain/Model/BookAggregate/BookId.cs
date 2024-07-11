// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

using BridgingIT.DevKit.Domain.Model;
using EnsureThat;

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

    public bool IsEmpty => this.Value == Guid.Empty;

    public static implicit operator Guid(BookId id) => id?.Value ?? default; // allows a BookId value to be implicitly converted to a Guid.
    public static implicit operator BookId(Guid id) => id; // allows a Guid value to be implicitly converted to a BookId object.

    public static BookId CreateUnique()
    {
        return new BookId(Guid.NewGuid());
    }

    public static BookId Create(Guid id)
    {
        return new BookId(id);
    }

    public static BookId Create(string id)
    {
        EnsureArg.IsNotNullOrWhiteSpace(id, nameof(id));
        return new BookId(Guid.Parse(id));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }
}

//public class BookId : AggregateRootId
//{
//    public static implicit operator Guid(BookId id) => id?.Value ?? default; // allows a BookId value to be implicitly converted to a Guid.
//    public static implicit operator BookId(Guid value) => value; // allows a Guid value to be implicitly converted to a BookId object.
//}

//public class AggregateRootId : AggregateRootId<Guid>
//{
//    public AggregateRootId()
//    {
//    }

//    public AggregateRootId(Guid guid)
//    {
//        this.Value = guid;
//    }

//    public override Guid Value { get; protected set; }

//    //public static implicit operator Guid(AggregateRootId id) => id?.Value ?? default; // allows a AggregateRootId value to be implicitly converted to a Guid.
//    //public static implicit operator AggregateRootId(Guid value) => value; // allows a Guid value to be implicitly converted to a AggregateRootId object.

//    //public static implicit operator AggregateRootId<Guid>(AggregateRootId id) => Create(id.Value); // allows a BookId value to be implicitly converted to a AggregateRootId.
//    //public static implicit operator AggregateRootId(AggregateRootId<Guid> value) => value.Value; // allows a AggregateRootId value to be implicitly converted to a BookId object.

//    public static AggregateRootId CreateUnique()
//    {
//        return new AggregateRootId(Guid.NewGuid());
//    }

//    public static AggregateRootId Create(Guid value)
//    {
//        return new AggregateRootId(value);
//    }

//    public static AggregateRootId Create(string value)
//    {
//        return new AggregateRootId(Guid.Parse(value));
//    }

//    protected override IEnumerable<object> GetAtomicValues()
//    {
//        yield return this.Value;
//    }
//}