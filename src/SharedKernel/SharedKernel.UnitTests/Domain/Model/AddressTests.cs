// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.UnitTests.Domain;

using Bogus;
using DevKit.Domain;
using SharedKernel.Domain;

public class AddressTests
{
    private readonly Faker faker = new();

    [Fact]
    public void Create_ValidAddress_ReturnsAddressInstance()
    {
        // Arrange
        var name = this.faker.Person.FullName;
        var line1 = this.faker.Address.StreetAddress();
        var line2 = this.faker.Address.SecondaryAddress();
        var postalCode = this.faker.Address.ZipCode();
        var city = this.faker.Address.City();
        var country = this.faker.Address.Country();

        // Act
        var sut = Address.Create(name, line1, line2, postalCode, city, country);

        // Assert
        sut.ShouldNotBeNull();
        sut.Name.ShouldBe(name);
        sut.Line1.ShouldBe(line1);
        sut.Line2.ShouldBe(line2);
        sut.PostalCode.ShouldBe(postalCode);
        sut.City.ShouldBe(city);
        sut.Country.ShouldBe(country);
    }

    [Fact]
    public void Create_InvalidAddress_ThrowsDomainRuleException()
    {
        // Arrange
        var name = string.Empty;
        var line1 = string.Empty;
        var line2 = this.faker.Address.SecondaryAddress();
        var postalCode = string.Empty;
        var city = this.faker.Address.City();
        var country = string.Empty;

        // Act & Assert
        Should.Throw<DomainRuleException>(() =>
                Address.Create(name, line1, line2, postalCode, city, country))
            .Message.ShouldBe("Invalid address");
    }

    [Fact]
    public void GetAtomicValues_ReturnsAllProperties()
    {
        // Arrange
        var name = this.faker.Person.FullName;
        var line1 = this.faker.Address.StreetAddress();
        var line2 = this.faker.Address.SecondaryAddress();
        var postalCode = this.faker.Address.ZipCode();
        var city = this.faker.Address.City();
        var country = this.faker.Address.Country();
        var sut = Address.Create(name, line1, line2, postalCode, city, country);

        // Act
        var atomicValues = sut.GetType()
            .GetMethod("GetAtomicValues", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(sut, null) as IEnumerable<object>;

        // Assert
        atomicValues.ShouldNotBeNull();
        atomicValues.ShouldContain(name);
        atomicValues.ShouldContain(line1);
        atomicValues.ShouldContain(line2);
        atomicValues.ShouldContain(postalCode);
        atomicValues.ShouldContain(city);
        atomicValues.ShouldContain(country);
    }
}