// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using System.Diagnostics;
using System.Text.RegularExpressions;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Model;

[DebuggerDisplay("CountryCode={CountryCode}, Number={Number}")]
public class PhoneNumber : ValueObject
{
    private const string Prefix = "+";
    private static readonly Regex PhoneRegex = new(@"^\+?(\d{1,3})[-.\s]?\(?\d{1,3}\)?[-.\s]?\d{1,4}[-.\s]?\d{1,4}$", RegexOptions.Compiled);

    private PhoneNumber() { } // Private constructor for EF Core

    private PhoneNumber(string countryCode, string number)
    {
        this.CountryCode = countryCode;
        this.Number = number;
    }

    public string CountryCode { get; private set; }

    public string Number { get; private set; }

    public static implicit operator string(PhoneNumber phoneNumber) =>phoneNumber.ToString();

    public static implicit operator PhoneNumber(string value) => Create(value);

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null; //throw new DomainRuleException("PhoneNumber cannot be empty.");
        }

        if (!IsValid(value))
        {
            throw new DomainRuleException("Invalid phone number format.");
        }

        var cleanNumber = CleanNumber(value);
        var countryCode = ExtractCountryCode(cleanNumber);
        var number = cleanNumber[countryCode.Length..];

        return new PhoneNumber(countryCode, number);
    }

    public override string ToString()
    {
        return $"+{this.CountryCode} {this.Number}";
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.CountryCode;
        yield return this.Number;
    }

    private static bool IsValid(string value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               PhoneRegex.IsMatch(CleanNumber(value));
    }

    private static string CleanNumber(string value)
    {
        return new string(value.Where(char.IsDigit).ToArray());
    }

    private static string ExtractCountryCode(string value)
    {
        if (value.StartsWith("00"))
        {
            return value.Substring(2, 1);
        }

        return value.StartsWith(Prefix)
            ? value.Substring(1, 1)
            : string.Empty;
    }
}