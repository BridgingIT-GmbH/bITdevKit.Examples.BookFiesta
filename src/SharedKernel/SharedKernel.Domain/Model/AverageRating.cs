// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

[DebuggerDisplay("Value={Value}, Amount={Amount}")]
public class AverageRating : ValueObject
{
    private double value;

    private AverageRating() { }

    private AverageRating(double value, int amount)
    {
        this.Value = value;
        this.Amount = amount;
    }

    public double? Value
    {
        get => this.Amount > 0 ? this.value : null;
        private set => this.value = value!.Value;
    }

    public int Amount { get; private set; }

    public static implicit operator double(AverageRating rating)
    {
        return rating.Value ?? 0;
    }

    public static AverageRating Create(double value = 0, int amount = 0)
    {
        return new AverageRating(value, amount);
    }

    public void Add(Rating rating)
    {
        // ReSharper disable once ArrangeRedundantParentheses
        this.Value = ((this.value * this.Amount) + rating.Value) / ++this.Amount;
    }

    public void Remove(Rating rating)
    {
        if (this.Amount == 0)
        {
            return;
        }

        // ReSharper disable once ArrangeRedundantParentheses
        this.Value = ((this.Value * this.Amount) - rating.Value) / --this.Amount;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }
}