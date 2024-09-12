// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.UnitTests.Domain;

using DevKit.Domain;
using SharedKernel.Domain;

[UnitTest("SharedKernel:Domain")]
public class VatNumberTests
{
    [Fact]
    public void Create_ValidGermanVatNumber_ReturnsVatNumberObject()
    {
        // Arrange
        var validVat = "DE123456789";

        // Act
        var sut = VatNumber.Create(validVat);

        // Assert
        sut.ShouldNotBeNull();
        sut.CountryCode.ShouldBe("DE");
        sut.Number.ShouldBe("123456789");
    }

    [Fact]
    public void Create_ValidUKVatNumber_ReturnsVatNumberObject()
    {
        // Arrange
        var validVat = "GB999999973";

        // Act
        var sut = VatNumber.Create(validVat);

        // Assert
        sut.ShouldNotBeNull();
        sut.CountryCode.ShouldBe("GB");
        sut.Number.ShouldBe("999999973");
    }

    [Fact]
    public void Create_ValidUSEinNumber_ReturnsVatNumberObject()
    {
        // Arrange
        var validEin = "US12-3456789";

        // Act
        var sut = VatNumber.Create(validEin);

        // Assert
        sut.ShouldNotBeNull();
        sut.CountryCode.ShouldBe("US");
        sut.Number.ShouldBe("12-3456789");
    }

    [Fact]
    public void Create_InvalidVatNumber_ThrowsDomainRuleException()
    {
        // Arrange
        var invalidVat = "DE12345";

        // Act & Assert
        Should.Throw<DomainRuleException>(() => VatNumber.Create(invalidVat))
            .Message.ShouldBe($"Invalid VAT/EIN number ({invalidVat}) format for country DE.");
    }

    [Fact]
    public void Create_EmptyString_ReturnsNull()
    {
        // Arrange
        var emptyVat = string.Empty;

        // Act
        var sut = VatNumber.Create(emptyVat);

        // Assert
        sut.ShouldBeNull();
    }

    [Fact]
    public void TryParse_ValidVatNumber_ReturnsTrue()
    {
        // Arrange
        var validVat = "FR12345678901";

        // Act
        var result = VatNumber.TryParse(validVat, out var sut);

        // Assert
        result.ShouldBeTrue();
        sut.ShouldNotBeNull();
        sut.CountryCode.ShouldBe("FR");
        sut.Number.ShouldBe("12345678901");
    }

    [Fact]
    public void TryParse_ValidVatNumberUnknownCountry_ReturnsTrue()
    {
        // Arrange
        var validVat = "XX12345";

        // Act
        var result = VatNumber.TryParse(validVat, out var sut);

        // Assert
        result.ShouldBeTrue();
        sut.CountryCode.ShouldBe("XX");
        sut.Number.ShouldBe("12345");
    }

    [Fact]
    public void TryParse_InvalidVatNumber_ReturnsFalse()
    {
        // Arrange
        var invalidVat = "00012345";

        // Act
        var result = VatNumber.TryParse(invalidVat, out var sut);

        // Assert
        result.ShouldBeFalse();
        sut.ShouldBeNull();
    }

    [Fact]
    public void ToString_USEinNumber_ReturnsFormattedString()
    {
        // Arrange
        var sut = VatNumber.Create("US12-3456789");

        // Act
        var result = sut.ToString();

        // Assert
        result.ShouldBe("US12-3456789");
    }

    [Fact]
    public void ToString_NonUSVatNumber_ReturnsUnformattedString()
    {
        // Arrange
        var sut = VatNumber.Create("DE123456789");

        // Act
        var result = sut.ToString();

        // Assert
        result.ShouldBe("DE123456789");
    }
}