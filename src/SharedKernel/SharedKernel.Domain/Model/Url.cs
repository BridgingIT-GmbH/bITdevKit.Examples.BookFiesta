// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using System.Text.RegularExpressions;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Model;

[DebuggerDisplay("Value={Value}")]
public partial class Url : ValueObject
{
    private static readonly Regex AbsoluteUrlRegex = AbsoluteRegex();
    private static readonly Regex RelativeUrlRegex = RelativeRegex();
    private static readonly Regex LocalUrlRegex = LocalRegex();

    private Url() { } // Private constructor required by EF Core

    private Url(string url)
    {
        this.Value = url;
    }

    public string Value { get; private set; }

    public UrlType Type => DetermineType(this.Value);

    public static implicit operator string(Url url)
    {
        return url.Value;
    }

    public static implicit operator Url(string value)
    {
        return Create(value);
    }

    public static Url Create(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null; //throw new DomainRuleException("Url cannot be empty.");
        }

        var normalizedUrl = Normalize(value);
        if (!IsValid(normalizedUrl))
        {
            throw new DomainRuleException($"Invalid URL format: {value}");
        }

        return new Url(normalizedUrl);
    }

    public bool IsAbsolute()
    {
        return this.Type == UrlType.Absolute;
    }

    public bool IsRelative()
    {
        return this.Type == UrlType.Relative;
    }

    public bool IsLocal()
    {
        return this.Type == UrlType.Local;
    }

    public string ToAbsolute(string value)
    {
        if (this.IsAbsolute())
        {
            return this.Value;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Base URL is required for converting relative or local URLs to absolute.", nameof(value));
        }

        var normalizedBaseUrl = Normalize(value);
        return this.IsRelative() ? $"{normalizedBaseUrl}{this.Value}" : $"{normalizedBaseUrl}/{this.Value}";
    }

    public override string ToString()
    {
        return this.Value;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
        yield return this.Type;
    }

    private static string Normalize(string value)
    {
        return value?.Trim()
            ?.TrimEnd('/');
    }

    private static UrlType DetermineType(string value)
    {
        if (AbsoluteUrlRegex.IsMatch(value))
        {
            return UrlType.Absolute;
        }
        else if (RelativeUrlRegex.IsMatch(value))
        {
            return UrlType.Relative;
        }
        else if (LocalUrlRegex.IsMatch(value))
        {
            return UrlType.Local;
        }

        return UrlType.Invalid;
    }

    private static bool IsValid(string value)
    {
        return IsValid(value, DetermineType(value));
    }

    private static bool IsValid(string value, UrlType type)
    {
        return type switch
        {
            UrlType.Absolute => AbsoluteUrlRegex.IsMatch(value),
            UrlType.Relative => RelativeUrlRegex.IsMatch(value),
            UrlType.Local => LocalUrlRegex.IsMatch(value),
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