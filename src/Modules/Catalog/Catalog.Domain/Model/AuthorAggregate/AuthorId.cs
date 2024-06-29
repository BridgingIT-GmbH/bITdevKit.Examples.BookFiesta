// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

using BridgingIT.DevKit.Domain.Model;

public class AuthorId : AggregateRootId<Guid>
{
    private AuthorId()
    {
    }

    private AuthorId(Guid guid)
    {
        this.Value = guid;
    }

    public override Guid Value { get; protected set; }

    public static implicit operator Guid(AuthorId value) => value?.Value ?? default; // allows a AuthorId value to be implicitly converted to a Guid.
    public static implicit operator AuthorId(Guid value) => value; // allows a Guid value to be implicitly converted to a AuthorId object.
    //public static implicit operator AuthorId(AggregateRootId<Guid> value) => value.Value; // allows a AggregateRootId value to be implicitly converted to a AuthorId object.

    public static AuthorId CreateUnique()
    {
        return new AuthorId(Guid.NewGuid());
    }

    public static AuthorId Create(Guid value)
    {
        return new AuthorId(value);
    }

    public static AuthorId Create(string value)
    {
        return new AuthorId(Guid.Parse(value));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }
}