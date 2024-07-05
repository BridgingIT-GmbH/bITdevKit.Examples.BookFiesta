namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

using System.Text.RegularExpressions;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Model;

public partial class EmailAddress : ValueObject
{
    private EmailAddress()
    {
    }

    private EmailAddress(string email)
    {
        this.Value = email;
    }

    public string Value { get; private set; }

    public static implicit operator string(EmailAddress email) => email.Value;

    public static EmailAddress Create(string email)
    {
        email = Normalize(email);
        if (!IsValid(email))
        {
            throw new BusinessRuleNotSatisfiedException("Invalid email address");
        }

        return new EmailAddress(email);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }

    private static string Normalize(string email)
    {
        return email?.Trim()?.ToLowerInvariant() ?? string.Empty;
    }

    [GeneratedRegex(@"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex IsValidRegex();

    private static bool IsValid(string email)
    {
        return !string.IsNullOrEmpty(email) && email.Length <= 255 && IsValidRegex().IsMatch(email);
    }
}