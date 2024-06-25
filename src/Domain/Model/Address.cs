// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.GettingStarted.Domain;

using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Model;

public class Address : ValueObject
{
    private Address()
    {
    }

    private Address(string line1, string line2, string postalCode, string city, string country)
    {
        this.Line1 = line1;
        this.Line2 = line2;
        this.PostalCode = postalCode;
        this.City = city;
        this.Country = country;
    }

    public string Line1 { get; private set; }

    public string Line2 { get; private set; }

    public string PostalCode { get; private set; }

    public string City { get; private set; }

    public string Country { get; private set; }

    public static Address Create(string line1, string line2, string postalCode, string city, string country)
    {
        var address = new Address(line1, line2, postalCode, city, country);
        Check.Throw(
            AddressRules.IsValid(address));

        return address;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Line1;
        yield return this.Line2;
        yield return this.PostalCode;
        yield return this.City;
        yield return this.Country;
    }
}