// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Model;

[DebuggerDisplay("CountryCode={CountryCode}, Number={Number}")]
public class PhoneNumber : ValueObject
{
    // This regex pattern allows for 1 to 3 digit country codes, followed by the rest of the number
    private static readonly Regex PhoneRegex = new(@"^\+?(\d{1,3})([0-9\s\-\(\)\.]{1,20})$", RegexOptions.Compiled);

    // Source: ITU-T Recommendation E.164 (as of 2021)
    // Note: This list should be periodically reviewed and updated
    private static readonly HashSet<string> CountryCodes = new HashSet<string>
    {
        // Single-digit country codes
        "1",
        // Two-digit country codes
        "20", "27", "30", "31", "32", "33", "34", "36", "39", "40", "41", "43", "44", "45", "46", "47", "48", "49",
        "51", "52", "53", "54", "55", "56", "57", "58", "60", "61", "62", "63", "64", "65", "66", "81", "82", "84",
        "86", "90", "91", "92", "93", "94", "95", "98",
        // Three-digit country codes
        "210", "211", "212", "213", "216", "218", "220", "221", "222", "223", "224", "225", "226", "227", "228",
        "229", "230", "231", "232", "233", "234", "235", "236", "237", "238", "239", "240", "241", "242", "243",
        "244", "245", "246", "247", "248", "249", "250", "251", "252", "253", "254", "255", "256", "257", "258",
        "260", "261", "262", "263", "264", "265", "266", "267", "268", "269", "290", "291", "297", "298", "299",
        "350", "351", "352", "353", "354", "355", "356", "357", "358", "359", "370", "371", "372", "373", "374",
        "375", "376", "377", "378", "379", "380", "381", "382", "383", "385", "386", "387", "389", "420", "421",
        "423", "500", "501", "502", "503", "504", "505", "506", "507", "508", "509", "590", "591", "592", "593",
        "594", "595", "596", "597", "598", "599", "670", "672", "673", "674", "675", "676", "677", "678", "679",
        "680", "681", "682", "683", "685", "686", "687", "688", "689", "690", "691", "692", "800", "808", "850",
        "852", "853", "855", "856", "870", "878", "880", "881", "882", "883", "886", "888", "960", "961", "962",
        "963", "964", "965", "966", "967", "968", "970", "971", "972", "973", "974", "975", "976", "977", "979",
        "992", "993", "994", "995", "996", "998"
    };

    private PhoneNumber(string countryCode, string number)
    {
        this.CountryCode = countryCode;
        this.Number = number;
    }

    public string CountryCode { get; }

    public string Number { get; }

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber?.ToString();

    public static implicit operator PhoneNumber(string value) => Create(value);

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        var cleanNumber = CleanNumber(value);
        if (!IsValid(cleanNumber))
        {
            throw new DomainRuleException("Invalid phone number format.");
        }

        var countryCode = ExtractCountryCode(cleanNumber);
        var number = cleanNumber.Substring(countryCode.Length).TrimStart('0');

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
        return !string.IsNullOrWhiteSpace(value) && PhoneRegex.IsMatch(value);
    }

    private static string CleanNumber(string value)
    {
        return new string(value.Where(c => char.IsDigit(c) && c != '+').ToArray()).TrimStart('0');
    }

    private static string ExtractCountryCode(string value)
    {
        foreach (var length in new[] { 3, 2, 1 })
        {
            if (value.Length >= length)
            {
                var potentialCode = value.Substring(0, length);
                if (CountryCodes.Contains(potentialCode))
                {
                    return potentialCode;
                }
            }
        }

        throw new DomainRuleException("Invalid or unsupported country code.");
    }
}