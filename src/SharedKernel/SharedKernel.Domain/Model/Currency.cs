// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

[DebuggerDisplay("Code={Code}")]
public class Currency : ValueObject
{
    private static readonly Dictionary<string, string> Currencies = new()
    {
        { "ALL", "Lek" },
        { "AFN", "؋" },
        { "ARS", "$" },
        { "AWG", "ƒ" },
        { "AUD", "$" },
        { "AZN", "₼" },
        { "BSD", "$" },
        { "BBD", "$" },
        { "BYN", "Br" },
        { "BZD", "BZ$" },
        { "BMD", "$" },
        { "BOB", "$b" },
        { "BAM", "KM" },
        { "BWP", "P" },
        { "BGN", "лв" },
        { "BRL", "R$" },
        { "BND", "$" },
        { "KHR", "៛" },
        { "CAD", "$" },
        { "KYD", "$" },
        { "CLP", "$" },
        { "CNY", "¥" },
        { "COP", "$" },
        { "CRC", "₡" },
        { "HRK", "kn" },
        { "CUP", "₱" },
        { "CZK", "Kč" },
        { "DKK", "kr" },
        { "DOP", "RD$" },
        { "XCD", "$" },
        { "EGP", "£" },
        { "SVC", "$" },
        { "EUR", "€" },
        { "FKP", "£" },
        { "FJD", "$" },
        { "GHS", "¢" },
        { "GIP", "£" },
        { "GTQ", "Q" },
        { "GGP", "£" },
        { "GYD", "$" },
        { "HNL", "L" },
        { "HKD", "$" },
        { "HUF", "Ft" },
        { "ISK", "kr" },
        { "INR", "₹" },
        { "IDR", "Rp" },
        { "IRR", "﷼" },
        { "IMP", "£" },
        { "ILS", "₪" },
        { "JMD", "J$" },
        { "JPY", "¥" },
        { "JEP", "£" },
        { "KZT", "лв" },
        { "KPW", "₩" },
        { "KRW", "₩" },
        { "KGS", "лв" },
        { "LAK", "₭" },
        { "LBP", "£" },
        { "LRD", "$" },
        { "MKD", "ден" },
        { "MYR", "RM" },
        { "MUR", "₨" },
        { "MXN", "$" },
        { "MNT", "₮" },
        { "MZN", "MT" },
        { "NAD", "$" },
        { "NPR", "₨" },
        { "ANG", "ƒ" },
        { "NZD", "$" },
        { "NIO", "C$" },
        { "NGN", "₦" },
        { "NOK", "kr" },
        { "OMR", "﷼" },
        { "PKR", "₨" },
        { "PAB", "B/." },
        { "PYG", "Gs" },
        { "PEN", "S/." },
        { "PHP", "₱" },
        { "PLN", "zł" },
        { "QAR", "﷼" },
        { "RON", "lei" },
        { "RUB", "₽" },
        { "SHP", "£" },
        { "SAR", "﷼" },
        { "RSD", "Дин." },
        { "SCR", "₨" },
        { "SGD", "$" },
        { "SBD", "$" },
        { "SOS", "S" },
        { "ZAR", "R" },
        { "LKR", "₨" },
        { "SEK", "kr" },
        { "CHF", "CHF" },
        { "SRD", "$" },
        { "SYP", "£" },
        { "TWD", "NT$" },
        { "THB", "฿" },
        { "TTD", "TT$" },
        { "TRY", "₺" },
        { "TVD", "$" },
        { "UAH", "₴" },
        { "GBP", "£" },
        { "USD", "$" },
        { "UYU", "$U" },
        { "UZS", "лв" },
        { "VEF", "Bs" },
        { "VND", "₫" },
        { "YER", "﷼" },
        { "ZWD", "Z$" }
    };

    private Currency() { } // Private constructor required by EF Core

    private Currency(string code)
    {
        this.Code = string.Intern(code.ToUpperInvariant());
    }

    public static Currency UsDollar => Create("USD");

    public static Currency Euro => Create("EUR");

    public static Currency BritishPound => Create("GBP");

    public static Currency JapaneseYen => Create("JPY");

    public static Currency SwissFranc => Create("CHF");

    public static Currency CanadianDollar => Create("CAD");

    public static Currency AustralianDollar => Create("AUD");

    public static Currency ChineseYuan => Create("CNY");

    public string Code { get; private set; }

    public string Symbol => Currencies.First(c => c.Key == this.Code).Value;

    public static implicit operator string(Currency currency) => currency?.Code;

    public static implicit operator Currency(string value) => new(value);

    public static Currency Create(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            throw new DomainRuleException("Currency code cannot be null or empty");
        }

        if (code.Length != 3)
        {
            throw new ArgumentException("Currency code must be exactly three characters long.");
        }

        code = code.ToUpperInvariant();

        if (!Currencies.ContainsKey(code))
        {
            throw new DomainRuleException($"Invalid currency code: {code}");
        }

        return new Currency(code); //Currencies.First(c => c.Key == code).Value; //Currencies[code];
    }

    public override string ToString()
    {
        return this.Symbol;
        // https://social.technet.microsoft.com/wiki/contents/articles/27931.currency-formatting-in-c.aspx
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Code;
    }
}