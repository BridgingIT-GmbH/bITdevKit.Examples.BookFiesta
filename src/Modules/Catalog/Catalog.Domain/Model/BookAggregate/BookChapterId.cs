// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

using BridgingIT.DevKit.Domain.Model;

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

    public static implicit operator BookChapterId(Guid value) => value; // allows a Guid value to be implicitly converted to a BookChapterId object.

    public static BookChapterId CreateUnique()
    {
        return new BookChapterId(Guid.NewGuid());
    }

    public static BookChapterId Create(Guid value)
    {
        return new BookChapterId(value);
    }

    public static BookChapterId Create(string value)
    {
        return new BookChapterId(Guid.Parse(value));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }
}
