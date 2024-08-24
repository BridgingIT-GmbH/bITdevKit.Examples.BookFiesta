// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using System.Text.RegularExpressions;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Model;

public partial class Website : ValueObject
{
    private Website() { } // Private constructor required by EF Core

    private Website(string website)
    {
        this.Value = website;
    }

    public string Value { get; private set; }

    public static implicit operator string(Website website) => website?.Value; // allows a Website value to be implicitly converted to a string.

    public static implicit operator Website(string website) => Create(website);

    public static Website Create(string website)
    {
        website = Normalize(website);
        if (!IsValid(website))
        {
            throw new DomainRuleException("Invalid website");
        }

        return new Website(website);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }

    private static bool IsValid(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return IsValidRegex().IsMatch(value);
    }

    private static string Normalize(string website)
    {
        website = website?.Trim()?.ToLowerInvariant() ?? string.Empty;
        if (website?.StartsWith("http://") != false || website.StartsWith("https://"))
        {
            return website;
        }

        return "https://" + website;
    }

    [GeneratedRegex(@"^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex IsValidRegex();
}