// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.UnitTests.Domain;

using Bogus;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

[UnitTest("SharedKernel:Domain")]
public class TagTests
{
    private readonly Faker faker = new();

    [Fact]
    public void Create_ValidParameters_ReturnsTagObject()
    {
        // Arrange
        var tenantId = TenantId.Create(this.faker.Random.Guid());
        var name = this.faker.Lorem.Word();
        var category = this.faker.Lorem.Word();

        // Act
        var sut = Tag.Create(tenantId, name, category);

        // Assert
        sut.ShouldNotBeNull();
        sut.TenantId.ShouldBe(tenantId);
        sut.Name.ShouldBe(name);
        sut.Category.ShouldBe(category);
    }

    [Fact]
    public void Create_NullTenantId_ThrowsArgumentNullException()
    {
        // Arrange
        var name = this.faker.Lorem.Word();
        var category = this.faker.Lorem.Word();

        // Act & Assert
        Should.Throw<DomainRuleException>(() => Tag.Create(null, name, category));
    }

    [Fact]
    public void Create_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = TenantId.Create(this.faker.Random.Guid());
        var name = string.Empty;
        var category = this.faker.Lorem.Word();

        // Act & Assert
        Should.Throw<DomainRuleException>(() => Tag.Create(tenantId, name, category));
    }

    [Fact]
    public void SetName_ValidName_UpdatesName()
    {
        // Arrange
        var sut = this.CreateTag();
        var newName = this.faker.Lorem.Word();

        // Act
        sut.SetName(newName);

        // Assert
        sut.Name.ShouldBe(newName);
    }

    [Fact]
    public void SetName_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var sut = this.CreateTag();
        var emptyName = string.Empty;

        // Act & Assert
        Should.Throw<DomainRuleException>(() => sut.SetName(emptyName));
    }

    [Fact]
    public void SetCategory_ValidCategory_UpdatesCategory()
    {
        // Arrange
        var sut = this.CreateTag();
        var newCategory = this.faker.Lorem.Word();

        // Act
        sut.SetCategory(newCategory);

        // Assert
        sut.Category.ShouldBe(newCategory);
    }

    [Fact]
    public void SetCategory_NullCategory_SetsNullCategory()
    {
        // Arrange
        var sut = this.CreateTag();

        // Act
        sut.SetCategory(null);

        // Assert
        sut.Category.ShouldBeNull();
    }

    [Fact]
    public void ImplicitConversion_TagToString_ReturnsTagName()
    {
        // Arrange
        var sut = this.CreateTag();

        // Act
        string result = sut;

        // Assert
        result.ShouldBe(sut.Name);
    }

    [Fact]
    public void Equals_SameTag_ReturnsTrue()
    {
        // Arrange
        var tag1 = this.CreateTag();
        var tag2 = tag1;

        // Act
        var result = tag1.Equals(tag2);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Equals_DifferentTags_ReturnsFalse()
    {
        // Arrange
        var tag1 = this.CreateTag();
        var tag2 = this.CreateTag();

        // Act
        var result = tag1.Equals(tag2);

        // Assert
        result.ShouldBeFalse();
    }

    private Tag CreateTag()
    {
        return Tag.Create(TenantId.Create(this.faker.Random.Guid()), this.faker.Lorem.Word(), this.faker.Lorem.Word());
    }
}