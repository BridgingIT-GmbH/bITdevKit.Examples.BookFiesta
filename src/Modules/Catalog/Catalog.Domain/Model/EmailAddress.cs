namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

using System.Text.RegularExpressions;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Model;

public class EmailAddress : ValueObject
{
    private static readonly Regex EmailRegex = new Regex(
        @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private EmailAddress()
    {
    }

    private EmailAddress(string value)
    {
        this.Value = value;
    }

    public string Value { get; private set; }

    public static implicit operator string(EmailAddress email) => email.Value;

    public static EmailAddress Create(string value)
    {
        var email = new EmailAddress(value?.Trim().ToLowerInvariant());
        if (!email.IsValid())
        {
            throw new BusinessRuleNotSatisfiedException("Invalid email address");
        }

        return email;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.Value;
    }

    private bool IsValid()
    {
        return !string.IsNullOrEmpty(this.Value) && this.Value.Length <= 255 && EmailRegex.IsMatch(this.Value);
    }
}