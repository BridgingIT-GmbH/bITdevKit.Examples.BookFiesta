// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.UnitTests.Domain;

using Bogus;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

[UnitTest("SharedKernel:Domain")]
public class MoneyTests
{
    private readonly Faker faker = new();

    [Fact]
    public void Create_ValidAmountAndCurrency_ReturnsMoneyInstance()
    {
        // Arrange
        var amount = this.faker.Random.Decimal(0, 1000000);
        var currency = Currency.Euro;

        // Act
        var sut = Money.Create(amount, currency);

        // Assert
        sut.ShouldNotBeNull();
        sut.Amount.ShouldBe(amount);
        sut.Currency.ShouldBe(currency);
    }

    [Fact]
    public void Create_ValidAmount_ReturnsMoneyInstanceWithUSDollar()
    {
        // Arrange
        var amount = this.faker.Random.Decimal(0, 1000000);

        // Act
        var sut = Money.Create(amount);

        // Assert
        sut.ShouldNotBeNull();
        sut.Amount.ShouldBe(amount);
        sut.Currency.ShouldBe(Currency.USDollar);
    }

    [Fact]
    public void Zero_ReturnsZeroMoneyWithUSDollar()
    {
        // Act
        var sut = Money.Zero();

        // Assert
        sut.ShouldNotBeNull();
        sut.Amount.ShouldBe(0);
        sut.Currency.ShouldBe(Currency.USDollar);
    }

    [Fact]
    public void Zero_WithCurrency_ReturnsZeroMoneyWithSpecifiedCurrency()
    {
        // Arrange
        var currency = Currency.Euro;

        // Act
        var sut = Money.Zero(currency);

        // Assert
        sut.ShouldNotBeNull();
        sut.Amount.ShouldBe(0);
        sut.Currency.ShouldBe(currency);
    }

    [Fact]
    public void IsZero_ZeroAmount_ReturnsTrue()
    {
        // Arrange
        var sut = Money.Zero();

        // Act
        var result = sut.IsZero();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsZero_NonZeroAmount_ReturnsFalse()
    {
        // Arrange
        var sut = Money.Create(10);

        // Act
        var result = sut.IsZero();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ImplicitConversion_MoneyToDecimal_ReturnsAmount()
    {
        // Arrange
        var amount = this.faker.Random.Decimal(0, 1000000);
        var sut = Money.Create(amount);

        // Act
        decimal result = sut;

        // Assert
        result.ShouldBe(amount);
    }

    [Fact]
    public void ImplicitConversion_MoneyToString_ReturnsFormattedString()
    {
        // Arrange
        var amount = 1234.56m;
        var sut = Money.Create(amount, Currency.USDollar);

        // Act
        string result = sut;

        // Assert
        result.ShouldBe("$1,234.56");
    }

    [Fact]
    public void EqualityOperator_SameMoneyValues_ReturnsTrue()
    {
        // Arrange
        var amount = this.faker.Random.Decimal(0, 1000000);
        var currency = Currency.Euro;
        var money1 = Money.Create(amount, currency);
        var money2 = Money.Create(amount, currency);

        // Act
        var result = money1 == money2;

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void InequalityOperator_DifferentMoneyValues_ReturnsTrue()
    {
        // Arrange
        var money1 = Money.Create(10, Currency.USDollar);
        var money2 = Money.Create(20, Currency.USDollar);

        // Act
        var result = money1 != money2;

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void AdditionOperator_SameCurrency_ReturnsCorrectSum()
    {
        // Arrange
        var money1 = Money.Create(10, Currency.Euro);
        var money2 = Money.Create(20, Currency.Euro);

        // Act
        var result = money1 + money2;

        // Assert
        result.Amount.ShouldBe(30);
        result.Currency.ShouldBe(Currency.Euro);
    }

    [Fact]
    public void AdditionOperator_DifferentCurrencies_ThrowsInvalidOperationException()
    {
        // Arrange
        var money1 = Money.Create(10, Currency.Euro);
        var money2 = Money.Create(20, Currency.USDollar);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => money1 + money2)
            .Message.ShouldBe("Cannot calculate money with different currencies");
    }

    [Fact]
    public void SubtractionOperator_SameCurrency_ReturnsCorrectDifference()
    {
        // Arrange
        var money1 = Money.Create(30, Currency.Euro);
        var money2 = Money.Create(20, Currency.Euro);

        // Act
        var result = money1 - money2;

        // Assert
        result.Amount.ShouldBe(10);
        result.Currency.ShouldBe(Currency.Euro);
    }

    [Fact]
    public void SubtractionOperator_DifferentCurrencies_ThrowsInvalidOperationException()
    {
        // Arrange
        var money1 = Money.Create(30, Currency.Euro);
        var money2 = Money.Create(20, Currency.USDollar);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => money1 - money2)
            .Message.ShouldBe("Cannot calculate money with different currencies");
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var amount = 1234.56m;
        var sut = Money.Create(amount, Currency.USDollar);

        // Act
        var result = sut.ToString();

        // Assert
        result.ShouldBe("$1,234.56");
    }

    [Fact]
    public void ToString_UncommonCurrency_ReturnsFormattedString()
    {
        // Arrange
        var amount = 1234.56m;
        var uncommonCurrency = Currency.Create("KHR"); // Cambodian Riel
        var sut = Money.Create(amount, uncommonCurrency);

        // Act
        var result = sut.ToString();

        // Assert
        result.ShouldBe("1.234,56៛");
    }

    [Fact]
    public void Create_InvalidCurrency_ThrowsDomainRuleException()
    {
        // Arrange
        var amount = 1234.56m;
        var invalidCurrencyCode = "XYZ";

        // Act & Assert
        Should.Throw<DomainRuleException>(() => Money.Create(amount, Currency.Create(invalidCurrencyCode)))
            .Message.ShouldBe($"Invalid currency code: {invalidCurrencyCode}");
    }

    [Fact]
    public void GetHashCode_SameMoneyValues_ReturnsSameHashCode()
    {
        // Arrange
        var amount = this.faker.Random.Decimal(0, 1000000);
        var currency = Currency.Euro;
        var money1 = Money.Create(amount, currency);
        var money2 = Money.Create(amount, currency);

        // Act
        var hashCode1 = money1.GetHashCode();
        var hashCode2 = money2.GetHashCode();

        // Assert
        hashCode1.ShouldBe(hashCode2);
    }
}