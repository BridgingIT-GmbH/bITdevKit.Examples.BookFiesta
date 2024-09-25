// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.UnitTests.Domain;

using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

[UnitTest("SharedKernel:Domain")]
public class RatingTests
{
    [Fact]
    public void Create_MustCreateRatingWithGivenValue()
    {
        // Arrange
        const int expectedValue = 5;

        // Act
        var rating = Rating.Create(expectedValue);

        // Assert
        rating.ShouldNotBeNull();
        rating.Value.ShouldBe(expectedValue);
    }

    [Fact]
    public void Create_ZeroRatingValue_ShouldFail()
    {
        // Arrange
        const int expectedValue = 0;

        // Act/Assert
        Should.Throw<DomainRuleException>(() => Rating.Create(expectedValue));
    }

    [Fact]
    public void Create_NegativeRatingValue_ShouldFail()
    {
        // Arrange
        const int expectedValue = -1;

        // Act/Assert
        Should.Throw<DomainRuleException>(() => Rating.Create(expectedValue));
    }

    [Fact]
    public void Create_MaxRatingValue_ShouldFail()
    {
        // Arrange
        const int expectedValue = 9;

        // Act/Assert
        Should.Throw<DomainRuleException>(() => Rating.Create(expectedValue));
    }
}