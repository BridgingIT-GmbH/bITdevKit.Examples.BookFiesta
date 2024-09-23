// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

public class Address : ValueObject
{
    private Address() { } // Private constructor required by EF Core

    private Address(
        string name,
        string line1,
        string line2,
        string postalCode,
        string city,
        string country)
    {
        this.Name = name;
        this.Line1 = line1;
        this.Line2 = line2;
        this.PostalCode = postalCode;
        this.City = city;
        this.Country = country;
    }

    public string Name { get; }

    public string Line1 { get; }

    public string Line2 { get; }

    public string PostalCode { get; }

    public string City { get; }

    public string Country { get; }

    public static Address Create(
        string name,
        string line1,
        string line2,
        string postalCode,
        string city,
        string country)
    {
        var address = new Address(name, line1, line2, postalCode, city, country);
        if (!IsValid(address))
        {
            throw new DomainRuleException("Invalid address");
        }

        return address;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Name;
        yield return this.Line1;
        yield return this.Line2;
        yield return this.PostalCode;
        yield return this.City;
        yield return this.Country;
    }

    private static bool IsValid(Address address)
    {
        return !string.IsNullOrEmpty(address.Name) &&
            !string.IsNullOrEmpty(address.Line1) &&
            !string.IsNullOrEmpty(address.PostalCode) &&
            !string.IsNullOrEmpty(address.Country) &&
            !string.IsNullOrEmpty(address.Country);
    }
}