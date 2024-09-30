// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.UnitTests.Domain;

using System;
using Bogus;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using Shouldly;
using Xunit;

public class ProductSkuTests
{
    private readonly Faker faker = new();

    [Fact]
    public void Create_ValidSku_ReturnsProductSku()
    {
        // Arrange
        var validSku = this.faker.Random.String2(8, 12, "0123456789");

        // Act
        var sut = ProductSku.Create(validSku);

        // Assert
        sut.Value.ShouldBe(validSku.ToUpperInvariant());
    }

    [Fact]
    public void Create_NullSku_ThrowsArgumentException()
    {
        // Arrange
        // Act & Assert
        Should.Throw<ArgumentException>(() => ProductSku.Create(null))
            .Message.ShouldBe("SKU cannot be empty.");
    }

    [Fact]
    public void Create_EmptySku_ThrowsArgumentException()
    {
        // Arrange
        var emptySku = string.Empty;

        // Act & Assert
        Should.Throw<ArgumentException>(() => ProductSku.Create(emptySku))
            .Message.ShouldBe("SKU cannot be empty.");
    }

    [Fact]
    public void Create_InvalidSkuFormat_ThrowsArgumentException()
    {
        // Arrange
        var invalidSku = this.faker.Random.AlphaNumeric(7);

        // Act & Assert
        Should.Throw<ArgumentException>(() => ProductSku.Create(invalidSku))
            .Message.ShouldBe("SKU must be 8-12 digits long.");
    }

    [Fact]
    public void Create_SkuWithWhitespace_TrimsAndReturnsProductSku()
    {
        // Arrange
        var skuWithWhitespace = $" {this.faker.Random.String2(8, 12, "0123456789")} ";

        // Act
        var sut = ProductSku.Create(skuWithWhitespace);

        // Assert
        sut.Value.ShouldBe(skuWithWhitespace.Trim().ToUpperInvariant());
    }

    [Fact]
    public void ImplicitConversion_StringToProductSku_ReturnsProductSku()
    {
        // Arrange
        var validSku = this.faker.Random.String2(8, 12, "0123456789");

        // Act
        ProductSku sut = validSku;

        // Assert
        sut.Value.ShouldBe(validSku.ToUpperInvariant());
    }

    [Fact]
    public void ImplicitConversion_ProductSkuToString_ReturnsSkuValue()
    {
        // Arrange
        var validSku = this.faker.Random.String2(8, 12, "0123456789");
        var productSku = ProductSku.Create(validSku);

        // Act
        string result = productSku;

        // Assert
        result.ShouldBe(validSku.ToUpperInvariant());
    }
}