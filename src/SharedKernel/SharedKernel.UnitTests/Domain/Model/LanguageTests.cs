// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.UnitTests.Domain;

using Bogus;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

public class LanguageTests
{
    private readonly Faker faker = new();

    [Fact]
    public void Create_ValidCode_ReturnsLanguageInstance()
    {
        // Arrange
        var validCode = "en";

        // Act
        var sut = Language.Create(validCode);

        // Assert
        sut.ShouldNotBeNull();
        sut.Code.ShouldBe(validCode);
        sut.Name.ShouldBe("English");
    }

    [Fact]
    public void Create_InvalidCode_ThrowsArgumentException()
    {
        // Arrange
        var invalidCode = "xx";

        // Act & Assert
        Should.Throw<ArgumentException>(() => Language.Create(invalidCode))
            .Message.ShouldBe($"Invalid language code: {invalidCode}");
    }

    [Fact]
    public void Create_NullOrEmptyCode_ThrowsArgumentException()
    {
        // Arrange
        var nullCode = null as string;
        var emptyCode = string.Empty;

        // Act & Assert
        Should.Throw<ArgumentException>(() => Language.Create(nullCode))
            .Message.ShouldBe("Language code cannot be null or empty.");
        Should.Throw<ArgumentException>(() => Language.Create(emptyCode))
            .Message.ShouldBe("Language code cannot be null or empty.");
    }

    [Fact]
    public void Create_CodeWithInvalidLength_ThrowsArgumentException()
    {
        // Arrange
        var invalidLengthCode = this.faker.Random.String2(3);

        // Act & Assert
        Should.Throw<ArgumentException>(() => Language.Create(invalidLengthCode))
            .Message.ShouldBe("Language code must be exactly two characters long.");
    }

    [Fact]
    public void Create_SameCode_ReturnsSameInstance()
    {
        // Arrange
        var code = "fr";

        // Act
        var sut1 = Language.Create(code);
        var sut2 = Language.Create(code);

        // Assert
        sut1.Code.ShouldBeSameAs(sut2.Code);
    }

    [Fact]
    public void Create_DifferentCaseCode_ReturnsSameInstance()
    {
        // Arrange
        var lowerCode = "de";
        var upperCode = "DE";

        // Act
        var sut1 = Language.Create(lowerCode);
        var sut2 = Language.Create(upperCode);

        // Assert
        sut1.Code.ShouldBeSameAs(sut2.Code);
    }

    [Fact]
    public void ImplicitConversionToString_ReturnsCode()
    {
        // Arrange
        var code = "es";
        var sut = Language.Create(code);

        // Act
        string result = sut;

        // Assert
        result.ShouldBe(code);
    }

    [Fact]
    public void Equals_SameCode_ReturnsTrue()
    {
        // Arrange
        var code = "it";
        var sut1 = Language.Create(code);
        var sut2 = Language.Create(code);

        // Act
        var result = sut1.Equals(sut2);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Equals_DifferentCode_ReturnsFalse()
    {
        // Arrange
        var sut1 = Language.Create("ja");
        var sut2 = Language.Create("ko");

        // Act
        var result = sut1.Equals(sut2);

        // Assert
        result.ShouldBeFalse();
    }
}