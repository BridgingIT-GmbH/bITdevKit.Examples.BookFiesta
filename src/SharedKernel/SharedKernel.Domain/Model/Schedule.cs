// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using System;
using System.Collections.Generic;
using System.Globalization;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Model;

public class Schedule : ValueObject, IComparable<Schedule>
{
    private Schedule() { } // Private constructor required by EF Core

    private Schedule(DateOnly startDate, DateOnly? endDate)
    {
        this.StartDate = startDate;
        this.EndDate = endDate;
    }

    public DateOnly StartDate { get; private set; }

    public DateOnly? EndDate { get; private set; }

    public bool IsOpenEnded => !this.EndDate.HasValue;

    public static bool operator <(Schedule left, Schedule right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(Schedule left, Schedule right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(Schedule left, Schedule right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(Schedule left, Schedule right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static Schedule Create(DateOnly startDate, DateOnly? endDate = null)
    {
        if (endDate <= startDate)
        {
            throw new DomainRuleException("End date must be after start date");
        }

        return new Schedule(startDate, endDate);
    }

    public bool IsActive(DateOnly date)
    {
        return date >= this.StartDate && (!this.EndDate.HasValue || date <= this.EndDate.Value);
    }

    public bool OverlapsWith(Schedule other)
    {
        if (other == null)
        {
            return false;
        }

        if (this.IsOpenEnded || other.IsOpenEnded)
        {
            return this.StartDate <= (other.EndDate ?? DateOnly.MaxValue) &&
                   other.StartDate <= (this.EndDate ?? DateOnly.MaxValue);
        }

        return this.StartDate <= other.EndDate.Value && other.StartDate <= this.EndDate.Value;
    }

    public override string ToString()
    {
        const string dateFormat = "dd-MM-yyyy";
        return this.EndDate.HasValue
            ? $"{this.StartDate.ToString(dateFormat, CultureInfo.InvariantCulture)} to {this.EndDate.Value.ToString(dateFormat, CultureInfo.InvariantCulture)}"
            : $"{this.StartDate.ToString(dateFormat, CultureInfo.InvariantCulture)} (Open-ended)";
    }

    public int CompareTo(Schedule other)
    {
        if (other == null)
        {
            return 1;
        }

        return this.StartDate.CompareTo(other.StartDate);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.StartDate;
        yield return this.EndDate;
    }
}