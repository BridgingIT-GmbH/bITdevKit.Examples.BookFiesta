namespace BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

using System.Text.RegularExpressions;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Model;

[DebuggerDisplay("CountryCode={CountryCode}, Number={Number}")]
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

    private VatNumber() { } // Private constructor required by EF Core

    private VatNumber(string countryCode, string number)
    {
        this.CountryCode = countryCode;
        this.Number = number;
    }

    public string CountryCode { get; }

    public string Number { get; }

    public static implicit operator string(VatNumber vatNumber) =>vatNumber.ToString();

    public static implicit operator VatNumber(string value) => Create(value);

    public static VatNumber Create(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null; //throw new DomainRuleException("VatNumber number cannot be empty.");
        }

        value = Normalize(value);
        var countryCode = value[..2];
        var number = value[2..];

        if (CountryVatFormats.TryGetValue(countryCode, out var regex))
        {
            if (!regex.IsMatch(value))
            {
                throw new DomainRuleException($"Invalid VAT/EIN number ({value}) format for country {countryCode}.");
            }
        }
        else if (!GeneralVatFormat.IsMatch(value))
        {
            throw new DomainRuleException($"Invalid VAT number  ({value}) format.");
        }
        else
        {
            Console.WriteLine($"Warning: No specific validation for VAT number ({value})  country code {countryCode}.");
        }

        return new VatNumber(countryCode, number);
    }

    public static bool TryParse(string value, out VatNumber result)
    {
        try
        {
            result = Create(value);

            return true;
        }
        catch (DomainRuleException)
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

    private static string Normalize(string value)
    {
        return value?.ToUpperInvariant()
            .Replace(" ", string.Empty)
            .Replace("--", "-");
    }
}