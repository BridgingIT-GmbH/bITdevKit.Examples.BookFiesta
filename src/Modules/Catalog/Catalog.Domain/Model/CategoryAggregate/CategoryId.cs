// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

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

    public bool IsEmpty => this.Value == Guid.Empty;

    //public static implicit operator Guid(CategoryId id) => id?.Value ?? default; // allows a CategoryId value to be implicitly converted to a Guid.
    //public static implicit operator CategoryId(Guid id) => id; // allows a Guid value to be implicitly converted to a CategoryId object.

    public static CategoryId Create()
    {
        return new CategoryId(Guid.NewGuid());
    }

    public static CategoryId Create(Guid id)
    {
        return new CategoryId(id);
    }

    public static CategoryId Create(string id)
    {
        EnsureArg.IsNotNullOrWhiteSpace(id, nameof(id));
        return new CategoryId(Guid.Parse(id));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }
}