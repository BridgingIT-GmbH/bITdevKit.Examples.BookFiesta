// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.UnitTests.Domain;

using Bogus;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

[UnitTest("SharedKernel:Domain")]
public class UrlTests
{
    private readonly Faker faker = new();

    [Fact]
    public void Create_ValidAbsoluteUrl_ReturnsUrlObject()
    {
        // Arrange
        var validUrl = this.faker.Internet.Url();

        // Act
        var sut = Url.Create(validUrl);

        // Assert
        sut.ShouldNotBeNull();
        sut.Value.ShouldBe(validUrl);
        sut.Type.ShouldBe(UrlType.Absolute);
    }

    [Fact]
    public void Create_ValidRelativeUrl_ReturnsUrlObject()
    {
        // Arrange
        var validUrl = $"/{this.faker.Internet.DomainWord()}/{this.faker.Internet.DomainWord()}";

        // Act
        var sut = Url.Create(validUrl);

        // Assert
        sut.ShouldNotBeNull();
        sut.Value.ShouldBe(validUrl);
        sut.Type.ShouldBe(UrlType.Relative);
    }

    [Fact]
    public void Create_ValidLocalUrl_ReturnsUrlObject()
    {
        // Arrange
        var validUrl = $"{this.faker.Internet.DomainWord()}/{this.faker.Internet.DomainWord()}";

        // Act
        var sut = Url.Create(validUrl);

        // Assert
        sut.ShouldNotBeNull();
        sut.Value.ShouldBe(validUrl);
        sut.Type.ShouldBe(UrlType.Local);
    }

    [Fact]
    public void Create_InvalidUrl_ThrowsDomainRuleException()
    {
        // Arrange
        var invalidUrl = $"{this.faker.Lorem.Word()}:{this.faker.Lorem.Word()}";

        // Act & Assert
        Should.Throw<DomainRuleException>(() => Url.Create(invalidUrl))
            .Message.ShouldBe($"Invalid URL format: {invalidUrl}");
    }

    [Fact]
    public void Create_EmptyString_ReturnsNull()
    {
        // Arrange
        var emptyUrl = string.Empty;

        // Act
        var sut = Url.Create(emptyUrl);

        // Assert
        sut.ShouldBeNull();
    }

    [Fact]
    public void ToAbsolute_AbsoluteUrl_ReturnsSameUrl()
    {
        // Arrange
        var absoluteUrl = this.faker.Internet.Url();
        var sut = Url.Create(absoluteUrl);

        // Act
        var result = sut.ToAbsolute(this.faker.Internet.Url());

        // Assert
        result.ShouldBe(absoluteUrl);
    }

    [Fact]
    public void ToAbsolute_RelativeUrl_ReturnsAbsoluteUrl()
    {
        // Arrange
        var relativeUrl = $"/{this.faker.Internet.DomainWord()}/{this.faker.Internet.DomainWord()}";
        var baseUrl = this.faker.Internet.Url();
        var sut = Url.Create(relativeUrl);

        // Act
        var result = sut.ToAbsolute(baseUrl);

        // Assert
        result.ShouldBe($"{baseUrl.TrimEnd('/')}{relativeUrl}");
    }

    [Fact]
    public void ToAbsolute_LocalUrl_ReturnsAbsoluteUrl()
    {
        // Arrange
        var localUrl = $"{this.faker.Internet.DomainWord()}/{this.faker.Internet.DomainWord()}";
        var baseUrl = this.faker.Internet.Url();
        var sut = Url.Create(localUrl);

        // Act
        var result = sut.ToAbsolute(baseUrl);

        // Assert
        result.ShouldBe($"{baseUrl.TrimEnd('/')}/{localUrl}");
    }

    [Fact]
    public void ToAbsolute_EmptyBaseUrl_ThrowsArgumentException()
    {
        // Arrange
        var relativeUrl = $"/{this.faker.Internet.DomainWord()}";
        var sut = Url.Create(relativeUrl);

        // Act & Assert
        Should.Throw<ArgumentException>(() => sut.ToAbsolute(string.Empty))
            .Message.ShouldBe(
                "Base URL is required for converting relative or local URLs to absolute. (Parameter 'value')");
    }
}