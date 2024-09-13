// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using System.Text.RegularExpressions;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Model;

[DebuggerDisplay("Value={Value}")]
public partial class Website : ValueObject
{
    private Website() { } // Private constructor required by EF Core

    private Website(string website)
    {
        this.Value = website;
    }

    public string Value { get; private set; }

    public static implicit operator string(Website website)
    {
        return website?.Value;
        // allows a Website value to be implicitly converted to a string.
    }

    public static implicit operator Website(string value)
    {
        return Create(value);
    }

    public static Website Create(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null; //throw new DomainRuleException("Website cannot be empty.");
        }

        value = Normalize(value);
        if (!IsValidRegex()
                .IsMatch(value))
        {
            throw new DomainRuleException("Invalid website");
        }

        return new Website(value);
    }

    public override string ToString()
    {
        return this.Value;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }

    private static string Normalize(string value)
    {
        value = value?.Trim()
            .ToLowerInvariant() ?? string.Empty;
        if (value?.StartsWith("http://") != false || value.StartsWith("https://"))
        {
            return value;
        }

        return "https://" + value;
    }

    [GeneratedRegex(@"^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex IsValidRegex();
}