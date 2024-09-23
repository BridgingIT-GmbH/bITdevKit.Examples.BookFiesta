// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.UnitTests.Domain;

using System.Reflection;
using Bogus;
using DevKit.Domain;
using SharedKernel.Domain;

[UnitTest("SharedKernel:Domain")]
public class CurrencyTests
{
    private readonly Faker faker = new();

    [Fact]
    public void Create_ValidCurrencyCode_ReturnsCurrencyInstance()
    {
        // Arrange
        var validCode = "USD";

        // Act
        var sut = Currency.Create(validCode);

        // Assert
        sut.ShouldNotBeNull();
        sut.Code.ShouldBe(validCode);
        sut.Symbol.ShouldBe("$");
    }

    [Fact]
    public void Create_InvalidCurrencyCode_ThrowsDomainRuleException()
    {
        // Arrange
        var invalidCode = this.faker.Random.AlphaNumeric(3);

        // Act & Assert
        Should.Throw<DomainRuleException>(() => Currency.Create(invalidCode))
            .Message.ShouldBe($"Invalid currency code: {invalidCode}");
    }

    [Fact]
    public void ImplicitConversion_StringToCurrency_ReturnsCurrencyInstance()
    {
        // Arrange
        var currencyCode = "EUR";

        // Act
        Currency sut = currencyCode;

        // Assert
        sut.ShouldNotBeNull();
        sut.Code.ShouldBe(currencyCode);
        sut.Symbol.ShouldBe("€");
    }

    [Fact]
    public void ImplicitConversion_CurrencyToString_ReturnsCode()
    {
        // Arrange
        var sut = Currency.Create("GBP");

        // Act
        string result = sut;

        // Assert
        result.ShouldBe("GBP");
    }

    [Fact]
    public void ToString_ReturnsCurrencySymbol()
    {
        // Arrange
        var sut = Currency.Create("JPY");

        // Act
        var result = sut.ToString();

        // Assert
        result.ShouldBe("¥");
    }

    [Fact]
    public void GetAtomicValues_ReturnsCode()
    {
        // Arrange
        var sut = Currency.Create("CAD");

        // Act
        var atomicValues = sut.GetType()
            .GetMethod("GetAtomicValues", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(sut, null) as IEnumerable<object>;

        // Assert
        atomicValues.ShouldNotBeNull();
        atomicValues.Single()
            .ShouldBe("CAD");
    }

    [Fact]
    public void StaticProperties_ReturnCorrectCurrencies()
    {
        // Arrange & Act & Assert
        Currency.Euro.Code.ShouldBe("EUR");
        Currency.USDollar.Code.ShouldBe("USD");
        Currency.GBPound.Code.ShouldBe("GBP");
        Currency.JapanYen.Code.ShouldBe("JPY");
    }

    [Fact]
    public void SafeNull_NullCurrencyCode_ThrowsDomainRuleException()
    {
        // Arrange
        string nullCode = null;

        // Act & Assert
        Should.Throw<DomainRuleException>(() => Currency.Create(nullCode))
            .Message.ShouldBe("Invalid currency code: ");
    }
}