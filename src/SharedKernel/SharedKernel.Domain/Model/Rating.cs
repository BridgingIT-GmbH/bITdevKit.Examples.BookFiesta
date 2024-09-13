// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Model;

[DebuggerDisplay("Value={Value}")]
public class Rating : ValueObject
{
    private Rating() { }

    private Rating(int value)
    {
        this.Value = value;
    }

    public int Value { get; private set; }

    public static Rating Poor()
    {
        return new Rating(1);
    }

    public static Rating Fair()
    {
        return new Rating(2);
    }

    public static Rating Good()
    {
        return new Rating(3);
    }

    public static Rating VeryGood()
    {
        return new Rating(4);
    }

    public static Rating Excellent()
    {
        return new Rating(5);
    }

    public static Rating Create(int value)
    {
        DomainRules.Apply([RatingRules.ShouldBeInRange(value)]);

        return new Rating(value);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }
}