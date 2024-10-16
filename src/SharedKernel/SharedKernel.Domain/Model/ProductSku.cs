// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

[DebuggerDisplay("Value={Value}")]
public partial class ProductSku : ValueObject
{
    private static readonly Regex SkuRegex = GeneratedRegexes.SkuRegex();

    private ProductSku() { }

    private ProductSku(string value)
    {
        this.Validate(value);
        this.Value = value;
    }

    public string Value { get; private set; }

    public static implicit operator ProductSku(string value)
    {
        return Create(value);
    }

    public static implicit operator string(ProductSku sku)
    {
        return sku.Value;
    }

    public static ProductSku Create(string value)
    {
        value = value?.Trim().ToUpperInvariant() ?? string.Empty;
        return new ProductSku(value);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }

    private void Validate(string sku)
    {
        if (string.IsNullOrEmpty(sku))
        {
            throw new ArgumentException("SKU cannot be empty.");
        }

        if (!SkuRegex.IsMatch(sku))
        {
            throw new ArgumentException("SKU must be 8-12 digits long.");
        }
    }

    public static partial class GeneratedRegexes
    {
        [GeneratedRegex(@"^\d{8,12}$", RegexOptions.Compiled)]
        public static partial Regex SkuRegex();
    }
}