// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using System.Text.RegularExpressions;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Model;

public partial class Url : ValueObject
{
    private static readonly Regex AbsoluteUrlRegex = AbsoluteRegex();
    private static readonly Regex RelativeUrlRegex = RelativeRegex();
    private static readonly Regex LocalUrlRegex = LocalRegex();

    private Url(string url)
    {
        this.Value = url;
    }

    public string Value { get; private set; }

    public UrlType Type => DetermineType(this.Value);

    public static implicit operator string(Url url) => url.Value;

    public static implicit operator Url(string url) => Create(url);

    public static Url Create(string url)
    {
        var normalizedUrl = NormalizeUrl(url);

        if (!IsValid(normalizedUrl))
        {
            throw new BusinessRuleNotSatisfiedException($"Invalid URL format: {url}");
        }

        return new Url(normalizedUrl);
    }

    public static bool IsValid(string url)
    {
        url = NormalizeUrl(url);
        var type = DetermineType(url);

        return IsValid(url, type);
    }

    public bool IsAbsolute() => this.Type == UrlType.Absolute;

    public bool IsRelative() => this.Type == UrlType.Relative;

    public bool IsLocal() => this.Type == UrlType.Local;

    public string ToAbsolute(string baseUrl)
    {
        if (this.IsAbsolute())
        {
            return this.Value;
        }

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ArgumentException("Base URL is required for converting relative or local URLs to absolute.", nameof(baseUrl));
        }

        var normalizedBaseUrl = NormalizeUrl(baseUrl);
        return this.IsRelative()
            ? $"{normalizedBaseUrl}{this.Value}"
            : $"{normalizedBaseUrl}/{this.Value}";
    }

    public override string ToString() => this.Value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
        yield return this.Type;
    }

    private static string NormalizeUrl(string url)
    {
        return url.Trim().TrimEnd('/');
    }

    private static UrlType DetermineType(string url)
    {
        if (AbsoluteUrlRegex.IsMatch(url))
        {
            return UrlType.Absolute;
        }
        else if (RelativeUrlRegex.IsMatch(url))
        {
            return UrlType.Relative;
        }
        else if (LocalUrlRegex.IsMatch(url))
        {
            return UrlType.Local;
        }

        return UrlType.Invalid;
    }

    private static bool IsValid(string url, UrlType type)
    {
        if (string.IsNullOrEmpty(url))
        {
            return true;
        }

        return type switch
        {
            UrlType.Absolute => AbsoluteUrlRegex.IsMatch(url),
            UrlType.Relative => RelativeUrlRegex.IsMatch(url),
            UrlType.Local => LocalUrlRegex.IsMatch(url),
            _ => false
        };
    }

    [GeneratedRegex(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex AbsoluteRegex();
    [GeneratedRegex(@"^(\/|\.\.?\/)([\w\.-]+\/?)*$", RegexOptions.Compiled)]
    private static partial Regex RelativeRegex();
    [GeneratedRegex(@"^[\w\.-]+(\/[\w\.-]+)*\/?$", RegexOptions.Compiled)]
    private static partial Regex LocalRegex();
}

public enum UrlType
{
    Absolute,
    Relative,
    Local,
    Invalid
}