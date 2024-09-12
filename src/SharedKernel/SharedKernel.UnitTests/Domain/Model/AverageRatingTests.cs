// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.UnitTests.Domain;

using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

[UnitTest("SharedKernel:Domain")]
public class AverageRatingTests
{
    private readonly AverageRating sut = AverageRating.Create(3, 1);

    [Fact]
    public void Add_ValidRating_RatingAddedAndAmountIncremented()
    {
        // Arrange & Act
        this.sut.Add(Rating.Create(3));

        // Assert
        this.sut.Value.ShouldBe(3);
        this.sut.Amount.ShouldBe(2);
    }

    [Fact]
    public void Add_MultipleValidRatings_RatingsAddedAndAmountIncremented()
    {
        // Arrange & Act
        this.sut.Add(Rating.Create(3));
        this.sut.Add(Rating.Create(4));
        this.sut.Add(Rating.Create(5));

        // Assert
        this.sut.Value.ShouldBe(3.75);
        this.sut.Amount.ShouldBe(4);
    }

    [Fact]
    public void Remove_ValidRating_RatingRemovedAndAmountDecremented()
    {
        // Arrange
        this.sut.Add(Rating.Create(3));

        // Act
        this.sut.Remove(Rating.Create(3));

        // Assert
        this.sut.Value.ShouldBe(3);
        this.sut.Amount.ShouldBe(1);
    }

    [Fact]
    public void Remove_MultipleValidRatings_RatingsRemovedThenValueNullAndAmountDecremented()
    {
        // Arrange
        this.sut.Add(Rating.Create(3));

        // Act
        this.sut.Remove(Rating.Create(3));
        this.sut.Remove(Rating.Create(3));
        this.sut.Remove(Rating.Create(3));

        // Assert
        this.sut.Value.ShouldBeNull();
        this.sut.Amount.ShouldBe(0);
    }

    [Fact]
    public void Value_NoRatings_ReturnsNull()
    {
        // Arrange & Act
        this.sut.Remove(Rating.Create(3));

        // Assert
        this.sut.Value.ShouldBeNull();
        this.sut.Amount.ShouldBe(0);
    }
}