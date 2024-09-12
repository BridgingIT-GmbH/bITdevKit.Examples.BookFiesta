// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.UnitTests.Domain;

using Bogus;
using SharedKernel.Domain;

[UnitTest("SharedKernel:Domain")]
public class PersonFormalNameTests
{
    private readonly Faker faker = new();

    [Fact]
    public void Create_ValidParts_ReturnsPersonFormalNameInstance()
    {
        // Arrange
        var parts = new[] { "John", "Doe" };

        // Act
        var sut = PersonFormalName.Create(parts);

        // Assert
        sut.ShouldNotBeNull();
        sut.Parts.ShouldBe(parts);
    }

    [Fact]
    public void Create_ValidPartsWithTitleAndSuffix_ReturnsPersonFormalNameInstance()
    {
        // Arrange
        var parts = new[] { "John", "Doe" };
        var title = "Dr.";
        var suffix = "Jr.";

        // Act
        var sut = PersonFormalName.Create(parts, title, suffix);

        // Assert
        sut.ShouldNotBeNull();
        sut.Parts.ShouldBe(parts);
        sut.Title.ShouldBe(title);
        sut.Suffix.ShouldBe(suffix);
    }

    [Fact]
    public void Create_EmptyParts_ThrowsArgumentException()
    {
        // Arrange
        var parts = Array.Empty<string>();

        // Act & Assert
        Should.Throw<ArgumentException>(() => PersonFormalName.Create(parts))
            .Message.ShouldBe("PersonFormalName parts cannot be empty.");
    }

    [Fact]
    public void Create_NullParts_ThrowsArgumentException()
    {
        // Arrange
        string[] parts = null;

        // Act & Assert
        Should.Throw<ArgumentException>(() => PersonFormalName.Create(parts))
            .Message.ShouldBe("PersonFormalName parts cannot be empty.");
    }

    [Fact]
    public void Create_InvalidNamePart_ThrowsArgumentException()
    {
        // Arrange
        var parts = new[] { "John", "Doe123" };

        // Act & Assert
        Should.Throw<ArgumentException>(() => PersonFormalName.Create(parts))
            .Message.ShouldBe("PersonFormalName part contains invalid characters.");
    }

    [Fact]
    public void Create_InvalidTitle_ThrowsArgumentException()
    {
        // Arrange
        var parts = new[] { "John", "Doe" };
        var invalidTitle = "Dr123";

        // Act & Assert
        Should.Throw<ArgumentException>(() => PersonFormalName.Create(parts, invalidTitle))
            .Message.ShouldBe("PersonFormalName title contains invalid characters.");
    }

    [Fact]
    public void Create_InvalidSuffix_ThrowsArgumentException()
    {
        // Arrange
        var parts = new[] { "John", "Doe" };
        var invalidSuffix = "Jr123";

        // Act & Assert
        Should.Throw<ArgumentException>(() => PersonFormalName.Create(parts, suffix: invalidSuffix))
            .Message.ShouldBe("PersonFormalName suffix contains invalid characters.");
    }

    [Fact]
    public void ToString_ReturnsFullName()
    {
        // Arrange
        var parts = new[] { "John", "Doe" };
        var title = "Dr.";
        var suffix = "Jr.";
        var sut = PersonFormalName.Create(parts, title, suffix);

        // Act
        var result = sut.ToString();

        // Assert
        result.ShouldBe("Dr. John Doe, Jr.");
    }

    [Fact]
    public void ToString_WithoutTitleAndSuffix_ReturnsFullName()
    {
        // Arrange
        var parts = new[] { "John", "Doe" };
        var sut = PersonFormalName.Create(parts);

        // Act
        var result = sut.ToString();

        // Assert
        result.ShouldBe("John Doe");
    }

    [Fact]
    public void ImplicitConversion_PersonFormalNameToString_ReturnsFullName()
    {
        // Arrange
        var parts = new[] { "John", "Doe" };
        var title = "Dr.";
        var suffix = "Jr.";
        var sut = PersonFormalName.Create(parts, title, suffix);

        // Act
        string result = sut;

        // Assert
        result.ShouldBe("Dr. John Doe, Jr.");
    }

    [Fact]
    public void GetAtomicValues_ReturnsAllParts()
    {
        // Arrange
        var parts = new[] { "John", "Doe" };
        var title = "Dr.";
        var suffix = "Jr.";
        var sut = PersonFormalName.Create(parts, title, suffix);

        // Act
        var atomicValues = sut.GetType()
            .GetMethod("GetAtomicValues", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
            .Invoke(sut, null) as IEnumerable<object>;

        // Assert
        atomicValues.ShouldNotBeNull();
        atomicValues.ShouldBe(new object[] { title, "John", "Doe", suffix });
    }

    [Fact]
    public void Full_GetterReturnsFullName()
    {
        // Arrange
        var parts = new[] { "John", "Doe" };
        var title = "Dr.";
        var suffix = "Jr.";
        var sut = PersonFormalName.Create(parts, title, suffix);

        // Act
        var result = sut.Full;

        // Assert
        result.ShouldBe("Dr. John Doe, Jr.");
    }

    [Fact]
    public void Full_SetterDoesNotChangeValue()
    {
        // Arrange
        var parts = new[] { "John", "Doe" };
        var sut = PersonFormalName.Create(parts);
        var originalFull = sut.Full;

        // Act
        sut.Full = "Jane Smith";

        // Assert
        sut.Full.ShouldBe(originalFull);
    }
}