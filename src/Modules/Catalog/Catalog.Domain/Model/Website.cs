// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

using System.Text.RegularExpressions;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Model;

public class Website : ValueObject
{
    private Website()
    {
    }

    private Website(string value)
    {
        this.Value = value;
    }

    public string Value { get; private set; }

    public static implicit operator string(Website website) => website?.Value; // allows a Website value to be implicitly converted to a string.

    public static Website Create(string value)
    {
        var website = new Website(NormalizeUrl(value));
        if (!IsValid(website))
        {
            throw new BusinessRuleNotSatisfiedException("Invalid website format.");
        }

        return website;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }

    private static string NormalizeUrl(string url)
    {
        url = url?.Trim().ToLowerInvariant();
        if (url?.StartsWith("http://") != false || url.StartsWith("https://"))
        {
            return url;
        }

        return "https://" + url;
    }

    private static bool IsValid(Website website)
    {
        if (string.IsNullOrWhiteSpace(website.Value))
        {
            return false;
        }

        // Basic URL validation regex
        const string pattern = @"^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$";

        return Regex.IsMatch(website.Value, pattern, RegexOptions.IgnoreCase);
    }
}