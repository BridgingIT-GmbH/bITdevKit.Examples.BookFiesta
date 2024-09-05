// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using BridgingIT.DevKit.Domain.Model;

[DebuggerDisplay("Value={Value}, Amount={Amount}")]
public class AverageRating : ValueObject
{
    private double value;

    private AverageRating()
    {
    }

    private AverageRating(double value, int amount)
    {
        this.Value = value;
        this.Amount = amount;
    }

    public double? Value { get => this.Amount > 0 ? this.value : null; private set => this.value = value!.Value; }

    public int Amount { get; private set; }

    public static implicit operator double(AverageRating rating) => rating.Value ?? 0;

    public static AverageRating Create(double value = 0, int numRatings = 0)
    {
        return new AverageRating(value, numRatings);
    }

    public void Add(Rating rating)
    {
        this.Value = ((this.value * this.Amount) + rating.Value) / ++this.Amount;
    }

    public void Remove(Rating rating)
    {
        if (this.Amount == 0)
        {
            return;
        }

        this.Value = ((this.Value * this.Amount) - rating.Value) / --this.Amount;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }
}