// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Organization.Domain;

public class TenantSubscriptionId : AggregateRootId<Guid>
{
    private TenantSubscriptionId()
    {
    }

    private TenantSubscriptionId(Guid guid)
    {
        this.Value = guid;
    }

    public override Guid Value { get; protected set; }

    public bool IsEmpty => this.Value == Guid.Empty;

    public static implicit operator Guid(TenantSubscriptionId id) => id?.Value ?? default; // allows a SubscriptionId value to be implicitly converted to a Guid.
    public static implicit operator string(TenantSubscriptionId id) => id?.Value.ToString(); // allows a SubscriptionId value to be implicitly converted to a string.
    public static implicit operator TenantSubscriptionId(Guid id) => id; // allows a Guid value to be implicitly converted to a SubscriptionId object.

    public static TenantSubscriptionId Create()
    {
        return new TenantSubscriptionId(Guid.NewGuid());
    }

    public static TenantSubscriptionId Create(Guid id)
    {
        return new TenantSubscriptionId(id);
    }

    public static TenantSubscriptionId Create(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Id cannot be null or whitespace.");
        }

        return new TenantSubscriptionId(Guid.Parse(id));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }
}