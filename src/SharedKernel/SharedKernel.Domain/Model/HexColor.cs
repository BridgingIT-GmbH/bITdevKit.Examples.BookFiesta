// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using System.Text.RegularExpressions;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Model;

public class HexColor : ValueObject
{
    private static readonly Regex HexColorRegex = new(@"^#(?:[0-9a-fA-F]{3}){1,2}$", RegexOptions.Compiled);

    private HexColor() { } // Private constructor required by EF Core

    private HexColor(string value)
    {
        this.Value = value;
    }

    public string Value { get; }

    public static implicit operator string(HexColor color) => color.Value;

    public static implicit operator HexColor(string color) => Create(color);

    public static HexColor Create(string color)
    {
        color = Normalize(color);
        if (!IsValid(color))
        {
            throw new DomainRuleException($"Invalid hex color format: {color}. Use the format #RGB or #RRGGBB.");
        }

        return new HexColor(color);
    }

    public static HexColor Create(byte r, byte g, byte b)
    {
        return Create($"#{r:X2}{g:X2}{b:X2}");
    }

    public static bool IsValid(string color)
    {
        return string.IsNullOrWhiteSpace(color) || HexColorRegex.IsMatch(color);
    }

    public override string ToString() => this.Value;

    public (byte R, byte G, byte B) ToRGB()
    {
        var hex = this.Value.TrimStart('#');
        return (
            Convert.ToByte(hex.Substring(0, 2), 16),
            Convert.ToByte(hex.Substring(2, 2), 16),
            Convert.ToByte(hex.Substring(4, 2), 16)
        );
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }

    private static string Normalize(string color)
    {
        color = color?.ToUpperInvariant() ?? string.Empty;
        if (color.Length == 4) // #RGB format
        {
            return $"#{color[1]}{color[1]}{color[2]}{color[2]}{color[3]}{color[3]}";
        }

        return color;
    }
}