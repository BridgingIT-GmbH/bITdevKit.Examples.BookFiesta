// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

public class Location : ValueObject
{
    private Location() { } // Private constructor required by EF Core

    private Location(string aisle, string shelf, string bin)
    {
        this.Aisle = aisle;
        this.Shelf = shelf;
        this.Bin = bin;
    }

    public string Aisle { get; }

    public string Shelf { get; }

    public string Bin { get; }

    public static implicit operator string(Location location) => location.ToString();

    public static implicit operator Location(string value)
    {
        var parts = value.Split('|');
        if (parts.Length != 3)
        {
            throw new DomainRuleException("Invalid location format. Expected 'Aisle|Shelf|Bin'.");
        }

        return Create(parts[0], parts[1], parts[2]);
    }

    public static Location Create(string aisle, string shelf, string bin)
    {
        if (string.IsNullOrWhiteSpace(aisle))
        {
            throw new DomainRuleException("Aisle cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(shelf))
        {
            throw new DomainRuleException("Shelf cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(bin))
        {
            throw new DomainRuleException("Bin cannot be empty.");
        }

        return new Location(aisle, shelf, bin);
    }

    public override string ToString()
    {
        return $"{this.Aisle}|{this.Shelf}|{this.Bin}";
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Aisle;
        yield return this.Shelf;
        yield return this.Bin;
    }
}