// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

public class BookChapterId : EntityId<Guid>
{
    private BookChapterId()
    {
    }

    private BookChapterId(Guid guid)
    {
        this.Value = guid;
    }

    public override Guid Value { get; protected set; }

    public bool IsEmpty => this.Value == Guid.Empty;

    //public static implicit operator Guid(BookChapterId id) => id?.Value ?? default; // allows a BookChapterId value to be implicitly converted to a Guid.
    //public static implicit operator BookChapterId(Guid id) => id; // allows a Guid value to be implicitly converted to a BookChapterId object.

    public static BookChapterId Create()
    {
        return new BookChapterId(Guid.NewGuid());
    }

    public static BookChapterId Create(Guid id)
    {
        return new BookChapterId(id);
    }

    public static BookChapterId Create(string id)
    {
        EnsureArg.IsNotNullOrWhiteSpace(id, nameof(id));
        return new BookChapterId(Guid.Parse(id));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }
}