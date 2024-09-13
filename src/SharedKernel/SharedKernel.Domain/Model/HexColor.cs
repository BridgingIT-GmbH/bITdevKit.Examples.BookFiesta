// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using System.Text.RegularExpressions;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Model;

[DebuggerDisplay("Value={Value}")]
public class HexColor : ValueObject
{
    private static readonly Regex HexColorRegex = new(@"^#(?:[0-9a-fA-F]{3}){1,2}$", RegexOptions.Compiled);

    private HexColor() { } // Private constructor required by EF Core

    private HexColor(string value)
    {
        this.Value = value;
    }

    public string Value { get; }

    public static implicit operator string(HexColor color)
    {
        return color.Value;
    }

    public static implicit operator HexColor(string value)
    {
        return Create(value);
    }

    public static HexColor Create(string value)
    {
        value = Normalize(value);
        if (!IsValid(value))
        {
            throw new DomainRuleException($"Invalid hex color format: {value}. Use the format #RGB or #RRGGBB.");
        }

        return new HexColor(value);
    }

    public static HexColor Create(byte r, byte g, byte b)
    {
        return Create($"#{r:X2}{g:X2}{b:X2}");
    }

    public static bool IsValid(string value)
    {
        return string.IsNullOrWhiteSpace(value) || HexColorRegex.IsMatch(value);
    }

    public override string ToString()
    {
        return this.Value;
    }

    public (byte R, byte G, byte B) ToRgb()
    {
        var hex = this.Value.TrimStart('#');
        return (Convert.ToByte(hex.Substring(0, 2), 16), Convert.ToByte(hex.Substring(2, 2), 16), Convert.ToByte(hex.Substring(4, 2), 16));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }

    private static string Normalize(string value)
    {
        value = value?.ToUpperInvariant() ?? string.Empty;
        if (value.Length == 4) // #RGB format
        {
            return $"#{value[1]}{value[1]}{value[2]}{value[2]}{value[3]}{value[3]}";
        }

        return value;
    }
}