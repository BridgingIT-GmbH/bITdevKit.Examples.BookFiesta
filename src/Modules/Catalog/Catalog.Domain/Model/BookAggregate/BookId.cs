// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

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
    public static implicit operator string(BookId id) => id?.Value.ToString(); // allows a BookId value to be implicitly converted to a string.
    public static implicit operator BookId(Guid id) => id; // allows a Guid value to be implicitly converted to a BookId object.

    public static BookId Create()
    {
        return new BookId(Guid.NewGuid());
    }

    public static BookId Create(Guid id)
    {
        return new BookId(id);
    }

    public static BookId Create(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Id cannot be null or whitespace.");
        }

        return new BookId(Guid.Parse(id));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }
}