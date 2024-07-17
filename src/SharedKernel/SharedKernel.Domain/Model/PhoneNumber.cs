// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.SharedKernel.Domain;

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

    public static PhoneNumber Create(string phoneNumber)
    {
        if (!IsValid(phoneNumber))
        {
            throw new BusinessRuleNotSatisfiedException("Invalid phone number format.");
        }

        var cleanNumber = CleanNumber(phoneNumber);
        var countryCode = ExtractCountryCode(cleanNumber);
        var number = cleanNumber[countryCode.Length..];

        return new PhoneNumber(countryCode, number);
    }

    public static bool IsValid(string phoneNumber)
    {
        return !string.IsNullOrWhiteSpace(phoneNumber) &&
            PhoneRegex.IsMatch(CleanNumber(phoneNumber));
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

    private static string CleanNumber(string phoneNumber)
    {
        return new string(phoneNumber.Where(char.IsDigit).ToArray());
    }

    private static string ExtractCountryCode(string cleanNumber)
    {
        if (cleanNumber.StartsWith("00"))
        {
            return cleanNumber.Substring(2, 1);
        }

        return cleanNumber.StartsWith(Prefix)
            ? cleanNumber.Substring(1, 1)
            : string.Empty;
    }
}