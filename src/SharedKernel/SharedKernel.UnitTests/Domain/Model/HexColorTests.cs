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
public class HexColorTests
{
    private readonly Faker faker = new();

    [Theory]
    // [InlineData("#FFF")]
    [InlineData("#FFFFFF")]
    [InlineData("#000000")]
    [InlineData("#A1B2C3")]
    public void Create_ValidHexColor_ReturnsHexColorInstance(string validColor)
    {
        // Act
        var sut = HexColor.Create(validColor);

        // Assert
        sut.ShouldNotBeNull();
        sut.Value.ShouldBe(validColor.ToUpperInvariant());
    }

    [Theory]
    // [InlineData("#GGG")]
    [InlineData("#GGGGGG")]
    [InlineData("Invalid")]
    public void Create_InvalidHexColor_ThrowsDomainRuleException(string invalidColor)
    {
        // Act & Assert
        Should.Throw<DomainRuleException>(() => HexColor.Create(invalidColor));
    }

    [Fact]
    public void Create_FromRgbValues_ReturnsCorrectHexColor()
    {
        // Arrange
        byte r = 255, g = 128, b = 64;

        // Act
        var sut = HexColor.Create(r, g, b);

        // Assert
        sut.Value.ShouldBe("#FF8040");
    }

    [Fact]
    public void ImplicitConversion_StringToHexColor_ReturnsHexColorInstance()
    {
        // Arrange
        var hexColor = "#ABC";

        // Act
        HexColor sut = hexColor;

        // Assert
        sut.ShouldNotBeNull();
        sut.Value.ShouldBe("#AABBCC");
    }

    [Fact]
    public void ImplicitConversion_HexColorToString_ReturnsHexColorString()
    {
        // Arrange
        var sut = HexColor.Create("#DEF");

        // Act
        string result = sut;

        // Assert
        result.ShouldBe("#DDEEFF");
    }

    [Fact]
    public void ToString_ReturnsHexColorString()
    {
        // Arrange
        var sut = HexColor.Create("#123456");

        // Act
        var result = sut.ToString();

        // Assert
        result.ShouldBe("#123456");
    }

    [Fact]
    public void GetAtomicValues_ReturnsHexColorValue()
    {
        // Arrange
        var sut = HexColor.Create("#789ABC");

        // Act
        var atomicValues = sut.GetType()
            .GetMethod("GetAtomicValues", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(sut, null) as IEnumerable<object>;

        // Assert
        atomicValues.ShouldNotBeNull();
        atomicValues.Single().ShouldBe("#789ABC");
    }

    [Fact]
    public void Create_ShortHexColor_NormalizesToLongFormat()
    {
        // Arrange
        var shortHexColor = "#ABC";

        // Act
        var sut = HexColor.Create(shortHexColor);

        // Assert
        sut.Value.ShouldBe("#AABBCC");
    }

    [Fact]
    public void Create_LowercaseHexColor_NormalizesToUppercase()
    {
        // Arrange
        var lowercaseHexColor = "#abcdef";

        // Act
        var sut = HexColor.Create(lowercaseHexColor);

        // Assert
        sut.Value.ShouldBe("#ABCDEF");
    }

    [Fact]
    public void ToRgb_ReturnsCorrectRgbValues()
    {
        // Arrange
        var sut = HexColor.Create("#1A2B3C");

        // Act
        var (r, g, b) = sut.ToRgb();

        // Assert
        r.ShouldBe((byte)26);
        g.ShouldBe((byte)43);
        b.ShouldBe((byte)60);
    }

    [Fact]
    public void IsValid_ValidHexColors_ReturnsTrue()
    {
        // Act & Assert
        HexColor.IsValid("#000").ShouldBeTrue();
        HexColor.IsValid("#FFFFFF").ShouldBeTrue();
        HexColor.IsValid("#1a2B3c").ShouldBeTrue();
    }

    [Fact]
    public void IsValid_InvalidHexColors_ReturnsFalse()
    {
        // Act & Assert
        HexColor.IsValid("#GGG").ShouldBeFalse();
        HexColor.IsValid("#GGGGGG").ShouldBeFalse();
        HexColor.IsValid("Invalid").ShouldBeFalse();
    }
}