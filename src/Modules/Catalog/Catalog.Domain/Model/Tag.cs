// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

[DebuggerDisplay("Id={Id}, Name={Name}")]
public class Tag : Entity<TagId>, IConcurrent
{
    private Tag() { } // Private constructor required by EF Core

    private Tag(string name)
    {
        this.SetName(name);
    }

    public string Name { get; private set; }

    public Guid Version { get; set; }

    public static implicit operator string(Tag tag) => tag?.Name; // allows a Tag value to be implicitly converted to a string.

    public static Tag Create(string name) => new(name);

    public Tag SetName(string name)
    {
        // Validate name
        this.Name = name;
        return this;
    }
}