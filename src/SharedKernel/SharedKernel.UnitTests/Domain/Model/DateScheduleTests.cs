// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.UnitTests.Domain;

using System;
using System.Collections.Generic;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using Bogus;
using Shouldly;
using Xunit;

public class DateScheduleTests
{
    private readonly Faker faker;

    public DateScheduleTests()
    {
        this.faker = new Faker();
    }

    [Fact]
    public void Create_ValidDates_ReturnsSchedule()
    {
        // Arrange
        var startDate = this.faker.Date.FutureDateOnly();
        var endDate = startDate.AddDays(this.faker.Random.Int(1, 365));

        // Act
        var sut = DateSchedule.Create(startDate, endDate);

        // Assert
        sut.ShouldNotBeNull();
        sut.StartDate.ShouldBe(startDate);
        sut.EndDate.ShouldBe(endDate);
    }

    [Fact]
    public void Create_EndDateBeforeStartDate_ThrowsDomainRuleException()
    {
        // Arrange
        var startDate = this.faker.Date.FutureDateOnly();
        var endDate = startDate.AddDays(-1);

        // Act & Assert
        Should.Throw<DomainRuleException>(() => DateSchedule.Create(startDate, endDate))
            .Message.ShouldBe("End date must be after start date");
    }

    [Fact]
    public void IsActive_DateWithinSchedule_ReturnsTrue()
    {
        // Arrange
        var startDate = this.faker.Date.PastDateOnly();
        var endDate = this.faker.Date.FutureDateOnly();
        var sut = DateSchedule.Create(startDate, endDate);
        var testDate = this.faker.Date.Between(
            startDate.ToDateTime(TimeOnly.MinValue),
            endDate.ToDateTime(TimeOnly.MinValue));

        // Act
        var result = sut.IsActive(DateOnly.FromDateTime(testDate));

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsActive_DateOutsideSchedule_ReturnsFalse()
    {
        // Arrange
        var startDate = this.faker.Date.FutureDateOnly();
        var endDate = startDate.AddDays(30);
        var sut = DateSchedule.Create(startDate, endDate);
        var testDate = startDate.AddDays(-1);

        // Act
        var result = sut.IsActive(testDate);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void OverlapsWith_OverlappingSchedules_ReturnsTrue()
    {
        // Arrange
        var startDate1 = this.faker.Date.PastDateOnly();
        var endDate1 = startDate1.AddDays(30);
        var sut = DateSchedule.Create(startDate1, endDate1);

        var startDate2 = endDate1.AddDays(-5);
        var endDate2 = startDate2.AddDays(30);
        var other = DateSchedule.Create(startDate2, endDate2);

        // Act
        var result = sut.OverlapsWith(other);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void OverlapsWith_NonOverlappingSchedules_ReturnsFalse()
    {
        // Arrange
        var startDate1 = this.faker.Date.PastDateOnly();
        var endDate1 = startDate1.AddDays(30);
        var sut = DateSchedule.Create(startDate1, endDate1);

        var startDate2 = endDate1.AddDays(1);
        var endDate2 = startDate2.AddDays(30);
        var other = DateSchedule.Create(startDate2, endDate2);

        // Act
        var result = sut.OverlapsWith(other);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ToString_ClosedSchedule_ReturnsCorrectString()
    {
        // Arrange
        var startDate = new DateOnly(2023, 1, 1);
        var endDate = new DateOnly(2023, 12, 31);
        var sut = DateSchedule.Create(startDate, endDate);

        // Act
        var result = sut.ToString();

        // Assert
        result.ShouldBe("01-01-2023 to 31-12-2023");
    }

    [Fact]
    public void ToString_OpenEndedSchedule_ReturnsCorrectString()
    {
        // Arrange
        var startDate = new DateOnly(2023, 1, 1);
        var sut = DateSchedule.Create(startDate);

        // Act
        var result = sut.ToString();

        // Assert
        result.ShouldBe("01-01-2023 (Open-ended)");
    }

    [Fact]
    public void CompareTo_EarlierStartDate_ReturnsNegative()
    {
        // Arrange
        var startDate1 = new DateOnly(2023, 1, 1);
        var startDate2 = new DateOnly(2023, 2, 1);
        var sut = DateSchedule.Create(startDate1);
        var other = DateSchedule.Create(startDate2);

        // Act
        var result = sut.CompareTo(other);

        // Assert
        result.ShouldBeLessThan(0);
    }

    [Fact]
    public void GetAtomicValues_ReturnsCorrectValues()
    {
        // Arrange
        var startDate = this.faker.Date.PastDateOnly();
        var endDate = this.faker.Date.FutureDateOnly();
        var sut = DateSchedule.Create(startDate, endDate);

        // Act
        var result = sut.GetType().GetMethod("GetAtomicValues", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(sut, null) as IEnumerable<object>;

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain(startDate);
        result.ShouldContain(endDate);
    }
}