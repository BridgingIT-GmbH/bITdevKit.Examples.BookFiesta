namespace BridgingIT.DevKit.Examples.BookStore.SharedKernel.Domain;

using System.Text.RegularExpressions;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Model;

public class VatNumber : ValueObject
{
    private static readonly Regex GeneralVatFormat = new(@"^[A-Z]{2}[0-9A-Z]+$", RegexOptions.Compiled);

    private static readonly Dictionary<string, Regex> CountryVatFormats = new()
    {
        ["DE"] = new Regex(@"^DE[0-9]{9}$", RegexOptions.Compiled),
        ["GB"] = new Regex(@"^GB([0-9]{9}([0-9]{3})?|[A-Z]{2}[0-9]{3})$", RegexOptions.Compiled),
        ["FR"] = new Regex(@"^FR[A-Z0-9]{2}[0-9]{9}$", RegexOptions.Compiled),
        ["US"] = new Regex(@"^US[0-9]{2}-[0-9]{7}$", RegexOptions.Compiled),
        // Add more countries as needed
    };

    private VatNumber(string countryCode, string number)
    {
        this.CountryCode = countryCode;
        this.Number = number;
    }

    public string CountryCode { get; }

    public string Number { get; }

    public static VatNumber Create(string vatNumber)
    {
        if (string.IsNullOrWhiteSpace(vatNumber))
        {
            throw new BusinessRuleNotSatisfiedException("VAT/EIN number cannot be empty.");
        }

        vatNumber = vatNumber.ToUpperInvariant().Replace(" ", string.Empty);

        var countryCode = vatNumber[..2];
        var number = vatNumber[2..];

        if (CountryVatFormats.TryGetValue(countryCode, out var regex))
        {
            if (!regex.IsMatch(vatNumber))
            {
                throw new BusinessRuleNotSatisfiedException($"Invalid VAT/EIN number format for country {countryCode}.");
            }
        }
        else if (!GeneralVatFormat.IsMatch(vatNumber))
        {
            throw new BusinessRuleNotSatisfiedException("Invalid VAT number format.");
        }
        else
        {
            Console.WriteLine($"Warning: No specific validation for VAT number country code {countryCode}.");
        }

        return new VatNumber(countryCode, number);
    }

    public static bool TryParse(string vatNumber, out VatNumber result)
    {
        try
        {
            result = Create(vatNumber);

            return true;
        }
        catch (BusinessRuleNotSatisfiedException)
        {
            result = null;

            return false;
        }
    }

    public override string ToString()
    {
        if (this.CountryCode == "US")
        {
            return $"{this.CountryCode}{this.Number[..2]}-{this.Number[2..]}";
        }

        return $"{this.CountryCode}{this.Number}";
    }

    public bool IsValid()
    {
        // Here you could implement more complex validation logic,
        // such as checksum validation for certain country codes
        return true;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.CountryCode;
        yield return this.Number;
    }
}