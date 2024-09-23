// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

[DebuggerDisplay("Id={Id}, Name={Name}")]
[TypedEntityId<Guid>]
public class Tag : Entity<TagId>, IConcurrent
{
    private Tag() { } // Private constructor required by EF Core

    private Tag(TenantId tenantId, string name, string category = null)
    {
        this.TenantId = tenantId;
        this.SetName(name);
        this.SetCategory(category);
    }

    public TenantId TenantId { get; private set; }

    public string Name { get; private set; }

    public string Category { get; private set; }

    /// <summary>
    ///     Gets or sets the concurrency token to handle optimistic concurrency.
    /// </summary>
    public Guid Version { get; set; }

    public static implicit operator string(Tag tag)
    {
        return tag?.Name;
        // allows a Tag value to be implicitly converted to a string.
    }

    public static Tag Create(TenantId tenantId, string name, string category)
    {
        _ = tenantId ?? throw new DomainRuleException("TenantId cannot be empty.");

        return new Tag(tenantId, name, category);
    }

    public Tag SetName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new DomainRuleException("Tag name cannot be empty.");
        }

        this.Name = value;
        return this;
    }

    public Tag SetCategory(string value)
    {
        this.Category = value;
        return this;
    }
}