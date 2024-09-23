namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.UnitTests.Domain;

using System.Reflection;
using SharedKernel.Domain;

[UnitTest("SharedKernel:Domain")]
public class PhoneNumberTests
{
    public static IEnumerable<object[]> ValidPhoneNumbers()
    {
        yield return ["+044 20 7123 4567", "44", "2071234567"]; // UK
        yield return ["+44 20 7123 4567", "44", "2071234567"]; // UK
        yield return ["+44 (020) 7123 4567", "44", "2071234567"]; // UK
        yield return ["+33 1 23 45 67 89", "33", "123456789"]; // France
        yield return ["+49 30 12345678", "49", "3012345678"]; // Germany
        yield return ["+39 16 1234 5678", "39", "1612345678"]; // Italy
        yield return ["+34 91 123 4567", "34", "911234567"]; // Spain
        yield return ["+31 20 123 4567", "31", "201234567"]; // Netherlands
        yield return ["+46 8 123 456 78", "46", "812345678"]; // Sweden
        yield return ["+41 44 123 45 67", "41", "441234567"]; // Switzerland

        yield return ["+1 555 123 4567", "1", "5551234567"]; // USA
        yield return ["+01 0555 123 4567", "1", "5551234567"]; // USA
        yield return ["+1 416 555 0123", "1", "4165550123"]; // Canada

        yield return ["+351 21 123 4567", "351", "211234567"]; // Portugal
        yield return ["+852 2345 6789", "852", "23456789"]; // Hong Kong
        yield return ["+971 4 123 4567", "971", "41234567"]; // United Arab Emirates
    }

    [Theory]
    [MemberData(nameof(ValidPhoneNumbers))]
    public void Create_ValidPhoneNumber_ReturnsPhoneNumberInstance(
        string input,
        string expectedCountryCode,
        string expectedNumber)
    {
        // Act
        var sut = PhoneNumber.Create(input);

        // Assert
        sut.ShouldNotBeNull();
        sut.CountryCode.ShouldBe(expectedCountryCode);
        sut.Number.ShouldBe(expectedNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Create_NullOrEmptyInput_ReturnsNull(string input)
    {
        // Act
        var result = PhoneNumber.Create(input);

        // Assert
        result.ShouldBeNull();
    }

    // [Theory]
    // [InlineData("123")]
    // [InlineData("abc123")]
    // [InlineData("+1234567890123456")]
    // public void Create_InvalidPhoneNumber_ThrowsDomainRuleException(string input)
    // {
    //     // Act & Assert
    //     Should.Throw<DomainRuleException>(() => PhoneNumber.Create(input))
    //         .Message.ShouldBe("Invalid phone number format.");
    // }

    [Fact]
    public void ToString_ReturnsFormattedPhoneNumber()
    {
        // Arrange
        var sut = PhoneNumber.Create("+44 20 7123 4567");

        // Act
        var result = sut.ToString();

        // Assert
        result.ShouldBe("+44 2071234567");
    }

    [Fact]
    public void ImplicitConversion_StringToPhoneNumber_ReturnsPhoneNumberInstance()
    {
        // Arrange
        var phoneNumberString = "+49 30 12345678";

        // Act
        PhoneNumber sut = phoneNumberString;

        // Assert
        sut.ShouldNotBeNull();
        sut.CountryCode.ShouldBe("49");
        sut.Number.ShouldBe("3012345678");
    }

    [Fact]
    public void ImplicitConversion_PhoneNumberToString_ReturnsFormattedString()
    {
        // Arrange
        var sut = PhoneNumber.Create("+33 1 23 45 67 89");

        // Act
        string result = sut;

        // Assert
        result.ShouldBe("+33 123456789");
    }

    [Fact]
    public void GetAtomicValues_ReturnsCountryCodeAndNumber()
    {
        // Arrange
        var sut = PhoneNumber.Create("+39 16 1234 5678");

        // Act
        var atomicValues = sut.GetType()
            .GetMethod("GetAtomicValues", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(sut, null) as IEnumerable<object>;

        // Assert
        atomicValues.ShouldNotBeNull();
        atomicValues.ShouldBe(["39", "1612345678"]);
    }

    [Theory]
    [InlineData("0044 20 7123 4567", "44", "2071234567")]
    [InlineData("0033 1 23 45 67 89", "33", "123456789")]
    public void Create_PhoneNumberStartingWith00_ExtractsCountryCodeCorrectly(
        string input,
        string expectedCountryCode,
        string expectedNumber)
    {
        // Act
        var sut = PhoneNumber.Create(input);

        // Assert
        sut.ShouldNotBeNull();
        sut.CountryCode.ShouldBe(expectedCountryCode);
        sut.Number.ShouldBe(expectedNumber);
    }
}