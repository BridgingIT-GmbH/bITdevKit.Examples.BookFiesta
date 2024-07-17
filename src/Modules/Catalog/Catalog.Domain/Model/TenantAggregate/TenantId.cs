// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

public class TenantId : AggregateRootId<Guid>
{
    private TenantId()
    {
    }

    private TenantId(Guid guid)
    {
        this.Value = guid;
    }

    public override Guid Value { get; protected set; }

    public bool IsEmpty => this.Value == Guid.Empty;

    public static implicit operator Guid(TenantId id) => id?.Value ?? default; // allows a TenantId value to be implicitly converted to a Guid.
    public static implicit operator string(TenantId id) => id?.Value.ToString(); // allows a TenantId value to be implicitly converted to a string.
    public static implicit operator TenantId(Guid id) => id; // allows a Guid value to be implicitly converted to a TenantId object.

    public static TenantId Create()
    {
        return new TenantId(Guid.NewGuid());
    }

    public static TenantId Create(Guid id)
    {
        return new TenantId(id);
    }

    public static TenantId Create(string id)
    {
        EnsureArg.IsNotNullOrWhiteSpace(id, nameof(id));
        return new TenantId(Guid.Parse(id));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }
}
