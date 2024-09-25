namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.UnitTests.Domain;

using Bogus;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

[UnitTest("SharedKernel:Domain")]
public class WebsiteTests
{
    private readonly Faker faker = new();

    [Fact]
    public void Create_ValidWebsiteWithHttps_ReturnsWebsiteObject()
    {
        // Arrange
        var validWebsite = this.faker.Internet.Url();

        // Act
        var sut = Website.Create(validWebsite);

        // Assert
        sut.ShouldNotBeNull();
        sut.Value.ShouldBe(validWebsite);
    }

    [Fact]
    public void Create_ValidWebsiteWithoutProtocol_ReturnsWebsiteObjectWithHttps()
    {
        // Arrange
        var inputWebsite = this.faker.Internet.DomainName();

        // Act
        var sut = Website.Create(inputWebsite);

        // Assert
        sut.ShouldNotBeNull();
        sut.Value.ShouldBe($"https://{inputWebsite}");
    }

    [Fact]
    public void Create_InvalidWebsite_ThrowsDomainRuleException()
    {
        // Arrange
        var invalidWebsite = this.faker.Lorem.Sentence();

        // Act & Assert
        Should.Throw<DomainRuleException>(() => Website.Create(invalidWebsite)).Message.ShouldBe("Invalid website");
    }

    [Fact]
    public void Create_EmptyString_ReturnsNull()
    {
        // Arrange
        var emptyWebsite = string.Empty;

        // Act
        var sut = Website.Create(emptyWebsite);

        // Assert
        sut.ShouldBeNull();
    }

    [Fact]
    public void ToString_ValidWebsite_ReturnsWebsiteString()
    {
        // Arrange
        var websiteString = this.faker.Internet.Url();
        var sut = Website.Create(websiteString);

        // Act
        var result = sut.ToString();

        // Assert
        result.ShouldBe(websiteString);
    }

    [Fact]
    public void ImplicitConversion_WebsiteToString_ReturnsWebsiteValue()
    {
        // Arrange
        var websiteString = this.faker.Internet.Url();
        var sut = Website.Create(websiteString);

        // Act
        string result = sut;

        // Assert
        result.ShouldBe(websiteString);
    }

    [Fact]
    public void ImplicitConversion_StringToWebsite_ReturnsWebsiteObject()
    {
        // Arrange
        var websiteString = this.faker.Internet.Url();

        // Act
        Website sut = websiteString;

        // Assert
        sut.ShouldNotBeNull();
        sut.Value.ShouldBe(websiteString);
    }
}