// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.UnitTests.Domain;

using Bogus;
using DevKit.Domain;
using SharedKernel.Domain;

[UnitTest("SharedKernel:Domain")]
public class EmailAddressTests
{
    private readonly Faker faker;

    public EmailAddressTests()
    {
        this.faker = new Faker();
    }

    [Fact]
    public void Create_ValidEmail_ReturnsEmailAddressInstance()
    {
        // Arrange
        var validEmail = this.faker.Internet.Email();

        // Act
        var sut = EmailAddress.Create(validEmail);

        // Assert
        sut.ShouldNotBeNull();
        sut.Value.ShouldBe(validEmail.ToLowerInvariant());
    }

    [Fact]
    public void Create_InvalidEmail_ThrowsDomainRuleException()
    {
        // Arrange
        var invalidEmail = "invalid-email";

        // Act & Assert
        Should.Throw<DomainRuleException>(() => EmailAddress.Create(invalidEmail))
            .Message.ShouldBe("Invalid email address");
    }

    [Fact]
    public void Create_NullOrEmptyEmail_ReturnsNull()
    {
        // Arrange
        string nullEmail = null;
        string emptyEmail = string.Empty;

        // Act
        var nullResult = EmailAddress.Create(nullEmail);
        var emptyResult = EmailAddress.Create(emptyEmail);

        // Assert
        nullResult.ShouldBeNull();
        emptyResult.ShouldBeNull();
    }

    [Fact]
    public void ImplicitConversion_StringToEmailAddress_ReturnsEmailAddressInstance()
    {
        // Arrange
        string email = this.faker.Internet.Email();

        // Act
        EmailAddress sut = email;

        // Assert
        sut.ShouldNotBeNull();
        sut.Value.ShouldBe(email.ToLowerInvariant());
    }

    [Fact]
    public void ImplicitConversion_EmailAddressToString_ReturnsEmailString()
    {
        // Arrange
        var email = this.faker.Internet.Email();
        var sut = EmailAddress.Create(email);

        // Act
        string result = sut;

        // Assert
        result.ShouldBe(email.ToLowerInvariant());
    }

    [Fact]
    public void ToString_ReturnsEmailString()
    {
        // Arrange
        var email = this.faker.Internet.Email();
        var sut = EmailAddress.Create(email);

        // Act
        var result = sut.ToString();

        // Assert
        result.ShouldBe(email.ToLowerInvariant());
    }

    [Fact]
    public void GetAtomicValues_ReturnsEmailValue()
    {
        // Arrange
        var email = this.faker.Internet.Email();
        var sut = EmailAddress.Create(email);

        // Act
        var atomicValues = sut.GetType()
            .GetMethod("GetAtomicValues", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(sut, null) as IEnumerable<object>;

        // Assert
        atomicValues.ShouldNotBeNull();
        atomicValues.Single().ShouldBe(email.ToLowerInvariant());
    }

    [Fact]
    public void Create_EmailWithUppercaseLetters_NormalizesToLowercase()
    {
        // Arrange
        var mixedCaseEmail = "Test.User@Example.com";

        // Act
        var sut = EmailAddress.Create(mixedCaseEmail);

        // Assert
        sut.Value.ShouldBe("test.user@example.com");
    }

    [Fact]
    public void Create_EmailWithSurroundingWhitespace_TrimsWhitespace()
    {
        // Arrange
        var emailWithWhitespace = "  user@example.com  ";

        // Act
        var sut = EmailAddress.Create(emailWithWhitespace);

        // Assert
        sut.Value.ShouldBe("user@example.com");
    }

    [Fact]
    public void Create_EmailExceedingMaxLength_ThrowsDomainRuleException()
    {
        // Arrange
        var longEmail = new string('a', 244) + "@example.com"; // 256 characters in total

        // Act & Assert
        Should.Throw<DomainRuleException>(() => EmailAddress.Create(longEmail))
            .Message.ShouldBe("Invalid email address");
    }
}