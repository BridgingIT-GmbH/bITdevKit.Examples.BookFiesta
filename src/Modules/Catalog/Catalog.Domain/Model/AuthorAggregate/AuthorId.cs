// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

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

    public bool IsEmpty => this.Value == Guid.Empty;

    public static implicit operator Guid(AuthorId id) => id?.Value ?? default; // allows a AuthorId value to be implicitly converted to a Guid.
    public static implicit operator string(AuthorId id) => id?.Value.ToString(); // allows a AuthorId value to be implicitly converted to a string.
    public static implicit operator AuthorId(Guid id) => id; // allows a Guid value to be implicitly converted to a AuthorId object.

    public static AuthorId Create()
    {
        return new AuthorId(Guid.NewGuid());
    }

    public static AuthorId Create(Guid id)
    {
        return new AuthorId(id);
    }

    public static AuthorId Create(string id)
    {
        EnsureArg.IsNotNullOrWhiteSpace(id, nameof(id));
        return new AuthorId(Guid.Parse(id));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }
}