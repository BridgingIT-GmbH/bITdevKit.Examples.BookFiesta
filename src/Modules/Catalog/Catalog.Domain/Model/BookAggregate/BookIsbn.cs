namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

using System.Text.RegularExpressions;

[DebuggerDisplay("Value={Value}, Type={Type}")]
public partial class BookIsbn : ValueObject
{
    private static readonly Regex IsbnRegex = Regexes.IsbnRegex();
    private static readonly Regex Isbn10Regex = Regexes.Isbn10Regex();
    private static readonly Regex Isbn13Regex = Regexes.Isbn13Regex();

    private BookIsbn() { }

    private BookIsbn(string value)
    {
        Validate(value);

        this.Value = value;

        // Determine ISBN type
        if (Isbn13Regex.IsMatch(value))
        {
            this.Type = "ISBN-13";
        }
        else if (Isbn10Regex.IsMatch(value))
        {
            this.Type = "ISBN-10";
        }
    }

    public string Value { get; private set; }

    public string Type { get; private set; }

    public static implicit operator BookIsbn(string value)
    {
        return Create(value);
        // allows a String value to be implicitly converted to a BookIsbn object.
    }

    public static implicit operator string(BookIsbn isbn)
    {
        return isbn.Value;
        // allows a BookIsbn value to be implicitly converted to a String.
    }

    public static BookIsbn Create(string value)
    {
        value = value?.ToUpperInvariant()
                ?.Replace("ISBN-10", string.Empty)
                ?.Replace("ISBN-13", string.Empty)
                ?.Replace("ISBN", string.Empty)
                ?.Trim() ??
            string.Empty;

        return new BookIsbn(value);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }

    private static void Validate(string isbn)
    {
        if (string.IsNullOrEmpty(isbn))
        {
            throw new ArgumentException("ISBN cannot be empty.");
        }

        if (!IsbnRegex.IsMatch(isbn))
        {
            throw new ArgumentException("ISBN is not a valid ISBN-10 or ISBN-13.");
        }
    }

    public static partial class Regexes
    {
        [GeneratedRegex(@"^(?:ISBN(?:-1[03])?:?\s*)?(?=[-0-9X ]{10,17}$)(?:97[89][ -]?)?\d{1,5}[ -]?\d{1,7}[ -]?\d{1,7}[ -]?[0-9X]$", RegexOptions.Compiled)]
        public static partial Regex IsbnRegex();

        [GeneratedRegex(@"^\d{1,5}[\s-]?\d{1,7}[\s-]?\d{1,7}[\s-]?[0-9X]$", RegexOptions.Compiled)]
        public static partial Regex Isbn10Regex();

        [GeneratedRegex(@"^(97[89])[\s-]?\d{1,5}[\s-]?\d{1,7}[\s-]?\d{1,7}[\s-]?\d$", RegexOptions.Compiled)]
        public static partial Regex Isbn13Regex();
    }
}